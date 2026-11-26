using System.Collections.Generic;
using UnityEngine;

// Coordenadas de diseño en píxeles del plano 1672 x 941, origen arriba a la izquierda.
// El escenario, las colisiones, los puntos y la navegación comparten esa transformación.
public class PisoSiete : MonoBehaviour
{
    public const float PixelesPorUnidad = 48f;
    public static Vector2 Mundo(float x, float y) => new Vector2(x / PixelesPorUnidad, (941f - y) / PixelesPorUnidad);
    public DirectorCampus director;
    public Transform jugador;
    public readonly List<Rect> obstaculos = new List<Rect>();
    readonly List<PuntoCampus> puntos = new List<PuntoCampus>();
    readonly Dictionary<string, SpriteRenderer> marcas = new Dictionary<string, SpriteRenderer>();
    readonly List<EnemigoPisoSiete> enemigos = new List<EnemigoPisoSiete>();
    readonly Dictionary<string, Texture2D> texturas = new Dictionary<string, Texture2D>();
    readonly List<Sprite> sprites = new List<Sprite>();
    float inmuneHasta;
    float pulsoHasta;
    float avisoHasta;
    float tiempoActivo;
    string aviso = "";
    string ventana = "";
    int cable;
    string error = "";
    GUIStyle texto;
    GUIStyle titulo;
    GUIStyle boton;
    SpriteRenderer halo;
    SpriteRenderer electricidad;
    SpriteRenderer toxico;
    Texture2D plano;
    SpriteRenderer jugadorDibujo;
    Color colorJugador;
    bool[,] libres;
    const float Paso = 0.25f;
    const int Columnas = 140;
    const int Filas = 80;

    public void Preparar(DirectorCampus juego, Transform actor)
    {
        director = juego;
        jugador = actor;
        EstadoPisoSiete.vida = 100;
        jugadorDibujo = actor.GetComponent<SpriteRenderer>();
        colorJugador = jugadorDibujo.color;
        plano = GetComponent<SpriteRenderer>().sprite.texture;
        inmuneHasta = Time.time + 3f;
        CrearMuros();
        CrearNavegacion();
        Punto("ascensor7", "Usar ascensor", 105, 590);
        Punto("fusible", "Recoger fusible y kit de refuerzo", 1370, 815, "objetos/energia.png");
        Punto("energia7", "Reparar panel eléctrico", 860, 275);
        Punto("clinica7", "Asistir al herido", 500, 268);
        Punto("archivo7", "Leer registro del Proyecto 07", 468, 568, "objetos/tarjeta.png");
        Punto("muestra", "Tomar muestra sellada", 1030, 640, "objetos/ficha.png");
        Punto("laboratorio7", "Usar sintetizador", 1330, 265);
        Punto("radio7", "Usar terminal de transmisión", 480, 820);
        Punto("estructura7", "Reforzar salida", 1225, 815);
        Punto("recursos7", "Administrar suministros", 1320, 558, "objetos/cofre_dorado.png");
        Punto("aturdidor", "Recoger aturdidor", 760, 750, "objetos/aturdidor.png");
        Punto("nucleo7", "Activar núcleo de salida", 882, 590);
        CrearEntidad("mutante_sombra", 990, 405, 730, 405, 2.1f, 18);
        CrearEntidad("arana_mutante", 1040, 740, 760, 795, 2.6f, 14);
        CrearEntidad("bestia_toxica", 980, 640, 1030, 805, 1.7f, 24);
        CrearEntidad("masa_corrupta", 1175, 570, 1480, 570, 1.4f, 16);
        CrearEntidad("tentaculo", 570, 580, 310, 580, 1.1f, 20);
        halo = CrearDibujo("Luz del núcleo", Mundo(882, 550), null, -10);
        halo.sprite = SpriteCirculo();
        halo.transform.localScale = new Vector3(3.4f, 1.7f, 1f);
        electricidad = Peligro("Descarga eléctrica", "trampas/trampa_electrica.png", 735, 450);
        toxico = Peligro("Fuga tóxica", "trampas/campo_toxico.png", 1000, 670);
        // Señala visualmente la conexión lateral de la cabina con el pasillo.
        var puertas = Fotogramas("interactivos/puerta_abriendo.png", 100, 4);
        if (puertas.Length > 0)
        {
            var puerta = CrearDibujo("Salida lateral del ascensor", Mundo(185, 410), puertas[puertas.Length - 1], 30);
            puerta.transform.localScale = new Vector3(0.9f, 0.9f, 1f);
        }
        ActualizarMarcas();
        if (!EstadoPisoSiete.presentado)
        {
            EstadoPisoSiete.presentado = true;
            Hablar("PISO 7 · PROYECTO 07", "El ascensor se detiene entre dos plantas. Una voz sale del intercomunicador.\n\n«Soy la doctora Vera. El núcleo colapsó y las muestras escaparon. Restaura la energía, ayúdame y recupera la evidencia. Solo entonces podremos abrir una salida».\n\nE: interactuar · Shift: correr · Espacio: aturdidor · I: inventario y misiones · Esc: pausa.\nBusca el fusible en ingeniería, abajo a la derecha.");
        }
    }

    Texture2D Textura(string ruta)
    {
        if (!texturas.TryGetValue(ruta, out var tex))
        {
            tex = RecursosCampus.Textura("PisoX/Pack/" + ruta);
            texturas[ruta] = tex;
        }
        return tex;
    }

    public Sprite[] Fotogramas(string ruta, int ancho, int cantidad)
    {
        var tex = Textura(ruta);
        if (tex == null) return new Sprite[0];
        int n = Mathf.Min(cantidad, tex.width / ancho);
        var resultado = new Sprite[n];
        for (int i = 0; i < n; i++)
        {
            resultado[i] = Sprite.Create(tex, new Rect(i * ancho, 0, ancho, tex.height), new Vector2(0.5f, 0.15f), 64f);
            sprites.Add(resultado[i]);
        }
        return resultado;
    }

    Sprite SpriteCirculo()
    {
        var tex = new Texture2D(32, 32, TextureFormat.RGBA32, false);
        for (int y = 0; y < 32; y++)
        for (int x = 0; x < 32; x++)
        {
            float distancia = Vector2.Distance(new Vector2(x, y), new Vector2(15.5f, 15.5f));
            tex.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Clamp01(16f - distancia) * 0.5f));
        }
        tex.Apply();
        texturas["halo"] = tex;
        var sprite = RecursosCampus.ComoSprite(tex, 32f, new Vector2(0.5f, 0.5f));
        sprites.Add(sprite);
        return sprite;
    }

    SpriteRenderer CrearDibujo(string nombre, Vector2 posicion, Sprite sprite, int orden)
    {
        var go = new GameObject(nombre);
        go.transform.SetParent(transform);
        go.transform.position = posicion;
        var dibujo = go.AddComponent<SpriteRenderer>();
        dibujo.sprite = sprite;
        dibujo.sortingOrder = orden;
        return dibujo;
    }

    SpriteRenderer Peligro(string nombre, string ruta, float x, float y)
    {
        var sprite = RecursosCampus.ComoSprite(Textura(ruta), 110f, new Vector2(0.5f, 0.5f));
        if (sprite != null) sprites.Add(sprite);
        return CrearDibujo(nombre, Mundo(x, y), sprite, -5);
    }

    void Punto(string id, string nombre, float x, float y, string arte = null)
    {
        puntos.Add(new PuntoCampus { id = id, nombre = nombre, posicion = Mundo(x, y), radio = 1.55f });
        Sprite sprite;
        if (arte != null)
        {
            var tex = Textura(arte);
            sprite = RecursosCampus.ComoSprite(tex, 80f, new Vector2(0.5f, 0.15f));
            if (sprite != null) sprites.Add(sprite);
        }
        else sprite = SpriteCirculo();
        var dibujo = CrearDibujo(nombre, Mundo(x, y), sprite, 160);
        if (arte == null) dibujo.transform.localScale = new Vector3(0.45f, 0.3f, 1f);
        marcas[id] = dibujo;
    }

    public PuntoCampus Cercano(Vector2 posicion)
    {
        PuntoCampus mejor = null;
        float distancia = float.MaxValue;
        foreach (var punto in puntos)
        {
            if ((punto.id == "fusible" && EstadoPisoSiete.Tiene("fusible")) ||
                (punto.id == "muestra" && EstadoPisoSiete.Tiene("muestra")) ||
                (punto.id == "aturdidor" && EstadoPisoSiete.Tiene("aturdidor"))) continue;
            float d = Vector2.Distance(posicion, punto.posicion);
            if (d > punto.radio || d >= distancia || !LineaLibre(posicion, punto.posicion)) continue;
            distancia = d;
            mejor = punto;
        }
        return mejor;
    }

    public void Usar(string id)
    {
        if (director.bloqueado) return;
        switch (id)
        {
            case "ascensor7":
                AbrirPrueba("ascensor");
                break;
            case "fusible":
                EstadoPisoSiete.objetos.Add("fusible");
                EstadoPisoSiete.objetos.Add("refuerzo");
                Hablar("OBJETOS RECUPERADOS", "Guardas un fusible de repuesto y un kit de anclajes.\nLa etiqueta del fusible indica: CIAN → MAGENTA → VERDE.\n\nVera: «Instálalo en el panel eléctrico, al norte. No toques los cables en otro orden».");
                break;
            case "aturdidor":
                EstadoPisoSiete.objetos.Add("aturdidor");
                Hablar("ATURDIDOR DE EMERGENCIA", "Pulsa Espacio para emitir un pulso. Aturde durante 3 segundos a las criaturas cercanas que puedas ver. Consume 25 de aire y necesita 2,5 segundos de recarga.\n\nNo elimina a las criaturas. Úsalo para abrirte paso.");
                break;
            case "energia7":
                if (Hecha("energia", "El fusible está estable. Las salas tienen energía.")) break;
                if (!Requiere(EstadoPisoSiete.Tiene("fusible"), "Falta el fusible. Busca en ingeniería, abajo a la derecha.")) break;
                AbrirPrueba("energia");
                break;
            case "clinica7":
                if (EstadoPisoSiete.Lista("clinica"))
                {
                    EstadoPisoSiete.vida = 100;
                    Hablar("CLÍNICA · ZONA DE AUXILIO", "Vera revisa tus heridas. Recuperas toda la vida.\n«Los registros están en el archivo. Necesitarás mi reactivo y una muestra para frenar la contaminación».");
                    break;
                }
                if (Requiere(EstadoPisoSiete.Lista("energia"), "Vera: «El monitor está apagado. Restaura la energía antes de asistir al paciente».")) AbrirPrueba("clinica");
                break;
            case "archivo7":
                if (Hecha("archivo", "Registro 07: el ensayo de orientación abrió una conexión con un piso fuera del plano. El director ordenó ocultar el accidente. La evidencia debe salir por la antena de streaming.")) break;
                if (!Requiere(EstadoPisoSiete.Lista("energia"), "La cerradura del archivo necesita energía.")) break;
                Completar("archivo");
                EstadoPisoSiete.objetos.Add("tarjeta");
                EstadoPisoSiete.objetos.Add("registro");
                Hablar("ARCHIVO · REGISTRO 07", "«Ensayo 07. El núcleo no transporta personas: conecta lugares. Nadie debe conocer el accidente».\n\nRecoges la tarjeta de acceso y la copia del registro.\nVera: «No borres nada. Cuando tengas el antídoto, difunde la evidencia desde streaming».");
                break;
            case "muestra":
                if (!Requiere(EstadoPisoSiete.Lista("clinica"), "Necesitas los guantes y el recipiente sellado de la clínica. No toques la contaminación con las manos.")) break;
                EstadoPisoSiete.objetos.Add("muestra");
                Hablar("MUESTRA SELLADA", "Tomas una muestra sin abrir el recipiente. El laboratorio está arriba a la derecha. El reactivo de Vera debe neutralizarla dentro del sintetizador.");
                break;
            case "laboratorio7":
                if (Hecha("antidoto", "La contención está activa. Las criaturas se mueven más despacio y detectan a menor distancia.")) break;
                if (Requiere(EstadoPisoSiete.Tiene("muestra") && EstadoPisoSiete.Tiene("reactivo"), "El sintetizador necesita una muestra sellada y el reactivo de Vera. Revisa la clínica y el núcleo.")) AbrirPrueba("antidoto");
                break;
            case "radio7":
                if (Hecha("transmision", "La evidencia ya está fuera del edificio. La señal de auxilio permanece activa.")) break;
                if (Requiere(EstadoPisoSiete.Lista("archivo") && EstadoPisoSiete.Lista("antidoto"), "El transmisor pide el registro del archivo y que actives primero la contención del laboratorio.")) AbrirPrueba("transmision");
                break;
            case "estructura7":
                if (Hecha("estructura", "Los anclajes mantienen la salida estable. Vuelve al núcleo.")) break;
                if (Requiere(EstadoPisoSiete.Lista("transmision") && EstadoPisoSiete.Tiene("refuerzo"), "Ingeniería: «Primero transmite la señal. El kit de anclajes está junto al fusible».")) AbrirPrueba("estructura");
                break;
            case "recursos7":
                if (EstadoPisoSiete.Tiene("suministros")) Hablar("SUMINISTROS", "Ya recogiste el único lote disponible. H: usar botiquín. El escudo absorbe un golpe automáticamente. Puedes curarte en la clínica después de asistir a Vera.");
                else
                {
                    EstadoPisoSiete.objetos.Add("suministros");
                    EstadoPisoSiete.objetos.Add("botiquin");
                    EstadoPisoSiete.objetos.Add("escudo");
                    Hablar("SUMINISTROS DE EMERGENCIA", "Recoges un botiquín y un escudo.\nH: usar el botiquín para recuperar 50 de vida.\nEl escudo absorbe el siguiente golpe.\n\nLas reservas son limitadas; la clínica puede curarte después de rescatar al paciente.");
                }
                break;
            case "nucleo7":
                if (!Requiere(EstadoPisoSiete.PortalListo, "El núcleo sigue bloqueado.\n\n" + EstadoPisoSiete.Objetivo() + "\n\nPulsa I para consultar las seis tareas.")) break;
                AbrirPrueba("final");
                break;
        }
        ActualizarMarcas();
    }

    bool Requiere(bool listo, string mensaje)
    {
        if (!listo) Hablar("PROYECTO 07", mensaje);
        return listo;
    }

    bool Hecha(string id, string mensaje)
    {
        if (!EstadoPisoSiete.Lista(id)) return false;
        Hablar("REGISTRO DE MISIÓN", mensaje);
        return true;
    }

    void Hablar(string tituloDialogo, string mensaje)
    {
        if (tituloDialogo.Contains("RECUPERADOS") || tituloDialogo.Contains("RECUPERADO") || tituloDialogo.Contains("SUMINISTROS") || tituloDialogo == "MUESTRA SELLADA" || tituloDialogo == "ATURDIDOR DE EMERGENCIA")
            SonidoCampus.Emitir("recoger", .7f);
        if (tituloDialogo == "ARCHIVO · REGISTRO 07") SonidoCampus.Emitir("tarjeta", .7f);
        director.DialogoPisoSiete(tituloDialogo, mensaje);
    }

    void Completar(string id)
    {
        if (EstadoPisoSiete.misiones.Add(id))
            SonidoCampus.Emitir(id == "energia" ? "energia" : id == "clinica" ? "curar" : id == "transmision" ? "transmision" : id == "estructura" ? "anclajes" : "mision", .75f);
        ActualizarMarcas();
    }

    void AbrirPrueba(string id)
    {
        ventana = id;
        cable = 0;
        error = "";
        director.panel = PanelCampus.PisoSiete;
        jugador.GetComponent<JugadorCampus>().Detener();
    }

    void Update()
    {
        if (director == null || jugador == null) return;
        if (jugadorDibujo != null)
            jugadorDibujo.color = Time.time < inmuneHasta && !director.bloqueado && Mathf.Sin(Time.time * 25f) > 0f
                ? new Color(0.4f, 1f, 1f, 0.6f) : colorJugador;
        if (halo != null)
            halo.color = EstadoPisoSiete.PortalListo ? new Color(0.1f, 1f, 0.7f, 0.55f + Mathf.Sin(Time.time * 3f) * 0.15f) : new Color(0.6f, 0.1f, 0.8f, 0.35f);
        electricidad.enabled = !EstadoPisoSiete.Lista("energia");
        toxico.enabled = !EstadoPisoSiete.Lista("antidoto");
        if (director.panel == PanelCampus.PisoSiete && (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.I) || Input.GetKeyDown(KeyCode.M)))
        {
            director.CerrarPanel();
            ventana = "";
            return;
        }
        if (!director.jugando || director.bloqueado) return;
        tiempoActivo += Time.deltaTime;
        if (Input.GetKeyDown(KeyCode.I)) AbrirPrueba("inventario");
        else if (Input.GetKeyDown(KeyCode.M)) AbrirPrueba("mapa");
        if (electricidad.enabled && Vector2.Distance(jugador.position, electricidad.transform.position) < 0.65f)
            RecibirGolpe(8, "Una descarga te hirió.");
        if (toxico.enabled && Vector2.Distance(jugador.position, toxico.transform.position) < 0.65f)
            RecibirGolpe(8, "La fuga tóxica te hirió.");
        if (director.bloqueado) return;
        if (Input.GetKeyDown(KeyCode.H))
        {
            if (!EstadoPisoSiete.Tiene("botiquin")) Avisar("No tienes botiquín. Busca suministros o vuelve a la clínica.");
            else if (EstadoPisoSiete.vida >= 100) Avisar("Ya tienes la vida completa.");
            else
            {
                EstadoPisoSiete.objetos.Remove("botiquin");
                EstadoPisoSiete.vida = Mathf.Min(100, EstadoPisoSiete.vida + 50);
                SonidoCampus.Emitir("curar", .7f);
                Avisar("Botiquín usado: +50 de vida.");
            }
        }
        if (Input.GetKeyDown(KeyCode.Space)) Pulso();
    }

    void Pulso()
    {
        if (!EstadoPisoSiete.Tiene("aturdidor")) { Avisar("Busca el aturdidor en el pasillo inferior del núcleo."); return; }
        if (tiempoActivo < pulsoHasta) { Avisar("El aturdidor se está recargando."); return; }
        if (director.estamina < 25f) { Avisar("Necesitas 25 de aire para el pulso."); return; }
        director.estamina -= 25f;
        SonidoCampus.Emitir("pulso", .7f);
        pulsoHasta = tiempoActivo + 2.5f;
        int afectados = 0;
        foreach (var enemigo in enemigos)
            if (Vector2.Distance(enemigo.transform.position, jugador.position) < 4f && LineaLibre(enemigo.transform.position, jugador.position))
            { enemigo.Aturdir(); afectados++; }
        Avisar(afectados > 0 ? "Pulso emitido. Criaturas aturdidas: " + afectados : "Pulso emitido. Ninguna criatura a la vista.");
    }

    public void RecibirGolpe(int cantidad, string causa = "Una criatura te hirió.")
    {
        if (director.bloqueado || Time.time < inmuneHasta) return;
        inmuneHasta = Time.time + 1.6f;
        EstadoPisoSiete.golpes++;
        if (EstadoPisoSiete.objetos.Remove("escudo")) { SonidoCampus.Emitir("escudo", .75f); Avisar("El escudo absorbió el golpe."); return; }
        SonidoCampus.Emitir(causa.Contains("descarga") ? "electrico" : causa.Contains("tóxica") ? "toxico" : "golpe", .7f);
        EstadoPisoSiete.vida = Mathf.Max(0, EstadoPisoSiete.vida - cantidad);
        Avisar(causa + " Vida: " + EstadoPisoSiete.vida + ". Busca una zona segura.");
        if (EstadoPisoSiete.vida == 0) director.AlcanzadoEnPiso();
    }

    void Avisar(string mensaje) { aviso = mensaje; avisoHasta = Time.time + 3.5f; }

    void ActualizarMarcas()
    {
        foreach (var par in marcas)
        {
            bool recogido = (par.Key == "fusible" && EstadoPisoSiete.Tiene("fusible")) ||
                (par.Key == "muestra" && EstadoPisoSiete.Tiene("muestra")) ||
                (par.Key == "aturdidor" && EstadoPisoSiete.Tiene("aturdidor"));
            par.Value.enabled = !recogido;
            par.Value.color = new Color(0.3f, 1f, 1f);
            string mision = par.Key == "energia7" ? "energia" : par.Key == "clinica7" ? "clinica" :
                par.Key == "archivo7" ? "archivo" : par.Key == "laboratorio7" ? "antidoto" :
                par.Key == "radio7" ? "transmision" : par.Key == "estructura7" ? "estructura" : "";
            if (EstadoPisoSiete.Lista(mision)) par.Value.color = new Color(0.25f, 1f, 0.4f);
        }
    }

    void CrearMuros()
    {
        // Paredes recortadas alrededor de las entradas dibujadas; no se bloquea ninguna puerta.
        Muro(0, 0, 1672, 62); Muro(0, 900, 1672, 41);
        Muro(0, 0, 30, 941); Muro(1550, 0, 122, 941);
        // Clínica.
        Muro(185, 65, 470, 48); Muro(185, 80, 38, 235); Muro(615, 110, 40, 172);
        Muro(200, 292, 265, 42); Muro(600, 292, 55, 42);
        // Panel de energía y laboratorio.
        Muro(680, 62, 370, 48); Muro(680, 90, 37, 225); Muro(1012, 90, 38, 225);
        Muro(700, 300, 117, 25); Muro(935, 300, 115, 25);
        Muro(1080, 65, 460, 45); Muro(1080, 90, 35, 205); Muro(1500, 90, 40, 230);
        Muro(1095, 292, 100, 30); Muro(1400, 292, 140, 30);
        // Archivo: acceso lateral derecho.
        Muro(220, 375, 440, 47); Muro(220, 400, 35, 210);
        Muro(230, 605, 440, 35); Muro(640, 405, 35, 53);
        // Recursos: acceso lateral izquierdo.
        Muro(1130, 375, 410, 45); Muro(1500, 400, 40, 220);
        Muro(1130, 600, 410, 38); Muro(1125, 405, 35, 53);
        // Streaming y estructuras: accesos laterales hacia el corredor central.
        Muro(220, 645, 460, 45); Muro(220, 665, 35, 195); Muro(230, 850, 450, 42);
        Muro(655, 675, 35, 45); Muro(655, 830, 35, 35);
        Muro(1080, 645, 460, 45); Muro(1500, 665, 40, 200); Muro(1090, 850, 450, 42);
        Muro(1080, 675, 35, 45); Muro(1080, 830, 35, 35);
        // Ascensor. Salida lateral al pasillo entre clínica y archivo.
        Muro(30, 300, 170, 30); Muro(170, 405, 35, 240); Muro(30, 640, 175, 35);
        // Muebles y aparatos; dejan espacio frente a cada punto interactivo.
        Muro(310, 155, 280, 87); Muro(735, 125, 240, 105);
        Muro(1130, 150, 330, 75); Muro(275, 440, 325, 100);
        Muro(1175, 440, 300, 90); Muro(275, 700, 330, 90);
        Muro(1145, 700, 315, 85);
    }

    void Muro(float x, float y, float w, float h)
    {
        Vector2 centro = Mundo(x + w / 2f, y + h / 2f);
        Vector2 medida = new Vector2(w, h) / PixelesPorUnidad;
        obstaculos.Add(new Rect(centro - medida / 2f, medida));
        var go = new GameObject("Colisión piso 7");
        go.transform.SetParent(transform);
        go.transform.position = centro;
        go.AddComponent<BoxCollider2D>().size = medida;
    }

    public bool Libre(Vector2 p, float margen = 0.38f)
    {
        if (p.x < margen || p.y < margen || p.x > 1672f / 48f - margen || p.y > 941f / 48f - margen) return false;
        foreach (var rect in obstaculos)
            if (p.x > rect.xMin - margen && p.x < rect.xMax + margen && p.y > rect.yMin - margen && p.y < rect.yMax + margen) return false;
        return true;
    }

    public bool LineaLibre(Vector2 a, Vector2 b)
    {
        return SegmentoLibre(a, b, 0.12f);
    }

    bool SegmentoLibre(Vector2 a, Vector2 b, float margen)
    {
        int pasos = Mathf.CeilToInt(Vector2.Distance(a, b) / 0.15f);
        for (int i = 0; i <= pasos; i++)
            if (!Libre(Vector2.Lerp(a, b, pasos == 0 ? 0f : i / (float)pasos), margen)) return false;
        return true;
    }

    void CrearNavegacion()
    {
        libres = new bool[Columnas, Filas];
        for (int x = 0; x < Columnas; x++)
        for (int y = 0; y < Filas; y++) libres[x, y] = Libre(new Vector2(x * Paso, y * Paso));
    }

    int CeldaLibre(Vector2 p)
    {
        int mejor = -1;
        float distancia = float.MaxValue;
        for (int x = 0; x < Columnas; x++)
        for (int y = 0; y < Filas; y++)
        {
            if (!libres[x, y]) continue;
            float d = (new Vector2(x * Paso, y * Paso) - p).sqrMagnitude;
            if (d >= distancia) continue;
            distancia = d;
            mejor = y * Columnas + x;
        }
        return mejor;
    }

    // BFS en una rejilla pequeña: ruta real por puertas en vez de perseguir a través de muros.
    public Vector2 SiguientePaso(Vector2 inicio, Vector2 destino)
    {
        if (SegmentoLibre(inicio, destino, 0.38f)) return destino;
        int origen = CeldaLibre(inicio), fin = CeldaLibre(destino);
        if (origen < 0 || fin < 0 || origen == fin) return inicio;
        Vector2 centroOrigen = new Vector2((origen % Columnas) * Paso, (origen / Columnas) * Paso);
        if (Vector2.Distance(inicio, centroOrigen) > 0.1f) return centroOrigen;
        var previo = new int[Columnas * Filas];
        for (int i = 0; i < previo.Length; i++) previo[i] = -1;
        var cola = new Queue<int>();
        cola.Enqueue(origen); previo[origen] = origen;
        int[] dx = { 1, -1, 0, 0 }, dy = { 0, 0, 1, -1 };
        while (cola.Count > 0 && previo[fin] < 0)
        {
            int celda = cola.Dequeue();
            int x = celda % Columnas, y = celda / Columnas;
            for (int d = 0; d < 4; d++)
            {
                int nx = x + dx[d], ny = y + dy[d];
                if (nx < 0 || ny < 0 || nx >= Columnas || ny >= Filas || !libres[nx, ny]) continue;
                int vecino = ny * Columnas + nx;
                if (previo[vecino] >= 0) continue;
                previo[vecino] = celda; cola.Enqueue(vecino);
            }
        }
        if (previo[fin] < 0) return inicio;
        int siguiente = fin;
        while (previo[siguiente] != origen) siguiente = previo[siguiente];
        return new Vector2((siguiente % Columnas) * Paso, (siguiente / Columnas) * Paso);
    }

    void CrearEntidad(string nombre, float x, float y, float metaX, float metaY, float velocidad, int dano)
    {
        var frames = Fotogramas("monstruos/" + nombre + ".png", 96, 6);
        var dibujo = CrearDibujo(nombre, Mundo(x, y), frames.Length > 0 ? frames[0] : null, 80);
        var entidad = dibujo.gameObject.AddComponent<EnemigoPisoSiete>();
        entidad.Preparar(this, frames, Mundo(metaX, metaY), velocidad, dano);
        enemigos.Add(entidad);
    }

    void PrepararGui()
    {
        if (texto != null) return;
        texto = new GUIStyle(GUI.skin.label) { fontSize = 17, wordWrap = true };
        texto.normal.textColor = Color.white;
        titulo = new GUIStyle(texto) { fontSize = 25, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
        boton = new GUIStyle(GUI.skin.button) { fontSize = 17, wordWrap = true };
    }

    public void Dibujar(float ancho, float alto)
    {
        PrepararGui();
        if (director.panel != PanelCampus.Fin && director.panel != PanelCampus.Muerte)
        {
            var equipo = new Rect(ancho - 300f, 8f, 284f, 96f);
            Color previa = GUI.color;
            GUI.color = new Color(0.06f, 0.03f, 0.16f, 0.96f);
            GUI.DrawTexture(equipo, Texture2D.whiteTexture);
            GUI.color = previa;
            GUI.Box(equipo, "");
            GUI.Label(new Rect(equipo.x + 10f, equipo.y + 5f, 264f, 24f), "EQUIPO DEL PISO 7", texto);
            GUI.Label(new Rect(equipo.x + 10f, equipo.y + 32f, 264f, 24f), "Botiquín: " + (EstadoPisoSiete.Tiene("botiquin") ? "1" : "0") + "    Escudo: " + (EstadoPisoSiete.Tiene("escudo") ? "1" : "0"), texto);
            GUI.Label(new Rect(equipo.x + 10f, equipo.y + 59f, 264f, 24f), "Tarjeta: " + (EstadoPisoSiete.Tiene("tarjeta") ? "sí" : "—") + "    Pulso: " + (EstadoPisoSiete.Tiene("aturdidor") ? "sí" : "—"), texto);
        }
        if (director.panel == PanelCampus.PisoSiete) { DibujarVentana(ancho, alto); return; }
        if (director.panel != PanelCampus.Ninguno) return;
        var caja = new Rect(ancho - 300f, 158f, 284f, 108f);
        GUI.Box(caja, "");
        GUI.Label(new Rect(caja.x + 10f, caja.y + 5f, 265f, 24f), "PISO 7 · PROYECTO 07", texto);
        GUI.Label(new Rect(caja.x + 10f, caja.y + 32f, 265f, 24f), "Vida: " + EstadoPisoSiete.vida + "/100    Misiones: " + EstadoPisoSiete.misiones.Count + "/6", texto);
        GUI.Label(new Rect(caja.x + 10f, caja.y + 61f, 265f, 42f), "I · diario    M · mapa    H · curar\nEspacio · pulso aturdidor", texto);
        var mini = new Rect(ancho - 260f, alto - 176f, 244f, 138f);
        DibujarMapa(mini, false);
        if (Time.time < avisoHasta)
        {
            GUI.Box(new Rect(ancho / 2f - 300f, alto - 108f, 600f, 72f), "");
            GUI.Label(new Rect(ancho / 2f - 288f, alto - 98f, 576f, 60f), aviso, texto);
        }
        // Nombre de cada sala en el plano, sin tapar muebles ni pasillos.
        Rotulo("CLÍNICA", 430, 122, ancho, alto);
        Rotulo("ENERGÍA", 865, 112, ancho, alto);
        Rotulo("LABORATORIO", 1300, 120, ancho, alto);
        Rotulo("ARCHIVO 07", 445, 425, ancho, alto);
        Rotulo("SUMINISTROS", 1320, 425, ancho, alto);
        Rotulo("STREAMING", 440, 698, ancho, alto);
        Rotulo("INGENIERÍA", 1300, 698, ancho, alto);
    }

    void Rotulo(string nombre, float x, float y, float ancho, float alto)
    {
        var camara = Camera.main;
        if (camara == null) return;
        var p = camara.WorldToViewportPoint(Mundo(x, y));
        if (p.z < 0f || p.x < 0.08f || p.x > 0.92f || p.y < 0.08f || p.y > 0.92f) return;
        GUI.Box(new Rect(p.x * ancho - 80f, (1f - p.y) * alto - 14f, 160f, 28f), nombre);
    }

    void DibujarVentana(float ancho, float alto)
    {
        var caja = new Rect(ancho / 2f - 350f, alto / 2f - 265f, 700f, 530f);
        Color color = GUI.color;
        GUI.color = new Color(0.06f, 0.03f, 0.16f, 0.98f);
        GUI.DrawTexture(caja, Texture2D.whiteTexture); GUI.color = color;
        GUI.Box(caja, "");
        string nombre = "PROYECTO 07";
        string mensaje = "";
        if (ventana == "ascensor")
        {
            nombre = "ASCENSOR · PISO 7";
            mensaje = "La cabina sigue aquí. Tus hallazgos quedan registrados durante esta partida. Puedes regresar al campus o seguir investigando.\n\nAl volver al piso 7 conservas las seis tareas y los objetos que hayas recogido.";
            if (SonidoCampus.Boton(new Rect(caja.x + 30f, caja.y + 280f, 640f, 48f), "Volver al selector de pisos", boton)) director.RegresarDelPisoSiete();
        }
        else if (ventana == "mapa")
        {
            nombre = "PLANO DEL PISO 7 · TU POSICIÓN EN NARANJA";
            DibujarMapa(new Rect(caja.x + 48f, caja.y + 72f, 604f, 340f), true);
            GUI.Label(new Rect(caja.x + 30f, caja.y + 424f, 640f, 44f), "Cian: interacción · Verde: tarea completada · I: inventario", texto);
        }
        else if (ventana == "inventario")
        {
            nombre = "INVENTARIO Y MISIONES · PISO 7";
            string[] ids = { "energia", "clinica", "archivo", "antidoto", "transmision", "estructura" };
            string[] nombres = { "Restaurar energía", "Salvar al herido", "Recuperar el registro y la tarjeta", "Activar contención con el antídoto", "Transmitir la evidencia", "Reforzar la salida" };
            for (int i = 0; i < ids.Length; i++) mensaje += (EstadoPisoSiete.Lista(ids[i]) ? "[LISTO] " : "[  ] ") + nombres[i] + "\n";
            mensaje += "\nOBJETOS: " + Inventario() + "\n\n" + EstadoPisoSiete.Objetivo();
        }
        else if (ventana == "energia")
        {
            nombre = "REPARAR ENERGÍA";
            mensaje = "Instala el fusible y conecta los tres cables.\nEtiqueta: CIAN → MAGENTA → VERDE.\nConexiones correctas: " + cable + "/3\n\nUna secuencia incorrecta reinicia el panel; puedes volver a intentarlo.";
            string[] cables = { "CIAN", "MAGENTA", "VERDE" };
            for (int i = 0; i < 3; i++)
            {
                int elegido = i;
                if (SonidoCampus.Boton(new Rect(caja.x + 30f + i * 218f, caja.y + 270f, 204f, 54f), cables[i], boton))
                    ConectarCable(elegido);
            }
        }
        else if (ventana == "clinica")
        {
            nombre = "CLÍNICA · AYUDA DE EMERGENCIA";
            mensaje = "Vera: «La criatura dañó el monitor y el paciente no puede moverse. Necesito que reconectes los sensores y me acerques el material sellado».\n\nElige una acción. Esta es una situación ficticia del laboratorio.";
            Opcion(caja, 0, "Reconectar monitor y entregar material", "clinica");
            if (SonidoCampus.Boton(new Rect(caja.x + 30f, caja.y + 330f, 640f, 44f), "Forzar el traslado sin estabilizar", boton)) error = "Vera: «Aún no. Ayúdame con el monitor y el material primero».";
        }
        else if (ventana == "antidoto")
        {
            nombre = "LABORATORIO · CONTENCIÓN";
            mensaje = "El lector identifica la muestra del núcleo.\nVera: «Usa el reactivo dentro de la cámara sellada. El aire del piso llevará el neutralizante a las salas».\n\nSelecciona el procedimiento de contención.";
            Opcion(caja, 0, "Sellar cámara, añadir reactivo y neutralizar", "antidoto");
            if (SonidoCampus.Boton(new Rect(caja.x + 30f, caja.y + 330f, 640f, 44f), "Abrir la muestra en el pasillo", boton)) error = "Vera: «No abras el recipiente. La cámara sellada evita otra fuga».";
        }
        else if (ventana == "transmision")
        {
            nombre = "STREAMING · LA VERDAD DEL PISO 7";
            mensaje = "El registro acusa al Proyecto 07 de ocultar el accidente. La red vuelve a estar disponible.\n\nVera: «Si enviamos una copia, nadie podrá borrarlo. Incluye nuestra ubicación y la señal de auxilio».";
            Opcion(caja, 0, "Transmitir evidencia y pedir ayuda", "transmision");
        }
        else if (ventana == "estructura")
        {
            nombre = "INGENIERÍA · ÚLTIMO ANCLAJE";
            mensaje = "La señal ha abierto una ruta de regreso, pero el núcleo está deformando la salida.\n\nEl kit contiene anclajes para reforzar las columnas. El plano muestra los puntos de carga.";
            Opcion(caja, 0, "Instalar anclajes en los puntos de carga", "estructura");
            if (SonidoCampus.Boton(new Rect(caja.x + 30f, caja.y + 330f, 640f, 44f), "Retirar el soporte principal", boton)) error = "El plano advierte que retirar ese soporte bloquearía la salida. Usa los anclajes.";
        }
        else if (ventana == "final")
        {
            nombre = "NÚCLEO ESTABLE · REGRESO AL CAMPUS";
            mensaje = "La tarjeta abre el núcleo. El paciente está a salvo, la evidencia fue transmitida y la contaminación permanece contenida.\n\nVera: «El piso existió. Ahora todos sabrán lo que pasó. Cruza; yo llevaré al paciente por la cabina».\n\n¿Cruzar el portal?";
            if (SonidoCampus.Boton(new Rect(caja.x + 30f, caja.y + 280f, 640f, 48f), "Cruzar y completar el Proyecto 07", boton))
                CruzarPortal();
        }
        GUI.Label(new Rect(caja.x + 20f, caja.y + 16f, 660f, 42f), nombre, titulo);
        GUI.Label(new Rect(caja.x + 30f, caja.y + 76f, 640f, ventana == "inventario" ? 365f : 180f), mensaje, texto);
        GUI.Label(new Rect(caja.x + 30f, caja.y + 388f, 640f, 68f), error, texto);
        if (SonidoCampus.Boton(new Rect(caja.x + 240f, caja.yMax - 58f, 220f, 40f), "Volver al piso · Esc", boton)) CerrarPrueba();
    }

    void Opcion(Rect caja, int fila, string etiqueta, string mision)
    {
        if (!SonidoCampus.Boton(new Rect(caja.x + 30f, caja.y + 270f + fila * 56f, 640f, 48f), etiqueta, boton)) return;
        ElegirAccion(mision);
    }

    public void ConectarCable(int elegido)
    {
        if (ventana != "energia" || director.panel != PanelCampus.PisoSiete || EstadoPisoSiete.Lista("energia")) return;
        if (elegido == cable) cable++;
        else { SonidoCampus.Emitir("error", .55f); cable = 0; error = "Conexión rechazada. Consulta la etiqueta y empieza por cian."; }
        if (cable > 0) SonidoCampus.Emitir("correcto", .45f);
        if (cable != 3) return;
        Completar("energia");
        EstadoPartida.sistemas++;
        CerrarPrueba();
        Hablar("ENERGÍA RESTAURADA", "Las luces dejan de parpadear. El ascensor reconoce la tarjeta del Proyecto 07.\nVera: «El monitor vuelve a funcionar. Ven a la clínica, arriba a la izquierda».\n\nMisión completada. +1 Sistemas.");
    }

    public void CruzarPortal()
    {
        if (ventana != "final" || director.panel != PanelCampus.PisoSiete || !EstadoPisoSiete.PortalListo) return;
        EstadoPartida.pisoCruzado = true;
        SonidoCampus.Emitir("portal", .8f);
        director.panel = PanelCampus.Fin;
        jugador.GetComponent<JugadorCampus>().Detener();
    }

    public void ElegirAccion(string mision)
    {
        if (ventana != mision || director.panel != PanelCampus.PisoSiete || EstadoPisoSiete.Lista(mision)) return;
        if (mision != "clinica" && mision != "antidoto" && mision != "transmision" && mision != "estructura") return;
        Completar(mision);
        CerrarPrueba();
        switch (mision)
        {
            case "clinica":
                EstadoPisoSiete.vida = 100;
                EstadoPisoSiete.objetos.Add("reactivo");
                EstadoPartida.medicina++;
                Hablar("PACIENTE ESTABLE", "Vera: «Lo logramos. Lleva estos guantes, el recipiente y el reactivo al núcleo. Recupera también la tarjeta del archivo».\n\nRecuperas la vida. La clínica queda disponible como punto de curación. +1 Medicina.");
                break;
            case "antidoto":
                EstadoPisoSiete.objetos.Add("antidoto");
                EstadoPartida.medicina++;
                Hablar("CONTENCIÓN ACTIVADA", "El neutralizante entra en la ventilación. Las criaturas pierden velocidad y su radio de detección disminuye.\n\nVera: «Ya podemos transmitir desde streaming, abajo a la izquierda». +1 Medicina.");
                break;
            case "transmision":
                Hablar("SEÑAL ENVIADA", "«Aquí equipo de rescate. Recibimos la evidencia y su ubicación. Refuercen la salida antes de cruzar».\n\nEl registro está a salvo fuera del edificio. Falta instalar los anclajes en ingeniería.");
                break;
            case "estructura":
                EstadoPartida.sistemas++;
                Hablar("SALIDA ESTABILIZADA", "Los anclajes detienen la vibración del núcleo. Las seis tareas están registradas.\n\nVera: «Lleva la tarjeta al círculo central. Es hora de volver». +1 Sistemas.");
                break;
        }
    }

    string Inventario()
    {
        var nombres = new List<string>();
        string[] ids = { "fusible", "refuerzo", "tarjeta", "registro", "reactivo", "muestra", "antidoto", "aturdidor", "botiquin", "escudo" };
        string[] etiquetas = { "Fusible", "Kit de anclajes", "Tarjeta 07", "Registro 07", "Reactivo", "Muestra sellada", "Antídoto", "Aturdidor", "Botiquín (H)", "Escudo" };
        for (int i = 0; i < ids.Length; i++) if (EstadoPisoSiete.Tiene(ids[i])) nombres.Add(etiquetas[i]);
        return nombres.Count == 0 ? "Vacío" : string.Join(" · ", nombres);
    }

    void DibujarMapa(Rect zona, bool nombres)
    {
        if (plano == null) return;
        GUI.DrawTexture(zona, plano, ScaleMode.StretchToFill);
        Color previa = GUI.color;
        foreach (var punto in puntos)
        {
            if (!marcas[punto.id].enabled) continue;
            Vector2 uv = new Vector2(punto.posicion.x * 48f / 1672f, 1f - punto.posicion.y * 48f / 941f);
            GUI.color = marcas[punto.id].color;
            GUI.DrawTexture(new Rect(zona.x + uv.x * zona.width - 3f, zona.y + uv.y * zona.height - 3f, 6f, 6f), Texture2D.whiteTexture);
            GUI.color = previa;
            if (nombres)
            {
                if (punto.id == "fusible") continue;
                string corto = punto.id == "ascensor7" ? "ASCENSOR" : punto.id == "energia7" ? "ENERGÍA" : punto.id == "clinica7" ? "CLÍNICA" :
                    punto.id == "archivo7" ? "ARCHIVO" : punto.id == "laboratorio7" ? "LABORATORIO" : punto.id == "radio7" ? "STREAMING" :
                    punto.id == "estructura7" ? "INGENIERÍA" : punto.id == "recursos7" ? "SUMINISTROS" : punto.id == "nucleo7" ? "NÚCLEO" : punto.id.ToUpperInvariant();
                float desplazar = punto.id == "nucleo7" ? -28f : 6f;
                GUI.Box(new Rect(zona.x + uv.x * zona.width - 55f, zona.y + uv.y * zona.height + desplazar, 110f, 22f), corto);
            }
        }
        Vector2 pos = jugador.position;
        GUI.color = new Color(1f, 0.5f, 0.05f);
        GUI.DrawTexture(new Rect(zona.x + pos.x * 48f / 1672f * zona.width - 4f, zona.y + (1f - pos.y * 48f / 941f) * zona.height - 4f, 8f, 8f), Texture2D.whiteTexture);
        GUI.color = previa;
    }

    void CerrarPrueba() { ventana = ""; director.CerrarPanel(); }

    void OnDestroy()
    {
        if (jugadorDibujo != null) jugadorDibujo.color = colorJugador;
        foreach (var sprite in sprites) if (sprite != null) Destroy(sprite);
        foreach (var tex in texturas.Values) if (tex != null) Destroy(tex);
    }
}
