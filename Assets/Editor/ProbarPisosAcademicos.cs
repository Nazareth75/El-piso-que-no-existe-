using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class ProbarPisosAcademicos
{
    const string Pendiente = "Campus.Academico.Pendiente";
    static ProbarPisosAcademicos()
    {
        EditorApplication.playModeStateChanged += estado =>
        {
            if (estado != PlayModeStateChange.EnteredPlayMode) return;
            string opcion = SessionState.GetString(Pendiente, "");
            if (string.IsNullOrEmpty(opcion)) return;
            SessionState.EraseString(Pendiente);
            EditorApplication.delayCall += () => Ejecutar(opcion);
        };
    }

    [MenuItem("Pisos 2-5/Probar piso 2")] static void Piso2() => Abrir("2");
    [MenuItem("Pisos 2-5/Probar piso 3")] static void Piso3() => Abrir("3");
    [MenuItem("Pisos 2-5/Probar piso 4")] static void Piso4() => Abrir("4");
    [MenuItem("Pisos 2-5/Probar piso 5")] static void Piso5() => Abrir("5");
    [MenuItem("Pisos 2-5/Ver interior de aula")] static void Aula() => Abrir("aula_2_08");
    [MenuItem("Pisos 2-5/Ver interior de laboratorio")] static void Laboratorio() => Abrir("aula_2_01");
    [MenuItem("Pisos 2-5/Ver interior de semiología")] static void Clinica() => Abrir("aula_3_01");
    [MenuItem("Pisos 2-5/Validar las 41 entradas")] public static void ValidarTodas() => Abrir("validar");

    static void Abrir(string opcion)
    {
        if (EditorApplication.isPlaying) { Ejecutar(opcion); return; }
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        EditorSceneManager.OpenScene("Assets/Scenes/Campus.unity");
        SessionState.SetString(Pendiente, opcion);
        EditorApplication.isPlaying = true;
    }

    static void Ejecutar(string opcion)
    {
        var director = UnityEngine.Object.FindAnyObjectByType<DirectorCampus>();
        if (director == null) { Debug.LogError("Falta DirectorCampus en Campus.unity"); return; }
        if (opcion == "validar")
        {
            try { Validar(director); director.ProbarAulaAcademica("aula_2_08"); }
            catch (Exception error) { Debug.LogException(error); }
        }
        else if (opcion.StartsWith("aula_")) director.ProbarAulaAcademica(opcion);
        else director.ProbarPisoAcademico(int.Parse(opcion));
    }

    static void Exigir(bool condicion, string mensaje)
    {
        if (!condicion) throw new Exception("Aulas: " + mensaje);
    }

    static void Validar(DirectorCampus director)
    {
        Exigir(SalasAcademicas.Todas.Length == 41, "catálogo incompleto");
        var tipos = new HashSet<int>();
        foreach (var sala in SalasAcademicas.Todas)
        {
            director.ProbarAulaAcademica(sala.id);
            Exigir(director.EnAulaAcademica && director.AulaActual == sala.id && director.PisoActual == sala.piso, "no entra a " + sala.nombre);
            var raiz = GameObject.Find(sala.nombre + " · Piso " + sala.piso);
            var sprite = raiz.GetComponent<SpriteRenderer>().sprite;
            Exigir(sprite.rect.width == 724 && sprite.rect.height == 724 && sprite.rect.x == sala.tipo * 724,
                "panel incorrecto en " + sala.nombre);
            Exigir(sprite.texture.width == 2172 && sprite.texture.height == 724, "no usa la imagen original");
            if (tipos.Add(sala.tipo)) ValidarPasillos(director, sala.tipo);
            // La recogida usa el mismo método que la tecla E y no puede repetirse.
            var usar = typeof(DirectorCampus).GetMethod("UsarAulaAcademica", BindingFlags.Instance | BindingFlags.NonPublic);
            usar.Invoke(director, new object[] { "apunteAcademico" });
            int notas = SalasAcademicas.apuntes.Count;
            usar.Invoke(director, new object[] { "apunteAcademico" });
            Exigir(SalasAcademicas.apuntes.Contains(sala.id) && SalasAcademicas.apuntes.Count == notas, "recogida duplicada");
            director.CerrarPanel();
            director.SalirAulaAcademica();
            Exigir(!director.EnAulaAcademica && director.PisoActual == sala.piso && Vector2.Distance(director.PosicionJugador, sala.Posicion) < .01f,
                "no vuelve a la misma puerta en " + sala.nombre);
        }
        Debug.Log("AULAS_PRUEBAS_OK: 41 entradas y retornos, 3 paneles originales, muebles transitables y recogida sin duplicados.");
    }

    static void ValidarPasillos(DirectorCampus director, int tipo)
    {
        var campo = typeof(DirectorCampus).GetField("marcoGo", BindingFlags.Instance | BindingFlags.NonPublic);
        var marco = (GameObject)campo.GetValue(director);
        Physics2D.SyncTransforms();
        var rectangulos = new List<Rect>();
        foreach (var caja in marco.GetComponentsInChildren<BoxCollider2D>())
        {
            var limites = caja.bounds;
            rectangulos.Add(new Rect(limites.min.x, limites.min.y, limites.size.x, limites.size.y));
        }
        const float paso = .15f;
        const int lado = 121;
        var libres = new bool[lado * lado];
        for (int y = 0; y < lado; y++)
        for (int x = 0; x < lado; x++)
        {
            Vector2 centro = new Vector2(x * paso, y * paso + .32f);
            bool libre = true;
            foreach (var r in rectangulos)
            {
                Vector2 cercano = new Vector2(Mathf.Clamp(centro.x, r.xMin, r.xMax), Mathf.Clamp(centro.y, r.yMin, r.yMax));
                if ((centro - cercano).sqrMagnitude < .3f * .3f) { libre = false; break; }
            }
            libres[y * lado + x] = libre;
        }
        Vector2 spawn = director.PosicionJugador;
        int inicio = Mathf.RoundToInt(spawn.y / paso) * lado + Mathf.RoundToInt(spawn.x / paso);
        Exigir(libres[inicio], "spawn bloqueado en panel " + tipo);
        var visitado = new bool[lado * lado];
        var cola = new Queue<int>();
        cola.Enqueue(inicio); visitado[inicio] = true;
        while (cola.Count > 0)
        {
            int actual = cola.Dequeue(), x = actual % lado, y = actual / lado;
            int[] dx = { 1, -1, 0, 0 }, dy = { 0, 0, 1, -1 };
            for (int i = 0; i < 4; i++)
            {
                int nx = x + dx[i], ny = y + dy[i];
                if (nx < 0 || ny < 0 || nx >= lado || ny >= lado) continue;
                int siguiente = ny * lado + nx;
                if (!libres[siguiente] || visitado[siguiente]) continue;
                visitado[siguiente] = true; cola.Enqueue(siguiente);
            }
        }
        foreach (var destino in new Vector2[] { SalasAcademicas.Actividad(tipo), SalasAcademicas.Apunte(tipo), SalasAcademicas.Interior(355, 645) })
        {
            bool accesible = false;
            for (int i = 0; i < visitado.Length; i++)
                if (visitado[i] && Vector2.Distance(new Vector2(i % lado * paso, i / lado * paso), destino) < 1.2f) { accesible = true; break; }
            Exigir(accesible, "actividad, material o salida inaccesible en panel " + tipo + " · destino " + destino);
        }
    }
}
