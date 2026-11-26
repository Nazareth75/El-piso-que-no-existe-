using UnityEngine;

public class InterfazCampus : MonoBehaviour
{
    GUIStyle titulo;
    GUIStyle cuerpo;
    GUIStyle boton;
    GUIStyle pequeño;
    GUIStyle hudTexto;
    GUIStyle vacio;
    Texture2D blanco;
    Texture2D menuArt;
    Texture2D introArt;
    Texture2D pausaArt;
    Texture2D dialogoArt;
    Texture2D objetivoArt;
    Texture2D vidaArt;
    Texture2D objetosArt;
    Texture2D resultadosArt;
    Texture2D carrerasArt;
    Texture2D tutorArt;
    Texture2D avisoArt;
    bool listo;

    void Preparar()
    {
        if (listo) return;
        blanco = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        blanco.SetPixel(0, 0, Color.white);
        blanco.Apply();
        menuArt = RecursosCampus.Textura("Ui/menu_principal.png");
        introArt = RecursosCampus.Textura("Ui/introduccion.png");
        pausaArt = RecursosCampus.Textura("Ui/pausa.png");
        dialogoArt = RecursosCampus.Textura("Ui/dialogo.png");
        objetivoArt = RecursosCampus.Textura("Ui/objetivo.png");
        vidaArt = RecursosCampus.Textura("Ui/hud_vida.png");
        objetosArt = RecursosCampus.Textura("Ui/hud_objetos.png");
        resultadosArt = RecursosCampus.Textura("Ui/resultados.png");
        carrerasArt = RecursosCampus.Textura("Ui/carreras.png");
        tutorArt = RecursosCampus.Textura("Ui/tutor.png");
        avisoArt = RecursosCampus.Textura("Ui/notificaciones.png");
        vacio = new GUIStyle();

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

        hudTexto = new GUIStyle(cuerpo);
        hudTexto.fontSize = 14;
        hudTexto.alignment = TextAnchor.UpperLeft;
        hudTexto.wordWrap = true;

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

        if (juego.negro > 0.01f)
        {
            Color previa = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, juego.negro);
            GUI.DrawTexture(new Rect(0f, 0f, ancho, alto), blanco, ScaleMode.StretchToFill, true);
            GUI.color = previa;
            if (juego.negro > 0.85f && juego.panel == PanelCampus.Ninguno)
                GUI.Label(new Rect(0f, alto * 0.45f, ancho, 48f), "Te desmayas.", titulo);
        }

        if (juego.GritoVisible)
        {
            var caja = new Rect(ancho * 0.22f, alto * 0.16f, ancho * 0.56f, 52f);
            Relleno(caja, new Color(0.35f, 0.08f, 0.12f, 0.96f));
            Borde(caja, new Color(0.95f, 0.48f, 0.12f));
            GUI.Label(caja, juego.grito, pequeño);
        }
    }

    void Hud(DirectorCampus juego, float ancho)
    {
        float bajo = 64f;
        if (objetivoArt != null)
        {
            var barra = PonerArte(objetivoArt, 12f, 8f, 440f);
            var texto = Trozo(barra, objetivoArt, 58f, 40f, 230f, 38f);
            Relleno(texto, new Color(0.012f, 0.07f, 0.176f, 1f));
            GUI.Label(new Rect(texto.x + 6f, texto.y + 4f, texto.width - 10f, texto.height - 8f), EstadoPartida.Objetivo(), hudTexto);
            bajo = barra.yMax + 4f;
        }

        if (vidaArt != null)
        {
            var vida = PonerArte(vidaArt, 12f, bajo, 250f);
            float lleno = Mathf.Clamp01(juego.estamina / 100f);
            var hueco = Trozo(vida, vidaArt, 102f, 53f, 148f, 13f);
            Relleno(hueco, new Color(0.01f, 0.06f, 0.2f, 1f));
            Relleno(new Rect(hueco.x, hueco.y, hueco.width * lleno, hueco.height), new Color(0.05f, 0.85f, 0.95f, 1f));
        }
        else
        {
            float lleno = Mathf.Clamp01(juego.estamina / 100f);
            Relleno(new Rect(16f, 66f, 220f, 14f), new Color(0f, 0f, 0f, 0.7f));
            Relleno(new Rect(18f, 68f, 216f * lleno, 10f), new Color(0.25f, 0.85f, 1f, 0.95f));
        }

        if (objetosArt != null)
            PonerArte(objetosArt, ancho - 284f, 8f, 272f);

        string rumbo = "Sistemas " + EstadoPartida.sistemas + "   Medicina " + EstadoPartida.medicina + "   " + EstadoPartida.NombreTercera;
        GUI.Label(new Rect(ancho - 520f, objetosArt != null ? 112f : 66f, 500f, 24f), rumbo, pequeño);
    }

    void Prompt(DirectorCampus juego, float ancho, float alto)
    {
        var cerca = juego.Cercano();
        if (cerca == null) return;
        string texto = cerca.id == "gradas"
            ? "E para subir por gradas al siguiente piso"
            : cerca.id == "gradasMez" || cerca.id == "gradasPiso"
                ? "Q para bajar\nE para subir"
                : "E   " + cerca.nombre;
        var camara = Camera.main;
        if (camara != null)
        {
            float escala = Mathf.Max(0.5f, Screen.height / 720f);
            Vector3 pantalla = camara.WorldToScreenPoint(cerca.posicion + new Vector2(0f, 1.45f));
            if (pantalla.z > 0f)
            {
                float x = pantalla.x / escala;
                float y = (Screen.height - pantalla.y) / escala;
                bool gradasPiso = cerca.id == "gradasMez" || cerca.id == "gradasPiso";
                float anchoAviso = cerca.id == "gradas" ? 460f : gradasPiso ? 280f : 340f;
                float altoAviso = gradasPiso ? 58f : 44f;
                var flotante = new Rect(x - anchoAviso * 0.5f, y - altoAviso * 0.5f, anchoAviso, altoAviso);
                Relleno(flotante, new Color(0.06f, 0.08f, 0.14f, 0.94f));
                Borde(flotante, new Color(0.95f, 0.48f, 0.12f));
                GUI.Label(flotante, texto, pequeño);
            }
        }
    }

    void Menu(DirectorCampus juego, float ancho, float alto)
    {
        if (menuArt == null)
        {
            MenuPlano(juego, ancho, alto);
            return;
        }

        Velo(ancho, alto, 0.55f);
        var puesta = Encajar(menuArt, new Rect(0f, 0f, ancho, alto - 64f));
        GUI.DrawTexture(puesta, menuArt, ScaleMode.StretchToFill, true);
        if (GUI.Button(Trozo(puesta, menuArt, 36f, 110f, 164f, 30f), "", vacio)) juego.NuevaPartida();
        if (GUI.Button(Trozo(puesta, menuArt, 36f, 278f, 164f, 28f), "", vacio)) Salir();

        float x = ancho * 0.5f - 160f;
        GUI.backgroundColor = EstadoPartida.personaje == 0 ? new Color(0.95f, 0.48f, 0.12f) : new Color(0.16f, 0.22f, 0.36f);
        if (GUI.Button(new Rect(x, alto - 54f, 150f, 40f), "Mochila", boton)) juego.ElegirPersonaje(0);
        GUI.backgroundColor = EstadoPartida.personaje == 1 ? new Color(0.95f, 0.48f, 0.12f) : new Color(0.16f, 0.22f, 0.36f);
        if (GUI.Button(new Rect(x + 170f, alto - 54f, 150f, 40f), "Cabello corto", boton)) juego.ElegirPersonaje(1);
        GUI.backgroundColor = Color.white;
    }

    void MenuPlano(DirectorCampus juego, float ancho, float alto)
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
        if (introArt == null)
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
            return;
        }

        Velo(ancho, alto, 0.45f);
        var puesta = Encajar(introArt, new Rect(ancho * 0.08f, 24f, ancho * 0.84f, alto - 48f));
        GUI.DrawTexture(puesta, introArt, ScaleMode.StretchToFill, true);
        if (GUI.Button(Trozo(puesta, introArt, 155f, 255f, 190f, 36f), "", vacio))
            juego.ContinuarIntro();
    }

    void Tutorial(DirectorCampus juego, float ancho, float alto)
    {
        if (dialogoArt == null)
        {
            var caja = new Rect(ancho * 0.5f - 320f, alto * 0.5f - 120f, 640f, 240f);
            Relleno(caja, new Color(0.05f, 0.04f, 0.12f, 0.97f));
            Borde(caja, new Color(0.55f, 0.35f, 0.95f));
            GUI.backgroundColor = new Color(0.95f, 0.48f, 0.12f);
            if (GUI.Button(new Rect(caja.x + caja.width * 0.5f - 110f, caja.yMax - 64f, 220f, 44f), "Entendido", boton))
                juego.Entendido();
            GUI.backgroundColor = Color.white;
            return;
        }

        Velo(ancho, alto, 0.4f);
        float anchoDialogo = Mathf.Min(760f, ancho * 0.9f);
        float altoDialogo = anchoDialogo * dialogoArt.height / dialogoArt.width;
        float y = (alto - altoDialogo) * 0.5f - 20f;
        if (tutorArt != null)
        {
            var tutor = PonerArte(tutorArt, ancho * 0.5f - 280f, 16f, 560f);
            y = tutor.yMax + 8f;
        }
        var puesta = new Rect(ancho * 0.5f - anchoDialogo * 0.5f, y, anchoDialogo, altoDialogo);
        GUI.DrawTexture(puesta, dialogoArt, ScaleMode.StretchToFill, true);
        GUI.backgroundColor = new Color(0.95f, 0.48f, 0.12f);
        if (GUI.Button(new Rect(puesta.x + puesta.width * 0.5f - 110f, puesta.yMax + 8f, 220f, 42f), "Entendido", boton))
            juego.Entendido();
        GUI.backgroundColor = Color.white;
    }

    void Frase(DirectorCampus juego, float ancho, float alto)
    {
        Velo(ancho, alto, 0.35f);
        float y = 70f;
        bool cajaDiseno = dialogoArt != null && juego.fraseTitulo != "CÓMO MOVERTE";
        var tex = cajaDiseno ? null : juego.Tarjeta(juego.fraseTarjeta);
        if (tex != null)
        {
            var zona = new Rect(ancho * 0.5f - 300f, y, 600f, 180f);
            GUI.DrawTexture(zona, tex, ScaleMode.ScaleToFit, true);
            y += 190f;
        }

        if (cajaDiseno)
        {
            float anchoDialogo = Mathf.Min(680f, ancho * 0.78f);
            float altoDialogo = anchoDialogo * dialogoArt.height / dialogoArt.width;
            float altoBloque = altoDialogo + 52f;
            y = Mathf.Max(16f, (alto - altoBloque) * 0.5f);
            var puesta = new Rect(ancho * 0.5f - anchoDialogo * 0.5f, y, anchoDialogo, altoDialogo);
            GUI.DrawTexture(puesta, dialogoArt, ScaleMode.StretchToFill, true);
            var zona = Trozo(puesta, dialogoArt, 96f, 40f, 192f, 84f);
            Relleno(zona, new Color(0.008f, 0.063f, 0.165f, 1f));
            GUI.Label(new Rect(zona.x + 8f, zona.y + 4f, zona.width - 16f, 22f), juego.fraseTitulo, hudTexto);
            GUI.Label(new Rect(zona.x + 8f, zona.y + 28f, zona.width - 16f, zona.height - 34f), juego.fraseCuerpo, hudTexto);
            GUI.backgroundColor = new Color(0.95f, 0.48f, 0.12f);
            string textoDialogo = juego.confirmarHuida ? "Correr" : "Cerrar";
            if (GUI.Button(new Rect(puesta.x + puesta.width * 0.5f - 90f, puesta.yMax + 10f, 180f, 40f), textoDialogo, boton))
                juego.CerrarPanel();
            GUI.backgroundColor = Color.white;
            return;
        }

        float altoTexto = Mathf.Clamp(cuerpo.CalcHeight(new GUIContent(juego.fraseCuerpo), 560f) + 8f, 70f, 220f);
        float altoCaja = Mathf.Max(210f, 70f + altoTexto + 64f);
        if (juego.fraseTitulo == "CÓMO MOVERTE")
            y = (alto - altoCaja) * 0.5f;
        else if (y + altoCaja > alto - 12f)
            y = Mathf.Max(12f, alto - 12f - altoCaja);
        var caja = new Rect(ancho * 0.5f - 320f, y, 640f, altoCaja);
        Relleno(caja, new Color(0.05f, 0.04f, 0.12f, 0.97f));
        Borde(caja, new Color(0.55f, 0.35f, 0.95f));
        GUI.Label(new Rect(caja.x + 20f, caja.y + 10f, caja.width - 40f, 28f), juego.fraseTitulo, pequeño);
        GUI.Label(new Rect(caja.x + 24f, caja.y + 44f, caja.width - 48f, altoTexto), juego.fraseCuerpo, cuerpo);
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
        PintarCaja(caja);
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

        float avisoY = y + 56f;
        if (juego.BotonXListo)
        {
            GUI.Label(new Rect(0f, y + 52f, ancho, 28f), "PISO X", pequeño);
            GUI.backgroundColor = new Color(0.55f, 0.2f, 0.75f);
            if (GUI.Button(new Rect(ancho * 0.5f - 29f, y + 84f, 58f, 42f), "X", boton))
                juego.ElegirPiso("X");
            avisoY = y + 136f;
        }
        else if (EstadoPartida.desperto)
        {
            GUI.Label(new Rect(0f, y + 52f, ancho, 28f), "¿Cuál piso no está?", pequeño);
            GUI.backgroundColor = new Color(0.95f, 0.48f, 0.12f);
            string[] extra = { "6", "7", "8" };
            float ex = ancho * 0.5f - 96f;
            for (int i = 0; i < extra.Length; i++)
            {
                if (GUI.Button(new Rect(ex + i * 64f, y + 84f, 58f, 42f), extra[i], boton))
                    juego.ElegirPiso(extra[i]);
            }

            avisoY = y + 136f;
        }

        if (!string.IsNullOrEmpty(juego.aviso))
            GUI.Label(new Rect(ancho * 0.15f, avisoY, ancho * 0.7f, 48f), juego.aviso, pequeño);

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
        PintarCaja(caja);
        GUI.Label(new Rect(caja.x, caja.y + 16f, caja.width, 36f), "Pulsa " + juego.flecha, titulo);
        GUI.Label(new Rect(caja.x, caja.y + 70f, caja.width, 30f), "Aciertos " + juego.aciertos + " / 3", pequeño);
        GUI.backgroundColor = new Color(0.2f, 0.2f, 0.25f);
        if (GUI.Button(new Rect(caja.x + caja.width * 0.5f - 70f, caja.yMax - 52f, 140f, 36f), "Salir", boton))
            juego.CerrarPanel();
        GUI.backgroundColor = Color.white;
    }

    void Pausa(DirectorCampus juego, float ancho, float alto)
    {
        if (pausaArt == null)
        {
            var caja = CajaCentral(ancho, alto, 420f, 280f);
            GUI.Label(new Rect(caja.x, caja.y + 16f, caja.width, 40f), "Pausa", titulo);
            GUI.backgroundColor = new Color(0.95f, 0.48f, 0.12f);
            if (GUI.Button(new Rect(caja.x + 70f, caja.y + 80f, 280f, 44f), "Continuar", boton)) juego.CerrarPanel();
            GUI.backgroundColor = new Color(0.16f, 0.22f, 0.36f);
            if (EstadoPartida.huidaActiva && GUI.Button(new Rect(caja.x + 70f, caja.y + 136f, 280f, 44f), "Reintentar huida", boton)) juego.Reintentar();
            if (GUI.Button(new Rect(caja.x + 70f, caja.y + (EstadoPartida.huidaActiva ? 192f : 136f), 280f, 44f), "Menú principal", boton)) juego.VolverAlMenu();
            GUI.backgroundColor = Color.white;
            return;
        }

        Velo(ancho, alto, 0.45f);
        var puesta = Encajar(pausaArt, new Rect(ancho * 0.5f - 180f, alto * 0.5f - 200f, 360f, 360f));
        GUI.DrawTexture(puesta, pausaArt, ScaleMode.StretchToFill, true);
        if (GUI.Button(Trozo(puesta, pausaArt, 16f, 40f, 166f, 28f), "", vacio)) juego.CerrarPanel();
        if (GUI.Button(Trozo(puesta, pausaArt, 16f, 156f, 166f, 26f), "", vacio)) juego.VolverAlMenu();
        if (EstadoPartida.huidaActiva)
        {
            GUI.backgroundColor = new Color(0.95f, 0.48f, 0.12f);
            if (GUI.Button(new Rect(puesta.x, puesta.yMax + 8f, puesta.width, 40f), "Reintentar huida", boton)) juego.Reintentar();
            GUI.backgroundColor = Color.white;
        }
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
        bool cruzo = EstadoPartida.pisoCruzado;
        string resumen = (cruzo
                ? "Cruzaste el círculo del centro. Ese piso es el Proyecto 07.\n\n"
                : "El ascensor sigue sin un piso 7.\n\n") +
            "Sistemas: " + EstadoPartida.sistemas + "    Medicina: " + EstadoPartida.medicina + "\n" +
            "Tercera: " + EstadoPartida.NombreTercera + "\n" +
            EstadoPartida.Rumbo();

        if (resultadosArt == null)
        {
            var caja = CajaCentral(ancho, alto, 680f, 460f);
            GUI.Label(new Rect(caja.x, caja.y + 12f, caja.width, 40f), cruzo ? "Cruzaste el piso que no existe" : "El piso sigue cerrado", titulo);
            GUI.Label(new Rect(caja.x + 28f, caja.y + 64f, caja.width - 56f, 250f), resumen + "\n\nPIXEL404\nKathlen Escalante, Nazareth Padilla, José Mamani y Dilan Paco.\nUNIFRANZ", cuerpo);
            GUI.backgroundColor = new Color(0.95f, 0.48f, 0.12f);
            if (GUI.Button(new Rect(caja.x + caja.width * 0.5f - 120f, caja.yMax - 64f, 240f, 44f), "Volver al menú", boton))
                juego.VolverAlMenu();
            GUI.backgroundColor = Color.white;
            return;
        }

        Velo(ancho, alto, 0.5f);
        float altoCarreras = 0f;
        if (carrerasArt != null)
        {
            var facultades = Encajar(carrerasArt, new Rect(ancho * 0.5f - 280f, 8f, 560f, 200f));
            GUI.DrawTexture(facultades, carrerasArt, ScaleMode.StretchToFill, true);
            altoCarreras = facultades.yMax;
        }
        var puesta = Encajar(resultadosArt, new Rect(ancho * 0.5f - 300f, altoCarreras + 8f, 600f, alto - altoCarreras - 16f));
        GUI.DrawTexture(puesta, resultadosArt, ScaleMode.StretchToFill, true);
        var datos = new Rect(puesta.x + puesta.width * 0.08f, puesta.y + puesta.height * 0.28f, puesta.width * 0.62f, puesta.height * 0.42f);
        Relleno(datos, new Color(0.03f, 0.04f, 0.1f, 1f));
        GUI.Label(datos, resumen, cuerpo);
        if (GUI.Button(Trozo(puesta, resultadosArt, 70f, 150f, 180f, 32f), "", vacio))
            juego.VolverAlMenu();
    }

    Rect PonerArte(Texture2D tex, float x, float y, float ancho)
    {
        float alto = ancho * tex.height / tex.width;
        var zona = new Rect(x, y, ancho, alto);
        GUI.DrawTexture(zona, tex, ScaleMode.StretchToFill, true);
        return zona;
    }

    Rect Encajar(Texture2D tex, Rect zona)
    {
        float escala = Mathf.Min(zona.width / tex.width, zona.height / tex.height);
        float w = tex.width * escala;
        float h = tex.height * escala;
        return new Rect(zona.x + (zona.width - w) * 0.5f, zona.y + (zona.height - h) * 0.5f, w, h);
    }

    Rect Trozo(Rect puesta, Texture2D tex, float px, float py, float pw, float ph)
    {
        return new Rect(
            puesta.x + px / tex.width * puesta.width,
            puesta.y + py / tex.height * puesta.height,
            pw / tex.width * puesta.width,
            ph / tex.height * puesta.height);
    }

    Rect CajaCentral(float ancho, float alto, float w, float h)
    {
        var caja = new Rect(ancho * 0.5f - w * 0.5f, alto * 0.5f - h * 0.5f, w, h);
        PintarCaja(caja);
        return caja;
    }

    void PintarCaja(Rect caja)
    {
        if (dialogoArt != null)
        {
            Relleno(caja, new Color(0.05f, 0.04f, 0.12f, 0.97f));
            Borde(caja, new Color(0.55f, 0.35f, 0.95f));
        }
        else
        {
            Relleno(caja, new Color(0.07f, 0.09f, 0.16f, 0.94f));
            Borde(caja, new Color(0.95f, 0.48f, 0.12f));
        }
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
