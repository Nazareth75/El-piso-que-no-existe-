using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public enum PanelCampus
{
    Ninguno,
    Menu,
    Intro,
    Tutorial,
    Frase,
    Clinica,
    Ascensor,
    Game,
    Pausa,
    Muerte,
    Fin
}

public class PuntoCampus
{
    public string id;
    public string nombre;
    public Vector2 posicion;
    public float radio = 1.35f;
}

public class DirectorCampus : MonoBehaviour
{
    public static DirectorCampus Instancia { get; private set; }

    public PanelCampus panel = PanelCampus.Menu;
    public bool jugando;
    public float estamina = 100f;
    public string fraseTitulo = "";
    public string fraseCuerpo = "";
    public string aviso = "";
    public string grito = "";
    public int aciertos;
    public string flecha = "ARRIBA";

    public Texture2D ascensor;
    public Texture2D pisoX;

    readonly Dictionary<string, Texture2D> tarjetas = new Dictionary<string, Texture2D>();
    readonly List<PuntoCampus> puntos = new List<PuntoCampus>();
    readonly List<PuertaVisual> puertas = new List<PuertaVisual>();

    JugadorCampus jugador;
    GameObject mapaGo;
    GameObject murosGo;
    GameObject npcsGo;
    GameObject interiorGo;
    GameObject marcoGo;
    bool dentro;
    string salaId = "";
    float salaAncho = 32f;
    float salaAlto = 18f;
    float puedeSalir;
    Vector2 marcaSala;
    Vector3 retornoSala;
    static readonly Vector2 Nacimiento = new Vector2(33.2f, 1.9f);
    PerseguidorCampus presencia;
    SpriteRenderer zonaVerde;
    AudioSource fuente;
    AudioClip tonoBajo;
    AudioClip tonoAlto;
    BoxCollider2D muroHuella;
    BoxCollider2D puertaEntrada;
    Camera camara;
    SpriteRenderer[] capasMapa;
    bool cerrarDespierta;
    Vector2 portalPiso;
    float gritoHasta;
    KeyCode teclaJuego = KeyCode.UpArrow;
    bool avisoPisoX;

    public bool bloqueado => panel != PanelCampus.Ninguno;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Asegurar()
    {
        if (FindAnyObjectByType<DirectorCampus>() != null) return;
        var go = new GameObject("Director");
        go.AddComponent<DirectorCampus>();
        go.AddComponent<InterfazCampus>();
    }

    void Awake()
    {
        if (Instancia != null && Instancia != this)
        {
            Destroy(gameObject);
            return;
        }

        Instancia = this;
        if (GetComponent<InterfazCampus>() == null) gameObject.AddComponent<InterfazCampus>();
    }

    void Start()
    {
        fuente = gameObject.AddComponent<AudioSource>();
        camara = Camera.main;
        if (camara == null)
        {
            var camaraGo = new GameObject("Main Camera");
            camaraGo.tag = "MainCamera";
            camara = camaraGo.AddComponent<Camera>();
            camaraGo.AddComponent<AudioListener>();
        }

        camara.orthographic = true;
        camara.clearFlags = CameraClearFlags.SolidColor;
        camara.backgroundColor = new Color(0.04f, 0.1f, 0.07f);
        CargarArte();
        ConstruirMapa();
        ConstruirJugador();
        ConstruirNpcs();
        ConstruirPuntos();
        ConstruirPuertas();
        ConstruirPresencia();
        ConstruirZona();
        AplicarMundo();
        panel = PanelCampus.Menu;
        jugando = false;
        int frames = jugador != null && jugador.frames != null ? jugador.frames.Length : 0;
        Debug.Log("Campus listo. Personaje " + frames + " fotogramas. Puntos " + puntos.Count + ". Puertas " + puertas.Count);
    }

    void Update()
    {
        if (jugador == null) return;

        if (dentro && salaId != "pisox" && salaId != "mezanine" && jugando && panel == PanelCampus.Ninguno && jugador.transform.position.y < 1.7f && Time.time > puedeSalir)
            SalirSala();

        if (jugando && panel == PanelCampus.Ninguno && Input.GetKeyDown(KeyCode.E))
        {
            var cerca = Cercano();
            if (cerca != null)
            {
                if (cerca.id == "gradasMez") SubirDesdeMezanine();
                else Usar(cerca.id);
            }
        }

        if (jugando && panel == PanelCampus.Ninguno && Input.GetKeyDown(KeyCode.Q))
        {
            var cerca = Cercano();
            if (cerca != null && cerca.id == "gradasMez")
                SalirSala();
        }

        if (jugando && (panel == PanelCampus.Ninguno || panel == PanelCampus.Pausa) && Input.GetKeyDown(KeyCode.Escape))
        {
            panel = panel == PanelCampus.Pausa ? PanelCampus.Ninguno : PanelCampus.Pausa;
        }

        if (panel == PanelCampus.Clinica)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1)) Ayudar();
            if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2)) Seguir();
        }

        if (panel == PanelCampus.Game && Input.anyKeyDown)
        {
            RevisarMinijuego();
        }

        if (EstadoPartida.huidaActiva && presencia != null && presencia.persigue && panel == PanelCampus.Ninguno)
        {
            float distancia = Vector2.Distance(presencia.transform.position, jugador.transform.position);
            if (distancia < 0.72f) Morir();
            else if (zonaVerde != null && Vector2.Distance(jugador.transform.position, zonaVerde.transform.position) < 0.95f)
                Escapar();
        }
    }

    void LateUpdate()
    {
        if (camara == null) return;
        if (!jugando || jugador == null)
        {
            camara.transform.position = new Vector3(20f, 10f, -10f);
            camara.orthographicSize = 10.4f;
            return;
        }

        Vector3 deseada = jugador.transform.position;
        deseada.z = -10f;
        camara.transform.position = Vector3.Lerp(camara.transform.position, deseada, 1f - Mathf.Exp(-8f * Time.deltaTime));
        camara.orthographicSize = Mathf.Lerp(camara.orthographicSize, 4.5f, 1f - Mathf.Exp(-6f * Time.deltaTime));

        float topeX = dentro ? salaAncho : 40f;
        float topeY = dentro ? salaAlto : 20f;
        float vertical = camara.orthographicSize;
        float horizontal = vertical * camara.aspect;
        float x = camara.transform.position.x;
        float y = camara.transform.position.y;
        if (horizontal * 2f < topeX) x = Mathf.Clamp(x, horizontal, topeX - horizontal);
        if (vertical * 2f < topeY) y = Mathf.Clamp(y, vertical, topeY - vertical);
        camara.transform.position = new Vector3(x, y, -10f);
    }

    public PuntoCampus Cercano()
    {
        if (jugador == null) return null;
        if (dentro)
        {
            Vector2 aqui = jugador.transform.position;
            if (salaId == "pisox")
            {
                if (Vector2.Distance(aqui, portalPiso) <= 1.8f)
                    return new PuntoCampus { id = "cruzar", nombre = "Cruzar el círculo", posicion = portalPiso, radio = 1.8f };
                if (aqui.x < 3.2f)
                    return new PuntoCampus { id = "volver", nombre = "Volver al campus", posicion = aqui, radio = 3f };
                return null;
            }

            if (salaId == "mezanine")
            {
                var grada = new Vector2(10.94f, 14.15f);
                if (Vector2.Distance(aqui, grada) <= 2.4f)
                    return new PuntoCampus { id = "gradasMez", nombre = "gradas", posicion = grada, radio = 2.4f };
                return null;
            }

            float alcance = salaId == "admisiones" ? 2.4f : 1.45f;
            if (Vector2.Distance(aqui, marcaSala) <= alcance)
                return new PuntoCampus { id = salaId, nombre = NombreSala(salaId), posicion = marcaSala, radio = alcance };
            if (aqui.y < 2.4f)
                return new PuntoCampus { id = "salir", nombre = "Salir al pasillo", posicion = aqui, radio = 3f };
            return null;
        }

        Vector2 pos = jugador.transform.position;
        PuntoCampus mejor = null;
        float mejorDistancia = 999f;
        foreach (var punto in puntos)
        {
            float distancia = Vector2.Distance(pos, punto.posicion);
            if (distancia <= punto.radio && distancia < mejorDistancia)
            {
                mejor = punto;
                mejorDistancia = distancia;
            }
        }

        return mejor;
    }

    public Texture2D Tarjeta(string id)
    {
        tarjetas.TryGetValue(id, out var tex);
        return tex;
    }

    public bool GritoVisible => Time.time < gritoHasta && !string.IsNullOrEmpty(grito);

    public void NuevaPartida()
    {
        EstadoPartida.Reiniciar();
        estamina = 100f;
        aviso = "";
        grito = "";
        AplicarMundo();
        panel = PanelCampus.Intro;
        jugando = false;
    }

    public void ElegirPersonaje(int indice)
    {
        EstadoPartida.personaje = indice == 0 ? 0 : 1;
        AplicarAspecto();
    }

    public void ContinuarIntro()
    {
        panel = PanelCampus.Tutorial;
        jugando = true;
        if (jugador != null) jugador.Detener();
    }

    public void Entendido()
    {
        jugando = true;
        Decir("CÓMO MOVERTE", "Ves el campus desde arriba, como el plano.\nFlechas o W A S D: caminar\nShift: correr. La barra de aire se gasta.\nE: hablar, leer y usar la huella\nEn la clínica: 1 ayudar, 2 seguir de largo\nEsc: pausa\nSi la presencia te alcanza, repites solo esa huida.", "");
    }

    public void CerrarPanel()
    {
        if (panel == PanelCampus.Fin || panel == PanelCampus.Muerte || panel == PanelCampus.Menu || panel == PanelCampus.Intro || panel == PanelCampus.Tutorial)
            return;
        bool lanzarHuida = confirmarHuida && panel == PanelCampus.Frase;
        bool despertar = cerrarDespierta && panel == PanelCampus.Frase;
        confirmarHuida = false;
        cerrarDespierta = false;
        panel = PanelCampus.Ninguno;
        aviso = "";
        if (despertar)
        {
            Despertar();
            return;
        }

        if (lanzarHuida)
        {
            ColocarHuida();
            Gritar("Algo baja por las escaleras. Corre a la zona verde.");
        }
    }

    public void VolverAlMenu()
    {
        EstadoPartida.Reiniciar();
        estamina = 100f;
        AplicarMundo();
        panel = PanelCampus.Menu;
        jugando = false;
    }

    public void Reintentar()
    {
        estamina = 100f;
        aviso = "";
        panel = PanelCampus.Ninguno;
        jugando = true;
        if (EstadoPartida.enPisoX)
        {
            EntrarPisoX();
            Gritar("Otra vez. Llega al círculo del centro.");
            return;
        }

        ColocarHuida();
        Gritar("Otra vez. Corre a la zona verde.");
    }

    public void AlcanzadoEnPiso()
    {
        if (!EstadoPartida.enPisoX || panel != PanelCampus.Ninguno) return;
        panel = PanelCampus.Muerte;
        if (jugador != null) jugador.Detener();
    }

    public void EntrarAlPiso()
    {
        if (!EstadoPartida.desperto || !EstadoPartida.sistemasListo) return;
        panel = PanelCampus.Ninguno;
        EntrarPisoX();
    }

    public void Ayudar()
    {
        if (!EstadoPartida.desperto || EstadoPartida.clinica) return;
        EstadoPartida.clinica = true;
        EstadoPartida.ayudo = true;
        EstadoPartida.medicina += 1;
        Decir("CLÍNICA", "Ayudaste con el tobillo. Medicina suma un punto. Seguir de largo no habría cambiado nada.", "clinica");
    }

    public void Seguir()
    {
        if (!EstadoPartida.desperto || EstadoPartida.clinica) return;
        EstadoPartida.clinica = true;
        EstadoPartida.ayudo = false;
        Decir("CLÍNICA", "Seguiste de largo. Medicina no cambia.", "clinica");
    }

    public void ElegirPiso(string piso)
    {
        if (!EstadoPartida.desperto) return;
        if (piso == "7")
        {
            if (!EstadoPartida.pista)
            {
                aviso = "Primero lee la nota que quedó en recepción.";
                return;
            }

            if (EstadoPartida.sistemas == 0)
            {
                EstadoPartida.sistemas = 1;
                EstadoPartida.sistemasListo = true;
            }

            avisoPisoX = true;
            aviso = "El siete no está. Aparece PISO X. El color de ese piso no es el de la universidad.";
            return;
        }

        avisoPisoX = EstadoPartida.sistemasListo;
        aviso = "Ese piso existe. El que buscas no tiene botón.";
    }

    public bool MostrarPisoX => avisoPisoX || EstadoPartida.sistemasListo;

    void Usar(string id)
    {
        if (id == "salir")
        {
            SalirSala();
            return;
        }

        if (id == "cruzar")
        {
            EstadoPartida.pisoCruzado = true;
            panel = PanelCampus.Fin;
            if (jugador != null) jugador.Detener();
            return;
        }

        if (id == "volver")
        {
            SalirPisoX();
            return;
        }

        if (id == "gradas")
        {
            SubirMezanine();
            return;
        }

        if (id == "bajar")
        {
            SalirSala();
            return;
        }

        if (!dentro && !EstadoPartida.huidaActiva && !EstadoPartida.desperto && TieneInterior(id))
        {
            EntrarSala(id);
            return;
        }

        switch (id)
        {
            case "entrada":
                EstadoPartida.entrada = true;
                if (puertaEntrada != null) puertaEntrada.enabled = false;
                Decir("ENTRADA", "La estudiante te recibe.\n\nTe muestro la universidad: recepción está al frente, ahí pides información. Siguiendo el pasillo están admisiones, la clínica odontológica y gestión financiera. Del otro lado quedan el auditorio, servicios estudiantiles, la cafetería y la game room. Al fondo están el ascensor y las escaleras.", "entrada");
                break;
            case "recepcion":
                EstadoPartida.entrada = true;
                if (!EstadoPartida.desperto)
                {
                    EstadoPartida.nota = true;
                    Decir("RECEPCIÓN", "La recepcionista te atiende.\n\nAquí pides información: horarios, aulas y el plano del primer día. Sigue al hall cuando termines.", "recepcion");
                    break;
                }

                if (!EstadoPartida.pista)
                {
                    EstadoPartida.pista = true;
                    StartCoroutine(Campanita());
                    Decir("RECEPCIÓN", "No hay nadie. En el mostrador quedó una nota.\n\nSi encontraste esto, no intentes salir todavía. El Proyecto 07 habla de un piso que no existe.", "recepcion");
                    break;
                }

                Decir("RECEPCIÓN", "La nota sigue ahí. El Proyecto 07 nombra un piso que no debería existir.", "recepcion");
                break;
            case "hall":
                EstadoPartida.entrada = true;
                if (!EstadoPartida.desperto)
                {
                    EstadoPartida.hall = true;
                    Decir("HALL", "La estudiante sigue el recorrido.\n\nEste es el hall. Por aquí se llega a recepción y, al otro lado, al auditorio, la cafetería y las escaleras.", "hall");
                    break;
                }

                Decir("HALL", "El hall está vacío. No queda nadie del recorrido. El ascensor no nombra todos los pisos.", "hall");
                break;
            case "clinica":
                if (!EstadoPartida.desperto)
                {
                    Decir("CLÍNICA", "La odontóloga te muestra la clínica. Aquí atienden a los estudiantes. Hoy no hay ninguna urgencia.", "clinica");
                    break;
                }

                if (EstadoPartida.clinica)
                {
                    Decir("CLÍNICA", EstadoPartida.ayudo ? "Ya ayudaste. Medicina quedó en " + EstadoPartida.medicina + "." : "Ya seguiste de largo. Medicina no cambió.", "clinica");
                    break;
                }

                panel = PanelCampus.Clinica;
                break;
            case "huella":
                if (!EstadoPartida.Conoce)
                {
                    Decir("CONTROL DE HUELLA", "Primero termina el recorrido: la puerta, recepción y el hall. Después podrás ver el ascensor.", "control_huella");
                    break;
                }

                EstadoPartida.huella = true;
                AbrirPuertas();
                Decir("CONTROL DE HUELLA", EstadoPartida.desperto
                    ? "La huella sigue abierta. Del otro lado no hay nadie."
                    : "Huella del primer día. Las puertas se abren. El ascensor está al fondo.", "control_huella");
                break;
            case "ascensor":
                if (!EstadoPartida.Conoce)
                {
                    Decir("ASCENSOR", "Primero conoce la universidad. La estudiante te guía en la entrada, en recepción y en el hall.", "primera_vez");
                    break;
                }

                if (!EstadoPartida.huella)
                {
                    Decir("ASCENSOR", "El ascensor está del otro lado del control de huella.", "primera_vez");
                    break;
                }

                if (!EstadoPartida.desperto)
                {
                    Decir("ASCENSOR", "Ves el ascensor. Están la planta baja, el mezanine y los pisos 1 al 5.\n\nYa conoces la universidad. Al cerrar esto, el día termina.", "primera_vez");
                    cerrarDespierta = true;
                    break;
                }

                aviso = EstadoPartida.sistemasListo ? "PISO X está abierto. Puedes entrar." : "";
                avisoPisoX = EstadoPartida.sistemasListo;
                AbrirAscensor();
                panel = PanelCampus.Ascensor;
                break;
            case "escaleras":
                IntentarHuida();
                break;
            case "cafeteria":
                Decir("CAFETERÍA", EstadoPartida.desperto
                    ? "La cafetería está apagada. No quedó nadie."
                    : "La cafetería sirve durante el día. Forma parte del recorrido.", "cafeteria");
                break;
            case "game":
                if (EstadoPartida.desperto)
                {
                    Decir("GAME ROOM", "Las máquinas están apagadas. No hay nadie.", "game_room");
                    break;
                }

                if (EstadoPartida.gameRoom)
                {
                    Decir("GAME ROOM", "Ya participaste. Esto no cambia tu carrera.", "game_room");
                    break;
                }

                aciertos = 0;
                NuevaFlecha();
                panel = PanelCampus.Game;
                break;
            case "gestion":
                Decir("GESTIÓN FINANCIERA", EstadoPartida.desperto
                    ? "El escritorio está vacío. La colegiatura de este semestre no nombra el Proyecto 07."
                    : "En gestión financiera se paga la colegiatura del semestre.", "gestion_financiera");
                break;
            case "admisiones":
                Decir("ADMISIONES", EstadoPartida.desperto
                    ? "Admisiones quedó cerrado. En un registro abierto se leen tres rumbos: Ingeniería de Sistemas, Medicina y una tercera carrera que sigue por definir."
                    : "El encargado te explica el ingreso. Aquí queda tu registro del primer día. Caminas con las flechas o W A S D, corres con Shift y hablas con E.", "admisiones");
                break;
            case "auditorio":
                Decir("AUDITORIO", EstadoPartida.desperto
                    ? "El auditorio está a oscuras. La exposición de la mañana ya no está."
                    : "En el auditorio hay una exposición para los estudiantes nuevos.", "auditorio");
                break;
            case "servicios":
                Decir("SERVICIOS ESTUDIANTILES", EstadoPartida.desperto
                    ? "Servicios estudiantiles está vacío. No hay quién atienda un trámite."
                    : "En servicios estudiantiles te ayudan con trámites del primer día.", "servicios_estudiantiles");
                break;
        }
    }

    void IntentarHuida()
    {
        if (!EstadoPartida.desperto)
        {
            Decir("ESCALERAS Y BAÑOS", "Los baños están junto a las escaleras. Por ahora solo forman parte del recorrido.", "escaleras_banos");
            return;
        }

        if (!EstadoPartida.DecisionesListas)
        {
            Decir("ESCALERAS Y BAÑOS", "Los baños están junto a las escaleras.\n\nAún no. " + Pendiente(), "escaleras_banos");
            return;
        }

        if (EstadoPartida.huida)
        {
            panel = PanelCampus.Fin;
            return;
        }

        Decir("ESCALERAS Y BAÑOS", "Los baños están junto a las escaleras.\n\nAlgo sube desde abajo. Al cerrar esto apareces al pie de las escaleras. Corre hasta la zona verde. Shift es correr.", "escaleras_banos");
        confirmarHuida = true;
    }

    static string Pendiente()
    {
        if (!EstadoPartida.pista) return "Primero lee la nota de recepción.";
        if (!EstadoPartida.clinica) return "Primero atiende el caso de la clínica.";
        if (!EstadoPartida.huella) return "Primero valida tu huella.";
        if (!EstadoPartida.sistemasListo) return "Primero halla el piso que falta en el ascensor.";
        return "";
    }

    void Decir(string titulo, string cuerpo, string tarjeta)
    {
        confirmarHuida = false;
        fraseTitulo = titulo;
        fraseCuerpo = cuerpo;
        fraseTarjeta = tarjeta;
        panel = PanelCampus.Frase;
    }

    public string fraseTarjeta = "entrada";
    public bool confirmarHuida;

    void Gritar(string texto)
    {
        grito = texto;
        gritoHasta = Time.time + 4f;
    }

    void Morir()
    {
        if (jugador != null) jugador.Detener();
        if (presencia != null) presencia.persigue = false;
        panel = PanelCampus.Muerte;
    }

    void Escapar()
    {
        EstadoPartida.huida = true;
        EstadoPartida.huidaActiva = false;
        if (presencia != null) presencia.persigue = false;
        if (jugador != null) jugador.Detener();
        panel = PanelCampus.Fin;
    }

    void ColocarHuida()
    {
        if (dentro) SalirSala();
        EstadoPartida.huidaActiva = true;
        EstadoPartida.huella = true;
        AbrirPuertas();
        if (zonaVerde != null) zonaVerde.enabled = true;
        Vector2 salida = Celda(4, 14);
        Vector2 acecho = Celda(4, 16);
        if (jugador != null)
        {
            jugador.Detener();
            jugador.transform.position = salida;
            var cuerpo = jugador.GetComponent<Rigidbody2D>();
            if (cuerpo != null) cuerpo.position = salida;
        }

        if (presencia != null)
        {
            presencia.transform.position = acecho;
            presencia.persigue = true;
            presencia.objetivo = jugador.transform;
        }
    }

    void AplicarMundo()
    {
        if (dentro) SalirSala();
        Vector2 spawn = Nacimiento;
        if (jugador != null)
        {
            jugador.Detener();
            jugador.transform.position = spawn;
            var cuerpo = jugador.GetComponent<Rigidbody2D>();
            if (cuerpo != null) cuerpo.position = spawn;
        }

        AplicarAspecto();
        Teñir(EstadoPartida.desperto);
        if (npcsGo != null) npcsGo.SetActive(!EstadoPartida.desperto);
        if (puertaEntrada != null) puertaEntrada.enabled = !EstadoPartida.entrada;
        if (EstadoPartida.huella) AbrirPuertas();
        else CerrarPuertas();

        if (presencia != null) presencia.persigue = false;
        if (zonaVerde != null) zonaVerde.enabled = EstadoPartida.DecisionesListas || EstadoPartida.huidaActiva;
        avisoPisoX = EstadoPartida.sistemasListo;

        if (EstadoPartida.huidaActiva) ColocarHuida();
    }

    void AplicarAspecto()
    {
        if (jugador == null) return;
        string carpeta = EstadoPartida.personaje == 0 ? "estudiante_caminando" : "estudiante_mujer";
        jugador.frames = RecursosCampus.Carpeta(carpeta, 48f);
        var dibujo = jugador.GetComponent<SpriteRenderer>();
        if (dibujo != null && jugador.frames.Length > 0) dibujo.sprite = jugador.frames[0];
    }

    void CargarArte()
    {
        string[] ids =
        {
            "entrada", "recepcion", "hall", "clinica", "control_huella", "cafeteria", "game_room",
            "gestion_financiera", "admisiones", "auditorio", "servicios_estudiantiles", "escaleras_banos",
            "primera_vez"
        };

        foreach (var id in ids)
        {
            var tex = RecursosCampus.Textura("Salas/" + id + "/0.png");
            if (tex != null) tarjetas[id] = tex;
        }

        ascensor = RecursosCampus.Textura("Salas/nivel_1/0.png");
        pisoX = RecursosCampus.Textura("Salas/nivel_2/0.png");
        tonoBajo = Tono(523.25f);
        tonoAlto = Tono(659.25f);
    }

    void PonerLetrero(Texture2D tex, Transform mapa)
    {
        if (tex == null) return;
        int x0 = 768;
        int y0 = 350;
        int ancho = 192;
        int alto = 46;
        int uy = tex.height - (y0 + alto);
        if (uy < 0 || x0 + ancho > tex.width) return;

        var recorte = new Texture2D(ancho, alto, TextureFormat.RGBA32, false);
        recorte.filterMode = FilterMode.Point;
        recorte.SetPixels(tex.GetPixels(x0, uy, ancho, alto));
        recorte.Apply();

        var go = new GameObject("Letrero");
        go.transform.SetParent(mapa, false);
        var dibujo = go.AddComponent<SpriteRenderer>();
        dibujo.sprite = RecursosCampus.ComoSprite(recorte, 25.6f, new Vector2(0.5f, 0.5f));
        dibujo.sortingOrder = 300;
        float localX = (x0 + ancho * 0.5f - tex.width * 0.5f) / 25.6f;
        float localY = (tex.height * 0.5f - (y0 + alto * 0.5f)) / 25.6f;
        go.transform.localPosition = new Vector3(localX, localY, 0f);
    }

    void PonerHojas(Texture2D tex, Transform mapa)
    {
        if (tex == null) return;
        int ancho = tex.width;
        int alto = tex.height;
        var origen = tex.GetPixels32();
        var hoja = new bool[origen.Length];
        for (int i = 0; i < origen.Length; i++)
        {
            var c = origen[i];
            if (EsHoja(c.r, c.g, c.b)) hoja[i] = true;
        }

        var copia = (bool[])hoja.Clone();
        for (int y = 0; y < alto; y++)
        {
            for (int x = 0; x < ancho; x++)
            {
                int i = y * ancho + x;
                if (copia[i]) continue;
                var c = origen[i];
                if (c.r > c.g + 12 && c.b < 175) continue;
                bool cerca = false;
                for (int dy = -2; dy <= 2 && !cerca; dy++)
                {
                    int yy = y + dy;
                    if ((uint)yy >= (uint)alto) continue;
                    for (int dx = -2; dx <= 2; dx++)
                    {
                        int xx = x + dx;
                        if ((uint)xx >= (uint)ancho) continue;
                        if (copia[yy * ancho + xx])
                        {
                            cerca = true;
                            break;
                        }
                    }
                }

                if (cerca && c.g + 8 >= c.r && c.b < 140 && c.g < 130) hoja[i] = true;
            }
        }

        var pixeles = new Color32[origen.Length];
        for (int i = 0; i < origen.Length; i++)
        {
            if (!hoja[i]) continue;
            var c = origen[i];
            c.a = 255;
            pixeles[i] = c;
        }

        var recorte = new Texture2D(ancho, alto, TextureFormat.RGBA32, false);
        recorte.filterMode = FilterMode.Point;
        recorte.SetPixels32(pixeles);
        recorte.Apply();

        var go = new GameObject("Hojas");
        go.transform.SetParent(mapa, false);
        var dibujo = go.AddComponent<SpriteRenderer>();
        dibujo.sprite = RecursosCampus.ComoSprite(recorte, 25.6f, new Vector2(0.5f, 0.5f));
        dibujo.sortingOrder = 200;
    }

    static bool EsHoja(byte r, byte g, byte b)
    {
        if (r > g + 12 && b < 175) return false;
        return g > r + 2 && g + 2 >= b && g > 15;
    }

    void PonerSofas(Texture2D tex, Transform mapa)
    {
        if (tex == null) return;
        int ancho = tex.width;
        int alto = tex.height;
        var origen = tex.GetPixels32();
        int n = origen.Length;
        var nucleo = new bool[n];
        for (int i = 0; i < n; i++)
        {
            var c = origen[i];
            nucleo[i] = EsSofa(c.r, c.g, c.b);
        }

        var visto = new bool[n];
        var pintar = new bool[n];
        var cola = new Queue<int>();
        var miembros = new List<int>(2048);
        var hist = new int[256];
        for (int inicio = 0; inicio < n; inicio++)
        {
            if (!nucleo[inicio] || visto[inicio]) continue;
            cola.Clear();
            miembros.Clear();
            System.Array.Clear(hist, 0, 256);
            cola.Enqueue(inicio);
            visto[inicio] = true;
            int x0 = ancho, y0 = alto, x1 = 0, y1 = 0;
            while (cola.Count > 0)
            {
                int i = cola.Dequeue();
                miembros.Add(i);
                int x = i % ancho;
                int y = i / ancho;
                hist[origen[i].r]++;
                if (x < x0) x0 = x;
                if (y < y0) y0 = y;
                if (x > x1) x1 = x;
                if (y > y1) y1 = y;
                for (int dy = -2; dy <= 2; dy++)
                {
                    int yy = y + dy;
                    if ((uint)yy >= (uint)alto) continue;
                    for (int dx = -2; dx <= 2; dx++)
                    {
                        int xx = x + dx;
                        if ((uint)xx >= (uint)ancho) continue;
                        int j = yy * ancho + xx;
                        if (visto[j] || !nucleo[j]) continue;
                        visto[j] = true;
                        cola.Enqueue(j);
                    }
                }
            }

            int bw = x1 - x0 + 1;
            int bh = y1 - y0 + 1;
            if (miembros.Count < 800 || bw < 50 || bw > 100 || bh < 24 || bh > 70) continue;
            int mitad = miembros.Count / 2;
            int acum = 0;
            int mediana = 255;
            for (int r = 0; r < 256; r++)
            {
                acum += hist[r];
                if (acum > mitad)
                {
                    mediana = r;
                    break;
                }
            }

            if (mediana >= 75) continue;
            for (int k = 0; k < miembros.Count; k++) pintar[miembros[k]] = true;
        }

        var copia = (bool[])pintar.Clone();
        for (int y = 0; y < alto; y++)
        {
            for (int x = 0; x < ancho; x++)
            {
                if (!copia[y * ancho + x]) continue;
                for (int dy = -2; dy <= 2; dy++)
                {
                    int yy = y + dy;
                    if ((uint)yy >= (uint)alto) continue;
                    for (int dx = -2; dx <= 2; dx++)
                    {
                        int xx = x + dx;
                        if ((uint)xx >= (uint)ancho) continue;
                        int j = yy * ancho + xx;
                        if (pintar[j]) continue;
                        var c = origen[j];
                        if (EsContornoSofa(c.r, c.g, c.b)) pintar[j] = true;
                    }
                }
            }
        }

        var pixeles = new Color32[n];
        for (int i = 0; i < n; i++)
        {
            if (!pintar[i]) continue;
            var c = origen[i];
            c.a = 255;
            pixeles[i] = c;
        }

        var recorte = new Texture2D(ancho, alto, TextureFormat.RGBA32, false);
        recorte.filterMode = FilterMode.Point;
        recorte.SetPixels32(pixeles);
        recorte.Apply();

        var go = new GameObject("Sofas");
        go.transform.SetParent(mapa, false);
        var dibujo = go.AddComponent<SpriteRenderer>();
        dibujo.sprite = RecursosCampus.ComoSprite(recorte, 25.6f, new Vector2(0.5f, 0.5f));
        dibujo.sortingOrder = 190;
    }

    static bool EsSofa(byte r, byte g, byte b)
    {
        if (g > 155 || r > 110) return false;
        return b > 148 && b > r + 70 && b > g + 45;
    }

    static bool EsContornoSofa(byte r, byte g, byte b)
    {
        if (g > r + 12 && g > 70) return false;
        if (r > 150 && g > 100 && b < 160) return false;
        if (r > 110 && g > 110 && Mathf.Abs(r - g) < 28) return false;
        return r + g + b < 170;
    }

    void ConstruirMapa()
    {
        var tex = RecursosCampus.Textura("Campus/mapa.png");
        var mapa = new GameObject("Mapa");
        var dibujo = mapa.AddComponent<SpriteRenderer>();
        dibujo.sprite = RecursosCampus.ComoSprite(tex, 25.6f, new Vector2(0.5f, 0.5f));
        dibujo.sortingOrder = -20;
        mapa.transform.position = new Vector3(20f, 10f, 0f);
        if (dibujo.sprite != null)
        {
            float alto = dibujo.sprite.bounds.size.y;
            if (alto > 0.01f) mapa.transform.localScale = new Vector3(1f, 20f / alto, 1f);
        }

        mapaGo = mapa;
        PonerLetrero(tex, mapa.transform);
        PonerHojas(tex, mapa.transform);
        PonerSofas(tex, mapa.transform);
        capasMapa = mapa.GetComponentsInChildren<SpriteRenderer>();
        murosGo = new GameObject("Muros");
        string ruta = Path.Combine(Application.streamingAssetsPath, "Campus/colision.txt");
        if (!File.Exists(ruta)) return;
        var lineas = File.ReadAllLines(ruta);
        if (lineas.Length < 2) return;
        var medida = lineas[0].Split(' ');
        float pixelesAncho = float.Parse(medida[0]);
        float pixelesAlto = float.Parse(medida[1]);
        for (int i = 1; i < lineas.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lineas[i])) continue;
            var parte = lineas[i].Split(' ');
            if (parte.Length < 4) continue;
            float x = float.Parse(parte[0]);
            float y = float.Parse(parte[1]);
            float w = float.Parse(parte[2]);
            float h = float.Parse(parte[3]);
            var muro = new GameObject("Muro");
            muro.transform.SetParent(murosGo.transform);
            float cx = (x + w * 0.5f) / pixelesAncho * 40f;
            float cy = (pixelesAlto - (y + h * 0.5f)) / pixelesAlto * 20f;
            muro.transform.position = new Vector3(cx, cy, 0f);
            var caja = muro.AddComponent<BoxCollider2D>();
            caja.size = new Vector2(w / pixelesAncho * 40f, h / pixelesAlto * 20f);
        }
    }

    void ConstruirJugador()
    {
        var go = new GameObject("Jugador");
        var dibujo = go.AddComponent<SpriteRenderer>();
        dibujo.sortingOrder = 100;
        var cuerpo = go.AddComponent<Rigidbody2D>();
        cuerpo.gravityScale = 0f;
        cuerpo.constraints = RigidbodyConstraints2D.FreezeRotation;
        cuerpo.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        cuerpo.interpolation = RigidbodyInterpolation2D.Interpolate;
        var circulo = go.AddComponent<CircleCollider2D>();
        circulo.radius = 0.28f;
        circulo.offset = new Vector2(0f, 0.32f);
        jugador = go.AddComponent<JugadorCampus>();
        go.transform.position = Nacimiento;
    }

    void ConstruirNpcs()
    {
        npcsGo = new GameObject("Npcs");
        CrearNpc("recepcionista", new Vector2(36.33f, 12.6f), false);
        CrearNpc("odontologa", new Vector2(28.91f, 12.6f), false);
        CrearNpc("gestion_financiera", new Vector2(23.44f, 12.6f), false);
        CrearNpc("admisiones", new Vector2(32.66f, 11.73f), false);
        CrearNpc("servicios_estudiantiles", Celda(12, 3), false);
        CrearNpc("cafeteria", Celda(12, 16), false);
        CrearNpc("game_room", Celda(4, 16), false);
        CrearNpc("estudiante_sentado", Celda(11, 13), false);
        CrearNpc("estudiante_mujer", new Vector2(32.5f, 7.05f), false);
        CrearNpc("expositor", Celda(4, 4), true, Celda(3, 5), Celda(6, 5), Celda(6, 2), Celda(3, 2));
        CrearNpc("estudiante_caminando", Celda(32, 16), true, Celda(30, 16), Celda(36, 16), Celda(36, 12), Celda(30, 12), Celda(30, 5), Celda(34, 5));
        CrearNpc("estudiante_mujer", Celda(28, 4), true, Celda(28, 4), Celda(33, 4), Celda(33, 9), Celda(28, 9));
        CrearNpc("estudiante_caminando", Celda(4, 16), true, Celda(4, 16), Celda(6, 16), Celda(6, 10), Celda(4, 10), Celda(4, 4), Celda(6, 4));
    }

    void CrearNpc(string carpeta, Vector2 inicio, bool camina, params Vector2[] ruta)
    {
        var go = new GameObject(carpeta);
        if (npcsGo != null) go.transform.SetParent(npcsGo.transform);
        go.transform.position = inicio;
        var dibujo = go.AddComponent<SpriteRenderer>();
        var npc = go.AddComponent<NpcCampus>();
        npc.frames = RecursosCampus.Carpeta(carpeta, 48f);
        npc.camina = camina && ruta.Length > 1;
        npc.ruta = ruta;
        if (npc.frames.Length > 0) dibujo.sprite = npc.frames[0];
    }

    void ConstruirPuntos()
    {
        Punto("entrada", "Entrada", new Vector2(33.75f, 2.35f), 1.3f);
        Punto("recepcion", "Recepción", new Vector2(36.33f, 12.6f), 2.6f);
        Punto("clinica", "Clínica", new Vector2(28.91f, 12.6f), 2.6f);
        Punto("gestion", "Gestión financiera", new Vector2(23.44f, 12.6f), 2.6f);
        Punto("admisiones", "Admisiones", new Vector2(32.66f, 11.73f), 2.6f);
        Punto("hall", "Hall", new Vector2(32.5f, 7.05f), 3.4f);
        Punto("huella", "Control de huella", new Vector2(21.15f, 15.24f), 1.8f);
        Punto("huella", "Control de huella", new Vector2(21.15f, 12.87f), 1.8f);
        Punto("huella", "Control de huella", new Vector2(21.15f, 9.8f), 1.8f);
        Punto("huella", "Control de huella", new Vector2(21.15f, 6.65f), 1.8f);
        Punto("huella", "Control de huella", new Vector2(21.15f, 4.57f), 1.8f);
        Punto("ascensor", "Ascensor", Celda(15, 2), 1.45f);
        Punto("gradas", "Subir", new Vector2(14.38f, 16.1f), 1.7f);
        Punto("servicios", "Servicios estudiantiles", Celda(12, 3), 1.35f);
        Punto("auditorio", "Auditorio", Celda(4, 4), 1.5f);
        Punto("cafeteria", "Cafetería", Celda(12, 16), 1.4f);
        Punto("game", "Game room", Celda(4, 16), 1.45f);
        Punto("escaleras", "Escaleras y baños", Celda(4, 3), 1.6f);
    }

    void Punto(string id, string nombre, Vector2 posicion, float radio)
    {
        puntos.Add(new PuntoCampus { id = id, nombre = nombre, posicion = posicion, radio = radio });
    }

    void ConstruirPuertas()
    {
        var entrada = new GameObject("PuertaEntrada");
        if (murosGo != null) entrada.transform.SetParent(murosGo.transform);
        entrada.transform.position = new Vector3(33.83f, 4.65f, 0f);
        puertaEntrada = entrada.AddComponent<BoxCollider2D>();
        puertaEntrada.size = new Vector2(3.28f, 3.15f);

        var muro = new GameObject("MuroHuella");
        if (murosGo != null) muro.transform.SetParent(murosGo.transform);
        muro.transform.position = new Vector3(17.5f, 10f, 0f);
        muroHuella = muro.AddComponent<BoxCollider2D>();
        muroHuella.size = new Vector2(1.88f, 22f);
        muroHuella.enabled = false;
    }

    void PuertaEn(Vector2 posicion, Sprite[] frames)
    {
        var go = new GameObject("Puerta");
        if (murosGo != null) go.transform.SetParent(murosGo.transform);
        go.transform.position = posicion;
        var dibujo = go.AddComponent<SpriteRenderer>();
        dibujo.sortingOrder = 60 - Mathf.RoundToInt(posicion.y * 3f);
        var visual = go.AddComponent<PuertaVisual>();
        visual.frames = frames;
        visual.dibujo = dibujo;
        if (frames.Length > 0) dibujo.sprite = frames[0];
        puertas.Add(visual);
    }

    void AbrirPuertas()
    {
        if (muroHuella != null) muroHuella.enabled = false;
        foreach (var puerta in puertas) puerta.Abrir();
    }

    void CerrarPuertas()
    {
        if (muroHuella != null) muroHuella.enabled = false;
        foreach (var puerta in puertas) puerta.Cerrar();
    }

    void AbrirAscensor()
    {
        if (puertas.Count > 0) puertas[puertas.Count - 1].Abrir();
    }

    void ConstruirPresencia()
    {
        var go = new GameObject("Presencia");
        var dibujo = go.AddComponent<SpriteRenderer>();
        dibujo.sortingOrder = 140;
        presencia = go.AddComponent<PerseguidorCampus>();
        presencia.frames = RecursosCampus.Carpeta("estudiante_caminando", 48f);
        if (presencia.frames.Length > 0) dibujo.sprite = presencia.frames[0];
        go.transform.position = Celda(4, 16);
    }

    void ConstruirZona()
    {
        var tex = new Texture2D(32, 32, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        var pixeles = new Color[32 * 32];
        for (int y = 0; y < 32; y++)
        {
            for (int x = 0; x < 32; x++)
            {
                float dx = (x - 15.5f) / 16f;
                float dy = (y - 15.5f) / 16f;
                float dentro = dx * dx + dy * dy <= 1f ? 0.55f : 0f;
                pixeles[y * 32 + x] = new Color(0.25f, 0.95f, 0.4f, dentro);
            }
        }

        tex.SetPixels(pixeles);
        tex.Apply();
        var go = new GameObject("ZonaVerde");
        go.transform.position = Celda(3, 1);
        go.transform.localScale = new Vector3(1.8f, 1.8f, 1f);
        zonaVerde = go.AddComponent<SpriteRenderer>();
        zonaVerde.sprite = RecursosCampus.ComoSprite(tex, 32f, new Vector2(0.5f, 0.5f));
        zonaVerde.sortingOrder = -5;
        zonaVerde.enabled = false;
    }

    void NuevaFlecha()
    {
        int valor = Random.Range(0, 4);
        if (valor == 0) { teclaJuego = KeyCode.UpArrow; flecha = "ARRIBA"; }
        else if (valor == 1) { teclaJuego = KeyCode.DownArrow; flecha = "ABAJO"; }
        else if (valor == 2) { teclaJuego = KeyCode.LeftArrow; flecha = "IZQUIERDA"; }
        else { teclaJuego = KeyCode.RightArrow; flecha = "DERECHA"; }
    }

    void RevisarMinijuego()
    {
        bool bien = Input.GetKeyDown(teclaJuego) || TeclaEquivalente();
        bool flecha = Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow) ||
                      Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow) ||
                      Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A) ||
                      Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D);
        if (!flecha) return;
        if (!bien)
        {
            aciertos = 0;
            NuevaFlecha();
            return;
        }

        aciertos++;
        if (aciertos >= 3)
        {
            EstadoPartida.gameRoom = true;
            Decir("GAME ROOM", "Completaste el minijuego. No cambia tu carrera.", "game_room");
            return;
        }

        NuevaFlecha();
    }

    bool TeclaEquivalente()
    {
        if (teclaJuego == KeyCode.UpArrow) return Input.GetKeyDown(KeyCode.W);
        if (teclaJuego == KeyCode.DownArrow) return Input.GetKeyDown(KeyCode.S);
        if (teclaJuego == KeyCode.LeftArrow) return Input.GetKeyDown(KeyCode.A);
        if (teclaJuego == KeyCode.RightArrow) return Input.GetKeyDown(KeyCode.D);
        return false;
    }

    IEnumerator Campanita()
    {
        if (fuente == null || tonoBajo == null) yield break;
        fuente.PlayOneShot(tonoBajo, 0.35f);
        yield return new WaitForSeconds(0.28f);
        if (tonoAlto != null) fuente.PlayOneShot(tonoAlto, 0.35f);
    }

    static AudioClip Tono(float frecuencia)
    {
        int muestrasPorSegundo = 44100;
        float duracion = 0.28f;
        int cantidad = Mathf.CeilToInt(muestrasPorSegundo * duracion);
        var datos = new float[cantidad];
        for (int i = 0; i < cantidad; i++)
        {
            float tiempo = i / (float)muestrasPorSegundo;
            float envolvente = Mathf.Clamp01(tiempo / 0.02f) * Mathf.Clamp01((duracion - tiempo) / 0.08f);
            datos[i] = Mathf.Sin(2f * Mathf.PI * frecuencia * tiempo) * envolvente * 0.45f;
        }

        var clip = AudioClip.Create("tono", cantidad, 1, muestrasPorSegundo, false);
        clip.SetData(datos, 0);
        return clip;
    }

    static bool TieneInterior(string id)
    {
        return id == "auditorio" || id == "game" || id == "cafeteria" || id == "admisiones" || id == "servicios" || id == "clinica";
    }

    static string ArchivoSala(string id)
    {
        if (id == "game") return "game_room.png";
        if (id == "servicios") return "servicios.png";
        return id + ".png";
    }

    static string NombreSala(string id)
    {
        if (id == "game") return "Game room";
        if (id == "cafeteria") return "Cafetería";
        if (id == "admisiones") return "Admisiones";
        if (id == "servicios") return "Servicios estudiantiles";
        if (id == "clinica") return "Clínica";
        if (id == "auditorio") return "Auditorio";
        return "Sala";
    }

    void EntrarSala(string id)
    {
        var tex = RecursosCampus.Textura("Interiores/" + ArchivoSala(id));
        if (tex == null) return;
        dentro = true;
        salaId = id;
        retornoSala = jugador.transform.position;
        puedeSalir = Time.time + 0.6f;
        if (mapaGo != null) mapaGo.SetActive(false);
        if (murosGo != null) murosGo.SetActive(false);
        if (npcsGo != null) npcsGo.SetActive(false);
        if (zonaVerde != null) zonaVerde.enabled = false;

        salaAncho = tex.width / 32f;
        salaAlto = tex.height / 32f;
        interiorGo = new GameObject("Interior");
        var dibujo = interiorGo.AddComponent<SpriteRenderer>();
        dibujo.sprite = RecursosCampus.ComoSprite(tex, 32f, new Vector2(0.5f, 0.5f));
        dibujo.sortingOrder = -20;
        interiorGo.transform.position = new Vector3(salaAncho * 0.5f, salaAlto * 0.5f, 0f);
        marcaSala = new Vector2(salaAncho * 0.5f, salaAlto * 0.46f);
        marcoGo = new GameObject("Marco");
        Borde(0.4f, salaAlto * 0.5f, 0.8f, salaAlto);
        Borde(salaAncho - 0.4f, salaAlto * 0.5f, 0.8f, salaAlto);
        Borde(salaAncho * 0.5f, salaAlto - 0.4f, salaAncho, 0.8f);
        Borde(salaAncho * 0.5f, 0.45f, salaAncho, 0.9f);
        PonerJugador(new Vector2(salaAncho * 0.5f, 2.6f));
        if (id == "admisiones" || id == "clinica")
            jugador.transform.localScale = new Vector3(2.8f, 2.8f, 1f);
        if (id == "admisiones")
        {
            const float escala = 2.8f;
            marcaSala = new Vector2(salaAncho * 0.5f, salaAlto * 0.5f);
            var guia = new GameObject("GuiaAdmisiones");
            guia.transform.SetParent(interiorGo.transform);
            guia.transform.position = new Vector3(marcaSala.x, marcaSala.y, 0f);
            guia.transform.localScale = new Vector3(escala, escala, 1f);
            var dibujoGuia = guia.AddComponent<SpriteRenderer>();
            dibujoGuia.sortingOrder = 90;
            var npc = guia.AddComponent<NpcCampus>();
            npc.frames = RecursosCampus.Carpeta("admisiones", 48f);
            npc.camina = false;
            if (npc.frames.Length > 0) dibujoGuia.sprite = npc.frames[0];
        }
    }

    void Borde(float x, float y, float w, float h)
    {
        var go = new GameObject("Borde");
        go.transform.SetParent(marcoGo.transform);
        go.transform.position = new Vector3(x, y, 0f);
        var caja = go.AddComponent<BoxCollider2D>();
        caja.size = new Vector2(w, h);
    }

    void Despertar()
    {
        EstadoPartida.desperto = true;
        EstadoPartida.huidaActiva = false;
        if (presencia != null) presencia.persigue = false;
        if (dentro) SalirSala();
        Teñir(true);
        if (npcsGo != null) npcsGo.SetActive(false);
        PonerJugador(Nacimiento);
        Decir("DESPIERTAS", "Abres los ojos. No hay nadie. La universidad está vacía y la luz ya no es la del primer día.\n\nEn recepción quedó una nota. Habla del Proyecto 07 y de un piso que no existe.", "recepcion");
    }

    void Teñir(bool vacio)
    {
        var tinte = vacio ? new Color(0.58f, 0.42f, 0.82f, 1f) : Color.white;
        if (capasMapa != null)
        {
            foreach (var capa in capasMapa)
                if (capa != null) capa.color = tinte;
        }

        if (camara != null)
            camara.backgroundColor = vacio ? new Color(0.07f, 0.03f, 0.12f) : new Color(0.04f, 0.1f, 0.07f);
    }

    void SubirMezanine()
    {
        var tex = RecursosCampus.Textura("Campus/mezanine.jpg");
        if (tex == null) return;
        retornoSala = new Vector2(14.38f, 16.1f);
        dentro = true;
        salaId = "mezanine";
        puedeSalir = Time.time + 0.6f;
        if (presencia != null) presencia.persigue = false;
        if (mapaGo != null) mapaGo.SetActive(false);
        if (murosGo != null) murosGo.SetActive(false);
        if (npcsGo != null) npcsGo.SetActive(false);
        if (zonaVerde != null) zonaVerde.enabled = false;

        salaAncho = 40f;
        salaAlto = 20f;
        interiorGo = new GameObject("Mezanine");
        var dibujo = interiorGo.AddComponent<SpriteRenderer>();
        dibujo.sprite = RecursosCampus.ComoSprite(tex, 25.6f, new Vector2(0.5f, 0.5f));
        dibujo.sortingOrder = -20;
        interiorGo.transform.position = new Vector3(20f, 10f, 0f);
        marcoGo = new GameObject("Marco");
        Borde(0.4f, 10f, 0.8f, 20f);
        Borde(39.6f, 10f, 0.8f, 20f);
        Borde(20f, 19.6f, 40f, 0.8f);
        Borde(20f, 0.4f, 40f, 0.8f);
        if (jugador != null) jugador.transform.localScale = Vector3.one;
        PonerJugador(new Vector2(10.94f, 14.15f));
    }

    void SubirDesdeMezanine()
    {
        Decir("GRADAS", "Hacia arriba todavía no hay otro piso dibujado.", "");
    }

    void EntrarPisoX()
    {
        var tex = RecursosCampus.Textura("PisoX/fondo.png");
        if (tex == null) return;
        if (dentro) SalirSala();
        EstadoPartida.enPisoX = true;
        EstadoPartida.huidaActiva = false;
        if (presencia != null) presencia.persigue = false;
        dentro = true;
        salaId = "pisox";
        puedeSalir = Time.time + 2f;
        if (mapaGo != null) mapaGo.SetActive(false);
        if (murosGo != null) murosGo.SetActive(false);
        if (npcsGo != null) npcsGo.SetActive(false);
        if (zonaVerde != null) zonaVerde.enabled = false;

        salaAncho = tex.width / 48f;
        salaAlto = tex.height / 48f;
        interiorGo = new GameObject("PisoX");
        var dibujo = interiorGo.AddComponent<SpriteRenderer>();
        dibujo.sprite = RecursosCampus.ComoSprite(tex, 48f, new Vector2(0.5f, 0.5f));
        dibujo.sortingOrder = -20;
        interiorGo.transform.position = new Vector3(salaAncho * 0.5f, salaAlto * 0.5f, 0f);
        marcoGo = new GameObject("Marco");
        Borde(0.4f, salaAlto * 0.5f, 0.8f, salaAlto);
        Borde(salaAncho - 0.4f, salaAlto * 0.5f, 0.8f, salaAlto);
        Borde(salaAncho * 0.5f, salaAlto - 0.4f, salaAncho, 0.8f);
        Borde(salaAncho * 0.5f, 0.4f, salaAncho, 0.8f);
        portalPiso = new Vector2(salaAncho * 0.5f, salaAlto * 0.52f);
        if (jugador != null) jugador.transform.localScale = new Vector3(1.35f, 1.35f, 1f);
        PonerJugador(new Vector2(2.6f, salaAlto * 0.48f));
        PonerBot("robot", new Vector2(7.9f, 15.4f), new Vector2(12.1f, 15.4f));
        PonerBot("criatura", new Vector2(10.0f, 11.6f), new Vector2(14.2f, 11.6f));
        PonerBot("monstruo", new Vector2(14.2f, 8.1f), new Vector2(18.3f, 8.1f));
        if (camara != null) camara.backgroundColor = new Color(0.05f, 0.02f, 0.1f);
    }

    void PonerBot(string carpeta, Vector2 inicio, Vector2 meta)
    {
        var go = new GameObject(carpeta);
        go.transform.SetParent(interiorGo.transform);
        go.transform.position = inicio;
        var dibujo = go.AddComponent<SpriteRenderer>();
        dibujo.sortingOrder = 80;
        var bot = go.AddComponent<BotPiso>();
        bot.frames = RecursosCampus.CarpetaPiso(carpeta);
        bot.objetivo = jugador.transform;
        bot.meta = meta;
        if (bot.frames != null && bot.frames.Length > 0) dibujo.sprite = bot.frames[0];
    }

    void SalirPisoX()
    {
        EstadoPartida.enPisoX = false;
        if (jugador != null) jugador.transform.localScale = Vector3.one;
        SalirSala();
        Teñir(true);
        if (npcsGo != null) npcsGo.SetActive(false);
        PonerJugador(Celda(15, 2));
    }

    void SalirSala()
    {
        if (!dentro) return;
        dentro = false;
        salaId = "";
        if (jugador != null) jugador.transform.localScale = Vector3.one;
        if (interiorGo != null) Destroy(interiorGo);
        if (marcoGo != null) Destroy(marcoGo);
        interiorGo = null;
        marcoGo = null;
        if (mapaGo != null) mapaGo.SetActive(true);
        if (murosGo != null) murosGo.SetActive(true);
        if (npcsGo != null) npcsGo.SetActive(!EstadoPartida.desperto);
        if (zonaVerde != null) zonaVerde.enabled = EstadoPartida.huidaActiva || EstadoPartida.DecisionesListas;
        PonerJugador(retornoSala);
    }

    void PonerJugador(Vector2 sitio)
    {
        if (jugador == null) return;
        jugador.Detener();
        jugador.transform.position = sitio;
        var cuerpo = jugador.GetComponent<Rigidbody2D>();
        if (cuerpo != null) cuerpo.position = sitio;
    }

    public static Vector2 Celda(int columna, int filaDesdeArriba)
    {
        return new Vector2(columna + 0.5f, (19 - filaDesdeArriba) + 0.5f);
    }
}

public class PuertaVisual : MonoBehaviour
{
    public Sprite[] frames;
    public SpriteRenderer dibujo;
    public bool abierta;
    float tiempo;

    public void Abrir()
    {
        abierta = true;
    }

    public void Cerrar()
    {
        abierta = false;
        tiempo = 0f;
        if (dibujo != null && frames != null && frames.Length > 0) dibujo.sprite = frames[0];
    }

    void Update()
    {
        if (dibujo == null || frames == null || frames.Length == 0) return;
        if (!abierta)
        {
            dibujo.sprite = frames[0];
            return;
        }

        tiempo += Time.deltaTime * 14f;
        int indice = Mathf.Min(frames.Length - 1, Mathf.FloorToInt(tiempo));
        dibujo.sprite = frames[indice];
    }
}
