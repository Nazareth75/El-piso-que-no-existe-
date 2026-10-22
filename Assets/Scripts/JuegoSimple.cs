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
    public bool Ganaste => ganaste;
    public bool Termino => gameOver;

    void Awake()
    {
        cuerpoPlayer = GetComponent<Rigidbody2D>();
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
        if (!preparado || gameOver || Piso7.Core.GameMenu.BloqueaJuego) return;
        float direccion = 0;
        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed) direccion -= 1;
            if (Keyboard.current.dKey.isPressed) direccion += 1;
        }
        cuerpoPlayer.linearVelocity = new Vector2(direccion * velocidadPlayer, cuerpoPlayer.linearVelocity.y);

        float diferencia = transform.position.x - monstruo.position.x;
        float paso = Mathf.Clamp(diferencia / Time.fixedDeltaTime, -velocidadMonstruo, velocidadMonstruo);
        cuerpoMonstruo.linearVelocity = new Vector2(paso, cuerpoMonstruo.linearVelocity.y);
        if (dibujoMonstruo != null && Mathf.Abs(diferencia) > 0.01f)
            dibujoMonstruo.flipX = diferencia < 0;
    }

    void OnCollisionEnter2D(Collision2D choque)
    {
        if (TocaVictoria(choque.transform)) { Ganar(); return; }
        if (choque.transform == monstruo) Perder();
    }

    void OnTriggerEnter2D(Collider2D otro)
    {
        if (TocaVictoria(otro.transform)) { Ganar(); return; }
        if (otro.transform == monstruo) Perder();
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
    }

    public void Reiniciar()
    {
        if (!preparado) return;
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
        if (!gameOver) return;
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
