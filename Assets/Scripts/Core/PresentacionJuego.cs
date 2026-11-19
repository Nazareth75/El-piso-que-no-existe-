using UnityEngine;
using UnityEngine.SceneManagement;

namespace Piso7.Core
{
    /// <summary>
    /// Introducción y tutorial del plan de entrega, sin depender del piso 7.
    /// </summary>
    public class PresentacionJuego : MonoBehaviour
    {
        public static PresentacionJuego Instancia { get; private set; }
        public static bool CubrePantalla { get; private set; }

        bool _intro;
        bool _tutorial;
        bool _cierre;
        string _resumenCierre = "";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void AlAbrir()
        {
            Asegurar();
        }

        public static PresentacionJuego Asegurar()
        {
            if (Instancia != null)
                return Instancia;
            var go = new GameObject("PresentacionJuego");
            return go.AddComponent<PresentacionJuego>();
        }

        public static void PedirTutorial()
        {
            Asegurar()._tutorial = true;
        }

        public static void MostrarCierre(string resumen)
        {
            var presentacion = Asegurar();
            presentacion._cierre = true;
            presentacion._resumenCierre = resumen;
            presentacion._tutorial = false;
        }

        void Awake()
        {
            if (Instancia != null && Instancia != this)
            {
                Destroy(gameObject);
                return;
            }

            Instancia = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += AlCargar;
            Revisar(SceneManager.GetActiveScene().name);
        }

        void OnDestroy()
        {
            if (Instancia == this)
                SceneManager.sceneLoaded -= AlCargar;
        }

        void Update()
        {
            CubrePantalla = _intro || _tutorial || _cierre;
            if (_tutorial || _cierre)
                GameMenu.FijarBloqueo(true);
        }

        void AlCargar(Scene escena, LoadSceneMode modo)
        {
            Revisar(escena.name);
        }

        void Revisar(string escena)
        {
            _intro = escena == "Intro_UNIFRANZ";
            if (escena == "Campus_EntradaPrincipal" && _tutorial)
                GameMenu.FijarBloqueo(true);
            if (escena == "MainMenu")
            {
                _intro = false;
                _tutorial = false;
                _cierre = false;
            }
        }

        void OnGUI()
        {
            if (_intro)
                Panel(
                    "El piso que no existe",
                    "Es tu primer día en la universidad.\nEl campus se reconoce, pero algo no cuadra.\nUna nota habla de un piso que no debería existir.\nEn el camino vas a esquivar una entidad, leer, decidir y huir.\nCada decisión empuja tu futuro hacia Ingeniería de Sistemas o hacia Medicina.",
                    "Continuar",
                    SeguirAlTutorial);
            else if (_tutorial)
                Panel(
                    "Tutorial",
                    "A y D, o las flechas, caminan.\nShift corre. Espacio salta.\nE lee, habla y usa lo que está cerca.\nEn la clínica, 1 ayuda y 2 sigue de largo.\nEsquiva a la entidad. Si te alcanza, Reintentar vuelve a empezar esa sala.",
                    "Entendido",
                    CerrarTutorial);
            else if (_cierre)
                Panel(
                    "Nivel 3 completo",
                    "Llegaste a la zona segura.\n" + _resumenCierre + "\n\nEn esta entrega el piso 7 permanece cerrado.\nLos tres niveles jugables terminan aquí.",
                    "Volver al menú",
                    VolverAlMenu);
        }

        void SeguirAlTutorial()
        {
            _intro = false;
            _tutorial = true;
            SceneManager.LoadScene("Campus_EntradaPrincipal");
        }

        void CerrarTutorial()
        {
            _tutorial = false;
            CubrePantalla = false;
            GameMenu.FijarBloqueo(false);
        }

        void VolverAlMenu()
        {
            _cierre = false;
            CubrePantalla = false;
            GameMenu.FijarBloqueo(false);
            SceneManager.LoadScene("MainMenu");
        }

        static void Panel(string titulo, string cuerpo, string boton, UnityEngine.Events.UnityAction alPulsar)
        {
            float ancho = Mathf.Min(720f, Screen.width - 40f);
            float alto = Mathf.Min(460f, Screen.height - 40f);
            var area = new Rect((Screen.width - ancho) * 0.5f, (Screen.height - alto) * 0.5f, ancho, alto);
            Color anterior = GUI.color;
            GUI.color = new Color(0.06f, 0.07f, 0.09f, 0.94f);
            GUI.DrawTexture(area, Texture2D.whiteTexture);
            GUI.color = anterior;

            var estilo = new GUIStyle(GUI.skin.label)
            {
                fontSize = 22,
                alignment = TextAnchor.UpperCenter,
                wordWrap = true,
                normal = { textColor = Color.white }
            };
            GUI.Label(new Rect(area.x + 24f, area.y + 28f, area.width - 48f, 40f), titulo, estilo);
            estilo.fontSize = 18;
            estilo.alignment = TextAnchor.UpperLeft;
            GUI.Label(new Rect(area.x + 36f, area.y + 84f, area.width - 72f, alto - 180f), cuerpo, estilo);
            if (GUI.Button(new Rect(area.x + (area.width - 220f) * 0.5f, area.y + alto - 70f, 220f, 40f), boton))
                alPulsar();
        }
    }
}
