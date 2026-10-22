using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Piso7.Core
{
    /// <summary>
    /// Menú de inicio y pausa. Jugar, pausa, opciones, casa, carta y salir.
    /// </summary>
    public class GameMenu : MonoBehaviour
    {
        public static GameMenu Instance { get; private set; }
        public static bool BloqueaJuego { get; private set; }

        const string MenuScene = "MainMenu";
        const string FirstScene = "Campus_EntradaPrincipal";

        GameObject _canvas;
        GameObject _options;
        GameObject _help;
        GameObject _characters;
        Image[] _portraitFrames;
        GameObject _banner;
        Text _bannerText;
        bool _built;
        bool _silenced;
        bool _startAfterChoice;
        GameObject _outcome;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void ResetClock()
        {
            Time.timeScale = 1f;
            BloqueaJuego = false;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (Instance != null)
                return;
            var go = new GameObject("MenuJuego");
            go.AddComponent<GameMenu>();
        }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            MenuAudio.Ensure();
            Build();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void Start()
        {
            ApplyScene(SceneManager.GetActiveScene().name);
        }

        void OnDestroy()
        {
            if (Instance == this)
                SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            ApplyScene(scene.name);
        }

        void ApplyScene(string sceneName)
        {
            if (_outcome != null) { Destroy(_outcome); _outcome = null; }
            bool menu = sceneName == MenuScene;
            _silenced = false;
            if (menu)
            {
                Time.timeScale = 1f;
                BloqueaJuego = false;
                ClosePanels();
                if (_banner != null)
                    _banner.SetActive(false);
                Show(true);
                MenuAudio.PlayMenu();
                return;
            }

            Time.timeScale = 1f;
            BloqueaJuego = false;
            ClosePanels();
            if (_banner != null)
                _banner.SetActive(false);
            Show(false);
            MenuAudio.PlayCampus();
        }

        void Update()
        {
            if (_outcome != null) return;
            var keyboard = Keyboard.current;
            if (!MobileControls.Pause && (keyboard == null || !keyboard.escapeKey.wasPressedThisFrame))
                return;

            if (_options != null && _options.activeSelf || _help != null && _help.activeSelf || _characters != null && _characters.activeSelf)
            {
                MenuAudio.Click();
                ClosePanels();
                return;
            }

            if (SceneManager.GetActiveScene().name == MenuScene)
                return;

            if (_canvas != null && _canvas.activeSelf)
                OnPlay();
            else
                OpenOverGame();
        }

        void OpenOverGame()
        {
            Time.timeScale = 0f;
            BloqueaJuego = true;
            Show(true);
            MenuAudio.PlayMenu();
            if (_bannerText != null)
                _bannerText.text = "RECORRIDO EN PAUSA";
            if (_banner != null)
                _banner.SetActive(true);
        }

        void Build()
        {
            if (_built)
                return;
            _built = true;

            EnsureEvents();
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null)
                font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            _canvas = new GameObject("MenuCanvas");
            _canvas.transform.SetParent(transform, false);
            var canvas = _canvas.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 400;
            var scaler = _canvas.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            _canvas.AddComponent<GraphicRaycaster>();

            var fondo = Sprite("Menu/fondo");
            var bg = Stretch(_canvas.transform, "Fondo", fondo, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            bg.color = Color.white;
            bg.raycastTarget = true;

            var jugar = BigButton("Jugar", "Menu/boton_jugar", "Menu/boton_jugar_pressed", new Vector2(0.5f, 0.64f), OnPlay);
            var personaje = BigButton("Personaje", "Menu/boton_personaje", "Menu/boton_personaje_pressed", new Vector2(0.5f, 0.48f), OnCharacters);
            var opciones = BigButton("Opciones", "Menu/boton_opciones", "Menu/boton_opciones_pressed", new Vector2(0.5f, 0.32f), OnOptions);
            IconButton("Sobre", "Menu/boton_sobre", "Menu/boton_sobre_pressed", new Vector2(0.78f, 0.13f), OnHelp);
            IconButton("Salir", "Menu/boton_salir", "Menu/boton_salir_pressed", new Vector2(0.90f, 0.13f), OnExit);

            _banner = new GameObject("Aviso");
            _banner.transform.SetParent(_canvas.transform, false);
            _bannerText = _banner.AddComponent<Text>();
            _bannerText.font = font;
            _bannerText.fontSize = 28;
            _bannerText.alignment = TextAnchor.MiddleCenter;
            _bannerText.color = new Color(1f, 0.86f, 0.45f);
            _bannerText.text = "EN PAUSA";
            var br = _bannerText.rectTransform;
            br.anchorMin = new Vector2(0.5f, 0.72f);
            br.anchorMax = new Vector2(0.5f, 0.72f);
            br.sizeDelta = new Vector2(700f, 50f);
            _banner.SetActive(false);

            _options = Panel(font, "Opciones", out var optionsBody);
            OptionsBody(optionsBody, font);
            _help = Panel(font, "Cómo jugar", out var helpBody);
            HelpBody(helpBody, font);
            _characters = Panel(font, "Escoger personaje", out var characterBody);
            CharacterBody(characterBody, font);

            jugar.name = "BotonJugar";
            personaje.name = "BotonPersonaje";
            opciones.name = "BotonOpciones";
        }

        void OptionsBody(Transform parent, Font font)
        {
            Label(parent, font, "Musica", 28, new Vector2(0f, 150f));
            VolumeBar(parent, new Vector2(0f, 100f), MenuAudio.MusicVolume, MenuAudio.SetMusic);
            Label(parent, font, "Efectos", 28, new Vector2(0f, 30f));
            float lastSfx = MenuAudio.SfxVolume;
            VolumeBar(parent, new Vector2(0f, -20f), MenuAudio.SfxVolume, value =>
            {
                MenuAudio.SetSfx(value);
                if (Mathf.Abs(value - lastSfx) < 0.12f)
                    return;
                lastSfx = value;
                MenuAudio.Click();
            });
            TextButton(parent, font, "Pantalla completa", new Vector2(0f, -110f), () =>
            {
                Screen.fullScreen = !Screen.fullScreen;
            });
            TextButton(parent, font, "Volver", new Vector2(0f, -190f), ClosePanels);
        }

        void HelpBody(Transform parent, Font font)
        {
            var text = Label(parent, font,
                Application.isMobilePlatform
                    ? "IZQ y DER: caminar\nMantén CORRER y una dirección para correr\nSALTAR: saltar    ATACAR: golpear (tres golpes)\nUSAR: interactuar con puertas y ascensor\nPAUSA: abrir o cerrar este menú\nDetente en las flechas para cambiar de lugar"
                    : "A y D caminan\nShift corre    Espacio salta\nF o clic derecho ataca; tres golpes derrotan al enemigo\nLlega a una flecha para cambiar de lugar\nEsc abre este menú\nEl ascensor se usa con clic o con E",
                26, new Vector2(0f, 20f));
            text.alignment = TextAnchor.MiddleCenter;
            text.rectTransform.sizeDelta = new Vector2(760f, 280f);
            TextButton(parent, font, "Volver", new Vector2(0f, -190f), ClosePanels);
        }

        void OnPlay()
        {
            MenuAudio.Click();
            _silenced = false;
            Time.timeScale = 1f;
            BloqueaJuego = false;
            if (_banner != null)
                _banner.SetActive(false);
            ClosePanels();

            if (SceneManager.GetActiveScene().name == MenuScene)
            {
                _startAfterChoice = true;
                OnCharacters();
                return;
            }

            Show(false);
            MenuAudio.PlayCampus();
        }

        void OnPause()
        {
            MenuAudio.Click();
            bool inTour = SceneManager.GetActiveScene().name != MenuScene;
            if (inTour)
            {
                Time.timeScale = 0f;
                BloqueaJuego = true;
                Show(true);
            }

            _silenced = !_silenced;
            if (_silenced)
            {
                if (!inTour)
                {
                    Time.timeScale = 0f;
                    BloqueaJuego = true;
                }

                MenuAudio.PauseMusic();
                if (_bannerText != null)
                    _bannerText.text = inTour ? "RECORRIDO EN PAUSA" : "EN PAUSA";
                if (_banner != null)
                    _banner.SetActive(true);
                return;
            }

            if (!inTour)
            {
                Time.timeScale = 1f;
                BloqueaJuego = false;
            }

            MenuAudio.PlayMenu();
            if (_banner != null)
                _banner.SetActive(inTour);
            if (inTour && _bannerText != null)
                _bannerText.text = "RECORRIDO EN PAUSA";
        }

        void OnOptions()
        {
            MenuAudio.Click();
            if (_help != null)
                _help.SetActive(false);
            if (_characters != null)
                _characters.SetActive(false);
            if (_options != null)
                _options.SetActive(true);
        }

        void OnCharacters()
        {
            MenuAudio.Click();
            if (_options != null)
                _options.SetActive(false);
            if (_help != null)
                _help.SetActive(false);
            if (_characters != null)
                _characters.SetActive(true);
            RefreshPortraits();
        }

        void OnHelp()
        {
            MenuAudio.Click();
            if (_options != null)
                _options.SetActive(false);
            if (_characters != null)
                _characters.SetActive(false);
            if (_help != null)
                _help.SetActive(true);
        }

        public void ShowFallRecovery()
        {
            ClosePanels();
            if (_outcome != null) Destroy(_outcome);
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _outcome = Panel(font, "Fuera del recorrido", out var body);
            Label(body, font, "Te devolvimos a una zona segura.", 26, new Vector2(0, 80));
            TextButton(body, font, "Volver al juego", new Vector2(0, -50), () =>
            { Destroy(_outcome); _outcome = null; Show(false); });
            TextButton(body, font, "Menú principal", new Vector2(0, -170), OnHome);
            Time.timeScale = 1f;
            Show(true); _outcome.SetActive(true); _outcome.transform.SetAsLastSibling();
        }

        void OnGUI()
        {
            if (SceneManager.GetActiveScene().name == MenuScene || _outcome != null || BloqueaJuego) return;
            if (GUI.Button(new Rect(12, 42, 135, 34), "Menú / Pausa")) OpenOverGame();
        }

        public void ReturnToMainMenu() => OnHome();

        void OnHome()
        {
            MenuAudio.Click();
            _silenced = false;
            Time.timeScale = 1f;
            BloqueaJuego = false;
            ClosePanels();
            if (SceneManager.GetActiveScene().name == MenuScene)
            {
                if (_banner != null)
                    _banner.SetActive(false);
                MenuAudio.PlayMenu();
                return;
            }

            SceneManager.LoadScene(MenuScene);
        }

        void OnExit()
        {
            MenuAudio.Click();
            Time.timeScale = 1f;
            BloqueaJuego = false;
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        void ClosePanels()
        {
            _startAfterChoice = false;
            if (_options != null)
                _options.SetActive(false);
            if (_help != null)
                _help.SetActive(false);
            if (_characters != null)
                _characters.SetActive(false);
        }

        void CharacterBody(Transform parent, Font font)
        {
            Label(parent, font, "Elige quien camina por el campus", 24, new Vector2(0f, 170f));
            _portraitFrames = new Image[2];
            Portrait(parent, font, 0, "MUJER", new Vector2(-190f, 0f), Sprite("Player/Personaje1/retrato"));
            Portrait(parent, font, 1, "HOMBRE", new Vector2(190f, 0f), Sprite("Player/Personaje2/frame_00"));
            TextButton(parent, font, "Volver", new Vector2(0f, -200f), ClosePanels);
            RefreshPortraits();
        }

        void Portrait(Transform parent, Font font, int index, string caption, Vector2 pos, Sprite picture)
        {
            var go = new GameObject(caption);
            go.transform.SetParent(parent, false);
            var frame = go.AddComponent<Image>();
            frame.color = new Color(0.12f, 0.18f, 0.3f, 1f);
            var rt = frame.rectTransform;
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(240f, 300f);
            var button = go.AddComponent<Button>();
            button.targetGraphic = frame;
            int captured = index;
            button.onClick.AddListener(() =>
            {
                MenuAudio.Click();
                Player.PlayerWalkAnimator.Select(captured);
                RefreshPortraits();
                if (_startAfterChoice)
                {
                    _startAfterChoice = false;
                    ClosePanels();
                    Show(false);
                    GameSession.Ensure().ResetRun();
                    SceneManager.LoadScene(FirstScene);
                }
            });
            _portraitFrames[index] = frame;

            var photoGo = new GameObject("Foto");
            photoGo.transform.SetParent(go.transform, false);
            var photo = photoGo.AddComponent<Image>();
            photo.sprite = picture;
            photo.preserveAspect = true;
            photo.raycastTarget = false;
            var photoRt = photo.rectTransform;
            photoRt.anchorMin = new Vector2(0.5f, 0.58f);
            photoRt.anchorMax = new Vector2(0.5f, 0.58f);
            photoRt.sizeDelta = new Vector2(170f, 210f);

            var name = Label(go.transform, font, caption, 22, new Vector2(0f, -118f));
            name.color = new Color(1f, 0.95f, 0.86f);
        }

        void RefreshPortraits()
        {
            if (_portraitFrames == null)
                return;

            int selected = Player.PlayerWalkAnimator.Selected;
            for (int i = 0; i < _portraitFrames.Length; i++)
            {
                if (_portraitFrames[i] == null)
                    continue;
                _portraitFrames[i].color = i == selected
                    ? new Color(0.85f, 0.62f, 0.22f, 1f)
                    : new Color(0.12f, 0.18f, 0.3f, 1f);
            }
        }

        public void ShowCombatResult(bool victory, System.Action retry, System.Action continueTour)
        {
            ClosePanels();
            if (_outcome != null) Destroy(_outcome);
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _outcome = Panel(font, victory ? "Victoria" : "Game Over", out var body);
            var text = Label(body, font, victory ? "El enemigo ha sido derrotado. Puedes continuar el recorrido." : "El enemigo te ha atrapado. Inténtalo de nuevo.", 26, new Vector2(0f, 70f));
            text.rectTransform.sizeDelta = new Vector2(700f, 110f);
            TextButton(body, font, victory ? "Continuar" : "Reintentar", new Vector2(0f, -70f), () =>
            {
                Destroy(_outcome); _outcome = null; Show(false);
                if (victory) continueTour(); else retry();
            });
            TextButton(body, font, "Menú principal", new Vector2(0f, -180f), () =>
            {
                Destroy(_outcome); _outcome = null; OnHome();
            });
            Show(true); _outcome.SetActive(true); _outcome.transform.SetAsLastSibling();
        }

        void Show(bool visible)
        {
            if (_canvas != null)
                _canvas.SetActive(visible);
            BloqueaJuego = visible || Time.timeScale < 0.01f;
        }

        static void EnsureEvents()
        {
            var existing = FindFirstObjectByType<EventSystem>();
            if (existing != null)
            {
                DontDestroyOnLoad(existing.gameObject);
                return;
            }

            var events = new GameObject("EventSystem");
            events.AddComponent<EventSystem>();
            events.AddComponent<InputSystemUIInputModule>();
            DontDestroyOnLoad(events);
        }

        GameObject BigButton(string name, string sprite, string pressed, Vector2 anchor, UnityEngine.Events.UnityAction action)
        {
            return MakeButton(name, sprite, pressed, anchor, new Vector2(504f, 144f), action);
        }

        GameObject IconButton(string name, string sprite, string pressed, Vector2 anchor, UnityEngine.Events.UnityAction action)
        {
            return MakeButton(name, sprite, pressed, anchor, new Vector2(156f, 132f), action);
        }

        GameObject MakeButton(string name, string sprite, string pressed, Vector2 anchor, Vector2 size, UnityEngine.Events.UnityAction action)
        {
            var go = new GameObject(name);
            go.transform.SetParent(_canvas.transform, false);
            var image = go.AddComponent<Image>();
            image.sprite = Sprite(sprite);
            image.preserveAspect = true;
            var button = go.AddComponent<Button>();
            button.targetGraphic = image;
            button.transition = Selectable.Transition.SpriteSwap;
            var state = button.spriteState;
            state.pressedSprite = Sprite(pressed);
            state.highlightedSprite = Sprite(sprite);
            button.spriteState = state;
            button.onClick.AddListener(action);
            go.AddComponent<MenuButtonSound>();
            var rt = image.rectTransform;
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            return go;
        }

        GameObject Panel(Font font, string title, out Transform body)
        {
            var go = new GameObject(title);
            go.transform.SetParent(_canvas.transform, false);
            var image = go.AddComponent<Image>();
            image.sprite = Sprite("Menu/panel");
            image.type = Image.Type.Simple;
            image.color = new Color(1f, 0.96f, 0.9f, 1f);
            var rt = image.rectTransform;
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(860f, 560f);
            var heading = Label(go.transform, font, title.ToUpperInvariant(), 36, new Vector2(0f, 220f));
            heading.color = new Color(0.12f, 0.18f, 0.28f);
            body = go.transform;
            go.SetActive(false);
            return go;
        }

        GameObject TextButton(Transform parent, Font font, string label, Vector2 pos, UnityEngine.Events.UnityAction action)
        {
            var go = new GameObject(label);
            go.transform.SetParent(parent, false);
            var image = go.AddComponent<Image>();
            image.sprite = Sprite("Menu/boton_vacio");
            image.preserveAspect = true;
            var button = go.AddComponent<Button>();
            button.targetGraphic = image;
            button.transition = Selectable.Transition.SpriteSwap;
            var state = button.spriteState;
            state.pressedSprite = Sprite("Menu/boton_vacio_pressed");
            button.spriteState = state;
            button.onClick.AddListener(() =>
            {
                MenuAudio.Click();
                action();
            });
            var rt = image.rectTransform;
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(420f, 90f);
            var text = Label(go.transform, font, label.ToUpperInvariant(), 24, Vector2.zero);
            text.color = new Color(0.1f, 0.16f, 0.28f);
            text.rectTransform.sizeDelta = new Vector2(400f, 80f);
            return go;
        }

        static Text Label(Transform parent, Font font, string value, int size, Vector2 pos)
        {
            var go = new GameObject(value);
            go.transform.SetParent(parent, false);
            var text = go.AddComponent<Text>();
            text.font = font;
            text.fontSize = size;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = new Color(0.14f, 0.2f, 0.32f);
            text.text = value;
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            var rt = text.rectTransform;
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(700f, 50f);
            return text;
        }

        static void VolumeBar(Transform parent, Vector2 pos, float value, UnityEngine.Events.UnityAction<float> changed)
        {
            var go = new GameObject("Slider");
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(520f, 28f);

            var bg = go.AddComponent<Image>();
            bg.color = new Color(0.45f, 0.3f, 0.2f, 1f);

            var fillArea = new GameObject("Fill");
            fillArea.transform.SetParent(go.transform, false);
            var fillRt = fillArea.AddComponent<RectTransform>();
            fillRt.anchorMin = Vector2.zero;
            fillRt.anchorMax = Vector2.one;
            fillRt.offsetMin = new Vector2(6f, 6f);
            fillRt.offsetMax = new Vector2(-6f, -6f);
            var fill = fillArea.AddComponent<Image>();
            fill.color = new Color(0.85f, 0.62f, 0.22f, 1f);

            var handleGo = new GameObject("Handle");
            handleGo.transform.SetParent(go.transform, false);
            var handleRt = handleGo.AddComponent<RectTransform>();
            handleRt.sizeDelta = new Vector2(22f, 36f);
            var handle = handleGo.AddComponent<Image>();
            handle.color = new Color(0.12f, 0.18f, 0.3f, 1f);

            var slider = go.AddComponent<Slider>();
            slider.fillRect = fillRt;
            slider.handleRect = handleRt;
            slider.targetGraphic = handle;
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = value;
            slider.onValueChanged.AddListener(changed);
        }

        static Image Stretch(Transform parent, string name, Sprite sprite, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var image = go.AddComponent<Image>();
            image.sprite = sprite;
            var rt = image.rectTransform;
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;
            return image;
        }

        static Sprite Sprite(string path)
        {
            return Resources.Load<Sprite>(path);
        }
    }

    public class MenuButtonSound : MonoBehaviour, IPointerEnterHandler
    {
        public void OnPointerEnter(PointerEventData eventData)
        {
            MenuAudio.Hover();
        }
    }
}




