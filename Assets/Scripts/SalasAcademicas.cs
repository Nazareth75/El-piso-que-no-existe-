using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class EntradaAcademica
{
    public string id;
    public int piso;
    // Panel de la lámina original: 0 aula, 1 laboratorio, 2 clínica.
    public int tipo;
    public string nombre;
    public float x;
    public float y;
    public float puertaX;
    public float puertaY;
    public Vector2 Posicion => new Vector2(x, y);
}

public static class SalasAcademicas
{
    [Serializable] class Catalogo { public EntradaAcademica[] salas = new EntradaAcademica[0]; }
    static EntradaAcademica[] entradas;
    public static readonly HashSet<string> visitadas = new HashSet<string>();
    public static readonly HashSet<string> apuntes = new HashSet<string>();

    public static EntradaAcademica[] Todas
    {
        get
        {
            if (entradas == null)
            {
                string ruta = Path.Combine(Application.streamingAssetsPath, "Pisos/aulas.json");
                if (!File.Exists(ruta))
                {
                    Debug.LogError("Falta el catálogo de aulas: " + ruta);
                    return new EntradaAcademica[0];
                }
                entradas = JsonUtility.FromJson<Catalogo>(File.ReadAllText(ruta)).salas ?? new EntradaAcademica[0];
            }
            return entradas;
        }
    }

    public static EntradaAcademica Buscar(string id)
    {
        foreach (var sala in Todas) if (sala.id == id) return sala;
        return null;
    }

    public static Vector2 Interior(float x, float y) => new Vector2(x / 724f * 18f, (724f - y) / 724f * 18f);

    // Obstáculos en píxeles del panel correspondiente; las tres ilustraciones permanecen intactas.
    public static Rect[] Muebles(int tipo)
    {
        var lista = new List<Rect>();
        if (tipo == 0)
        {
            lista.Add(new Rect(385, 270, 140, 65));
            lista.Add(new Rect(575, 140, 95, 142));
            float[] columnas = { 80, 208, 407, 550 };
            float[] filas = { 322, 414, 506 };
            foreach (float x in columnas)
                foreach (float y in filas) lista.Add(new Rect(x, y, 90, 74));
        }
        else if (tipo == 1)
        {
            lista.Add(new Rect(285, 236, 75, 45));
            lista.Add(new Rect(592, 160, 65, 128));
            foreach (float y in new float[] { 278, 390, 505 })
            {
                lista.Add(new Rect(164, y, 390, 55));
                foreach (float x in new float[] { 178, 268, 358, 448 })
                    lista.Add(new Rect(x, y + 55, 40, 25));
            }
        }
        else
        {
            lista.Add(new Rect(64, 280, 188, 95));
            lista.Add(new Rect(230, 260, 180, 116));
            lista.Add(new Rect(510, 170, 175, 185));
            lista.Add(new Rect(350, 460, 215, 98));
            lista.Add(new Rect(620, 385, 50, 150));
            lista.Add(new Rect(60, 435, 200, 145));
        }
        return lista.ToArray();
    }

    public static Vector2 Actividad(int tipo) => tipo == 0 ? Interior(544, 318) : tipo == 1 ? Interior(395, 265) : Interior(285, 402);
    public static Vector2 Apunte(int tipo) => tipo == 0 ? Interior(523, 490) : tipo == 1 ? Interior(582, 478) : Interior(582, 570);

    public static void Reiniciar()
    {
        visitadas.Clear();
        apuntes.Clear();
    }
}

// Cada interior crea un sprite de un panel de la lámina, y lo libera al salir.
public class ArteSalaAcademica : MonoBehaviour
{
    public Sprite sprite;
    void OnDestroy() { if (sprite != null) Destroy(sprite); }
}
