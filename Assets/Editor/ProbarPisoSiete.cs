using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class ProbarPisoSiete
{
    const string Pendiente = "Proyecto07.ProbarPisoSiete";
    const string Pruebas = "Proyecto07.PruebasPisoSiete";

    static ProbarPisoSiete()
    {
        EditorApplication.playModeStateChanged += Cambio;
    }

    [MenuItem("Piso 7/Probar desde el ascensor")]
    public static void Abrir()
    {
        if (EditorApplication.isPlaying)
        {
            var director = Object.FindAnyObjectByType<DirectorCampus>();
            if (director != null) director.ProbarPisoSiete();
            return;
        }
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        EditorSceneManager.OpenScene("Assets/Scenes/Campus.unity");
        SessionState.SetBool(Pendiente, true);
        EditorApplication.isPlaying = true;
    }

    static void Cambio(PlayModeStateChange estado)
    {
        if (estado != PlayModeStateChange.EnteredPlayMode || !SessionState.GetBool(Pendiente, false)) return;
        SessionState.SetBool(Pendiente, false);
        EditorApplication.delayCall += () =>
        {
            var director = Object.FindAnyObjectByType<DirectorCampus>();
            if (director != null)
            {
                director.ProbarPisoSiete();
                if (SessionState.GetBool(Pruebas, false))
                {
                    SessionState.SetBool(Pruebas, false);
                    Validar(director);
                }
            }
        };
    }

    [MenuItem("Piso 7/Validar recorrido completo")]
    public static void ValidarDesdeEditor()
    {
        if (!EditorApplication.isPlaying)
        {
            SessionState.SetBool(Pruebas, true);
            Abrir();
            return;
        }
        var director = Object.FindAnyObjectByType<DirectorCampus>();
        if (director != null)
        {
            director.ProbarPisoSiete();
            Validar(director);
        }
    }

    static void Validar(DirectorCampus director)
    {
        try
        {
            EjecutarRecorrido(director);
            Debug.Log("PISO7_PRUEBAS_OK: seis misiones, orden de cables, inventario, bloqueo de portal, reintento y reinicio.");
            if (Application.isBatchMode) EditorApplication.Exit(0);
            else director.ProbarPisoSiete();
        }
        catch (System.Exception error)
        {
            Debug.LogException(error);
            if (Application.isBatchMode) EditorApplication.Exit(1);
        }
    }

    // Entrada para Unity -batchmode -executeMethod ProbarPisoSiete.PruebasEnLote.
    public static void PruebasEnLote()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/Campus.unity");
        SessionState.SetBool(Pruebas, true);
        SessionState.SetBool(Pendiente, true);
        EditorApplication.isPlaying = true;
    }

    static void Comprobar(bool condicion, string mensaje)
    {
        if (!condicion) throw new System.Exception("Piso 7: " + mensaje);
    }

    static void EjecutarRecorrido(DirectorCampus director)
    {
        var nivel = director.NivelSiete;
        Comprobar(nivel != null && EstadoPartida.enPisoX, "no se construyó el escenario");
        director.CerrarPanel();
        nivel.Usar("nucleo7");
        Comprobar(!EstadoPartida.pisoCruzado, "el portal permite saltarse las misiones");
        director.CerrarPanel();
        nivel.Usar("energia7");
        nivel.ConectarCable(0); nivel.ConectarCable(1); nivel.ConectarCable(2);
        Comprobar(!EstadoPisoSiete.Lista("energia"), "la energía no exige fusible");
        director.CerrarPanel();
        nivel.Usar("fusible"); director.CerrarPanel();
        nivel.Usar("energia7");
        nivel.ConectarCable(1); nivel.ConectarCable(2);
        Comprobar(!EstadoPisoSiete.Lista("energia"), "aceptó cables en orden incorrecto");
        nivel.ConectarCable(0); nivel.ConectarCable(1); nivel.ConectarCable(2);
        Comprobar(EstadoPisoSiete.Lista("energia"), "no se restauró la energía");
        int puntos = EstadoPartida.sistemas;
        nivel.ConectarCable(2);
        Comprobar(EstadoPartida.sistemas == puntos, "el panel duplica puntos");
        director.CerrarPanel();
        nivel.Usar("clinica7"); nivel.ElegirAccion("clinica"); director.CerrarPanel();
        nivel.Usar("archivo7"); director.CerrarPanel();
        nivel.Usar("muestra"); director.CerrarPanel();
        nivel.Usar("laboratorio7"); nivel.ElegirAccion("antidoto"); director.CerrarPanel();
        nivel.Usar("radio7"); nivel.ElegirAccion("transmision"); director.CerrarPanel();
        nivel.Usar("estructura7"); nivel.ElegirAccion("estructura"); director.CerrarPanel();
        Comprobar(EstadoPisoSiete.PortalListo && EstadoPisoSiete.misiones.Count == 6, "la cadena de seis misiones no termina");
        nivel.Usar("recursos7"); director.CerrarPanel();
        Comprobar(EstadoPisoSiete.Tiene("escudo") && EstadoPisoSiete.Tiene("botiquin"), "faltan suministros");
        int medicina = EstadoPartida.medicina;
        director.AlcanzadoEnPiso();
        Comprobar(director.panel == PanelCampus.Muerte, "no se muestra la muerte");
        director.Reintentar();
        Comprobar(EstadoPisoSiete.vida == 100 && EstadoPisoSiete.PortalListo && EstadoPartida.medicina == medicina,
            "el reintento pierde progreso o puntos");
        nivel = director.NivelSiete;
        director.CerrarPanel();
        nivel.Usar("clinica7"); director.CerrarPanel();
        Comprobar(EstadoPartida.medicina == medicina, "repetir clínica duplica puntos");
        nivel.Usar("nucleo7"); nivel.CruzarPortal();
        Comprobar(EstadoPartida.pisoCruzado && director.panel == PanelCampus.Fin, "no se alcanza el final");
        director.VolverAlMenu();
        Comprobar(!EstadoPartida.enPisoX && EstadoPisoSiete.misiones.Count == 0 && EstadoPisoSiete.objetos.Count == 0,
            "volver al menú no limpia la partida");
    }
}
