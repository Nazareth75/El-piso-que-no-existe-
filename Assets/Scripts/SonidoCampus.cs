using System.Collections.Generic;
using UnityEngine;

public class SonidoCampus : MonoBehaviour
{
    public static SonidoCampus Instancia { get; private set; }
    readonly Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();
    readonly Dictionary<string, float> ultimos = new Dictionary<string, float>();
    readonly List<AudioSource> voces = new List<AudioSource>();
    AudioSource ambienteA, ambienteB;
    string ambienteActual = "";
    bool usarA;
    int siguienteVoz;
    PanelCampus panelAnterior;
    bool primerPanel = true;
    float pasoHasta, latidoHasta, amenazaHasta;
    bool alternarPaso;
    public float volumen = .75f;
    public float ambientes = .55f;
    public float efectos = .8f;
    public int ClipsCargados => clips.Count;

    public static readonly string[] Biblioteca =
    {
        "ui_click", "ui_abrir", "ui_cerrar", "dialogo", "recoger", "mision", "correcto", "error", "huella",
        "ascensor_llegada", "curar", "tarjeta", "transmision", "energia", "victoria", "muerte",
        "paso_1", "paso_2", "metal_1", "metal_2", "puerta", "ascensor_motor", "golpe", "escudo",
        "pulso", "aturdido", "electrico", "toxico", "alarma", "monstruo", "alerta_monstruo", "latido",
        "portal", "desmayo", "anclajes", "amb_menu", "amb_campus", "amb_interior", "amb_vacio", "amb_pisox"
    };

    public static void Iniciar(GameObject propietario)
    {
        if (Instancia == null) propietario.AddComponent<SonidoCampus>();
    }

    void Awake()
    {
        Instancia = this;
        volumen = PlayerPrefs.GetFloat("audio.general", .75f);
        ambientes = PlayerPrefs.GetFloat("audio.ambientes", .55f);
        efectos = PlayerPrefs.GetFloat("audio.efectos", .8f);
        foreach (var clip in Resources.LoadAll<AudioClip>("AudioCampus")) clips[clip.name] = clip;
        for (int i = 0; i < 8; i++) voces.Add(Fuente("Efecto " + i));
        ambienteA = Fuente("Ambiente A"); ambienteA.loop = true;
        ambienteB = Fuente("Ambiente B"); ambienteB.loop = true;
        Debug.Log("Audio campus listo: " + clips.Count + " clips.");
    }

    AudioSource Fuente(string nombre)
    {
        var go = new GameObject(nombre); go.transform.SetParent(transform, false);
        var audio = go.AddComponent<AudioSource>(); audio.playOnAwake = false;
        audio.spatialBlend = 0; audio.volume = 0; return audio;
    }

    public static void Emitir(string id, float intensidad = 1f, float tono = 1f)
    {
        if (Instancia != null) Instancia.Reproducir(id, intensidad, tono, 0);
    }

    public static void EnPosicion(string id, Vector2 posicion, float intensidad = 1f)
    {
        if (Instancia == null || Camera.main == null) return;
        Vector2 diferencia = posicion - (Vector2)Camera.main.transform.position;
        float atenuacion = Mathf.Clamp01(1f - diferencia.magnitude / 12f);
        if (atenuacion > 0) Instancia.Reproducir(id, intensidad * atenuacion, Random.Range(.92f, 1.08f), Mathf.Clamp(diferencia.x / 8f, -.8f, .8f));
    }

    void Reproducir(string id, float intensidad, float tono, float panorama)
    {
        if (!clips.TryGetValue(id, out var clip)) return;
        float intervalo = id == "monstruo" ? 1.5f : id == "alerta_monstruo" ? .8f : .045f;
        if (ultimos.TryGetValue(id, out float ultimo) && Time.unscaledTime - ultimo < intervalo) return;
        ultimos[id] = Time.unscaledTime;
        AudioSource fuente = null;
        foreach (var voz in voces) if (!voz.isPlaying) { fuente = voz; break; }
        if (fuente == null) fuente = voces[siguienteVoz++ % voces.Count];
        fuente.clip = clip; fuente.pitch = Mathf.Clamp(tono, .6f, 1.5f);
        fuente.panStereo = panorama; fuente.volume = Mathf.Clamp01(volumen * efectos * intensidad);
        fuente.Play();
    }

    public static bool Boton(Rect rect, string texto, GUIStyle estilo = null)
    {
        bool pulsado = estilo == null ? GUI.Button(rect, texto) : GUI.Button(rect, texto, estilo);
        if (pulsado) Emitir("ui_click", .45f);
        return pulsado;
    }

    public static void Paso(bool corriendo, bool metal)
    {
        var audio = Instancia;
        if (audio == null || Time.time < audio.pasoHasta) return;
        audio.pasoHasta = Time.time + (corriendo ? .27f : .43f);
        audio.alternarPaso = !audio.alternarPaso;
        Emitir((metal ? "metal_" : "paso_") + (audio.alternarPaso ? "1" : "2"), corriendo ? .52f : .34f, Random.Range(.94f, 1.06f));
    }

    public static void Amenaza() { if (Instancia != null) Instancia.amenazaHasta = Time.time + 1f; }

    void Update()
    {
        var juego = DirectorCampus.Instancia;
        if (juego == null) return;
        if (primerPanel) { panelAnterior = juego.panel; primerPanel = false; }
        if (juego.panel != panelAnterior)
        {
            string sonido = juego.panel == PanelCampus.Muerte ? "muerte" : juego.panel == PanelCampus.Fin ? "victoria" :
                juego.panel == PanelCampus.Ascensor ? "ascensor_llegada" : juego.panel == PanelCampus.Frase ? "dialogo" :
                juego.panel == PanelCampus.Ninguno ? "ui_cerrar" : "ui_abrir";
            Emitir(sonido, .65f); panelAnterior = juego.panel;
        }
        string deseado = !juego.jugando ? "amb_menu" : EstadoPartida.enPisoX ? "amb_pisox" :
            EstadoPartida.desperto ? "amb_vacio" : juego.EnAulaAcademica ? "amb_interior" : "amb_campus";
        if (deseado != ambienteActual && clips.TryGetValue(deseado, out var clip))
        {
            ambienteActual = deseado; usarA = !usarA;
            var entrante = usarA ? ambienteA : ambienteB;
            entrante.Stop(); entrante.clip = clip; entrante.volume = 0; entrante.Play();
        }
        float nivel = volumen * ambientes * (juego.panel == PanelCampus.Pausa ? .35f : 1f);
        ambienteA.volume = Mathf.MoveTowards(ambienteA.volume, usarA ? nivel : 0, Time.unscaledDeltaTime * .35f);
        ambienteB.volume = Mathf.MoveTowards(ambienteB.volume, usarA ? 0 : nivel, Time.unscaledDeltaTime * .35f);
        if (juego.jugando && !juego.bloqueado && (juego.estamina < 18 || Time.time < amenazaHasta) && Time.time > latidoHasta)
        { Emitir("latido", .45f); latidoHasta = Time.time + .9f; }
    }

    public void Controles(Rect rect)
    {
        GUI.Box(rect, "SONIDO");
        volumen = Deslizador(rect, 28, "General", volumen);
        ambientes = Deslizador(rect, 57, "Ambiente", ambientes);
        efectos = Deslizador(rect, 86, "Efectos", efectos);
    }

    float Deslizador(Rect rect, float y, string nombre, float valor)
    {
        GUI.Label(new Rect(rect.x + 12, rect.y + y, 85, 24), nombre);
        float nuevo = GUI.HorizontalSlider(new Rect(rect.x + 100, rect.y + y + 7, rect.width - 145, 18), valor, 0, 1);
        GUI.Label(new Rect(rect.xMax - 40, rect.y + y, 35, 24), Mathf.RoundToInt(nuevo * 100) + "%");
        return nuevo;
    }

    void OnDisable()
    {
        PlayerPrefs.SetFloat("audio.general", volumen); PlayerPrefs.SetFloat("audio.ambientes", ambientes); PlayerPrefs.SetFloat("audio.efectos", efectos);
        if (Instancia == this) Instancia = null;
    }
}
