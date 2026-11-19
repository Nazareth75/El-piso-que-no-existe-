using Piso7.Interaction;
using UnityEngine;

namespace Piso7.Core
{
    /// <summary>
    /// Caso de Medicina en la clínica. Ayudar suma un punto. Seguir de largo no suma.
    /// </summary>
    public class CasoClinica : MonoBehaviour, IInteractable
    {
        const string Caso =
            "Un estudiante se dobló el tobillo en las gradas. Puede caminar, pero pide que alguien lo vea antes de seguir a clase.";

        bool _abierta;
        RumboCarrera _rumbo;

        public string Prompt => "Pulsa E para atender";
        public bool CanInteract => true;

        void Awake()
        {
            var col = GetComponent<BoxCollider2D>();
            if (col == null)
                col = gameObject.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(1.15f, 1.9f);
            col.offset = new Vector2(0f, 0.95f);

            var cuerpo = GetComponent<SpriteRenderer>();
            if (cuerpo == null)
                cuerpo = gameObject.AddComponent<SpriteRenderer>();
            cuerpo.sprite = CrearEnfermera();
            cuerpo.sortingOrder = 6;

            CrearMesa();
            CrearLampara();
        }

        void Start()
        {
            _rumbo = RumboCarrera.Asegurar();
        }

        public void Interact(PlayerInteractor interactor)
        {
            if (!Cerca() || GameMenu.BloqueaJuego)
                return;
            _abierta = !_abierta;
        }

        void Update()
        {
            if (_rumbo == null)
                _rumbo = RumboCarrera.Asegurar();

            if (!Cerca())
                _abierta = false;

            if (!_abierta || _rumbo.ClinicaElegida || GameMenu.BloqueaJuego)
                return;

            var teclado = UnityEngine.InputSystem.Keyboard.current;
            if (teclado == null)
                return;
            if (teclado.digit1Key.wasPressedThisFrame || teclado.numpad1Key.wasPressedThisFrame)
                _rumbo.ElegirClinica(true);
            if (teclado.digit2Key.wasPressedThisFrame || teclado.numpad2Key.wasPressedThisFrame)
                _rumbo.ElegirClinica(false);
        }

        bool Cerca()
        {
            var jugador = GameObject.FindGameObjectWithTag("Player");
            if (jugador == null)
                return false;
            return Vector2.Distance(jugador.transform.position, transform.position) <= 1.8f;
        }

        void OnGUI()
        {
            if (GameMenu.BloqueaJuego || !Cerca() || _rumbo == null)
                return;

            DibujarLetrero();
            if (_abierta)
                DibujarFicha();
            else
                DibujarAviso();
        }

        void DibujarLetrero()
        {
            var camara = Camera.main;
            if (camara == null)
                return;

            var punto = camara.WorldToScreenPoint(transform.position + Vector3.up * 2.15f);
            if (punto.z < 0f)
                return;

            var estilo = Estilo(15, FontStyle.Bold, new Color(0.85f, 0.97f, 0.9f), TextAnchor.MiddleCenter);
            float y = Screen.height - punto.y;
            var rect = new Rect(punto.x - 70f, y - 14f, 140f, 26f);
            GUI.color = new Color(0.08f, 0.28f, 0.24f, 0.92f);
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = Color.white;
            GUI.Label(rect, "ENFERMERÍA", estilo);
        }

        void DibujarAviso()
        {
            var estilo = Estilo(18, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter);
            var rect = new Rect((Screen.width - 280f) * 0.5f, Screen.height - 58f, 280f, 34f);
            GUI.color = new Color(0.07f, 0.22f, 0.2f, 0.88f);
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = Color.white;
            GUI.Label(rect, "Pulsa E  ·  atender", estilo);
        }

        void DibujarFicha()
        {
            float ancho = Mathf.Min(640f, Screen.width - 48f);
            float alto = _rumbo.ClinicaElegida ? 300f : 340f;
            var ficha = new Rect((Screen.width - ancho) * 0.5f, (Screen.height - alto) * 0.5f, ancho, alto);

            GUI.color = new Color(0f, 0f, 0f, 0.45f);
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);

            GUI.color = new Color(0.96f, 0.97f, 0.95f, 1f);
            GUI.DrawTexture(ficha, Texture2D.whiteTexture);
            GUI.color = new Color(0.12f, 0.45f, 0.38f, 1f);
            GUI.DrawTexture(new Rect(ficha.x, ficha.y, ficha.width, 54f), Texture2D.whiteTexture);
            GUI.color = new Color(0.75f, 0.16f, 0.18f, 1f);
            GUI.DrawTexture(new Rect(ficha.x + 18f, ficha.y + 16f, 8f, 22f), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(ficha.x + 11f, ficha.y + 23f, 22f, 8f), Texture2D.whiteTexture);

            GUI.color = Color.white;
            GUI.Label(new Rect(ficha.x + 44f, ficha.y + 8f, ficha.width - 60f, 24f),
                "CLÍNICA UNIFRANZ", Estilo(18, FontStyle.Bold, Color.white, TextAnchor.MiddleLeft));
            GUI.Label(new Rect(ficha.x + 44f, ficha.y + 30f, ficha.width - 60f, 18f),
                "Ficha de atención", Estilo(13, FontStyle.Normal, new Color(0.86f, 0.95f, 0.92f), TextAnchor.MiddleLeft));

            GUI.Label(new Rect(ficha.x + 22f, ficha.y + 70f, ficha.width - 44f, 72f),
                Caso, Estilo(16, FontStyle.Normal, new Color(0.16f, 0.2f, 0.2f), TextAnchor.UpperLeft, true));

            if (_rumbo.ClinicaElegida)
                DibujarResultado(ficha);
            else
                DibujarOpciones(ficha);

            var barra = new Rect(ficha.x + 22f, ficha.yMax - 36f, ficha.width - 44f, 22f);
            GUI.Label(barra, _rumbo.Resumen(), Estilo(13, FontStyle.Bold, new Color(0.12f, 0.4f, 0.34f), TextAnchor.MiddleLeft));
        }

        void DibujarOpciones(Rect ficha)
        {
            float anchoBoton = (ficha.width - 58f) * 0.5f;
            var ayudar = new Rect(ficha.x + 22f, ficha.y + 168f, anchoBoton, 64f);
            var seguir = new Rect(ayudar.xMax + 14f, ayudar.y, anchoBoton, 64f);

            if (Boton(ayudar, "1   Ayudar", "Revisas el tobillo y lo sientas.", new Color(0.16f, 0.55f, 0.42f)))
                _rumbo.ElegirClinica(true);
            if (Boton(seguir, "2   Seguir", "Pasas de largo. No suma.", new Color(0.45f, 0.48f, 0.5f)))
                _rumbo.ElegirClinica(false);
        }

        void DibujarResultado(Rect ficha)
        {
            string titulo = _rumbo.AyudoEnClinica ? "Medicina +1" : "Medicina no cambió";
            string detalle = _rumbo.AyudoEnClinica
                ? "Lo sentaste, le vendaste el tobillo y le dijiste que no corra hoy."
                : "Seguiste hacia la salida. El estudiante se quedó esperando.";
            var color = _rumbo.AyudoEnClinica ? new Color(0.12f, 0.42f, 0.32f) : new Color(0.35f, 0.38f, 0.4f);

            GUI.Label(new Rect(ficha.x + 22f, ficha.y + 156f, ficha.width - 44f, 26f),
                titulo, Estilo(20, FontStyle.Bold, color, TextAnchor.MiddleLeft));
            GUI.Label(new Rect(ficha.x + 22f, ficha.y + 184f, ficha.width - 44f, 44f),
                detalle, Estilo(15, FontStyle.Normal, new Color(0.2f, 0.24f, 0.24f), TextAnchor.UpperLeft, true));
        }

        static bool Boton(Rect rect, string titulo, string detalle, Color fondo)
        {
            GUI.color = fondo;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = Color.white;
            GUI.Label(new Rect(rect.x + 12f, rect.y + 8f, rect.width - 20f, 24f),
                titulo, Estilo(16, FontStyle.Bold, Color.white, TextAnchor.MiddleLeft));
            GUI.Label(new Rect(rect.x + 12f, rect.y + 32f, rect.width - 20f, 24f),
                detalle, Estilo(12, FontStyle.Normal, new Color(1f, 1f, 1f, 0.9f), TextAnchor.MiddleLeft));
            return GUI.Button(rect, GUIContent.none, GUIStyle.none);
        }

        static GUIStyle Estilo(int tamano, FontStyle peso, Color color, TextAnchor ancla, bool ajustar = false)
        {
            var estilo = new GUIStyle(GUI.skin.label)
            {
                fontSize = tamano,
                fontStyle = peso,
                alignment = ancla,
                wordWrap = ajustar
            };
            estilo.normal.textColor = color;
            return estilo;
        }

        void CrearMesa()
        {
            var mesa = new GameObject("MesaClinica");
            mesa.transform.SetParent(transform, false);
            mesa.transform.localPosition = new Vector3(0.15f, 0.42f, 0f);
            var dibujo = mesa.AddComponent<SpriteRenderer>();
            dibujo.sprite = CrearBloque(new Color(0.93f, 0.94f, 0.92f), new Color(0.55f, 0.75f, 0.7f));
            dibujo.sortingOrder = 5;
        }

        void CrearLampara()
        {
            var luz = new GameObject("LamparaClinica");
            luz.transform.SetParent(transform, false);
            luz.transform.localPosition = new Vector3(0f, 1.15f, 0f);
            var dibujo = luz.AddComponent<SpriteRenderer>();
            dibujo.sprite = CrearBrillo();
            dibujo.sortingOrder = 4;
            dibujo.color = new Color(0.75f, 1f, 0.9f, 0.35f);
        }

        static Sprite CrearEnfermera()
        {
            const int w = 8;
            const int h = 16;
            var tex = new Texture2D(w, h) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
            var vacio = new Color(0f, 0f, 0f, 0f);
            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                tex.SetPixel(x, y, vacio);

            Pintar(tex, 2, 5, 13, 15, new Color(0.2f, 0.16f, 0.14f));
            Pintar(tex, 3, 4, 12, 13, new Color(0.93f, 0.76f, 0.62f));
            Pintar(tex, 2, 5, 7, 12, new Color(0.95f, 0.96f, 0.94f));
            Pintar(tex, 3, 4, 9, 10, new Color(0.75f, 0.16f, 0.18f));
            Pintar(tex, 2, 5, 3, 6, new Color(0.2f, 0.45f, 0.42f));
            Pintar(tex, 2, 3, 1, 2, new Color(0.15f, 0.15f, 0.16f));
            Pintar(tex, 4, 5, 1, 2, new Color(0.15f, 0.15f, 0.16f));
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0f), 8f);
        }

        static Sprite CrearBloque(Color frente, Color canto)
        {
            var tex = new Texture2D(10, 6) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
            for (int y = 0; y < 6; y++)
            for (int x = 0; x < 10; x++)
                tex.SetPixel(x, y, y < 2 ? canto : frente);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 10, 6), new Vector2(0.5f, 0f), 8f);
        }

        static Sprite CrearBrillo()
        {
            const int n = 16;
            var tex = new Texture2D(n, n) { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
            var centro = (n - 1) * 0.5f;
            for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), new Vector2(centro, centro)) / centro;
                float a = Mathf.Clamp01(1f - d);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a * a));
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), 8f);
        }

        static void Pintar(Texture2D tex, int x0, int x1, int y0, int y1, Color color)
        {
            for (int y = y0; y <= y1; y++)
            for (int x = x0; x <= x1; x++)
                tex.SetPixel(x, y, color);
        }
    }
}
