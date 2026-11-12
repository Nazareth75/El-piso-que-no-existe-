using UnityEngine;
using UnityEngine.SceneManagement;

namespace Piso7.Interaction
{
    public class EsconditeBiblioteca : MonoBehaviour
    {
        [SerializeField] Vector2 tamanoZona = new Vector2(2.5f, 1.3f);
        [SerializeField] float velocidadEntidad = 2f;

        Transform jugador;
        Vector3 entidad;
        bool descubierto;
        bool paso;

        void Start()
        {
            var encontrado = GameObject.FindGameObjectWithTag("Player");
            if (encontrado != null) jugador = encontrado.transform;
            entidad = transform.position + Vector3.left * 7f;
        }

        void Update()
        {
            if (descubierto) return;
            entidad += Vector3.right * velocidadEntidad * Time.deltaTime;
            if (jugador != null && Mathf.Abs(entidad.x - jugador.position.x) < .25f)
            {
                if (DentroDeZona()) paso = true;
                else descubierto = true;
            }
        }

        bool DentroDeZona()
        {
            Vector2 diferencia = jugador.position - transform.position;
            return Mathf.Abs(diferencia.x) <= tamanoZona.x * .5f && Mathf.Abs(diferencia.y) <= tamanoZona.y * .5f;
        }

        void OnGUI()
        {
            if (Camera.main == null) return;
            Vector3 centro = Camera.main.WorldToScreenPoint(transform.position);
            float escala = Mathf.Abs(Camera.main.WorldToScreenPoint(transform.position + Vector3.right).x - centro.x);
            var zona = new Rect(centro.x - tamanoZona.x * escala / 2, Screen.height - centro.y - tamanoZona.y * escala / 2,
                tamanoZona.x * escala, tamanoZona.y * escala);
            GUI.Box(zona, "ESCONDITE");

            Vector3 puntoEntidad = Camera.main.WorldToScreenPoint(entidad);
            GUI.Box(new Rect(puntoEntidad.x - 18, Screen.height - puntoEntidad.y - 18, 36, 36), "?");

            if (paso) GUI.Label(new Rect(Screen.width / 2 - 120, 80, 240, 30), "La entidad pasó de largo.");
            if (!descubierto) return;

            var caja = new Rect(Screen.width / 2 - 150, Screen.height / 2 - 70, 300, 140);
            GUI.Box(caja, "TE DESCUBRIÓ");
            GUI.Label(new Rect(caja.x + 45, caja.y + 42, 210, 25), "No estabas en el escondite.");
            if (GUI.Button(new Rect(caja.x + 85, caja.y + 85, 130, 32), "Reintentar"))
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
