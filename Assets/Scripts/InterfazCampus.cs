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
        if(juego.panel!=PanelCampus.Ascensor)buscarPisoAscensor=false;

        float escala = Mathf.Max(0.5f, Screen.height / 720f);
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(escala, escala, 1f));
        float ancho = Screen.width / escala;
        float alto = Screen.height / escala;

        if (juego.jugando && juego.panel != PanelCampus.Menu && juego.panel != PanelCampus.Intro && !juego.EnAulaAcademica)
            Hud(juego, ancho);

        if (juego.EnAulaAcademica) juego.DibujarAulaAcademica(ancho, alto);

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
            case PanelCampus.SensorHuella:
                DibujarSensorHuella(juego,ancho,alto);
                break;
            case PanelCampus.Recorrido:
                MenuRecorrido(juego,ancho,alto);
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
        {
            Prompt(juego, ancho, alto);
            GuiaRecorridoUI.Dibujar(juego, ancho, alto);
        }

        if (EstadoPartida.enPisoX && juego.NivelSiete != null)
            juego.NivelSiete.Dibujar(ancho, alto);

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
        var misionArt=!EstadoPartida.desperto && !EstadoPartida.enPisoX?juego.Tarjeta(RutaPlantaBaja.TarjetaActual):null;
        if(misionArt!=null){
            var tarjeta=PonerArte(misionArt,12f,8f,320f);
            GUI.Label(new Rect(18,tarjeta.yMax+4,310,24),"Paso "+Mathf.Min(RutaPlantaBaja.Paso+1,RutaPlantaBaja.Total)+" / "+RutaPlantaBaja.Total,hudTexto);
            bajo=tarjeta.yMax+32;
        }
        else if (objetivoArt != null)
        {
            var barra = PonerArte(objetivoArt, 12f, 8f, 440f);
            var texto = Trozo(barra, objetivoArt, 58f, 40f, 230f, 38f);
            Relleno(texto, new Color(0.012f, 0.07f, 0.176f, 1f));
            GUI.Label(new Rect(texto.x + 6f, texto.y + 4f, texto.width - 10f, texto.height - 8f), EstadoPartida.Objetivo(), hudTexto);
            bajo = barra.yMax + 4f;
        }

        if (vidaArt != null)
        {
            // Keep the original bars, excluding the unrelated baked-in portrait.
            float factor=250f/vidaArt.width;
            const float recorte=86f;
            var vida=new Rect(12f-recorte*factor,bajo,250f,vidaArt.height*factor);
            var barras=new Rect(12f,bajo,(vidaArt.width-recorte)*factor,vida.height);
            GUI.DrawTextureWithTexCoords(barras,vidaArt,new Rect(recorte/vidaArt.width,0,1-recorte/vidaArt.width,1),true);
            float lleno = Mathf.Clamp01(juego.estamina / 100f);
            var hueco = Trozo(vida, vidaArt, 134f, 53f, 116f, 13f);
            Relleno(hueco, new Color(0.01f, 0.06f, 0.2f, 1f));
            Relleno(new Rect(hueco.x, hueco.y, hueco.width * lleno, hueco.height), new Color(0.05f, 0.85f, 0.95f, 1f));
        }
        else
        {
            float lleno = Mathf.Clamp01(juego.estamina / 100f);
            Relleno(new Rect(16f, 66f, 220f, 14f), new Color(0f, 0f, 0f, 0.7f));
            Relleno(new Rect(18f, 68f, 216f * lleno, 10f), new Color(0.25f, 0.85f, 1f, 0.95f));
        }

        if (objetosArt != null && !EstadoPartida.enPisoX)
            PonerArte(objetosArt, ancho - 284f, 8f, 272f);

        if(!EstadoPartida.desperto && !EstadoPartida.enPisoX && GUI.Button(new Rect(ancho-158,148,146,38),"Misiones [M]",boton))juego.panel=PanelCampus.Recorrido;

        string rumbo = "Sistemas " + EstadoPartida.sistemas + "   Medicina " + EstadoPartida.medicina + "   " + EstadoPartida.NombreTercera;
        GUI.Label(new Rect(ancho - 520f, objetosArt != null ? 112f : 66f, 500f, 24f), rumbo, pequeño);
    }

    Texture2D huellaSensor;
    void DibujarSensorHuella(DirectorCampus juego,float ancho,float alto)
    {
        var caja=MarcoActividad("MOLINETES","Validación de acceso · sensor de huella",ancho,alto);
        float w=caja.width;
        if(huellaSensor==null){
            huellaSensor=new Texture2D(128,128,TextureFormat.RGBA32,false);
            huellaSensor.filterMode=FilterMode.Point;
            var pixels=new Color[128*128];
            for(int y=0;y<128;y++)for(int x=0;x<128;x++){
                float dx=(x-64f)/44f,dy=(y-65f)/53f;
                float radio=Mathf.Sqrt(dx*dx+dy*dy);
                float onda=radio*52f+Mathf.Sin(Mathf.Atan2(dy,dx)*3f)*.65f;
                bool linea=radio<1 && Mathf.Repeat(onda,5f)<1.7f;
                pixels[y*128+x]=linea?new Color(.25f,.79f,.83f,.9f):Color.clear;
            }
            huellaSensor.SetPixels(pixels);huellaSensor.Apply();
        }
        float cy=caja.y+94,sz=170;
        var sensor=new Rect(caja.center.x-sz*.5f,cy,sz,sz);
        Relleno(sensor,new Color(.035f,.105f,.16f));Borde(sensor,new Color(.15f,.4f,.48f));
        GUI.DrawTexture(sensor,huellaSensor);
        float scan=sensor.y+sensor.height*(1-juego.sensorHuella.Posicion);
        Relleno(new Rect(sensor.x+4,scan,sensor.width-8,2),juego.sensorHuella.EnZona?new Color(.4f,1,.65f):new Color(.4f,.83f,1));
        ProgresoActividad(caja,juego.sensorHuella.Aciertos,cy+183);
        var barra=new Rect(caja.x+38,cy+220,w-76,24);
        Relleno(barra,new Color(.12f,.17f,.25f));
        Relleno(new Rect(barra.x+(juego.sensorHuella.Centro-.14f)*barra.width,barra.y,.28f*barra.width,barra.height),new Color(.13f,.58f,.35f));
        Relleno(new Rect(barra.x+juego.sensorHuella.Posicion*barra.width-2,barra.y-4,4,barra.height+8),Color.white);
        GUI.Label(new Rect(caja.x+28,cy+254,w-56,46),juego.sensorHuella.Mensaje,pequeño);
        if(GUI.Button(new Rect(caja.x+36,caja.yMax-104,w-72,44),"Leer sensor [Espacio]",boton))juego.LeerSensorHuella();
        if(GUI.Button(new Rect(caja.x+36,caja.yMax-50,w-72,32),"Cancelar [Esc]",boton))juego.panel=PanelCampus.Ninguno;
    }

    void MenuRecorrido(DirectorCampus juego,float ancho,float alto)
    {
        Velo(ancho,alto,.75f);float w=Mathf.Min(680,ancho-32),h=Mathf.Min(640,alto-32);
        var caja=new Rect((ancho-w)/2,(alto-h)/2,w,h);Relleno(caja,new Color(.035f,.045f,.1f,.98f));Borde(caja,new Color(1,.6f,.2f));
        GUI.Label(new Rect(caja.x+20,caja.y+14,w-190,42),"Conoce UNIFRANZ",titulo);
        if(GUI.Button(new Rect(caja.xMax-150,caja.y+16,130,36),"Volver [M]",boton))juego.panel=PanelCampus.Ninguno;
        var tarjeta=juego.Tarjeta(RutaPlantaBaja.TarjetaActual);
        if(tarjeta!=null)GUI.DrawTexture(new Rect(caja.x+24,caja.y+66,Mathf.Min(320,w-48),120),tarjeta,ScaleMode.ScaleToFit,true);
        else GUI.Label(new Rect(caja.x+24,caja.y+66,w-48,64),RutaPlantaBaja.Objetivo,cuerpo);
        float row=Mathf.Min(30,(h-248)/RutaPlantaBaja.Total);
        for(int i=0;i<RutaPlantaBaja.Total;i++){
            bool actual=i==RutaPlantaBaja.Paso;Color previo=GUI.color;GUI.color=i<RutaPlantaBaja.Paso?new Color(.45f,1,.72f):actual?new Color(1,.72f,.35f):new Color(.55f,.59f,.68f);
            GUI.Label(new Rect(caja.x+28,caja.y+204+i*row,w-56,row),(i<RutaPlantaBaja.Paso?"✓ ":actual?"› ":"  ")+(i+1)+". "+RutaPlantaBaja.Titulos[i],pequeño);GUI.color=previo;
        }
        GUI.Label(new Rect(caja.x+20,caja.yMax-34,w-40,26),"Sigue el orden · E: interactuar · M / Esc: cerrar",pequeño);
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
        Velo(ancho,alto,.68f);
        var caja=CajaCentral(ancho,alto,650f,420f);
        GUI.Label(new Rect(caja.x+20,caja.y+20,caja.width-40,42),"Tu primer día en UNIFRANZ",titulo);
        GUI.Label(new Rect(caja.x+30,caja.y+82,caja.width-60,230),
            "Conoce la universidad siguiendo las misiones de planta baja.\n\nWASD / flechas: caminar · Shift: correr\nE: hablar e interactuar\nM: consultar el recorrido · Esc: pausa\n\nLa flecha indica tu siguiente destino. Acércate y pulsa E cuando aparezca la indicación.",cuerpo);
        if(GUI.Button(new Rect(caja.center.x-120,caja.yMax-70,240,46),"Entendido",boton))juego.Entendido();
    }

    void Frase(DirectorCampus juego, float ancho, float alto)
    {
        Velo(ancho, alto, 0.35f);
        float y = 70f;
        bool cajaDiseno = dialogoArt != null && juego.fraseTitulo != "CÓMO MOVERTE" && !EstadoPartida.enPisoX;
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

        float altoTexto = Mathf.Clamp(cuerpo.CalcHeight(new GUIContent(juego.fraseCuerpo), 560f) + 8f, 70f, EstadoPartida.enPisoX ? 400f : 220f);
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

    bool buscarPisoAscensor;
    void Ascensor(DirectorCampus juego, float ancho, float alto)
    {
        var caja=MarcoActividad("ASCENSOR","Estás en "+juego.PlantaActual+" · selecciona el botón del panel",ancho,alto);
        float factorPanel=Mathf.Min(1.8f,(caja.width-40)/240f);
        var panelRect=new Rect(caja.center.x-120*factorPanel,caja.y+94,240*factorPanel,210*factorPanel);
        if(juego.ascensor!=null)GUI.DrawTexture(panelRect,juego.ascensor,ScaleMode.StretchToFill,true);
        if(!buscarPisoAscensor){
        string[] pisos={"PB","M","1","2","3","4","5"};
        for(int i=0;i<pisos.Length;i++){
            var zona=new Rect(panelRect.x+(16+(i%3)*72)*factorPanel,panelRect.y+(46+(i/3)*50)*factorPanel,64*factorPanel,40*factorPanel);
            bool actual=pisos[i]==juego.PlantaActual;
            bool disponible=pisos[i]!="1";
            if(!disponible)Relleno(zona,new Color(.03f,.04f,.09f,.72f));
            if(actual)Borde(zona,new Color(.4f,.95f,.7f));
            bool previo=GUI.enabled;GUI.enabled=disponible&&!actual;
            if(GUI.Button(zona,"",vacio)){buscarPisoAscensor=false;juego.ElegirPiso(pisos[i]);}
            GUI.enabled=previo;
        }
        var extra=new Rect(panelRect.x+88*factorPanel,panelRect.y+146*factorPanel,64*factorPanel,40*factorPanel);
        if(juego.BotonXListo || juego.MostrarPisoX){
            if(GUI.Button(extra,"PISO X",boton))juego.ElegirPiso("X");
        }else if(EstadoPartida.desperto){
            if(GUI.Button(extra,"?",boton))buscarPisoAscensor=true;
        }
        }
        if(buscarPisoAscensor){
            Relleno(panelRect,new Color(.10f,.09f,.18f,1));Borde(panelRect,new Color(.95f,.57f,.2f));
            GUI.Label(new Rect(panelRect.x+12,panelRect.y+30,panelRect.width-24,50),"¿Cuál piso no figura?",titulo);
            string[] secretos={"6","7","8"};
            for(int i=0;i<3;i++)if(GUI.Button(new Rect(panelRect.x+20+i*(panelRect.width-40)/3,panelRect.y+130,(panelRect.width-40)/3-6,66),secretos[i],boton)){juego.ElegirPiso(secretos[i]);buscarPisoAscensor=false;}
            if(GUI.Button(new Rect(panelRect.x+24,panelRect.y+260,panelRect.width-48,46),"Volver a destinos",boton))buscarPisoAscensor=false;
        }
        GUI.Label(new Rect(caja.x+18,caja.yMax-91,caja.width-36,30),string.IsNullOrEmpty(juego.aviso)?"Piso 1: sin plano disponible · PB: tecla 0 · M: tecla M":juego.aviso,pequeño);
        if(GUI.Button(new Rect(caja.x+30,caja.yMax-50,caja.width-60,36),"Cerrar [Esc]",boton)){buscarPisoAscensor=false;juego.CerrarPanel();}
    }

    Rect MarcoActividad(string nombre,string subtitulo,float ancho,float alto)
    {
        Velo(ancho,alto,.76f);
        float w=Mathf.Min(610,ancho-32),h=Mathf.Min(570,alto-32);
        var caja=new Rect((ancho-w)/2,(alto-h)/2,w,h);
        Relleno(new Rect(caja.x+6,caja.y+7,w,h),new Color(0,0,0,.4f));
        Relleno(caja,new Color(.065f,.055f,.14f,.99f));Borde(caja,new Color(.46f,.38f,.68f));
        Relleno(new Rect(caja.x+3,caja.y+3,w-6,74),new Color(.10f,.085f,.20f));
        Relleno(new Rect(caja.x+20,caja.y+78,w-40,2),new Color(.96f,.57f,.18f));
        GUI.Label(new Rect(caja.x+18,caja.y+10,w-36,36),nombre,titulo);
        GUI.Label(new Rect(caja.x+18,caja.y+47,w-36,24),subtitulo,pequeño);
        return caja;
    }
    void ProgresoActividad(Rect caja,int aciertos,float y)
    {
        float paso=50,start=caja.center.x-70;
        for(int i=0;i<3;i++){
            var ficha=new Rect(start+i*paso,y,40,26);
            Relleno(ficha,i<aciertos?new Color(.20f,.65f,.45f):new Color(.18f,.17f,.29f));
            GUI.Label(ficha,i<aciertos?"✓":(i+1).ToString(),pequeño);
        }
    }
    void Minijuego(DirectorCampus juego, float ancho, float alto)
    {
        var caja=MarcoActividad("GAME ROOM","Desafío de reflejos · tres aciertos seguidos",ancho,alto);
        ProgresoActividad(caja,juego.aciertos,caja.y+96);
        GUI.Label(new Rect(caja.x+24,caja.y+137,caja.width-48,36),juego.flecha,titulo);
        float cx=caja.center.x,cy=caja.y+221,paso=76,tam=66;
        KeyCode[] teclas={KeyCode.UpArrow,KeyCode.LeftArrow,KeyCode.DownArrow,KeyCode.RightArrow};
        string[] etiquetas={"↑\nW","←\nA","↓\nS","→\nD"};
        string[] nombres={"ARRIBA","IZQUIERDA","ABAJO","DERECHA"};
        Vector2[] offsets={new Vector2(0,-paso*.35f),new Vector2(-paso,paso*.65f),new Vector2(0,paso*.65f),new Vector2(paso,paso*.65f)};
        for(int i=0;i<4;i++){
            Color previo=GUI.backgroundColor;
            GUI.backgroundColor=juego.flecha==nombres[i]?new Color(.98f,.58f,.18f):new Color(.28f,.23f,.43f);
            if(GUI.Button(new Rect(cx+offsets[i].x-tam*.5f,cy+offsets[i].y,tam,tam),etiquetas[i],boton))juego.PulsarDireccionArcade(teclas[i]);
            GUI.backgroundColor=previo;
        }
        GUI.Label(new Rect(caja.x+30,caja.y+396,caja.width-60,64),juego.avisoArcade,pequeño);
        GUI.Label(new Rect(caja.x+20,caja.yMax-101,caja.width-40,25),"Flechas / WASD o pulsa los controles de pantalla",pequeño);
        if(GUI.Button(new Rect(caja.x+30,caja.yMax-59,caja.width-60,40),"Volver al campus [Esc]",boton))juego.CerrarPanel();
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
            ? "Vuelves al ascensor con la vida completa. Conservas los objetos recogidos y las misiones completadas."
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
                ? "Proyecto 07 completado: energía restaurada, paciente a salvo, evidencia transmitida y salida reforzada.\nMisiones del piso 7: " + EstadoPisoSiete.misiones.Count + "/6.\n\n"
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
