using UnityEngine;

public class BotPiso : MonoBehaviour
{
    public Transform objetivo;
    public Sprite[] frames;
    public float velocidad = 2.4f;
    public Vector2 meta;

    SpriteRenderer dibujo;
    float anim;
    Vector2 origen;
    bool haciaMeta = true;

    void Awake()
    {
        dibujo = GetComponent<SpriteRenderer>();
        origen = transform.position;
    }

    void Update()
    {
        var juego = DirectorCampus.Instancia;
        if (juego == null || juego.bloqueado || objetivo == null) return;

        Vector2 pos = transform.position;
        Vector2 jugador = objetivo.position;
        bool caza = Vector2.Distance(pos, jugador) < 4.2f;
        Vector2 destino = caza ? jugador : (haciaMeta ? meta : origen);
        Vector2 dir = destino - pos;
        if (!caza && dir.magnitude < 0.2f)
        {
            haciaMeta = !haciaMeta;
            return;
        }

        if (dir.sqrMagnitude > 0.01f)
        {
            dir.Normalize();
            transform.position = pos + dir * (caza ? velocidad : velocidad * 0.55f) * Time.deltaTime;
            if (dibujo != null) dibujo.flipX = dir.x < 0f;
        }

        if (dibujo != null && frames != null && frames.Length > 0)
        {
            anim += Time.deltaTime * (caza ? 10f : 6f);
            dibujo.sprite = frames[Mathf.FloorToInt(anim) % frames.Length];
            dibujo.sortingOrder = 90 - Mathf.RoundToInt(transform.position.y * 3f);
        }

        if (Vector2.Distance(transform.position, jugador) < 0.75f)
            juego.AlcanzadoEnPiso();
    }
}
