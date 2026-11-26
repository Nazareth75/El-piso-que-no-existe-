using UnityEngine;

public class JugadorCampus : MonoBehaviour
{
    public Sprite[] frames;
    public float caminar = 3.8f;
    public float correr = 6.5f;

    Rigidbody2D cuerpo;
    SpriteRenderer dibujo;
    float anim;

    void Awake()
    {
        cuerpo = GetComponent<Rigidbody2D>();
        dibujo = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        var juego = DirectorCampus.Instancia;
        if (juego == null || !juego.jugando || juego.bloqueado)
        {
            if (cuerpo != null) cuerpo.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 eje = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (eje.sqrMagnitude > 1f) eje.Normalize();

        bool quiereCorrer = (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && eje.sqrMagnitude > 0.01f;
        float velocidad = caminar;
        if (quiereCorrer && juego.estamina > 0f)
        {
            velocidad = correr;
            juego.estamina -= 30f * Time.deltaTime;
        }
        else if (juego.estamina < 100f)
        {
            juego.estamina += (eje.sqrMagnitude > 0.01f ? 8f : 18f) * Time.deltaTime;
        }

        juego.estamina = Mathf.Clamp(juego.estamina, 0f, 100f);
        cuerpo.linearVelocity = eje * velocidad;

        if (eje.x < -0.02f) dibujo.flipX = true;
        else if (eje.x > 0.02f) dibujo.flipX = false;

        if (frames != null && frames.Length > 0)
        {
            if (eje.sqrMagnitude > 0.01f) anim += Time.deltaTime * (velocidad > caminar + 0.2f ? 12f : 8f);
            dibujo.sprite = frames[Mathf.FloorToInt(anim) % frames.Length];
        }

        dibujo.sortingOrder = 120 - Mathf.RoundToInt(transform.position.y * 3f);
    }

    public void Detener()
    {
        if (cuerpo != null) cuerpo.linearVelocity = Vector2.zero;
    }
}
