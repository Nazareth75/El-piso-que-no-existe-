using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

// Un solo controlador para el movimiento, la persecucion y el panel final.
[RequireComponent(typeof(Rigidbody2D))]
public class JuegoSimple : MonoBehaviour
{
    public Transform monstruo;
    public float velocidadPlayer = 5f;
    public float velocidadMonstruo = 2f;
    [Header("Persecución en las gradas")]
    public bool persecucionEscaleras;
    public Transform zonaEscape;
    bool desmayado;
    float fundido;
    Quaternion visualRotation;
    [Min(10f)] public float duracionMinimaPersecucion = 10f;
    public float TiempoPersecucion => tiempoPersecucion;
    public bool EnEscenarioOscuro => escenarioOscuro != null && escenarioOscuro.activeSelf;
    float tiempoPersecucion;
    GameObject escenarioOscuro;
    readonly List<GameObject> objetosCampus = new List<GameObject>();
    Transform escapeCampus;
    GameObject puertaOscura;
    Sprite spriteEscenario;
    Material materialEscenario;
    Camera camaraPersecucion;
    Piso7.Campus.CampusPanCamera camaraCampus;
    Vector3 posicionCamaraCampus;
    bool camaraCampusActiva;

    Rigidbody2D cuerpoPlayer;
    Rigidbody2D cuerpoMonstruo;
    SpriteRenderer dibujoMonstruo;
    Animator animacionMonstruo;
    Vector3 inicioPlayer, inicioMonstruo;
    bool gameOver;
    bool ganaste;
    bool preparado;
    Piso7.Player.PlayerController2D campusPlayer;
    int enemyHealth = 3;
    float nextAttack, nextDamage, hitFlashUntil;
    Color enemyColor;
    Vector3 visualScale;
    Transform visual;
    float attackVisualUntil;
    public int EnemyHealth => enemyHealth;
    public bool CampusCombat => campusPlayer != null;
    public bool EncounterCleared { get; private set; }
    public bool BlocksTraversal => CampusCombat && !EncounterCleared;

    public bool Ganaste => ganaste;
    public bool Termino => gameOver;

    void Awake()
    {
        cuerpoPlayer = GetComponent<Rigidbody2D>();
        campusPlayer = GetComponent<Piso7.Player.PlayerController2D>();
        visual = transform.Find("Visual");
        if (campusPlayer != null) Piso7.Core.GameSession.Ensure();
        if (monstruo == null || cuerpoPlayer == null)
        {
            Debug.LogError("JuegoSimple: falta el monstruo o el Rigidbody2D del Player.", this);
            enabled = false;
            return;
        }
        cuerpoMonstruo = monstruo.GetComponent<Rigidbody2D>();
        if (cuerpoMonstruo == null)
        {
            Debug.LogError("JuegoSimple: el monstruo necesita Rigidbody2D.", this);
            enabled = false;
            return;
        }
        preparado = true;
        dibujoMonstruo = monstruo.GetComponent<SpriteRenderer>();
        animacionMonstruo = monstruo.GetComponent<Animator>();
        if (dibujoMonstruo != null) enemyColor = dibujoMonstruo.color;
        inicioPlayer = transform.position;
        inicioMonstruo = monstruo.position;
        if (visual != null) visualRotation = visual.localRotation;
        foreach (var objeto in FindObjectsByType<Transform>(FindObjectsSortMode.None))
        {
            if (!EsActivadorVictoria(objeto.name)) continue;
            var activador = objeto.GetComponent<Collider2D>();
            if (activador == null) activador = objeto.gameObject.AddComponent<CircleCollider2D>();
            activador.isTrigger = true;
        }
    }

    IEnumerator Start()
    {
        if (!persecucionEscaleras || !preparado) yield break;
        desmayado = true;
        campusPlayer.enabled = false;
        cuerpoPlayer.linearVelocity = Vector2.zero;
        cuerpoPlayer.simulated = false;
        cuerpoMonstruo.simulated = false;
        Piso7.Core.GameSession.Ensure().SetObjective("Un mareo te invade al llegar a las gradas...");
        float t = 0f;
        while (t < 1.2f)
        {
            if (!Piso7.Core.GameMenu.BloqueaJuego)
            {
                t += Time.deltaTime;
                fundido = Mathf.Clamp01(t / 1.2f);
                if (visual != null) visual.localRotation = visualRotation * Quaternion.Euler(0, 0, -90f * fundido);
            }
            yield return null;
        }
        yield return new WaitForSeconds(0.8f);
        CrearEscenarioOscuro();
        if (visual != null) visual.localRotation = visualRotation;
        transform.position = inicioPlayer;
        cuerpoPlayer.position = inicioPlayer;
        while (fundido > 0f)
        {
            if (!Piso7.Core.GameMenu.BloqueaJuego) fundido = Mathf.Max(0f, fundido - Time.deltaTime);
            yield return null;
        }
        desmayado = false;
        cuerpoPlayer.simulated = cuerpoMonstruo.simulated = true;
        campusPlayer.enabled = true;
        Piso7.Core.GameSession.Ensure().SetObjective("Despertaste en un pasillo oscuro. ¡Huye! La salida se abre en 10 segundos.");
    }

    // La pesadilla vive dentro de la escena: no depende de modificar la lista de niveles del equipo.
    void CrearEscenarioOscuro()
    {
        escapeCampus = zonaEscape;
        camaraPersecucion = Camera.main;
        if (camaraPersecucion != null)
        {
            posicionCamaraCampus = camaraPersecucion.transform.position;
            camaraCampus = camaraPersecucion.GetComponent<Piso7.Campus.CampusPanCamera>();
            if (camaraCampus != null) { camaraCampusActiva = camaraCampus.enabled; camaraCampus.enabled = false; }
        }
        foreach (var root in gameObject.scene.GetRootGameObjects())
        {
            if (!root.activeSelf || root == gameObject || root == monstruo.gameObject ||
                root.GetComponentInChildren<Camera>() != null ||
                root.GetComponentInChildren<UnityEngine.Rendering.Universal.Light2D>() != null) continue;
            objetosCampus.Add(root);
            root.SetActive(false);
        }
        escenarioOscuro = new GameObject("Pesadilla_PasilloOscuro");
        spriteEscenario = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, Texture2D.whiteTexture.width, Texture2D.whiteTexture.height), Vector2.one * 0.5f, Texture2D.whiteTexture.width);
        materialEscenario = new Material(Shader.Find("Sprites/Default"));
        BloqueOscuro("Fondo", new Vector2(1038f, 4f), new Vector2(85f, 20f), new Color(0.018f, 0.025f, 0.045f), -20, false);
        BloqueOscuro("Suelo", new Vector2(1036f, 0.35f), new Vector2(80f, 0.6f), new Color(0.15f, 0.19f, 0.24f), 0, true);
        BloqueOscuro("Borde luminoso", new Vector2(1036f, 0.67f), new Vector2(80f, 0.035f), new Color(0.23f, 0.45f, 0.5f), 1, false);
        BloqueOscuro("Limite izquierdo", new Vector2(998f, 3f), new Vector2(0.5f, 8f), Color.black, 0, true);
        BloqueOscuro("Limite derecho", new Vector2(1074f, 3f), new Vector2(0.5f, 8f), Color.black, 0, true);
        for (int i = 0; i < 15; i++)
        {
            float x = 1000f + i * 5f;
            BloqueOscuro("Columna", new Vector2(x, 3f), new Vector2(0.4f, 6f), new Color(0.055f, 0.07f, 0.1f), -10, false);
            BloqueOscuro("Luz de emergencia", new Vector2(x, 4.6f), new Vector2(0.65f, 0.1f), new Color(0.65f, 0.06f, 0.09f), -5, false);
        }
        BloqueOscuro("Obstaculo_1", new Vector2(1026f, 0.95f), new Vector2(1.2f, 0.6f), new Color(0.28f, 0.23f, 0.2f), 2, true);
        BloqueOscuro("Obstaculo_2", new Vector2(1047f, 0.95f), new Vector2(1.2f, 0.6f), new Color(0.28f, 0.23f, 0.2f), 2, true);
        puertaOscura = BloqueOscuro("Puerta cerrada", new Vector2(1068f, 3f), new Vector2(0.4f, 6f), new Color(0.7f, 0.08f, 0.12f), 4, true);
        zonaEscape = BloqueOscuro("Salida de la pesadilla", new Vector2(1070f, 2.5f), new Vector2(1.5f, 4f), new Color(0.15f, 0.75f, 0.45f, 0.7f), 3, false).transform;
        zonaEscape.gameObject.AddComponent<BoxCollider2D>().isTrigger = true;
        inicioPlayer = new Vector3(1006f, 1.7f, 0f);
        inicioMonstruo = new Vector3(1000f, 2.1f, 0f);
        cuerpoMonstruo.position = inicioMonstruo;
        monstruo.position = inicioMonstruo;
        // El perseguidor atraviesa obstáculos del sueño, pero sigue tocando al jugador.
        cuerpoMonstruo.gravityScale = 0f;
        monstruo.GetComponent<Collider2D>().isTrigger = true;
        tiempoPersecucion = 0f;
    }

    GameObject BloqueOscuro(string nombre, Vector2 centro, Vector2 tamano, Color color, int orden, bool solido)
    {
        var bloque = new GameObject(nombre);
        bloque.transform.SetParent(escenarioOscuro.transform);
        bloque.transform.position = centro;
        bloque.transform.localScale = new Vector3(tamano.x, tamano.y, 1f);
        var dibujo = bloque.AddComponent<SpriteRenderer>();
        dibujo.sprite = spriteEscenario;
        dibujo.sharedMaterial = materialEscenario;
        dibujo.color = color;
        dibujo.sortingOrder = orden;
        if (solido) bloque.AddComponent<BoxCollider2D>();
        return bloque;
    }

    void VolverALasGradas()
    {
        escenarioOscuro.SetActive(false);
        foreach (var objeto in objetosCampus) if (objeto != null) objeto.SetActive(true);
        zonaEscape = escapeCampus;
        cuerpoPlayer.linearVelocity = Vector2.zero;
        cuerpoPlayer.position = new Vector2(14.5f, 1.7f);
        transform.position = new Vector3(14.5f, 1.7f, 0f);
        if (camaraPersecucion != null) camaraPersecucion.transform.position = posicionCamaraCampus;
        if (camaraCampus != null) camaraCampus.enabled = camaraCampusActiva;
    }

    void FixedUpdate()
    {
        if (!preparado || desmayado || gameOver || EncounterCleared || Piso7.Core.GameMenu.BloqueaJuego) return;
        if (!CampusCombat)
        {
        float direccion = Piso7.Core.MobileControls.Move;
        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed) direccion -= 1;
            if (Keyboard.current.dKey.isPressed) direccion += 1;
        }
        cuerpoPlayer.linearVelocity = new Vector2(direccion * velocidadPlayer, cuerpoPlayer.linearVelocity.y);

        }
        float diferencia = transform.position.x - monstruo.position.x;
        float paso = Mathf.Clamp(diferencia / Time.fixedDeltaTime, -velocidadMonstruo, velocidadMonstruo);
        cuerpoMonstruo.linearVelocity = new Vector2(paso, cuerpoMonstruo.linearVelocity.y);
        if (dibujoMonstruo != null && Mathf.Abs(diferencia) > 0.01f)
            dibujoMonstruo.flipX = diferencia < 0;
    }

    void OnCollisionEnter2D(Collision2D choque)
    {
        if (TocaVictoria(choque.transform) && !CampusCombat) { Ganar(); return; }
        if (choque.transform == monstruo) ContactDamage();
    }

    void OnTriggerEnter2D(Collider2D otro)
    {
        if (TocaVictoria(otro.transform) && !CampusCombat) { Ganar(); return; }
        if (otro.transform == monstruo) ContactDamage();
    }

    void Update()
    {
        if (persecucionEscaleras)
        {
            if (!desmayado && !gameOver && !EncounterCleared && !Piso7.Core.GameMenu.BloqueaJuego)
            {
                tiempoPersecucion += Time.deltaTime;
                if (puertaOscura != null && tiempoPersecucion >= Mathf.Max(10f, duracionMinimaPersecucion)) puertaOscura.SetActive(false);
                if (transform.position.y < -4f) Perder();
            }
            if (!desmayado && !gameOver && !EncounterCleared && !Piso7.Core.GameMenu.BloqueaJuego && tiempoPersecucion >= Mathf.Max(10f, duracionMinimaPersecucion) && zonaEscape != null &&
                GetComponent<Collider2D>().bounds.Intersects(zonaEscape.GetComponent<Collider2D>().bounds)) Ganar();
            return;
        }
        if (!CampusCombat || !preparado || gameOver || EncounterCleared || Piso7.Core.GameMenu.BloqueaJuego) return;
        var keyboard = Keyboard.current;
        var mouse = Mouse.current;
        if ((keyboard != null && keyboard.fKey.wasPressedThisFrame) || (mouse != null && mouse.rightButton.wasPressedThisFrame) ||
            (Gamepad.current != null && Gamepad.current.rightShoulder.wasPressedThisFrame) || Piso7.Core.MobileControls.Attack) TryAttack();
    }

    void LateUpdate()
    {
        if (EnEscenarioOscuro && camaraPersecucion != null)
            camaraPersecucion.transform.position = new Vector3(Mathf.Clamp(transform.position.x + 2.5f, 1007f, 1067f), 2.72f, -10f);
        if (visual != null && attackVisualUntil > 0f && Time.time >= attackVisualUntil)
        { visual.localScale = visualScale; attackVisualUntil = 0f; }
        if (dibujoMonstruo != null && Time.time >= hitFlashUntil) dibujoMonstruo.color = enemyColor;
    }

    public bool TryAttack()
    {
        if (persecucionEscaleras || !CampusCombat || !preparado || gameOver || EncounterCleared || Piso7.Core.GameMenu.BloqueaJuego || Time.time < nextAttack) return false;
        nextAttack = Time.time + 0.4f;
        if (visual != null)
        {
            if (attackVisualUntil <= 0f) visualScale = visual.localScale;
            visual.localScale = visualScale * 1.1f; attackVisualUntil = Time.time + 0.13f;
        }
        Vector2 offset = monstruo.position - transform.position;
        float facing = campusPlayer.FacingRight ? 1f : -1f;
        if (offset.magnitude > 2.1f || offset.x * facing < -0.25f) return false;
        var enemyCollider = monstruo.GetComponent<Collider2D>();
        if (enemyCollider == null || !enemyCollider.enabled || !cuerpoMonstruo.simulated) return false;
        enemyHealth = Mathf.Max(0, enemyHealth - 1);
        hitFlashUntil = Time.time + 0.18f;
        if (dibujoMonstruo != null) dibujoMonstruo.color = Color.red;
        if (enemyHealth == 0) Ganar();
        return true;
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (CampusCombat && collision.transform == monstruo) ContactDamage();
    }

    void ContactDamage()
    {
        if (desmayado || Piso7.Core.GameMenu.BloqueaJuego) return;
        if (persecucionEscaleras) { Perder(); return; }
        if (!CampusCombat) { Perder(); return; }
        if (!preparado || gameOver || EncounterCleared || Piso7.Core.GameMenu.BloqueaJuego || Time.time < nextDamage) return;
        nextDamage = Time.time + 1f;
        var session = Piso7.Core.GameSession.Ensure();
        session.ApplyDamage(25f);
        if (session.Health <= 0f) Perder();
    }

    public void ContinueTour()
    {
        if (!ganaste) return;
        EncounterCleared = true; gameOver = false;
        cuerpoPlayer.simulated = true;
        monstruo.gameObject.SetActive(false);
        Piso7.Core.GameSession.Ensure().SetObjective("Continúa hacia la recepción.");
    }

    void OnDisable()
    {
        if (visual != null && attackVisualUntil > 0f) visual.localScale = visualScale;
    }

    void OnDestroy()
    {
        if (spriteEscenario != null) Destroy(spriteEscenario);
        if (materialEscenario != null) Destroy(materialEscenario);
    }

    static bool EsActivadorVictoria(string nombre)
    {
        return nombre.Replace(" ", "").Replace("_", "").ToLowerInvariant().StartsWith("activadorvictoria");
    }

    static bool TocaVictoria(Transform objeto)
    {
        for (var actual = objeto; actual != null; actual = actual.parent)
            if (EsActivadorVictoria(actual.name)) return true;
        return false;
    }

    void Ganar()
    {
        if (!preparado || gameOver) return;
        if (persecucionEscaleras)
        {
            ganaste = EncounterCleared = true;
            cuerpoMonstruo.linearVelocity = Vector2.zero;
            monstruo.gameObject.SetActive(false);
            if (EnEscenarioOscuro) VolverALasGradas();
            Piso7.Core.GameSession.Ensure().SetObjective("Escapaste. La persecución terminó; las salidas están libres.");
            return;
        }
        ganaste = true;
        Perder();
    }

    void Perder()
    {
        if (!preparado || gameOver) return;
        gameOver = true;
        cuerpoPlayer.linearVelocity = Vector2.zero;
        cuerpoMonstruo.linearVelocity = Vector2.zero;
        cuerpoPlayer.simulated = false;
        cuerpoMonstruo.simulated = false;
        if (animacionMonstruo != null) animacionMonstruo.enabled = false;
        if (CampusCombat && Piso7.Core.GameMenu.Instance != null)
            Piso7.Core.GameMenu.Instance.ShowCombatResult(ganaste, Reiniciar, ContinueTour);
    }

    public void Reiniciar()
    {
        if (!preparado) return;
        if (CampusCombat) Piso7.Core.GameSession.Ensure().ResetRun();
        enemyHealth = 3; nextAttack = nextDamage = 0f; EncounterCleared = false;
        monstruo.gameObject.SetActive(true);
        transform.position = inicioPlayer;
        monstruo.position = inicioMonstruo;
        cuerpoPlayer.position = inicioPlayer;
        cuerpoMonstruo.position = inicioMonstruo;
        cuerpoPlayer.simulated = true;
        cuerpoMonstruo.simulated = true;
        cuerpoPlayer.linearVelocity = Vector2.zero;
        cuerpoMonstruo.linearVelocity = Vector2.zero;
        if (animacionMonstruo != null) animacionMonstruo.enabled = true;
        gameOver = false;
        ganaste = false;
        if (persecucionEscaleras)
        {
            tiempoPersecucion = 0f;
            if (puertaOscura != null) puertaOscura.SetActive(true);
            Piso7.Core.GameSession.Ensure().SetObjective("¡Huye por el pasillo oscuro! Salta los obstáculos. La salida se abre después de 10 segundos.");
        }
    }

    void OnGUI()
    {
        if (persecucionEscaleras)
        {
            if (desmayado)
            {
                GUI.depth = -200;
                var oldColor = GUI.color;
                GUI.color = new Color(0f, 0f, 0f, fundido);
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
                GUI.color = Color.white;
                GUI.Label(new Rect(0, Screen.height * 0.4f, Screen.width, 100), "Te desmayas...\nAlgo te espera en las gradas.",
                    new GUIStyle(GUI.skin.label) { fontSize = 26, alignment = TextAnchor.MiddleCenter });
                GUI.color = oldColor;
            }
            else if (!gameOver && !Piso7.Core.GameMenu.BloqueaJuego)
                GUI.Box(new Rect((Screen.width - 480) / 2f, 12, 480, 55), EncounterCleared ?
                    "ESCAPASTE — despertaste en las gradas. Puedes continuar." :
                    "PASILLO OSCURO · ¡HUYE! → " + (tiempoPersecucion < Mathf.Max(10f, duracionMinimaPersecucion) ?
                    "Salida en " + Mathf.CeilToInt(Mathf.Max(10f, duracionMinimaPersecucion) - tiempoPersecucion) + " s" : "SALIDA ABIERTA") +
                    "\nA/D o flechas · Shift: correr · Espacio: saltar");
            return;
        }
        if (CampusCombat && !gameOver && !EncounterCleared && !Piso7.Core.GameMenu.BloqueaJuego)
        {
            float hudWidth = Mathf.Min(430f, Screen.width * 0.45f);
            var area = new Rect(Screen.width - hudWidth - 15f, 15f, hudWidth, 70f);
            var hudSprite = Resources.Load<Sprite>("Menu/panel");
            if (hudSprite != null) GUI.DrawTexture(area, hudSprite.texture);
            var style = new GUIStyle(GUI.skin.label) { fontSize = Mathf.Clamp(Screen.width / 65, 12, 20), alignment = TextAnchor.MiddleCenter, normal = { textColor = new Color(0.12f, 0.18f, 0.28f) } };
            GUI.Label(area, "Vida: " + Piso7.Core.GameSession.Ensure().Health + "   Enemigo: " + enemyHealth + "/3\n" + (Application.isMobilePlatform ? "Botón ATACAR: tres golpes" : "F / clic derecho: atacar"), style);
        }
        if (!gameOver || CampusCombat) return;
        GUI.depth = -100;
        GUI.Box(new Rect(0, 0, Screen.width, Screen.height), "");
        Rect panel = new Rect((Screen.width - 340) / 2f, (Screen.height - 180) / 2f, 340, 180);
        GUI.Box(panel, "");
        GUIStyle titulo = new GUIStyle(GUI.skin.label) { fontSize = 32, alignment = TextAnchor.MiddleCenter };
        GUI.Label(new Rect(panel.x, panel.y + 20, panel.width, 45), ganaste ? "GANASTE" : "GAME OVER", titulo);
        GUI.Label(new Rect(panel.x, panel.y + 70, panel.width, 25), ganaste ? "" : "El monstruo te atrapo.",
            new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter });
        if (GUI.Button(new Rect(panel.x + 80, panel.y + 115, 180, 40), "Reintentar")) Reiniciar();
    }
}



