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
    Fin,
    Recorrido,
    SensorHuella,
    PisoSiete
}

public class PuntoCampus
{
    public string id;
    public string nombre;
    public Vector2 posicion;
    public float radio = 1.35f;
    public int lado;
}

public partial class DirectorCampus : MonoBehaviour
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
    public PisoSiete NivelSiete { get; private set; }
    float gritoHasta;
    KeyCode teclaJuego = KeyCode.UpArrow;
    bool avisoPisoX;
    int pisoNumero;
    bool llegadaAscensor;
    static readonly Vector2 AscensorPB=new Vector2(10.7422f,16.661f);
    static readonly Vector2 AscensorSuperior=new Vector2(17.7734f,11.0219f);
    public string PlantaActual=>!dentro?"PB":salaId=="mezanine"?"M":salaId=="piso"?pisoNumero.ToString():salaId=="biblioteca"?"5":salaId=="pisox"?"X":"PB";
    bool aparecerEnPuerta;
    bool pidioPlantaBaja;
    bool desmayando;
    public float negro;
    public bool BotonXListo { get; private set; }
    readonly List<Vector2> gradasPiso = new List<Vector2>();
    readonly List<PuntoCampus> puntosPiso = new List<PuntoCampus>();
    static readonly Vector2 PuertaBibliotecaPiso = new Vector2(14.06f, 10.39f);
    static readonly Vector2 SpawnBiblioteca = new Vector2(19.06f, 3.2f);
    static readonly Vector2 SalidaBiblioteca = new Vector2(19.06f, 2.23f);

    public bool bloqueado => panel != PanelCampus.Ninguno || desmayando;

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
        if(panel==PanelCampus.Ascensor){
            if(Input.GetKeyDown(KeyCode.Escape)){CerrarPanel();return;}
            if(Input.GetKeyDown(KeyCode.Alpha0))ElegirPiso("PB");
            else if(Input.GetKeyDown(KeyCode.M))ElegirPiso("M");
            else if(Input.GetKeyDown(KeyCode.Alpha2))ElegirPiso("2");
            else if(Input.GetKeyDown(KeyCode.Alpha3))ElegirPiso("3");
            else if(Input.GetKeyDown(KeyCode.Alpha4))ElegirPiso("4");
            else if(Input.GetKeyDown(KeyCode.Alpha5))ElegirPiso("5");
            else if(Input.GetKeyDown(KeyCode.X))ElegirPiso("X");
            return;
        }
        if(panel==PanelCampus.SensorHuella){
            sensorHuella.Avanzar(Time.unscaledDeltaTime);
            if(Input.GetKeyDown(KeyCode.Escape)){panel=PanelCampus.Ninguno;return;}
            if(Input.GetKeyDown(KeyCode.Space))LeerSensorHuella();
            return;
        }
        if(jugando && !EstadoPartida.desperto && !EstadoPartida.enPisoX && Input.GetKeyDown(KeyCode.M) && (panel==PanelCampus.Ninguno || panel==PanelCampus.Recorrido))panel=panel==PanelCampus.Recorrido?PanelCampus.Ninguno:PanelCampus.Recorrido;
        if(panel==PanelCampus.Recorrido && Input.GetKeyDown(KeyCode.Escape)){panel=PanelCampus.Ninguno;return;}

        if (EnAulaAcademica && jugando && panel == PanelCampus.Ninguno && jugador.transform.position.y < 1.7f && Time.time > puedeSalir)
            SalirAulaAcademica();

        if (dentro && salaId != "pisox" && salaId != "mezanine" && salaId != "piso" && salaId != "biblioteca" && salaId != "aulaAcademica" && jugando && panel == PanelCampus.Ninguno && jugador.transform.position.y < 1.7f && Time.time > puedeSalir)
            SalirSala();

        if (jugando && panel == PanelCampus.Ninguno && Input.GetKeyDown(KeyCode.E))
        {
            var cerca = Cercano();
            if (cerca != null)
            {
                if (cerca.id == "gradasMez") SubirDesdeMezanine(cerca.lado);
                else if (cerca.id == "gradasPiso") SubirPiso(cerca.lado);
                else Usar(cerca.id);
            }
        }

        if (jugando && panel == PanelCampus.Ninguno && Input.GetKeyDown(KeyCode.Q))
        {
            var cerca = Cercano();
            if (cerca != null && cerca.id == "gradasMez")
                SalirSala();
            else if (cerca != null && cerca.id == "gradasPiso")
                BajarPiso(cerca.lado);
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
        if (EnAulaAcademica && jugando) { EnfocarAula(); return; }
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
            if (EnAulaAcademica) return CercaEnAula(aqui);
            if (salaId == "pisox")
            {
                return NivelSiete != null ? NivelSiete.Cercano(aqui) : null;
            }

            if (salaId == "mezanine")
            {
                PuntoCampus cerca = null;
                float limite = 999f;
                for (int i = 0; i < GradasMezanine.Length; i++)
                {
                    float distancia = Vector2.Distance(aqui, GradasMezanine[i]);
                    if (distancia > 1.4f || distancia >= limite) continue;
                    limite = distancia;
                    cerca = new PuntoCampus { id = "gradasMez", nombre = "gradas", posicion = GradasMezanine[i], radio = 1.4f, lado = i };
                }

                foreach (var punto in puntosPiso)
                {
                    float distancia = Vector2.Distance(aqui, punto.posicion);
                    if (distancia > punto.radio || distancia >= limite) continue;
                    limite = distancia;
                    cerca = punto;
                }

                return cerca;
            }

            if (salaId == "biblioteca")
            {
                if (Vector2.Distance(aqui, SalidaBiblioteca) <= 1.7f)
                    return new PuntoCampus { id = "salirBiblioteca", nombre = "Salir", posicion = SalidaBiblioteca, radio = 1.7f };
                return null;
            }

            if (salaId == "piso")
            {
                PuntoCampus cerca = null;
                float limite = 1.45f;
                for (int i = 0; i < gradasPiso.Count; i++)
                {
                    float distancia = Vector2.Distance(aqui, gradasPiso[i]);
                    if (distancia > 1.45f || distancia >= limite) continue;
                    limite = distancia;
                    cerca = new PuntoCampus { id = "gradasPiso", nombre = "gradas", posicion = gradasPiso[i], radio = 1.45f, lado = i };
                }

                if (pisoNumero == 5)
                {
                    float distanciaPuerta = Vector2.Distance(aqui, PuertaBibliotecaPiso);
                    if (distanciaPuerta <= 1.35f && (cerca == null || distanciaPuerta < limite))
                        cerca = new PuntoCampus { id = "biblioteca", nombre = "Biblioteca", posicion = PuertaBibliotecaPiso, radio = 1.35f };
                }

                foreach (var punto in puntosPiso)
                {
                    float distancia = Vector2.Distance(aqui, punto.posicion);
                    if (distancia > punto.radio) continue;
                    if (cerca != null && distancia >= limite) continue;
                    limite = distancia;
                    cerca = punto;
                }

                return cerca;
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

    public bool TryGuiaRecorrido(out Vector2 origen,out Vector2 destino,out bool alAlcance,out string objetivo)
    {
        origen=Vector2.zero;destino=Vector2.zero;alAlcance=false;objetivo="";
        if(jugador==null || !jugando || panel!=PanelCampus.Ninguno || EstadoPartida.desperto || RutaPlantaBaja.Completa || negro>.01f)return false;
        origen=jugador.transform.position;
        string id=RutaPlantaBaja.Destinos[RutaPlantaBaja.Paso];
        objetivo=id;
        if(dentro){
            if(salaId!=id){
                destino=new Vector2(Mathf.Clamp(origen.x,1.1f,salaAncho-1.1f),1.3f);
                objetivo="salir_"+salaId;return true;
            }
            destino=marcaSala;
            var cercanoInterior=Cercano();
            alAlcance=cercanoInterior!=null && cercanoInterior.id==id;
            return true;
        }
        PuntoCampus mejor=null;float distancia=float.PositiveInfinity;
        foreach(var punto in puntos){
            if(punto.id!=id)continue;
            float actual=(punto.posicion-origen).sqrMagnitude;
            if(actual<distancia){distancia=actual;mejor=punto;}
        }
        if(mejor==null)return false;
        destino=mejor.posicion;
        var cercano=Cercano();alAlcance=cercano!=null && cercano.id==id;
        return true;
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
        ReiniciarDesmayo();
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
        jugando=true;panel=PanelCampus.Ninguno;
        if(jugador!=null)jugador.Detener();
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
        ReiniciarDesmayo();
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
            Gritar("Vuelves al ascensor. Conservas los objetos y las misiones completadas.");
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
        if (!BotonXListo && (!EstadoPartida.desperto || !EstadoPartida.sistemasListo)) return;
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
        if(piso==PlantaActual){aviso="Ya estás en "+piso+".";return;}
        if(piso=="X" && !BotonXListo && !MostrarPisoX){aviso="Ese destino todavía no está disponible.";return;}

        if (piso == "X")
        {
            if (!BotonXListo && (!EstadoPartida.desperto || !EstadoPartida.sistemasListo))
            {
                aviso = "El piso 7 todavía no está disponible. Sigue la pista del ascensor.";
                return;
            }
            aviso = "";
            panel = PanelCampus.Ninguno;
            EntrarPisoX();
            return;
        }

        if (piso == "PB")
        {
            aviso = "";
            panel = PanelCampus.Ninguno;
            if(EstadoPartida.enPisoX)SalirPisoX();
            else if(dentro)SalirSala();
            PonerJugador(AscensorPB);
            aviso="";
            return;
        }

        if (piso == "M")
        {
            aviso = "";
            panel = PanelCampus.Ninguno;
            llegadaAscensor=true;
            SubirMezanine();
            return;
        }

        if (piso == "1")
        {
            aviso = "El piso 1 no llegó con los planos. Sigue en orden: 2, 3, 4 y 5.";
            return;
        }

        if (piso == "2" || piso == "3" || piso == "4" || piso == "5")
        {
            aviso = "";
            panel = PanelCampus.Ninguno;
            llegadaAscensor=true;
            EntrarPisoNumero(int.Parse(piso));
            return;
        }

        if (!EstadoPartida.desperto)
        {
            aviso = "Esos botones se usan cuando el campus ya está vacío.";
            return;
        }

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

    public readonly SensorHuellaJuego sensorHuella=new SensorHuellaJuego();
    public void LeerSensorHuella()
    {
        if(panel!=PanelCampus.SensorHuella || !sensorHuella.Leer())return;
        EstadoPartida.huella=true;AbrirPuertas();
        Decir("MOLINETES · ACCESO VALIDADO", "Tu huella fue registrada. Los molinetes están abiertos; continúa el recorrido.", "control_huella");
    }

    public bool MostrarPisoX => BotonXListo || avisoPisoX || EstadoPartida.sistemasListo;

    void Usar(string id)
    {
        if (EnAulaAcademica) { UsarAulaAcademica(id); return; }
        if (id.StartsWith("aula_")) { EntrarAulaAcademica(id); return; }
        if (dentro && salaId == "pisox" && NivelSiete != null)
        {
            NivelSiete.Usar(id);
            return;
        }

        if(!EstadoPartida.desperto && !RutaPlantaBaja.Permite(id)){
            Decir("RECORRIDO PASO A PASO", "Primero: "+RutaPlantaBaja.Objetivo+".\n\nPulsa M para consultar tu recorrido.", "");return;
        }

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

        if (id == "biblioteca")
        {
            EntrarBiblioteca();
            return;
        }

        if (id == "salirBiblioteca")
        {
            aparecerEnPuerta = true;
            EntrarPisoNumero(5);
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

                if(EstadoPartida.huella){
                    Decir("MOLINETES", "Tu huella ya está validada. Puedes pasar.", "control_huella");
                }else{
                    sensorHuella.Reiniciar();panel=PanelCampus.SensorHuella;jugador.Detener();
                }
                break;
            case "ascensor":
                if(!EstadoPartida.desperto)RutaPlantaBaja.Completar("primera_vez");
                if (pidioPlantaBaja && !BotonXListo && !desmayando)
                {
                    StartCoroutine(DesmayoEnAscensor());
                    break;
                }

                aviso = BotonXListo
                    ? "Pulsa X."
                    : EstadoPartida.desperto && EstadoPartida.sistemasListo ? "PISO X está abierto. Puedes entrar." : "";
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
                avisoArcade="Sigue la dirección iluminada. Consigue tres aciertos seguidos.";
                proximaPulsacionArcade=Time.unscaledTime+.25f;
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
            case "arca":
                Decir("ARCA", EstadoPartida.desperto
                    ? "El depósito ARCA está cerrado. No quedó nadie del archivo."
                    : "Depósito ARCA. Aquí se guarda el archivo del campus. Esta sala es la de este piso.", "");
                break;
            case "gaming":
                Decir("GAMING Y SONIDO", EstadoPartida.desperto
                    ? "La sala de gaming y sonido está apagada."
                    : "Sala de gaming y sonido. Los estudiantes usan las cabinas y los equipos.", "");
                break;
            case "mac":
                Decir("SALA MAC", EstadoPartida.desperto
                    ? "La sala MAC está vacía. Las computadoras quedaron apagadas."
                    : "Sala MAC. Estas computadoras son para las clases de este piso.", "");
                break;
            case "fotocopia":
                Decir("FOTOCOPIA", EstadoPartida.desperto
                    ? "La fotocopiadora está apagada."
                    : "Fotocopiadora de este piso. Sirve para las copias del día.", "");
                break;
            case "aula2":
                Decir("AULA GRANDE", EstadoPartida.desperto
                    ? "El aula grande está vacía."
                    : "Aula grande de este piso. Aquí son las clases.", "");
                break;
            case "bano2":
                Decir("BAÑOS", EstadoPartida.desperto
                    ? "Los baños de este piso están vacíos."
                    : "Baños de este piso. Forman parte del recorrido.", "");
                break;
            case "oficinas":
                Decir("OFICINAS DOCENTES", EstadoPartida.desperto
                    ? "Las oficinas docentes están cerradas. No quedó ningún profesor."
                    : "Oficinas docentes. Aquí atienden los profesores. Esta es la sala de docentes de este piso.", "");
                break;
            case "cajas":
                Decir("CAJAS", EstadoPartida.desperto
                    ? "Cajas está cerrado. No hay quién cobre un trámite."
                    : "Cajas. Aquí se pagan la colegiatura y los trámites del día.", "");
                break;
            case "direccion":
                Decir("DIRECCIÓN DE CARRERA", EstadoPartida.desperto
                    ? "Dirección de carrera quedó vacía. No hay quién oriente."
                    : "Dirección de carrera. Aquí te orientan sobre tu carrera y los trámites académicos.", "");
                break;
            case "postgrado":
                Decir("POSTGRADO", EstadoPartida.desperto
                    ? "Postgrado está apagado. No quedó nadie en las mesas."
                    : "Postgrado. Aquí están los programas de especialización y maestría.", "");
                break;
            case "facsalud":
                Decir("CIENCIAS DE LA SALUD", EstadoPartida.desperto
                    ? "La Facultad de Ciencias de la Salud está vacía."
                    : "Facultad de Ciencias de la Salud. Aquí estudian las carreras de salud.", "");
                break;
            case "facjuridica":
                Decir("CIENCIAS JURÍDICAS", EstadoPartida.desperto
                    ? "La Facultad de Ciencias Jurídicas y Sociales está vacía."
                    : "Facultad de Ciencias Jurídicas y Sociales. Aquí están las carreras de derecho y sociales.", "");
                break;
            case "faceconomia":
                Decir("CIENCIAS ECONÓMICAS", EstadoPartida.desperto
                    ? "La Facultad de Ciencias Económicas y Empresariales está vacía."
                    : "Facultad de Ciencias Económicas y Empresariales. Aquí están las carreras de economía y empresa.", "");
                break;
            case "facdiseno":
                Decir("DISEÑO Y CROSSMEDIA", EstadoPartida.desperto
                    ? "La Facultad de Diseño y Tecnologías Crossmedia está vacía."
                    : "Facultad de Diseño y Tecnologías Crossmedia. Aquí están las carreras de diseño.", "");
                break;
            case "facinge":
                Decir("INGENIERÍA", EstadoPartida.desperto
                    ? "La Facultad de Ingeniería está vacía."
                    : "Facultad de Ingeniería. Aquí están las carreras de ingeniería.", "");
                break;
            case "software":
                Decir("CENTRO DE SOFTWARE", EstadoPartida.desperto
                    ? "El centro de desarrollo de software está apagado."
                    : "Centro de desarrollo de software. Aquí se programan los proyectos de este piso.", "");
                break;
            case "labcomp":
                Decir("LABORATORIO", EstadoPartida.desperto
                    ? "El laboratorio de computación está vacío."
                    : "Laboratorio de computación. Estas máquinas son para las clases de este piso.", "");
                break;
            case "informatica":
                Decir("INFORMÁTICA", EstadoPartida.desperto
                    ? "El laboratorio de informática está cerrado."
                    : "Laboratorio de informática. Aquí están los equipos de este piso.", "");
                break;
            case "sonido":
                Decir("SALA DE SONIDO", EstadoPartida.desperto
                    ? "La sala de sonido está apagada."
                    : "Sala de sonido. Aquí se usan los equipos de audio.", "");
                break;
            case "streaming":
                Decir("SALA DE STREAMING", EstadoPartida.desperto
                    ? "La sala de streaming está apagada."
                    : "Sala de streaming. Desde aquí se graban las clases.", "");
                break;
            case "deposito":
                Decir("DEPÓSITO", EstadoPartida.desperto
                    ? "El depósito está cerrado."
                    : "Depósito de este piso. Aquí se guardan los materiales.", "");
                break;
            case "semiologia":
                Decir("SEMIOLOGÍA", EstadoPartida.desperto
                    ? "La sala de semiología está vacía."
                    : "Sala de semiología. Aquí son las prácticas de este piso.", "");
                break;
            case "consultorio":
                Decir("CONSULTORIO EMPRESARIAL", EstadoPartida.desperto
                    ? "El consultorio empresarial está cerrado."
                    : "Consultorio empresarial. Aquí se atienden los proyectos de empresa.", "");
                break;
            case "camara":
                Decir("CÁMARA", EstadoPartida.desperto
                    ? "La cámara está apagada. No hay sesión."
                    : "Cámara de este piso. Sirve para las observaciones y las prácticas.", "");
                break;
            case "daf":
                Decir("ALMACÉN DAF", EstadoPartida.desperto
                    ? "El almacén DAF está cerrado."
                    : "Almacén DAF. Aquí se guardan los materiales de este piso.", "");
                break;
            case "docentes":
                Decir("SALA DE DOCENTES", EstadoPartida.desperto
                    ? "La sala de docentes está vacía."
                    : "Sala de docentes. Aquí atienden los profesores de este piso.", "");
                break;
            case "adquisiciones":
                Decir("ADQUISICIONES", EstadoPartida.desperto
                    ? "Adquisiciones y almacén están cerrados."
                    : "Adquisiciones y almacén. Aquí se reciben los materiales del piso.", "");
                break;
            case "nacional":
                Decir("OFICINA NACIONAL", EstadoPartida.desperto
                    ? "La oficina nacional está cerrada."
                    : "Oficina nacional. Desde aquí se coordinan los trámites de este piso.", "");
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
        if(!EstadoPartida.desperto && (tarjeta!="control_huella" || EstadoPartida.huella) && (tarjeta!="game_room" || EstadoPartida.gameRoom))RutaPlantaBaja.Completar(tarjeta);
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
        ColisionesCampus.AgregarEstructura(murosGo.transform);
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
        var pies = go.AddComponent<CapsuleCollider2D>();
        pies.direction=CapsuleDirection2D.Horizontal;
        pies.size=new Vector2(.46f,.24f);pies.offset=new Vector2(0,.15f);
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
        CrearNpc("cafeteria", new Vector2(11.9f, 5.2f), false);
        CrearNpc("game_room", new Vector2(4.4f, 5.2f), false);
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
        Punto("ascensor", "Ascensor", AscensorPB, 1.9f);
        Punto("gradas", "Subir", new Vector2(14.38f, 16.1f), 1.7f);
        Punto("servicios", "Servicios estudiantiles", Celda(12, 3), 1.35f);
        Punto("auditorio", "Auditorio", new Vector2(6.21f, 15.39f), 1.8f);
        Punto("cafeteria", "Entrar a la cafetería", new Vector2(11.9f, 7.2f), 1.7f);
        Punto("game", "Entrar a la game room", new Vector2(4.4f, 7.2f), 1.7f);
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

    public string avisoArcade="Sigue la dirección iluminada. Consigue tres aciertos seguidos.";
    float proximaPulsacionArcade;
    public void PulsarDireccionArcade(KeyCode tecla)
    {
        if(panel!=PanelCampus.Game || Time.unscaledTime<proximaPulsacionArcade)return;
        proximaPulsacionArcade=Time.unscaledTime+.18f;
        if(tecla!=teclaJuego){aciertos=0;avisoArcade="Dirección incorrecta. Empieza otra vez; no hay límite de intentos.";NuevaFlecha();return;}
        aciertos++;
        if(aciertos>=3){EstadoPartida.gameRoom=true;Decir("GAME ROOM", "Completaste el minijuego. No cambia tu carrera.", "game_room");return;}
        avisoArcade="¡Bien! Sigue la siguiente dirección.";NuevaFlecha();
    }
    void RevisarMinijuego()
    {
        if(Input.GetKeyDown(KeyCode.Escape)){CerrarPanel();return;}
        if(Input.GetKeyDown(KeyCode.UpArrow)||Input.GetKeyDown(KeyCode.W))PulsarDireccionArcade(KeyCode.UpArrow);
        else if(Input.GetKeyDown(KeyCode.DownArrow)||Input.GetKeyDown(KeyCode.S))PulsarDireccionArcade(KeyCode.DownArrow);
        else if(Input.GetKeyDown(KeyCode.LeftArrow)||Input.GetKeyDown(KeyCode.A))PulsarDireccionArcade(KeyCode.LeftArrow);
        else if(Input.GetKeyDown(KeyCode.RightArrow)||Input.GetKeyDown(KeyCode.D))PulsarDireccionArcade(KeyCode.RightArrow);
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
        jugador.AjustarAltura(1.8f);
        ConstruirObstaculosSala(id);
        if(id=="auditorio")PonerJugador(new Vector2(salaAncho*.35f,2.6f));
        if(id=="admisiones")marcaSala=new Vector2(salaAncho*.50f,salaAlto*.37f);
        if(id=="cafeteria")marcaSala=new Vector2(salaAncho*.29f,salaAlto*.40f);
        if (id == "admisiones")
        {
            const float escala = 1f;
            var guia = new GameObject("GuiaAdmisiones");
            guia.transform.SetParent(interiorGo.transform);
            guia.transform.position = new Vector3(salaAncho*.5f,salaAlto*.55f,0f);
            guia.transform.localScale = new Vector3(escala, escala, 1f);
            var dibujoGuia = guia.AddComponent<SpriteRenderer>();
            dibujoGuia.sortingOrder = 90;
            var npc = guia.AddComponent<NpcCampus>();
            npc.frames = RecursosCampus.Carpeta("admisiones", 48f);
            npc.camina = false;
            if (npc.frames.Length > 0){dibujoGuia.sprite=npc.frames[0];float alto=npc.frames[0].bounds.size.y;guia.transform.localScale=Vector3.one*(alto>.01f?1.8f/alto:1f);}
        }
    }

    void Mueble(float x,float y,float w,float h)
    {
        Borde(x*salaAncho,y*salaAlto,w*salaAncho,h*salaAlto);
    }
    void ConstruirObstaculosSala(string id)
    {
        if(id=="clinica"){
            Mueble(.24f,.25f,.22f,.14f);Mueble(.75f,.25f,.22f,.14f);
            Mueble(.27f,.57f,.20f,.11f);Mueble(.73f,.57f,.20f,.11f);
        }else if(id=="admisiones"){
            Mueble(.16f,.48f,.18f,.10f);Mueble(.40f,.52f,.16f,.10f);Mueble(.59f,.52f,.16f,.10f);Mueble(.81f,.48f,.19f,.10f);
            Mueble(.17f,.14f,.22f,.10f);
        }else if(id=="cafeteria"){
            Mueble(.17f,.38f,.18f,.27f);
            Mueble(.38f,.42f,.12f,.08f);Mueble(.59f,.42f,.12f,.08f);Mueble(.80f,.42f,.12f,.08f);
            Mueble(.30f,.23f,.12f,.08f);Mueble(.55f,.23f,.12f,.08f);Mueble(.80f,.23f,.12f,.08f);
        }else if(id=="auditorio"){
            Mueble(.20f,.25f,.22f,.30f);Mueble(.51f,.25f,.22f,.30f);Mueble(.82f,.25f,.22f,.30f);
            Mueble(.51f,.70f,.65f,.10f);
        }else if(id=="game"){
            Mueble(.19f,.56f,.23f,.13f);Mueble(.52f,.29f,.17f,.15f);Mueble(.85f,.49f,.20f,.12f);Mueble(.86f,.14f,.20f,.09f);
        }else if(id=="servicios"){
            Mueble(.36f,.28f,.20f,.14f);Mueble(.78f,.32f,.23f,.18f);
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

    static readonly Vector2[] GradasMezanine =
    {
        new Vector2(10.94f, 15.5f),
        new Vector2(27.19f, 15.5f)
    };

    void SubirMezanine(int lado = 0)
    {
        var tex = RecursosCampus.Textura("Campus/mezanine.jpg");
        if (tex == null) return;
        SoltarInterior();
        pisoNumero = 0;
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
        if(dibujo.sprite!=null)interiorGo.transform.localScale=new Vector3(1,20f/dibujo.sprite.bounds.size.y,1);
        interiorGo.transform.position = new Vector3(20f, 10f, 0f);
        marcoGo = new GameObject("Marco");
        Borde(0.4f, 10f, 0.8f, 20f);
        Borde(39.6f, 10f, 0.8f, 20f);
        Borde(20f, 19.6f, 40f, 0.8f);
        Borde(20f, 0.4f, 40f, 0.8f);
        if (jugador != null) jugador.transform.localScale = Vector3.one;
        if (lado < 0 || lado >= GradasMezanine.Length) lado = 0;
        PrepararMezanine();
        if(llegadaAscensor){llegadaAscensor=false;PonerJugador(new Vector2(23.05f,15.05f));}
        else PonerJugador(GradasMezanine[lado]);
    }

    void PrepararMezanine()
    {
        puntosPiso.Clear();
        if (interiorGo == null) return;

        PuntoPiso("ascensor", "Ascensor", new Vector2(23.05f, 15.05f), 1.5f);
        PuntoPiso("arca", "ARCA", new Vector2(18.55f, 14.9f), 1.45f);
        PuntoPiso("oficinas", "Oficinas docentes", new Vector2(15.5f, 13.15f), 1.5f);
        PuntoPiso("cajas", "Cajas", new Vector2(26.15f, 12.75f), 1.4f);
        PuntoPiso("direccion", "Dirección de carrera", new Vector2(20.9f, 6.85f), 1.2f);
        PuntoPiso("postgrado", "Postgrado", new Vector2(34.4f, 15.6f), 1.45f);
        PuntoPiso("facsalud", "Ciencias de la Salud", new Vector2(6.33f, 5.35f), 1.15f);
        PuntoPiso("facjuridica", "Ciencias Jurídicas", new Vector2(13.2f, 5.35f), 1.15f);
        PuntoPiso("faceconomia", "Ciencias Económicas", new Vector2(20.9f, 4.75f), 1.05f);
        PuntoPiso("facdiseno", "Diseño y Crossmedia", new Vector2(28.05f, 5.35f), 1.15f);
        PuntoPiso("facinge", "Ingeniería", new Vector2(35.23f, 5.35f), 1.15f);

        Habitante("estudiante_caminando", new Vector2(6.2f, 15.4f));
        Habitante("admisiones", new Vector2(18.55f, 16.25f));
        Habitante("estudiante_mujer", new Vector2(15.2f, 11.35f));
        Habitante("gestion_financiera", new Vector2(26.15f, 11.4f));
        Habitante("recepcionista", new Vector2(18.4f, 6.85f));
        Habitante("estudiante_sentado", new Vector2(36.4f, 16.3f));
        Habitante("odontologa", new Vector2(6.33f, 3.3f));
        Habitante("servicios_estudiantiles", new Vector2(13.2f, 3.3f));
        Habitante("expositor", new Vector2(20.9f, 3.3f));
        Habitante("cafeteria", new Vector2(28.05f, 3.3f));
        Habitante("game_room", new Vector2(35.23f, 3.3f));
    }

    void SubirDesdeMezanine(int lado)
    {
        EntrarPisoNumero(2, lado);
    }

    void SubirPiso(int lado)
    {
        if (pisoNumero >= 5)
        {
            Decir("GRADAS", "Este es el último piso. El orden es 2, 3, 4 y 5.", "");
            return;
        }

        EntrarPisoNumero(pisoNumero + 1, lado);
    }

    void BajarPiso(int lado)
    {
        if (pisoNumero <= 2) SubirMezanine(lado);
        else EntrarPisoNumero(pisoNumero - 1, lado);
    }

    void EntrarPisoNumero(int n, int lado = 0)
    {
        var tex = RecursosCampus.Textura("Pisos/piso" + n + ".png");
        if (tex == null) return;
        SoltarInterior();
        retornoSala = Celda(15, 2);
        dentro = true;
        salaId = "piso";
        pisoNumero = n;
        puedeSalir = Time.time + 0.6f;
        if (presencia != null) presencia.persigue = false;
        if (mapaGo != null) mapaGo.SetActive(false);
        if (murosGo != null) murosGo.SetActive(false);
        if (npcsGo != null) npcsGo.SetActive(false);
        if (zonaVerde != null) zonaVerde.enabled = false;

        salaAncho = 40f;
        salaAlto = 40f * tex.height / tex.width;
        interiorGo = new GameObject("Piso" + n);
        var dibujo = interiorGo.AddComponent<SpriteRenderer>();
        dibujo.sprite = RecursosCampus.ComoSprite(tex, tex.width / salaAncho, new Vector2(0.5f, 0.5f));
        dibujo.sortingOrder = -20;
        interiorGo.transform.position = new Vector3(salaAncho * 0.5f, salaAlto * 0.5f, 0f);
        marcoGo = new GameObject("Marco");
        CargarMurosPiso(n);
        Borde(0.4f, salaAlto * 0.5f, 0.8f, salaAlto);
        Borde(salaAncho - 0.4f, salaAlto * 0.5f, 0.8f, salaAlto);
        Borde(salaAncho * 0.5f, salaAlto - 0.4f, salaAncho, 0.8f);
        Borde(salaAncho * 0.5f, 0.4f, salaAncho, 0.8f);
        if (jugador != null) jugador.transform.localScale = Vector3.one;
        if (aparecerEnPuerta && n == 5)
        {
            aparecerEnPuerta = false;
            PonerJugador(PuertaBibliotecaPiso);
        }
        else if (gradasPiso.Count == 0) PonerJugador(new Vector2(salaAncho * 0.5f, salaAlto * 0.5f));
        else
        {
            int indice = Mathf.Clamp(lado, 0, gradasPiso.Count - 1);
            PonerJugador(gradasPiso[indice]);
        }

        PrepararPiso(n);
        PrepararAulas(n);
        ColisionesCampus.UnirParedesDePiso(marcoGo.transform);
        // Resolve the stair landing again after combining the wall geometry.
        if(llegadaAscensor){llegadaAscensor=false;PonerJugador(AscensorSuperior);}
        else PonerJugador(jugador.transform.position);
    }

    void CrearTerminalAscensor()
    {
        var tex=new Texture2D(32,40,TextureFormat.RGBA32,false);tex.filterMode=FilterMode.Point;
        var pixels=new Color[32*40];
        for(int y=0;y<40;y++)for(int x=0;x<32;x++){
            Color c=new Color(.18f,.19f,.3f);
            if(x<2 || x>29 || y<2 || y>37)c=new Color(.66f,.6f,.8f);
            else if(y>29)c=new Color(.96f,.56f,.18f);
            else if(x>4 && x<26 && y>5 && y<28)c=new Color(.40f,.58f,.63f);
            if(x>=15 && x<=16 && y>5 && y<28)c=new Color(.14f,.18f,.26f);
            pixels[y*32+x]=c;
        }
        tex.SetPixels(pixels);tex.Apply();
        var go=new GameObject("Ascensor - terminal de llamada");go.transform.SetParent(interiorGo.transform,false);
        go.transform.position=AscensorSuperior+new Vector2(.55f,.45f);
        var sr=go.AddComponent<SpriteRenderer>();sr.sprite=RecursosCampus.ComoSprite(tex,48,new Vector2(.5f,0));sr.sortingOrder=95;
    }

    void PrepararPiso(int n)
    {
        puntosPiso.Clear();
        if (interiorGo == null) return;
        PuntoPiso("ascensor","Ascensor",AscensorSuperior,1.9f);
        CrearTerminalAscensor();

        if (n == 2)
        {
            PuntoPiso("mac", "Sala MAC", new Vector2(6.64f, 14.26f), 1.05f);
            PuntoPiso("software", "Centro de software", new Vector2(12.89f, 14.26f), 1.15f);
            PuntoPiso("labcomp", "Lab. de computación", new Vector2(22.34f, 14.3f), 1.05f);
            PuntoPiso("labcomp", "Lab. de computación", new Vector2(26.45f, 14.3f), 1.05f);
            PuntoPiso("informatica", "Lab. de informática", new Vector2(30.66f, 14.3f), 1.05f);
            PuntoPiso("sonido", "Sala de sonido", new Vector2(7.8f, 13.2f), 1f);
            PuntoPiso("postgrado", "Lab. de postgrado", new Vector2(6.56f, 11.95f), 1f);
            PuntoPiso("streaming", "Sala de streaming", new Vector2(8f, 6.6f), 1.1f);
            PuntoPiso("deposito", "Depósito", new Vector2(31.6f, 4.9f), 1.05f);
            Habitante("estudiante_sentado", new Vector2(6.64f, 14.26f));
            Habitante("game_room", new Vector2(12.89f, 14.26f));
            Habitante("estudiante_mujer", new Vector2(7.8f, 13.2f));
            Habitante("admisiones", new Vector2(31.6f, 4.9f));
            Caminante("estudiante_caminando", new Vector2(10.35f, 12.5f), new Vector2(18f, 12f), new Vector2(25.94f, 12f));
        }
        else if (n == 3)
        {
            PuntoPiso("semiologia", "Sala de semiología", new Vector2(6.52f, 14.92f), 1.15f);
            PuntoPiso("arca", "Depósito ARCA", new Vector2(7.03f, 8.01f), 1.15f);
            PuntoPiso("fotocopia", "Fotocopia", new Vector2(36.05f, 9.22f), 1.15f);
            PuntoPiso("deposito", "Depósito", new Vector2(31.8f, 4.6f), 1.05f);
            Habitante("odontologa", new Vector2(6.52f, 14.92f));
            Habitante("admisiones", new Vector2(7.03f, 8.01f));
            Habitante("gestion_financiera", new Vector2(36.05f, 9.22f));
            Caminante("estudiante_caminando", new Vector2(10.12f, 12.19f), new Vector2(17.77f, 12.15f), new Vector2(26f, 12f));
        }
        else if (n == 4)
        {
            PuntoPiso("consultorio", "Consultorio empresarial", new Vector2(4.18f, 8.79f), 1.15f);
            PuntoPiso("camara", "Cámara", new Vector2(21.09f, 12.11f), 1.05f);
            PuntoPiso("daf", "Almacén DAF", new Vector2(25.27f, 11.76f), 1.05f);
            PuntoPiso("docentes", "Sala de docentes", new Vector2(31.05f, 7.38f), 1.15f);
            PuntoPiso("deposito", "Depósito", new Vector2(31.5f, 4.7f), 1.05f);
            Habitante("expositor", new Vector2(4.18f, 8.79f));
            Habitante("recepcionista", new Vector2(31.05f, 7.38f));
            Habitante("servicios_estudiantiles", new Vector2(25.27f, 11.76f));
            Caminante("estudiante_mujer", new Vector2(9.92f, 12.11f), new Vector2(17.85f, 12.15f), new Vector2(26f, 12f));
        }
        else if (n == 5)
        {
            PuntoPiso("adquisiciones", "Adquisiciones y almacén", new Vector2(6.76f, 5.74f), 1.15f);
            PuntoPiso("nacional", "Oficina nacional", new Vector2(34.49f, 7.19f), 1.15f);
            Habitante("estudiante_sentado", new Vector2(14.06f, 10.39f));
            Habitante("gestion_financiera", new Vector2(6.76f, 5.74f));
            Habitante("admisiones", new Vector2(34.49f, 7.19f));
            Caminante("estudiante_caminando", new Vector2(17.85f, 12.15f), new Vector2(26f, 12f), new Vector2(34.49f, 7.19f));
        }
    }

    void Caminante(string carpeta, params Vector2[] ruta)
    {
        if (ruta == null || ruta.Length == 0 || interiorGo == null) return;
        var go = new GameObject(carpeta);
        go.transform.SetParent(interiorGo.transform);
        go.transform.position = ruta[0];
        var dibujo = go.AddComponent<SpriteRenderer>();
        var npc = go.AddComponent<NpcCampus>();
        npc.frames = RecursosCampus.Carpeta(carpeta, 48f);
        npc.camina = ruta.Length > 1;
        npc.ruta = ruta;
        if (npc.frames != null && npc.frames.Length > 0) dibujo.sprite = npc.frames[0];
    }

    void PuntoPiso(string id, string nombre, Vector2 posicion, float radio)
    {
        puntosPiso.Add(new PuntoCampus { id = id, nombre = nombre, posicion = posicion, radio = radio });
    }

    void Habitante(string carpeta, Vector2 posicion)
    {
        var go = new GameObject(carpeta);
        go.transform.SetParent(interiorGo.transform);
        go.transform.position = posicion;
        var dibujo = go.AddComponent<SpriteRenderer>();
        var npc = go.AddComponent<NpcCampus>();
        npc.frames = RecursosCampus.Carpeta(carpeta, 48f);
        npc.camina = false;
        if (npc.frames != null && npc.frames.Length > 0) dibujo.sprite = npc.frames[0];
    }

    void PonerPuertaBiblioteca()
    {
        var tex = RecursosCampus.Textura("Pisos/puerta_biblioteca.png");
        if (tex == null || interiorGo == null) return;
        var go = new GameObject("PuertaBiblioteca");
        go.transform.SetParent(interiorGo.transform);
        go.transform.position = new Vector3(18.46f, 10.85f, 0f);
        var dibujo = go.AddComponent<SpriteRenderer>();
        dibujo.sprite = RecursosCampus.ComoSprite(tex, tex.width / 3.5f, new Vector2(0.5f, 0.5f));
        dibujo.sortingOrder = 180;
    }

    void EntrarBiblioteca()
    {
        var tex = RecursosCampus.Textura("Pisos/biblioteca.jpg");
        if (tex == null) return;
        SoltarInterior();
        dentro = true;
        salaId = "biblioteca";
        pisoNumero = 5;
        puedeSalir = Time.time + 0.6f;
        if (presencia != null) presencia.persigue = false;
        if (mapaGo != null) mapaGo.SetActive(false);
        if (murosGo != null) murosGo.SetActive(false);
        if (npcsGo != null) npcsGo.SetActive(false);
        if (zonaVerde != null) zonaVerde.enabled = false;

        salaAncho = 40f;
        salaAlto = 20f;
        interiorGo = new GameObject("Biblioteca");
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
        PonerJugador(SpawnBiblioteca);
        if (!pidioPlantaBaja)
        {
            pidioPlantaBaja = true;
            Decir("BIBLIOTECA", "Vuelve a la planta baja por el ascensor.", "");
        }
    }

    void ReiniciarDesmayo()
    {
        pidioPlantaBaja = false;
        desmayando = false;
        negro = 0f;
        BotonXListo = false;
    }

    System.Collections.IEnumerator DesmayoEnAscensor()
    {
        desmayando = true;
        panel = PanelCampus.Ninguno;
        if (jugador != null) jugador.Detener();
        float t = 0f;
        while (t < 0.55f)
        {
            t += Time.deltaTime;
            negro = Mathf.Clamp01(t / 0.55f);
            yield return null;
        }

        negro = 1f;
        yield return new WaitForSeconds(1.15f);
        BotonXListo = true;
        aviso = "Pulsa X.";
        AbrirAscensor();
        panel = PanelCampus.Ascensor;
        t = 0f;
        while (t < 0.85f)
        {
            t += Time.deltaTime;
            negro = 1f - Mathf.Clamp01(t / 0.85f);
            yield return null;
        }

        negro = 0f;
        desmayando = false;
    }

    void CargarMurosPiso(int n)
    {
        gradasPiso.Clear();
        string ruta = Path.Combine(Application.streamingAssetsPath, "Pisos/piso" + n + ".txt");
        if (!File.Exists(ruta)) return;
        var lineas = File.ReadAllLines(ruta);
        if (lineas.Length < 2) return;
        var medida = lineas[0].Split(' ');
        float pixelesAncho = float.Parse(medida[0], System.Globalization.CultureInfo.InvariantCulture);
        float pixelesAlto = float.Parse(medida[1], System.Globalization.CultureInfo.InvariantCulture);
        var cultura = System.Globalization.CultureInfo.InvariantCulture;
        for (int i = 1; i < lineas.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lineas[i])) continue;
            var parte = lineas[i].Split(' ');
            if (parte[0] == "grada" && parte.Length >= 3)
            {
                float px = float.Parse(parte[1], cultura);
                float py = float.Parse(parte[2], cultura);
                gradasPiso.Add(new Vector2(px / pixelesAncho * salaAncho, (pixelesAlto - py) / pixelesAlto * salaAlto));
                continue;
            }

            if (parte[0] == "stair" && parte.Length >= 3)
            {
                float ux = float.Parse(parte[1], cultura);
                float uy = float.Parse(parte[2], cultura);
                float wx = ux / 104f * salaAncho;
                float wy = (uy + 54f) / 54f * salaAlto;
                gradasPiso.Add(new Vector2(wx, wy));
                continue;
            }

            if (parte.Length < 4) continue;
            float x = float.Parse(parte[0], cultura);
            float y = float.Parse(parte[1], cultura);
            float w = float.Parse(parte[2], cultura);
            float h = float.Parse(parte[3], cultura);
            var muro = new GameObject("Muro");
            muro.transform.SetParent(marcoGo.transform);
            float cx = (x + w * 0.5f) / pixelesAncho * salaAncho;
            float cy = (pixelesAlto - (y + h * 0.5f)) / pixelesAlto * salaAlto;
            muro.transform.position = new Vector3(cx, cy, 0f);
            var caja = muro.AddComponent<BoxCollider2D>();
            caja.size = new Vector2(w / pixelesAncho * salaAncho, h / pixelesAlto * salaAlto);
        }

        gradasPiso.Sort((a, b) => a.x.CompareTo(b.x));
    }

    void SoltarInterior()
    {
        DescartarExteriorAcademico();
        if (interiorGo != null){interiorGo.SetActive(false);Destroy(interiorGo);}
        if (marcoGo != null){marcoGo.SetActive(false);Destroy(marcoGo);}
        interiorGo = null;
        marcoGo = null;
        gradasPiso.Clear();
        puntosPiso.Clear();
    }

    void EntrarPisoX()
    {
        var tex = RecursosCampus.Textura("PisoX/BotsOriginales/fondo_piso_x.png");
        if (tex == null)
        {
            Decir("PISO 7", "Falta el escenario: PisoX/BotsOriginales/fondo_piso_x.png", "");
            return;
        }
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

        salaAncho = 1672f / 48f;
        salaAlto = 941f / 48f;
        interiorGo = new GameObject("PisoX");
        var dibujo = interiorGo.AddComponent<SpriteRenderer>();
        dibujo.sprite = RecursosCampus.ComoSprite(tex, tex.width / salaAncho, new Vector2(0.5f, 0.5f));
        dibujo.sortingOrder = -20;
        interiorGo.transform.position = new Vector3(salaAncho * 0.5f, salaAlto * 0.5f, 0f);
        marcoGo = new GameObject("Marco");
        Borde(0.4f, salaAlto * 0.5f, 0.8f, salaAlto);
        Borde(salaAncho - 0.4f, salaAlto * 0.5f, 0.8f, salaAlto);
        Borde(salaAncho * 0.5f, salaAlto - 0.4f, salaAncho, 0.8f);
        Borde(salaAncho * 0.5f, 0.4f, salaAncho, 0.8f);
        portalPiso = PisoSiete.Mundo(882, 590);
        if (jugador != null) jugador.transform.localScale = Vector3.one;
        PonerJugador(PisoSiete.Mundo(105, 590));
        NivelSiete = interiorGo.AddComponent<PisoSiete>();
        NivelSiete.Preparar(this, jugador.transform);
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
        PonerJugador(AscensorPB);
    }

    public void DialogoPisoSiete(string titulo, string cuerpo)
    {
        Decir(titulo, cuerpo, "");
        if (jugador != null) jugador.Detener();
    }

    public void RegresarDelPisoSiete()
    {
        panel = PanelCampus.Ninguno;
        SalirPisoX();
        AbrirAscensor();
        panel = PanelCampus.Ascensor;
    }

    public void ProbarPisoSiete()
    {
        EstadoPartida.Reiniciar();
        AplicarMundo();
        EstadoPartida.desperto = true;
        EstadoPartida.pista = true;
        EstadoPartida.sistemasListo = true;
        BotonXListo = true;
        estamina = 100f;
        jugando = true;
        panel = PanelCampus.Ninguno;
        EntrarPisoX();
    }

    void SalirSala()
    {
        if (!dentro) return;
        DescartarExteriorAcademico();
        EstadoPartida.enPisoX = false;
        NivelSiete = null;
        dentro = false;
        salaId = "";
        pisoNumero = 0;
        if (jugador != null) jugador.transform.localScale = Vector3.one;
        if (interiorGo != null){interiorGo.SetActive(false);Destroy(interiorGo);}
        if (marcoGo != null){marcoGo.SetActive(false);Destroy(marcoGo);}
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
        jugador.SincronizarPies();
        Physics2D.SyncTransforms();
        sitio=jugador.BuscarLugarLibre(sitio,new Vector2(dentro?salaAncho:40f,dentro?salaAlto:20f));
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
