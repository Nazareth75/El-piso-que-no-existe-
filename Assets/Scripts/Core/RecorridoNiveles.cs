using UnityEngine;
using UnityEngine.SceneManagement;

namespace Piso7.Core
{
    /// <summary>
    /// Lleva la partida en tres tramos con lo que ya está jugable.
    /// El piso 7 no se crea aquí: si Nazareth agrega la escena Campus_Piso7,
    /// la huida de las escaleras la abre sola.
    /// </summary>
    public class RecorridoNiveles : MonoBehaviour
    {
        public const string EscenaPiso7 = "Campus_Piso7";

        public static RecorridoNiveles Instancia { get; private set; }

        bool _notaLeida;
        bool _entradaCruzada;
        bool _sistemasRegistrado;
        bool _huidaCompleta;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void AlAbrirJuego()
        {
            Asegurar();
        }

        public static RecorridoNiveles Asegurar()
        {
            if (Instancia != null)
                return Instancia;

            var go = new GameObject("RecorridoNiveles");
            return go.AddComponent<RecorridoNiveles>();
        }

        public static bool Piso7Disponible()
        {
            return Application.CanStreamedLevelBeLoaded(EscenaPiso7);
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
            SceneManager.sceneLoaded += AlCargarEscena;
        }

        void OnDestroy()
        {
            if (Instancia == this)
                SceneManager.sceneLoaded -= AlCargarEscena;
        }

        public void NuevaPartida()
        {
            _notaLeida = false;
            _entradaCruzada = false;
            _sistemasRegistrado = false;
            _huidaCompleta = false;
            RumboCarrera.Asegurar().Reiniciar();
            Interaction.AcertijoSistemas.ReiniciarProgreso();
        }

        public void MarcarNotaLeida()
        {
            _notaLeida = true;
            Refrescar();
        }

        public void CompletarEntrada()
        {
            _entradaCruzada = true;
            GameSession.Ensure().SetObjective("Nivel 1 · La llegada. Entrada cruzada. Sigue a recepción y lee la nota.");
        }

        public void RegistrarSistemas()
        {
            _sistemasRegistrado = true;
            Refrescar();
        }

        public void CompletarHuida()
        {
            _huidaCompleta = true;
            if (_notaLeida && DecisionesListas() && Piso7Disponible())
            {
                GameSession.Ensure().SetObjective("Nivel 3 completo. El ascensor abre el piso 7.");
                Campus.CampusTransition.Load(EscenaPiso7);
                return;
            }

            if (!_notaLeida || !DecisionesListas())
            {
                GameSession.Ensure().SetObjective("Escapaste de las escaleras. " + Texto(SceneManager.GetActiveScene().name));
                return;
            }

            GameSession.Ensure().SetObjective("Nivel 3 completo. Llegaste a la zona segura. El piso 7 se abrirá cuando esa sala esté en el juego.");
            PresentacionJuego.MostrarCierre(RumboCarrera.Asegurar().Resumen());
        }

        public void Refrescar()
        {
            var escena = SceneManager.GetActiveScene().name;
            if (escena == "MainMenu" || string.IsNullOrEmpty(escena))
                return;

            GameSession.Ensure().SetObjective(Texto(escena));
        }

        public void PrepararEncuentro(bool escaleras)
        {
            GameSession.Ensure().SetObjective(escaleras
                ? "Nivel 3 · La huida. Llega a la zona verde. Shift corre y Espacio salta."
                : "Nivel 1 · La llegada. Cruza la entrada y evita a la entidad. Espacio salta.");
        }

        void AlCargarEscena(Scene escena, LoadSceneMode modo)
        {
            if (escena.name == "MainMenu")
                return;
            Refrescar();
        }

        string Texto(string escena)
        {
            if (!_notaLeida)
                return TextoNivel1(escena);
            if (!DecisionesListas())
                return TextoNivel2(escena);
            if (!_huidaCompleta)
                return "Nivel 2 completo. Nivel 3 · La huida: ve a las escaleras y llega a la zona verde.  " + RumboCarrera.Asegurar().Resumen();
            if (Piso7Disponible())
                return "Nivel 3 completo. El piso 7 ya está abierto.";
            return "Nivel 3 completo. El piso 7 se abrirá cuando esa sala esté en el juego.";
        }

        string TextoNivel1(string escena)
        {
            if (escena == "Campus_Recepcion")
                return _entradaCruzada
                    ? "Nivel 1 · La llegada. Pulsa E en la nota."
                    : "Nivel 1 · La llegada. Lee la nota con E. Si aún no cruzaste la entrada, vuelve y esquiva a la entidad.";
            if (escena == "Campus_EntradaPrincipal")
                return "Nivel 1 · La llegada. Cruza la entrada y evita a la entidad.";
            return "Nivel 1 · La llegada. En recepción, lee la nota con E.";
        }

        string TextoNivel2(string escena)
        {
            bool clinica = RumboCarrera.Asegurar().ClinicaElegida;
            if (escena == "Campus_Clinica" && !clinica)
                return "Nivel 2 · Las decisiones. Atiende el caso: 1 ayudar, 2 seguir.";
            if (escena == "Campus_Piso2" && !_sistemasRegistrado)
                return "Nivel 2 · Las decisiones. Resuelve una computadora con E.";
            if (!clinica && !_sistemasRegistrado)
                return "Nivel 1 completo. Nivel 2 · Las decisiones. En recepción: atiende la clínica y sube al piso 2 en el ascensor.";
            if (!clinica)
                return "Nivel 2 · Las decisiones. Sistemas ya quedó. Falta atender el caso en la clínica.  " + RumboCarrera.Asegurar().Resumen();
            return "Nivel 2 · Las decisiones. La clínica ya quedó. Falta una computadora en el piso 2.  " + RumboCarrera.Asegurar().Resumen();
        }

        bool DecisionesListas()
        {
            return RumboCarrera.Asegurar().ClinicaElegida && _sistemasRegistrado;
        }

        void OnGUI()
        {
            if (GameMenu.BloqueaJuego || PresentacionJuego.CubrePantalla)
                return;
            var escena = SceneManager.GetActiveScene().name;
            if (escena == "MainMenu" || string.IsNullOrEmpty(escena))
                return;

            var sesion = GameSession.Instance;
            if (sesion == null)
                return;

            float ancho = Mathf.Min(760f, Screen.width - 24f);
            var area = new Rect((Screen.width - ancho) * 0.5f, 8f, ancho, 68f);
            Color fondo = GUI.color;
            GUI.color = new Color(0.05f, 0.07f, 0.09f, 0.82f);
            GUI.DrawTexture(area, Texture2D.whiteTexture);
            GUI.color = fondo;
            var estilo = new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true,
                normal = { textColor = Color.white }
            };
            GUI.Label(area, sesion.CurrentObjective, estilo);
        }
    }
}
