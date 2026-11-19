using UnityEngine;
using UnityEngine.InputSystem;

// La entidad se evita: no hay combate ni puertas bloqueadas en la entrada.
[RequireComponent(typeof(Rigidbody2D))]
public class JuegoSimple : MonoBehaviour
{
    public Transform monstruo;
    public float velocidadPlayer = 5f;
    public float velocidadMonstruo = 3.8f;
    public bool persecucionEscaleras;
    public Transform zonaEscape;
    public float TiempoPersecucion { get; private set; }
    public bool EnEscenarioOscuro => false;
    public int EnemyHealth => 3;
    public bool CampusCombat => campusPlayer != null;
    public bool EncounterCleared { get; private set; }
    public bool BlocksTraversal => persecucionEscaleras && !EncounterCleared;
    public bool Ganaste => EncounterCleared;
    public bool Termino => gameOver;

    Rigidbody2D cuerpoPlayer, cuerpoMonstruo;
    Piso7.Player.PlayerController2D campusPlayer;
    Vector3 inicioPlayer, inicioMonstruo;
    SpriteRenderer dibujoMonstruo;
    AudioSource tension;
    AudioClip sonidoTension;
    bool preparado, gameOver;

    void Awake()
    {
        cuerpoPlayer = GetComponent<Rigidbody2D>();
        campusPlayer = GetComponent<Piso7.Player.PlayerController2D>();
        if (monstruo == null || (cuerpoMonstruo = monstruo.GetComponent<Rigidbody2D>()) == null)
        {
            Debug.LogError("JuegoSimple necesita una entidad con Rigidbody2D.", this);
            enabled = false;
            return;
        }
        preparado = true;
        inicioPlayer = transform.position;
        inicioMonstruo = monstruo.position;
        dibujoMonstruo = monstruo.GetComponent<SpriteRenderer>();
        // El volumen de la entidad detecta al estudiante sin impedirle saltar o pasar.
        foreach (var collider in monstruo.GetComponentsInChildren<Collider2D>()) collider.isTrigger = true;
        cuerpoMonstruo.gravityScale = 0f;
        cuerpoMonstruo.freezeRotation = true;
        CrearTension();
    }

    void Start()
    {
        if (!preparado) return;
        Piso7.Core.GameSession.Ensure().SetObjective(persecucionEscaleras ?
            "Huye por las escaleras hasta la zona verde. Shift: correr; Espacio: saltar." :
            "Cruza la entrada y evita a la entidad. Espacio: saltar.");
        if (persecucionEscaleras) campusPlayer?.StartEscapeBoost();
    }

    void CrearTension()
    {
        // Pulso sintetizado: el mismo clip sirve en ambas escenas sin recursos externos.
        const int rate = 22050;
        var samples = new float[rate * 2];
        for (int i = 0; i < samples.Length; i++)
        {
            float t = (float)i / rate;
            float phase = t % 0.5f;
            float envelope = Mathf.Exp(-phase * 18f) * Mathf.Min(1f, phase * 150f);
            samples[i] = Mathf.Sin(2f * Mathf.PI * 65f * t) * envelope * 0.3f;
        }
        sonidoTension = AudioClip.Create("TensionEntidad", samples.Length, 1, rate, false);
        sonidoTension.SetData(samples, 0);
        tension = gameObject.AddComponent<AudioSource>();
        tension.clip = sonidoTension;
        tension.loop = true;
        tension.playOnAwake = false;
        tension.spatialBlend = 0f;
        tension.volume = 0.45f;
    }

    void Update()
    {
        if (!preparado) return;
        bool activo = !gameOver && !EncounterCleared && !Piso7.Core.GameMenu.BloqueaJuego;
        tension.volume = Piso7.Core.MenuAudio.SfxVolume * 0.6f;
        if (activo && !tension.isPlaying) tension.Play();
        if (!activo && tension.isPlaying) tension.Stop();
        if (!activo) return;
        TiempoPersecucion += Time.deltaTime;
        // Primero se comprueba el refugio: dentro ya no puede atrapar la entidad.
        if (persecucionEscaleras && zonaEscape != null)
        {
            var safe = zonaEscape.GetComponent<Collider2D>();
            if (safe != null && safe.bounds.Intersects(GetComponent<Collider2D>().bounds))
            { Ganar(); return; }
        }
        if (transform.position.y < -4f) Perder();
    }

    void FixedUpdate()
    {
        if (!preparado) return;
        if (gameOver || EncounterCleared || Piso7.Core.GameMenu.BloqueaJuego)
        { cuerpoMonstruo.linearVelocity = Vector2.zero; return; }
        if (campusPlayer == null)
        {
            float input = Piso7.Core.MobileControls.Move;
            if (Keyboard.current != null)
            {
                if (Keyboard.current.aKey.isPressed) input -= 1f;
                if (Keyboard.current.dKey.isPressed) input += 1f;
            }
            cuerpoPlayer.linearVelocity = new Vector2(Mathf.Clamp(input, -1f, 1f) * velocidadPlayer, cuerpoPlayer.linearVelocity.y);
        }
        float difference = transform.position.x - monstruo.position.x;
        cuerpoMonstruo.linearVelocity = new Vector2(Mathf.Clamp(difference / Time.fixedDeltaTime, -velocidadMonstruo, velocidadMonstruo), 0f);
        if (dibujoMonstruo != null && Mathf.Abs(difference) > 0.01f) dibujoMonstruo.flipX = difference < 0f;
    }

    bool EsEntidad(Transform other) => other == monstruo || other.IsChildOf(monstruo);
    void OnTriggerEnter2D(Collider2D other) { Detectar(other); }
    void OnTriggerStay2D(Collider2D other) { Detectar(other); }
    void Detectar(Collider2D other)
    {
        if (gameOver || EncounterCleared || Piso7.Core.GameMenu.BloqueaJuego) return;
        if (persecucionEscaleras && zonaEscape != null)
        {
            var safe = zonaEscape.GetComponent<Collider2D>();
            if (safe != null && safe.bounds.Intersects(GetComponent<Collider2D>().bounds)) { Ganar(); return; }
        }
        if (EsEntidad(other.transform)) Perder();
    }
    void OnCollisionEnter2D(Collision2D other)
    {
        if (EsEntidad(other.transform)) Perder();
    }

    // Se conserva la firma para las herramientas antiguas del editor.
    public bool TryAttack() => false;
    public void ContinueTour() { if (EncounterCleared) cuerpoPlayer.simulated = true; }

    void Ganar()
    {
        if (gameOver || EncounterCleared) return;
        EncounterCleared = true;
        cuerpoMonstruo.linearVelocity = Vector2.zero;
        monstruo.gameObject.SetActive(false);
        tension.Stop();
        Piso7.Core.MenuAudio.PauseMusic();
        campusPlayer?.StopEscapeBoost();
        Piso7.Core.GameSession.Ensure().SetObjective("Escapaste. La zona es segura; puedes continuar el recorrido.");
    }

    void Perder()
    {
        if (!preparado || gameOver || EncounterCleared || Piso7.Core.GameMenu.BloqueaJuego) return;
        gameOver = true;
        tension.Stop();
        campusPlayer?.StopEscapeBoost();
        cuerpoPlayer.linearVelocity = cuerpoMonstruo.linearVelocity = Vector2.zero;
        cuerpoPlayer.simulated = cuerpoMonstruo.simulated = false;
        if (Piso7.Core.GameMenu.Instance != null)
            Piso7.Core.GameMenu.Instance.ShowCombatResult(false, Reiniciar, ContinueTour);
    }

    public void Reiniciar()
    {
        if (!preparado) return;
        Piso7.Core.GameSession.Ensure().ResetRun();
        gameOver = EncounterCleared = false;
        TiempoPersecucion = 0f;
        transform.position = inicioPlayer;
        monstruo.position = inicioMonstruo;
        cuerpoPlayer.position = inicioPlayer;
        cuerpoMonstruo.position = inicioMonstruo;
        cuerpoPlayer.linearVelocity = cuerpoMonstruo.linearVelocity = Vector2.zero;
        cuerpoPlayer.simulated = cuerpoMonstruo.simulated = true;
        monstruo.gameObject.SetActive(true);
        Start();
    }

    void OnDisable() { if (tension != null) tension.Stop(); }
    void OnDestroy() { if (sonidoTension != null) Destroy(sonidoTension); }
    void OnGUI()
    {
        if (!gameOver || Piso7.Core.GameMenu.Instance != null) return;
        GUI.Box(new Rect(Screen.width / 2f - 170, Screen.height / 2f - 90, 340, 180), "GAME OVER");
        if (GUI.Button(new Rect(Screen.width / 2f - 90, Screen.height / 2f, 180, 40), "Reintentar")) Reiniciar();
    }
}
