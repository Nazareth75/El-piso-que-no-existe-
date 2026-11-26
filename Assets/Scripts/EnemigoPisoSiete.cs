using UnityEngine;

public class EnemigoPisoSiete : MonoBehaviour
{
    PisoSiete nivel;
    Sprite[] frames;
    SpriteRenderer dibujo;
    Vector2 origen;
    Vector2 meta;
    Vector2 paso;
    Vector2 ultimaVista;
    float velocidad;
    int dano;
    float animacion;
    float recalcular;
    float aturdido;
    float memoria;
    bool regreso;
    float proximoGruñido;
    bool cazaba;

    public void Preparar(PisoSiete piso, Sprite[] animaciones, Vector2 destino, float rapidez, int golpe)
    {
        nivel = piso;
        frames = animaciones;
        dibujo = GetComponent<SpriteRenderer>();
        origen = transform.position;
        meta = destino;
        paso = origen;
        velocidad = rapidez;
        dano = golpe;
    }

    public void Aturdir() { SonidoCampus.EnPosicion("aturdido", transform.position, .4f); aturdido = 3f; memoria = 0f; }

    void Update()
    {
        if (nivel == null || nivel.director == null || nivel.director.bloqueado || !nivel.director.jugando) return;
        if (aturdido > 0f)
        {
            aturdido -= Time.deltaTime;
            dibujo.color = new Color(0.2f, 1f, 1f);
            return;
        }
        dibujo.color = Color.white;
        Vector2 pos = transform.position;
        Vector2 jugador = nivel.jugador.position;
        bool contenido = EstadoPisoSiete.Lista("antidoto");
        bool ve = Vector2.Distance(pos, jugador) < (contenido ? 3f : 5f) && nivel.LineaLibre(pos, jugador);
        memoria = ve ? 2f : Mathf.Max(0f, memoria - Time.deltaTime);
        if (ve) ultimaVista = jugador;
        bool caza = memoria > 0f;
        if (caza) SonidoCampus.Amenaza();
        if (caza && !cazaba) SonidoCampus.EnPosicion("alerta_monstruo", pos, .6f);
        cazaba = caza;
        if (Vector2.Distance(pos, jugador) < 7f && Time.time > proximoGruñido)
        {
            SonidoCampus.EnPosicion("monstruo", pos, .55f);
            proximoGruñido = Time.time + Random.Range(4.5f, 8f);
        }
        Vector2 destino = caza ? ultimaVista : (regreso ? origen : meta);
        if (!caza && Vector2.Distance(pos, destino) < 0.4f) regreso = !regreso;
        recalcular -= Time.deltaTime;
        if (recalcular <= 0f || Vector2.Distance(pos, paso) < 0.12f)
        {
            paso = nivel.SiguientePaso(pos, destino);
            recalcular = 0.35f;
        }
        Vector2 direccion = paso - pos;
        float rapidez = velocidad * (caza ? 1f : 0.5f) * (contenido ? 0.62f : 1f);
        Vector2 siguiente = Vector2.MoveTowards(pos, paso, rapidez * Time.deltaTime);
        if (nivel.Libre(siguiente)) transform.position = siguiente;
        else recalcular = 0f;
        dibujo.flipX = direccion.x < 0f;
        dibujo.sortingOrder = 120 - Mathf.RoundToInt(transform.position.y * 3f);
        if (frames.Length > 0)
        {
            animacion += Time.deltaTime * (caza ? 10f : 6f);
            dibujo.sprite = frames[Mathf.FloorToInt(animacion) % frames.Length];
        }
        if (Vector2.Distance(transform.position, jugador) < 0.8f && nivel.LineaLibre(transform.position, jugador)) nivel.RecibirGolpe(dano);
    }
}
