using UnityEngine;

public class InterfazCampus : MonoBehaviour
{
    GUIStyle titulo;
    GUIStyle cuerpo;
    GUIStyle boton;
    GUIStyle pequeño;
    Texture2D blanco;
    bool listo;

    void Preparar()
    {
        if (listo) return;
        blanco = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        blanco.SetPixel(0, 0, Color.white);
        blanco.Apply();

        titulo = new GUIStyle(GUI.skin.label);
        titulo.fontSize = 30;
        titulo.alignment = TextAnchor.MiddleCenter;
        titulo.fontStyle = FontStyle.Bold;
        titulo.normal.textColor = Color.white;
        titulo.wordWrap = true;

        cuerpo = new GUIStyle(GUI.skin.label);
        cuerpo.fontSize = 18;
        cuerpo.alignment = TextAnchor.UpperLeft;
        cuerpo.normal.textColor = Color.white;
        cuerpo.wordWrap = true;

        pequeño = new GUIStyle(cuerpo);
        pequeño.fontSize = 16;
        pequeño.alignment = TextAnchor.MiddleCenter;

        boton = new GUIStyle(GUI.skin.button);
        boton.fontSize = 18;
        boton.fontStyle = FontStyle.Bold;
        boton.alignment = TextAnchor.MiddleCenter;
        boton.normal.textColor = Color.white;
        boton.hover.textColor = new Color(1f, 0.95f, 0.8f);
        listo = true;
    }

    void OnGUI()
    {
        var juego = DirectorCampus.Instancia;
        if (juego == null) return;
        Preparar();

        float escala = Mathf.Max(0.5f, Screen.height / 720f);
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(escala, escala, 1f));
        float ancho = Screen.width / escala;
        float alto = Screen.height / escala;

        if (juego.jugando && juego.panel != PanelCampus.Menu && juego.panel != PanelCampus.Intro)
            Hud(juego, ancho);

        switch (juego.panel)
        {
            case PanelCampus.Menu:
                Menu(juego, ancho, alto);
                break;
            case PanelCampus.Intro:
                Intro(juego, ancho, alto);
                break;
            case PanelCampus.Tutorial:
                Tutorial(juego, ancho, alto);
                break;
            case PanelCampus.Frase:
                Frase(juego, ancho, alto);
                break;
            case PanelCampus.Clinica:
                Clinica(juego, ancho, alto);
                break;
            case PanelCampus.Ascensor:
                Ascensor(juego, ancho, alto);
                break;
            case PanelCampus.Game:
                Minijuego(juego, ancho, alto);
                break;
            case PanelCampus.Pausa:
                Pausa(juego, ancho, alto);
                break;
            case PanelCampus.Muerte:
                Muerte(juego, ancho, alto);
                break;
            case PanelCampus.Fin:
                Fin(juego, ancho, alto);
                break;
        }

        if (juego.jugando && juego.panel == PanelCampus.Ninguno)
            Prompt(juego, ancho, alto);

        if (juego.GritoVisible)
        {
            var caja = new Rect(ancho * 0.2f, alto * 0.18f, ancho * 0.6f, 64f);
            Relleno(caja, new Color(0.35f, 0.08f, 0.12f, 0.92f));
            GUI.Label(caja, juego.grito, pequeño);
        }
    }

    void Hud(DirectorCampus juego, float ancho)
    {
        var barra = new Rect(16f, 12f, ancho - 32f, 36f);
        Relleno(barra, new Color(0.06f, 0.08f, 0.14f, 0.88f));
        Borde(barra, new Color(0.95f, 0.48f, 0.12f));
        GUI.Label(barra, EstadoPartida.Objetivo(), pequeño);

        string rumbo = "Sistemas " + EstadoPartida.sistemas + "   Medicina " + EstadoPartida.medicina + "   " + EstadoPartida.NombreTercera;
        var esquina = new Rect(ancho - 430f, 54f, 414f, 28f);
        GUI.Label(esquina, rumbo, pequeño);

        Relleno(new Rect(16f, 56f, 204f, 16f), new Color(0f, 0f, 0f, 0.55f));
        Relleno(new Rect(18f, 58f, 2f * juego.estamina, 12f), new Color(0.35f, 0.85f, 0.95f, 0.95f));
        GUI.Label(new Rect(16f, 72f, 200f, 22f), "Aire al correr", pequeño);
    }

    void Prompt(DirectorCampus juego, float ancho, float alto)
    {
        var cerca = juego.Cercano();
        if (cerca == null) return;
        string texto = "E   " + cerca.nombre;
        var camara = Camera.main;
        if (camara != null)
        {
            float escala = Mathf.Max(0.5f, Screen.height / 720f);
            Vector3 pantalla = camara.WorldToScreenPoint(cerca.posicion + new Vector2(0f, 1.45f));
            if (pantalla.z > 0f)
            {
                float x = pantalla.x / escala;
                float y = (Screen.height - pantalla.y) / escala;
                var flotante = new Rect(x - 120f, y - 24f, 240f, 44f);
                Relleno(flotante, new Color(0.06f, 0.08f, 0.14f, 0.94f));
                Borde(flotante, new Color(0.95f, 0.48f, 0.12f));
                GUI.Label(flotante, texto, pequeño);
            }
        }

        var caja = new Rect(ancho * 0.3f, alto - 90f, ancho * 0.4f, 48f);
        Relleno(caja, new Color(0.06f, 0.08f, 0.14f, 0.94f));
        Borde(caja, new Color(0.95f, 0.48f, 0.12f));
        GUI.Label(caja, texto, pequeño);
    }

    void Menu(DirectorCampus juego, float ancho, float alto)
    {
        Velo(ancho, alto, 0.45f);
        GUI.Label(new Rect(0f, 70f, ancho, 48f), "EL PISO QUE NO EXISTE 2.0", titulo);
        GUI.Label(new Rect(0f, 122f, ancho, 30f), "UNIFRANZ  ·  primer día", pequeño);

        float x = ancho * 0.5f - 150f;
        GUI.backgroundColor = EstadoPartida.personaje == 0 ? new Color(0.95f, 0.48f, 0.12f) : new Color(0.16f, 0.22f, 0.36f);
        if (GUI.Button(new Rect(x, 190f, 140f, 42f), "Mochila", boton)) juego.ElegirPersonaje(0);
        GUI.backgroundColor = EstadoPartida.personaje == 1 ? new Color(0.95f, 0.48f, 0.12f) : new Color(0.16f, 0.22f, 0.36f);
        if (GUI.Button(new Rect(x + 160f, 190f, 140f, 42f), "Cabello corto", boton)) juego.ElegirPersonaje(1);

        GUI.backgroundColor = new Color(0.95f, 0.48f, 0.12f);
        if (GUI.Button(new Rect(ancho * 0.5f - 120f, 270f, 240f, 48f), "Jugar", boton)) juego.NuevaPartida();
        GUI.backgroundColor = new Color(0.16f, 0.22f, 0.36f);
        if (GUI.Button(new Rect(ancho * 0.5f - 120f, 332f, 240f, 42f), "Salir", boton)) Salir();
        GUI.backgroundColor = Color.white;
    }

    void Intro(DirectorCampus juego, float ancho, float alto)
    {
        var caja = CajaCentral(ancho, alto, 640f, 340f);
        GUI.Label(new Rect(caja.x, caja.y + 16f, caja.width, 40f), "Primer día", titulo);
        GUI.Label(new Rect(caja.x + 28f, caja.y + 70f, caja.width - 56f, 180f),
            "Es tu primer día en la universidad.\n\nUna estudiante te espera en la puerta y te hace el recorrido: recepción, admisiones, la clínica, gestión financiera, el auditorio, servicios, la cafetería y la game room.",
            cuerpo);
        GUI.backgroundColor = new Color(0.95f, 0.48f, 0.12f);
        if (GUI.Button(new Rect(caja.x + caja.width * 0.5f - 100f, caja.yMax - 64f, 200f, 44f), "Continuar", boton))
            juego.ContinuarIntro();
        GUI.backgroundColor = Color.white;
    }

    void Tutorial(DirectorCampus juego, float ancho, float alto)
    {
        var caja = CajaCentral(ancho, alto, 640f, 380f);
        GUI.Label(new Rect(caja.x, caja.y + 12f, caja.width, 40f), "Cómo moverte", titulo);
        GUI.Label(new Rect(caja.x + 28f, caja.y + 64f, caja.width - 56f, 230f),
            "Ves el campus desde arriba, como el plano.\n\nFlechas o W A S D: caminar\nShift: correr. La barra de aire se gasta.\nE: hablar, leer y usar la huella\nEn la clínica: 1 ayudar, 2 seguir de largo\nEsc: pausa\n\nSi la presencia te alcanza, repites solo esa huida. Tus carreras se conservan.",
            cuerpo);
        GUI.backgroundColor = new Color(0.95f, 0.48f, 0.12f);
        if (GUI.Button(new Rect(caja.x + caja.width * 0.5f - 110f, caja.yMax - 64f, 220f, 44f), "Entendido", boton))
            juego.Entendido();
        GUI.backgroundColor = Color.white;
    }

    void Frase(DirectorCampus juego, float ancho, float alto)
    {
        Velo(ancho, alto, 0.35f);
        float y = 70f;
        var tex = juego.Tarjeta(juego.fraseTarjeta);
        if (tex != null)
        {
            var zona = new Rect(ancho * 0.5f - 300f, y, 600f, 180f);
            GUI.DrawTexture(zona, tex, ScaleMode.ScaleToFit, true);
            y += 190f;
        }

        float altoTexto = Mathf.Clamp(cuerpo.CalcHeight(new GUIContent(juego.fraseCuerpo), 600f) + 8f, 70f, 260f);
        float altoCaja = Mathf.Max(200f, 56f + altoTexto + 64f);
        if (y + altoCaja > alto - 12f) y = Mathf.Max(12f, alto - 12f - altoCaja);
        var caja = new Rect(ancho * 0.5f - 320f, y, 640f, altoCaja);
        Relleno(caja, new Color(0.07f, 0.09f, 0.16f, 0.94f));
        Borde(caja, new Color(0.95f, 0.48f, 0.12f));
        GUI.Label(new Rect(caja.x + 16f, caja.y + 8f, caja.width - 32f, 32f), juego.fraseTitulo, pequeño);
        GUI.Label(new Rect(caja.x + 20f, caja.y + 44f, caja.width - 40f, altoTexto), juego.fraseCuerpo, cuerpo);
        GUI.backgroundColor = new Color(0.95f, 0.48f, 0.12f);
        string texto = juego.confirmarHuida ? "Correr" : "Cerrar";
        if (GUI.Button(new Rect(caja.x + caja.width * 0.5f - 90f, caja.yMax - 52f, 180f, 40f), texto, boton))
            juego.CerrarPanel();
        GUI.backgroundColor = Color.white;
    }

    void Clinica(DirectorCampus juego, float ancho, float alto)
    {
        Velo(ancho, alto, 0.4f);
        var tex = juego.Tarjeta("clinica");
        if (tex != null) GUI.DrawTexture(new Rect(ancho * 0.5f - 300f, 36f, 600f, 170f), tex, ScaleMode.ScaleToFit, true);
        var caja = new Rect(ancho * 0.5f - 320f, 210f, 640f, 220f);
        Relleno(caja, new Color(0.07f, 0.09f, 0.16f, 0.94f));
        Borde(caja, new Color(0.95f, 0.48f, 0.12f));
        GUI.Label(new Rect(caja.x + 20f, caja.y + 16f, caja.width - 40f, 90f),
            "Un estudiante se torció el tobillo en las escaleras.\n1 ayudar suma Medicina. 2 seguir de largo no cambia nada.",
            cuerpo);
        GUI.backgroundColor = new Color(0.2f, 0.55f, 0.35f);
        if (GUI.Button(new Rect(caja.x + 30f, caja.y + 130f, 270f, 46f), "1  Ayudar", boton)) juego.Ayudar();
        GUI.backgroundColor = new Color(0.35f, 0.28f, 0.2f);
        if (GUI.Button(new Rect(caja.x + 320f, caja.y + 130f, 290f, 46f), "2  Seguir de largo", boton)) juego.Seguir();
        GUI.backgroundColor = Color.white;
    }

    void Ascensor(DirectorCampus juego, float ancho, float alto)
    {
        Velo(ancho, alto, 0.45f);
        var panelTex = juego.MostrarPisoX && juego.pisoX != null ? juego.pisoX : juego.ascensor;
        if (panelTex != null)
            GUI.DrawTexture(new Rect(ancho * 0.5f - 150f, 24f, 300f, 250f), panelTex, ScaleMode.ScaleToFit, true);

        float y = 280f;
        string[] pisos = { "PB", "M", "1", "2", "3", "4", "5" };
        float total = pisos.Length * 64f;
        float x = ancho * 0.5f - total * 0.5f;
        GUI.backgroundColor = new Color(0.16f, 0.22f, 0.36f);
        for (int i = 0; i < pisos.Length; i++)
        {
            if (GUI.Button(new Rect(x + i * 64f, y, 58f, 42f), pisos[i], boton))
                juego.ElegirPiso(pisos[i]);
        }

        GUI.Label(new Rect(0f, y + 52f, ancho, 28f), "¿Cuál piso no está?", pequeño);
        GUI.backgroundColor = new Color(0.95f, 0.48f, 0.12f);
        string[] extra = { "6", "7", "8" };
        float ex = ancho * 0.5f - 96f;
        for (int i = 0; i < extra.Length; i++)
        {
            if (GUI.Button(new Rect(ex + i * 64f, y + 84f, 58f, 42f), extra[i], boton))
                juego.ElegirPiso(extra[i]);
        }

        if (!string.IsNullOrEmpty(juego.aviso))
            GUI.Label(new Rect(ancho * 0.15f, y + 136f, ancho * 0.7f, 48f), juego.aviso, pequeño);

        GUI.backgroundColor = new Color(0.55f, 0.2f, 0.75f);
        if (juego.MostrarPisoX && GUI.Button(new Rect(ancho * 0.5f - 120f, alto - 124f, 240f, 42f), "Entrar a PISO X", boton))
            juego.EntrarAlPiso();
        GUI.backgroundColor = new Color(0.2f, 0.2f, 0.25f);
        if (GUI.Button(new Rect(ancho * 0.5f - 80f, alto - 70f, 160f, 40f), "Cerrar", boton))
            juego.CerrarPanel();
        GUI.backgroundColor = Color.white;
    }

    void Minijuego(DirectorCampus juego, float ancho, float alto)
    {
        Velo(ancho, alto, 0.4f);
        var tex = juego.Tarjeta("game_room");
        if (tex != null) GUI.DrawTexture(new Rect(ancho * 0.5f - 280f, 40f, 560f, 160f), tex, ScaleMode.ScaleToFit, true);
        var caja = new Rect(ancho * 0.5f - 260f, 220f, 520f, 180f);
        Relleno(caja, new Color(0.07f, 0.09f, 0.16f, 0.94f));
        Borde(caja, new Color(0.95f, 0.48f, 0.12f));
        GUI.Label(new Rect(caja.x, caja.y + 16f, caja.width, 36f), "Pulsa " + juego.flecha, titulo);
        GUI.Label(new Rect(caja.x, caja.y + 70f, caja.width, 30f), "Aciertos " + juego.aciertos + " / 3", pequeño);
        GUI.backgroundColor = new Color(0.2f, 0.2f, 0.25f);
        if (GUI.Button(new Rect(caja.x + caja.width * 0.5f - 70f, caja.yMax - 52f, 140f, 36f), "Salir", boton))
            juego.CerrarPanel();
        GUI.backgroundColor = Color.white;
    }

    void Pausa(DirectorCampus juego, float ancho, float alto)
    {
        var caja = CajaCentral(ancho, alto, 420f, 280f);
        GUI.Label(new Rect(caja.x, caja.y + 16f, caja.width, 40f), "Pausa", titulo);
        GUI.backgroundColor = new Color(0.95f, 0.48f, 0.12f);
        if (GUI.Button(new Rect(caja.x + 70f, caja.y + 80f, 280f, 44f), "Continuar", boton)) juego.CerrarPanel();
        GUI.backgroundColor = new Color(0.16f, 0.22f, 0.36f);
        if (EstadoPartida.huidaActiva && GUI.Button(new Rect(caja.x + 70f, caja.y + 136f, 280f, 44f), "Reintentar huida", boton)) juego.Reintentar();
        if (GUI.Button(new Rect(caja.x + 70f, caja.y + (EstadoPartida.huidaActiva ? 192f : 136f), 280f, 44f), "Menú principal", boton)) juego.VolverAlMenu();
        GUI.backgroundColor = Color.white;
    }

    void Muerte(DirectorCampus juego, float ancho, float alto)
    {
        var caja = CajaCentral(ancho, alto, 520f, 240f);
        bool piso = EstadoPartida.enPisoX;
        GUI.Label(new Rect(caja.x, caja.y + 20f, caja.width, 40f), piso ? "Una entidad te alcanzó" : "La presencia te alcanzó", titulo);
        GUI.Label(new Rect(caja.x + 24f, caja.y + 80f, caja.width - 48f, 60f), piso
            ? "Repites solo el piso que no existe. Sistemas y Medicina se quedan como estaban."
            : "Repites solo la huida. Sistemas y Medicina se quedan como estaban.", cuerpo);
        GUI.backgroundColor = new Color(0.95f, 0.48f, 0.12f);
        if (GUI.Button(new Rect(caja.x + caja.width * 0.5f - 100f, caja.yMax - 64f, 200f, 44f), "Reintentar", boton))
            juego.Reintentar();
        GUI.backgroundColor = Color.white;
    }

    void Fin(DirectorCampus juego, float ancho, float alto)
    {
        var caja = CajaCentral(ancho, alto, 680f, 460f);
        bool cruzo = EstadoPartida.pisoCruzado;
        GUI.Label(new Rect(caja.x, caja.y + 12f, caja.width, 40f), cruzo ? "Cruzaste el piso que no existe" : "El piso sigue cerrado", titulo);
        GUI.Label(new Rect(caja.x + 28f, caja.y + 64f, caja.width - 56f, 250f),
            (cruzo
                ? "El círculo del centro te deja pasar. Ese piso no tiene el color de la universidad: es el lugar del Proyecto 07, y ahí se ven los futuros que tus decisiones fueron abriendo.\n\n"
                : "Llegaste a la zona verde de las escaleras. El ascensor sigue sin un piso 7.\n\n") +
            "Ingeniería de Sistemas: " + EstadoPartida.sistemas + "\n" +
            "Medicina: " + EstadoPartida.medicina + "\n" +
            "Tercera: " + EstadoPartida.NombreTercera + "\n\n" +
            EstadoPartida.Rumbo() + "\n\nPIXEL404\nKathlen Escalante, Nazareth Padilla, José Mamani y Dilan Paco.\nUNIFRANZ",
            cuerpo);
        GUI.backgroundColor = new Color(0.95f, 0.48f, 0.12f);
        if (GUI.Button(new Rect(caja.x + caja.width * 0.5f - 120f, caja.yMax - 64f, 240f, 44f), "Volver al menú", boton))
            juego.VolverAlMenu();
        GUI.backgroundColor = Color.white;
    }

    Rect CajaCentral(float ancho, float alto, float w, float h)
    {
        var caja = new Rect(ancho * 0.5f - w * 0.5f, alto * 0.5f - h * 0.5f, w, h);
        Relleno(caja, new Color(0.07f, 0.09f, 0.16f, 0.94f));
        Borde(caja, new Color(0.95f, 0.48f, 0.12f));
        return caja;
    }

    void Velo(float ancho, float alto, float alfa)
    {
        Relleno(new Rect(0f, 0f, ancho, alto), new Color(0f, 0f, 0f, alfa));
    }

    void Relleno(Rect zona, Color color)
    {
        Color previo = GUI.color;
        GUI.color = color;
        GUI.DrawTexture(zona, blanco);
        GUI.color = previo;
    }

    void Borde(Rect zona, Color color)
    {
        Relleno(new Rect(zona.x, zona.y, zona.width, 3f), color);
        Relleno(new Rect(zona.x, zona.yMax - 3f, zona.width, 3f), color);
        Relleno(new Rect(zona.x, zona.y, 3f, zona.height), color);
        Relleno(new Rect(zona.xMax - 3f, zona.y, 3f, zona.height), color);
    }

    static void Salir()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
