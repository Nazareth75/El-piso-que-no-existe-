using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Piso7.Core
{
    // Snapshot input before gameplay Update; pointer callbacks queue the next frame.
    [DefaultExecutionOrder(-1000)]
    public class MobileControls : MonoBehaviour
    {
        public static float Move { get; private set; }
        public static bool Run { get; private set; }
        public static bool Jump { get; private set; }
        public static bool Attack { get; private set; }
        public static bool Interact { get; private set; }
        public static bool Pause { get; private set; }
        public static bool Enabled => Application.isMobilePlatform || Application.isEditor;
        static MobileControls instance;
        readonly List<MobileHoldButton> buttons = new List<MobileHoldButton>();
        RectTransform safeArea;
        GameObject gameplay;
        bool jump, attack, interact, pause;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (Enabled && instance == null)
                new GameObject("Controles tactiles").AddComponent<MobileControls>();
        }

        void Awake()
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);
            scaler.matchWidthOrHeight = 0.5f;
            gameObject.AddComponent<GraphicRaycaster>();
            safeArea = new GameObject("Area segura", typeof(RectTransform)).GetComponent<RectTransform>();
            safeArea.SetParent(transform, false);
            gameplay = new GameObject("Acciones", typeof(RectTransform));
            var rect = (RectTransform)gameplay.transform;
            rect.SetParent(safeArea, false);
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            Hold("IZQ", new Vector2(75, 85), 0);
            Hold("DER", new Vector2(200, 85), 1);
            Hold("CORRER", new Vector2(138, 195), 2);
            Action("SALTAR", new Vector2(-75, 85), () => jump = true);
            Action("ATACAR", new Vector2(-200, 85), () => attack = true);
            Action("USAR", new Vector2(-138, 195), () => interact = true);
            var menu = MakeButton(safeArea, "PAUSA", new Vector2(1, 1), new Vector2(-85, -110));
            menu.gameObject.AddComponent<Button>().onClick.AddListener(() => pause = true);
            SceneManager.sceneLoaded += SceneLoaded;
        }

        void SceneLoaded(Scene scene, LoadSceneMode mode) { Clear(); }

        void Update()
        {
            var area = Screen.safeArea;
            safeArea.anchorMin = new Vector2(area.xMin / Screen.width, area.yMin / Screen.height);
            safeArea.anchorMax = new Vector2(area.xMax / Screen.width, area.yMax / Screen.height);
            safeArea.offsetMin = safeArea.offsetMax = Vector2.zero;
            bool playable = SceneManager.GetActiveScene().name != "MainMenu" && !GameMenu.BloqueaJuego;
            gameplay.SetActive(playable);
            Jump = playable && jump; Attack = playable && attack; Interact = playable && interact;
            Pause = pause;
            jump = attack = interact = pause = false;
            Move = playable ? (buttons[1].Held ? 1 : 0) - (buttons[0].Held ? 1 : 0) : 0;
            Run = playable && buttons[2].Held;
            if (!playable) foreach (var button in buttons) button.Release();
        }

        void Hold(string label, Vector2 position, int index)
        {
            var image = MakeButton(gameplay.transform, label, Vector2.zero, position);
            buttons.Add(image.gameObject.AddComponent<MobileHoldButton>());
        }

        void Action(string label, Vector2 position, UnityEngine.Events.UnityAction action)
        {
            var image = MakeButton(gameplay.transform, label, new Vector2(1, 0), position);
            image.gameObject.AddComponent<Button>().onClick.AddListener(action);
        }

        static Image MakeButton(Transform parent, string label, Vector2 anchor, Vector2 position)
        {
            var go = new GameObject(label, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var image = go.AddComponent<Image>();
            image.color = new Color(0.08f, 0.14f, 0.23f, 0.8f);
            var rect = image.rectTransform;
            rect.anchorMin = rect.anchorMax = anchor;
            rect.anchoredPosition = position; rect.sizeDelta = new Vector2(110, 90);
            var text = new GameObject("Texto", typeof(RectTransform)).AddComponent<Text>();
            text.transform.SetParent(go.transform, false);
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.text = label; text.fontSize = 20; text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white; text.raycastTarget = false;
            text.rectTransform.anchorMin = Vector2.zero; text.rectTransform.anchorMax = Vector2.one;
            text.rectTransform.offsetMin = text.rectTransform.offsetMax = Vector2.zero;
            return image;
        }

        void Clear()
        {
            Move = 0; Run = Jump = Attack = Interact = Pause = false;
            jump = attack = interact = pause = false;
            foreach (var button in buttons) button.Release();
        }
        void OnApplicationFocus(bool focused) { if (!focused) Clear(); }
        void OnApplicationPause(bool paused) { if (paused) Clear(); }
        void OnDestroy()
        {
            SceneManager.sceneLoaded -= SceneLoaded;
            if (instance == this) { Clear(); instance = null; }
        }
    }

    public class MobileHoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        readonly HashSet<int> pointers = new HashSet<int>();
        public bool Held => pointers.Count > 0;
        public void OnPointerDown(PointerEventData data) { pointers.Add(data.pointerId); }
        public void OnPointerUp(PointerEventData data) { pointers.Remove(data.pointerId); }
        public void OnPointerExit(PointerEventData data) { pointers.Remove(data.pointerId); }
        public void Release() { pointers.Clear(); }
        void OnDisable() { Release(); }
    }
}
