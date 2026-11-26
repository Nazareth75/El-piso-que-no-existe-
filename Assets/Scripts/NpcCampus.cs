using System.Collections.Generic;
using UnityEngine;

public class NpcCampus : MonoBehaviour
{
    public Sprite[] frames;
    public Vector2[] ruta;
    public float velocidad = 1.45f;
    public bool camina = true;

    const float Radio = 0.26f;
    const float Paso = 0.5f;
    static readonly Vector2 Cuerpo = new Vector2(0f, 0.32f);

    SpriteRenderer dibujo;
    int indice;
    float anim;
    float atascado;
    float distanciaPrevia = 999f;

    void Awake()
    {
        dibujo = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        if (!camina || ruta == null || ruta.Length < 2) return;
        var recorrido = new List<Vector2>();
        Vector2 aqui = transform.position;
        for (int i = 0; i < ruta.Length; i++)
        {
            foreach (var punto in Buscar(aqui, ruta[i]))
                recorrido.Add(punto);
            aqui = ruta[i];
        }

        if (recorrido.Count > 1)
            ruta = recorrido.ToArray();
        transform.position = CercaLibre(transform.position);
    }

    void Update()
    {
        var juego = DirectorCampus.Instancia;
        if (juego != null && juego.bloqueado) return;
        if (frames == null || frames.Length == 0 || dibujo == null) return;

        anim += Time.deltaTime * (camina ? 8f : 2.5f);
        dibujo.sprite = frames[Mathf.FloorToInt(anim) % frames.Length];
        dibujo.sortingOrder = 80 - Mathf.RoundToInt(transform.position.y * 3f);

        if (!camina || ruta == null || ruta.Length < 2) return;

        Vector2 destino = ruta[indice];
        Vector2 pos = transform.position;
        Vector2 dir = destino - pos;
        float distancia = dir.magnitude;
        if (distancia < 0.12f)
        {
            indice = (indice + 1) % ruta.Length;
            atascado = 0f;
            distanciaPrevia = 999f;
            return;
        }

        dir /= distancia;
        float avance = velocidad * Time.deltaTime;
        if (Libre(pos, pos + dir * avance))
        {
            transform.position = pos + dir * avance;
            atascado = distancia < distanciaPrevia - 0.01f ? 0f : atascado + Time.deltaTime;
        }
        else
        {
            Vector2 lado = new Vector2(-dir.y, dir.x);
            if (Libre(pos, pos + lado * avance)) transform.position = pos + lado * avance;
            else if (Libre(pos, pos - lado * avance)) transform.position = pos - lado * avance;
            atascado += Time.deltaTime;
        }

        distanciaPrevia = distancia;
        if (atascado > 1.4f)
        {
            indice = (indice + 1) % ruta.Length;
            atascado = 0f;
            distanciaPrevia = 999f;
        }

        if (dir.x < -0.02f) dibujo.flipX = true;
        else if (dir.x > 0.02f) dibujo.flipX = false;
    }

    static bool Libre(Vector2 desde, Vector2 hasta)
    {
        Vector2 delta = hasta - desde;
        float largo = delta.magnitude;
        if (largo < 0.0001f) return true;
        var golpe = Physics2D.CircleCast(desde + Cuerpo, Radio, delta / largo, largo, Physics2D.DefaultRaycastLayers, -Mathf.Infinity, Mathf.Infinity);
        return golpe.collider == null;
    }

    static bool CeldaLibre(Vector2 pies)
    {
        return Physics2D.OverlapCircle(pies + Cuerpo, Radio) == null;
    }

    static Vector2 CercaLibre(Vector2 pies)
    {
        if (CeldaLibre(pies)) return pies;
        for (float radio = Paso; radio <= 3f; radio += Paso)
        {
            for (int i = 0; i < 8; i++)
            {
                float angulo = i * Mathf.PI * 0.25f;
                var prueba = pies + new Vector2(Mathf.Cos(angulo), Mathf.Sin(angulo)) * radio;
                if (CeldaLibre(prueba)) return prueba;
            }
        }

        return pies;
    }

    static List<Vector2> Buscar(Vector2 desde, Vector2 hasta)
    {
        var vacio = new List<Vector2> { hasta };
        Vector2 origen = CercaLibre(desde);
        Vector2 meta = CercaLibre(hasta);
        if (!CeldaLibre(origen) || !CeldaLibre(meta)) return vacio;

        var inicio = Nodo(origen);
        var fin = Nodo(meta);
        var abiertos = new List<Vector2Int> { inicio };
        var vino = new Dictionary<Vector2Int, Vector2Int>();
        var costo = new Dictionary<Vector2Int, float> { { inicio, 0f } };
        var mejor = new Vector2Int(999, 999);

        while (abiertos.Count > 0)
        {
            int elegido = 0;
            float menor = 99999f;
            for (int i = 0; i < abiertos.Count; i++)
            {
                float f = costo[abiertos[i]] + Vector2Int.Distance(abiertos[i], fin);
                if (f < menor)
                {
                    menor = f;
                    elegido = i;
                }
            }

            var actual = abiertos[elegido];
            abiertos.RemoveAt(elegido);
            if (actual == fin)
            {
                mejor = actual;
                break;
            }

            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0) continue;
                    if (dx != 0 && dy != 0) continue;
                    var vecino = new Vector2Int(actual.x + dx, actual.y + dy);
                    if (vecino.x < 1 || vecino.y < 1 || vecino.x > 78 || vecino.y > 38) continue;
                    Vector2 pies = new Vector2(vecino.x * Paso, vecino.y * Paso);
                    if (!CeldaLibre(pies) || !Libre(new Vector2(actual.x * Paso, actual.y * Paso), pies)) continue;
                    float nuevo = costo[actual] + Paso;
                    if (costo.ContainsKey(vecino) && nuevo >= costo[vecino]) continue;
                    costo[vecino] = nuevo;
                    vino[vecino] = actual;
                    if (!abiertos.Contains(vecino)) abiertos.Add(vecino);
                }
            }
        }

        if (mejor.x == 999) return vacio;
        var camino = new List<Vector2Int>();
        var cursor = mejor;
        camino.Add(cursor);
        while (vino.ContainsKey(cursor))
        {
            cursor = vino[cursor];
            camino.Add(cursor);
        }

        camino.Reverse();
        var puntos = new List<Vector2>();
        for (int i = 0; i < camino.Count; i++)
        {
            if (i > 0 && i < camino.Count - 1)
            {
                var antes = camino[i] - camino[i - 1];
                var despues = camino[i + 1] - camino[i];
                if (antes == despues) continue;
            }

            puntos.Add(new Vector2(camino[i].x * Paso, camino[i].y * Paso));
        }

        puntos.Add(meta);
        return puntos;
    }

    static Vector2Int Nodo(Vector2 pies)
    {
        return new Vector2Int(Mathf.RoundToInt(pies.x / Paso), Mathf.RoundToInt(pies.y / Paso));
    }
}
