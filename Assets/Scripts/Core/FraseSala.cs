using Piso7.Interaction;
using UnityEngine;

namespace Piso7.Core
{
    /// <summary>
    /// Muestra una frase corta cuando el jugador está cerca y pulsa E.
    /// </summary>
    public class FraseSala : MonoBehaviour, IInteractable
    {
        [SerializeField] string nombre = "Compañera";
        [TextArea(2, 4)]
        [SerializeField] string frase = "Hoy el aula de la mañana está cerrada. Nos dijeron que esperemos aquí.";
        [SerializeField] string aviso = "Pulsa E para hablar";
        [SerializeField] Color camisa = new Color(0.25f, 0.38f, 0.62f);

        bool _abierta;
        SpriteRenderer _dibujo;

        public string Prompt => _abierta ? "Pulsa E para cerrar" : aviso;
        public bool CanInteract => true;

        void Awake()
        {
            var col = GetComponent<BoxCollider2D>();
            if (col == null)
                col = gameObject.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            if (col.size.sqrMagnitude < 0.01f)
                col.size = new Vector2(1.1f, 1.9f);

            _dibujo = GetComponent<SpriteRenderer>();
            if (_dibujo == null)
                _dibujo = gameObject.AddComponent<SpriteRenderer>();
            if (_dibujo.sprite == null)
                _dibujo.sprite = CrearFigura(camisa);
            _dibujo.sortingOrder = 6;
        }

        public void Interact(PlayerInteractor interactor)
        {
            if (!Cerca())
                return;
            _abierta = !_abierta;
        }

        void Update()
        {
            if (!Cerca())
                _abierta = false;
        }

        bool Cerca()
        {
            var jugador = GameObject.FindGameObjectWithTag("Player");
            if (jugador == null)
                return false;
            return Vector2.Distance(jugador.transform.position, transform.position) <= 1.7f;
        }

        void OnGUI()
        {
            if (GameMenu.BloqueaJuego || !Cerca())
                return;

            DibujarNombre();
            if (_abierta)
                DibujarFrase();
            else
                DibujarAviso();
        }

        void DibujarNombre()
        {
            var camara = Camera.main;
            if (camara == null)
                return;

            var punto = camara.WorldToScreenPoint(transform.position + Vector3.up * 1.25f);
            if (punto.z < 0f)
                return;

            var estilo = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 16,
                fontStyle = FontStyle.Bold
            };
            estilo.normal.textColor = Color.white;
            float y = Screen.height - punto.y;
            GUI.Label(new Rect(punto.x - 90f, y - 12f, 180f, 24f), nombre, estilo);
        }

        void DibujarAviso()
        {
            var estilo = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 18
            };
            estilo.normal.textColor = Color.white;
            GUI.Label(new Rect(0f, Screen.height - 52f, Screen.width, 28f), aviso, estilo);
        }

        void DibujarFrase()
        {
            float ancho = Mathf.Min(760f, Screen.width - 48f);
            var panel = new Rect((Screen.width - ancho) * 0.5f, Screen.height - 168f, ancho, 128f);
            GUI.Box(panel, GUIContent.none);

            var titulo = new GUIStyle(GUI.skin.label)
            {
                fontSize = 20,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.UpperLeft
            };
            titulo.normal.textColor = new Color(0.95f, 0.9f, 0.7f);

            var cuerpo = new GUIStyle(GUI.skin.label)
            {
                fontSize = 20,
                wordWrap = true,
                alignment = TextAnchor.UpperLeft
            };
            cuerpo.normal.textColor = Color.white;

            var cierre = new GUIStyle(cuerpo) { fontSize = 15 };
            cierre.normal.textColor = new Color(0.8f, 0.8f, 0.8f);

            GUI.Label(new Rect(panel.x + 18f, panel.y + 12f, panel.width - 36f, 28f), nombre, titulo);
            GUI.Label(new Rect(panel.x + 18f, panel.y + 42f, panel.width - 36f, 52f), frase, cuerpo);
            GUI.Label(new Rect(panel.x + 18f, panel.y + 96f, panel.width - 36f, 22f), "Pulsa E para cerrar", cierre);
        }

        static Sprite CrearFigura(Color camisa)
        {
            const int w = 8;
            const int h = 16;
            var tex = new Texture2D(w, h) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
            var vacio = new Color(0f, 0f, 0f, 0f);
            var piel = new Color(0.93f, 0.76f, 0.62f);
            var pelo = new Color(0.18f, 0.12f, 0.1f);
            var pantalon = new Color(0.16f, 0.17f, 0.22f);
            var zapato = new Color(0.12f, 0.1f, 0.1f);

            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                tex.SetPixel(x, y, vacio);

            Pintar(tex, 3, 4, 13, 15, piel);
            Pintar(tex, 2, 5, 14, 15, pelo);
            Pintar(tex, 2, 5, 8, 12, camisa);
            Pintar(tex, 2, 5, 3, 7, pantalon);
            Pintar(tex, 2, 3, 1, 2, zapato);
            Pintar(tex, 4, 5, 1, 2, zapato);
            tex.Apply();

            return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0f), 8f);
        }

        static void Pintar(Texture2D tex, int x0, int x1, int y0, int y1, Color color)
        {
            for (int y = y0; y <= y1; y++)
            for (int x = x0; x <= x1; x++)
                tex.SetPixel(x, y, color);
        }
    }
}
