using UnityEngine;
using UnityEngine.InputSystem;

// Un solo controlador para el movimiento, la persecucion y el panel final.
[RequireComponent(typeof(Rigidbody2D))]
public class JuegoSimple : MonoBehaviour
{
    public Transform monstruo;
    public float velocidadPlayer = 5f;
    public float velocidadMonstruo = 2f;

    Rigidbody2D cuerpoPlayer;
    Rigidbody2D cuerpoMonstruo;
    SpriteRenderer dibujoMonstruo;
    Animator animacionMonstruo;
    Vector3 inicioPlayer, inicioMonstruo;
    bool gameOver;
    bool ganaste;
    bool preparado;
    Collider2D playerHitbox, enemyHitbox;
    float stunnedUntil;
    const float AttackReach = 2.5f;
    const int AttackDamage = 2;
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
        playerHitbox = GetComponent<Collider2D>();
        enemyHitbox = monstruo.GetComponent<Collider2D>();
        preparado = true;
        dibujoMonstruo = monstruo.GetComponent<SpriteRenderer>();
        animacionMonstruo = monstruo.GetComponent<Animator>();
        if (dibujoMonstruo != null) enemyColor = dibujoMonstruo.color;
        inicioPlayer = transform.position;
        inicioMonstruo = monstruo.position;
        foreach (var objeto in FindObjectsByType<Transform>(FindObjectsSortMode.None))
        {
            if (!EsActivadorVictoria(objeto.name)) continue;
            var activador = objeto.GetComponent<Collider2D>();
            if (activador == null) activador = objeto.gameObject.AddComponent<CircleCollider2D>();
            activador.isTrigger = true;
        }
    }

    void FixedUpdate()
    {
        if (!preparado || gameOver || EncounterCleared || Piso7.Core.GameMenu.BloqueaJuego) return;
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
        if (CampusCombat && Time.time < stunnedUntil)
        {
            cuerpoMonstruo.linearVelocity = new Vector2(0f, cuerpoMonstruo.linearVelocity.y);
            return;
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
        if (!CampusCombat || !preparado || gameOver || EncounterCleared || Piso7.Core.GameMenu.BloqueaJuego) return;
        var keyboard = Keyboard.current;
        var mouse = Mouse.current;
        if ((keyboard != null && keyboard.fKey.wasPressedThisFrame) || (mouse != null && mouse.rightButton.wasPressedThisFrame) ||
            (Gamepad.current != null && Gamepad.current.rightShoulder.wasPressedThisFrame) || Piso7.Core.MobileControls.Attack) TryAttack();
    }

    void LateUpdate()
    {
        if (visual != null && attackVisualUntil > 0f && Time.time >= attackVisualUntil)
        { visual.localScale = visualScale; attackVisualUntil = 0f; }
        if (dibujoMonstruo != null && Time.time >= hitFlashUntil) dibujoMonstruo.color = enemyColor;
    }

    public bool TryAttack()
    {
        if (!CampusCombat || !preparado || gameOver || EncounterCleared || Piso7.Core.GameMenu.BloqueaJuego || Time.time < nextAttack) return false;
        nextAttack = Time.time + 0.4f;
        if (visual != null)
        {
            if (attackVisualUntil <= 0f) visualScale = visual.localScale;
            visual.localScale = visualScale * 1.1f; attackVisualUntil = Time.time + 0.13f;
        }
        if (playerHitbox == null || enemyHitbox == null || !enemyHitbox.enabled || !cuerpoMonstruo.simulated) return false;
        // Measure the space between actual hitboxes, never scaled transform centers.
        var separation = Physics2D.Distance(playerHitbox, enemyHitbox);
        if (!separation.isValid || (!separation.isOverlapped && separation.distance > AttackReach)) return false;
        enemyHealth = Mathf.Max(0, enemyHealth - AttackDamage);
        stunnedUntil = Time.time + 1.2f;
        nextDamage = Mathf.Max(nextDamage, stunnedUntil);
        cuerpoMonstruo.linearVelocity = new Vector2(0f, cuerpoMonstruo.linearVelocity.y);
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
        if (!CampusCombat) { Perder(); return; }
        if (!preparado || gameOver || EncounterCleared || Piso7.Core.GameMenu.BloqueaJuego || Time.time < nextDamage) return;
        if (Time.time < stunnedUntil || playerHitbox == null || enemyHitbox == null) return;
        var contact = Physics2D.Distance(playerHitbox, enemyHitbox);
        if (!contact.isValid || (!contact.isOverlapped && contact.distance > 0.03f)) return;
        nextDamage = Time.time + 1.5f;
        var session = Piso7.Core.GameSession.Ensure();
        session.ApplyDamage(10f);
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
        enemyHealth = 3; nextAttack = nextDamage = stunnedUntil = 0f; EncounterCleared = false;
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
    }

    void OnGUI()
    {
        if (CampusCombat && !gameOver && !EncounterCleared && !Piso7.Core.GameMenu.BloqueaJuego)
        {
            float hudWidth = Mathf.Min(430f, Screen.width * 0.45f);
            var area = new Rect(Screen.width - hudWidth - 15f, 15f, hudWidth, 70f);
            var hudSprite = Resources.Load<Sprite>("Menu/panel");
            if (hudSprite != null) GUI.DrawTexture(area, hudSprite.texture);
            var style = new GUIStyle(GUI.skin.label) { fontSize = Mathf.Clamp(Screen.width / 65, 12, 20), alignment = TextAnchor.MiddleCenter, normal = { textColor = new Color(0.12f, 0.18f, 0.28f) } };
            GUI.Label(area, "Vida: " + Piso7.Core.GameSession.Ensure().Health + "   Enemigo: " + enemyHealth + "/3\n" + (Application.isMobilePlatform ? "Botón ATACAR: dos golpes" : "F / clic derecho: dos golpes"), style);
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



