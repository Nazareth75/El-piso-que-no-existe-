using UnityEngine;

public class PerseguidorCampus : MonoBehaviour
{
    public Transform objetivo;
    public Sprite[] frames;
    public float velocidad = 4.7f;
    public bool persigue;

    SpriteRenderer dibujo;
    float anim;

    void Awake()
    {
        dibujo = GetComponent<SpriteRenderer>();
        if (dibujo != null) dibujo.enabled = false;
    }

    void Update()
    {
        if (dibujo == null) return;
        dibujo.enabled = persigue;
        if (!persigue || objetivo == null) return;

        var juego = DirectorCampus.Instancia;
        if (juego != null && juego.bloqueado) return;

        Vector2 pos = transform.position;
        Vector2 dir = (Vector2)objetivo.position - pos;
        if (dir.magnitude > 0.04f)
        {
            dir.Normalize();
            transform.position = pos + dir * velocidad * Time.deltaTime;
            dibujo.flipX = dir.x < 0f;
        }

        if (frames != null && frames.Length > 0)
        {
            anim += Time.deltaTime * 10f;
            dibujo.sprite = frames[Mathf.FloorToInt(anim) % frames.Length];
        }

        dibujo.color = new Color(0.42f, 0.16f, 0.58f, 1f);
        dibujo.sortingOrder = 130 - Mathf.RoundToInt(transform.position.y * 3f);
    }
}
