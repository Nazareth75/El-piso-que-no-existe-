using System.IO;
using UnityEngine;

public static class RecursosCampus
{
    public static bool Existe(string relativo)
    {
        return File.Exists(Path.Combine(Application.streamingAssetsPath, relativo));
    }

    public static Texture2D Textura(string relativo)
    {
        string ruta = Path.Combine(Application.streamingAssetsPath, relativo);
        if (!File.Exists(ruta)) return null;

        var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        tex.wrapMode = TextureWrapMode.Clamp;
        if (!tex.LoadImage(File.ReadAllBytes(ruta)))
        {
            Debug.LogError("No se pudo leer: " + ruta);
            return null;
        }

        return tex;
    }

    public static Sprite ComoSprite(Texture2D tex, float pixelesPorUnidad, Vector2 pivote)
    {
        if (tex == null) return null;
        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), pivote, pixelesPorUnidad);
    }

    public static Sprite[] Carpeta(string carpeta, float pixelesPorUnidad)
    {
        var lista = new System.Collections.Generic.List<Sprite>();
        for (int i = 0; i < 16; i++)
        {
            string figura = "Figuras/" + carpeta + "/" + i + ".png";
            string sala = "Salas/" + carpeta + "/" + i + ".png";
            string ruta = Existe(figura) ? figura : sala;
            if (!Existe(ruta)) break;
            var tex = Textura(ruta);
            if (tex == null) break;
            lista.Add(ComoSprite(tex, pixelesPorUnidad, new Vector2(0.5f, 0.08f)));
        }

        return lista.ToArray();
    }

    public static Sprite[] CarpetaPiso(string carpeta)
    {
        var lista = new System.Collections.Generic.List<Sprite>();
        for (int i = 0; i < 8; i++)
        {
            string ruta = "PisoX/" + carpeta + "/" + i + ".png";
            if (!Existe(ruta)) break;
            var tex = Textura(ruta);
            if (tex == null) break;
            lista.Add(ComoSprite(tex, 48f, new Vector2(0.5f, 0.5f)));
        }

        return lista.ToArray();
    }
}
