using UnityEngine;
using UnityEngine.UI;

namespace Piso7.Campus
{
    /// <summary>
    /// Título del lugar, pista de controles y el nombre de cada punto.
    /// </summary>
    public class CampusOverlay : MonoBehaviour
    {
        [SerializeField] Text title;
        [SerializeField] Text hint;
        [SerializeField] RectTransform labelRoot;
        [SerializeField] Font font;

        void Start()
        {
            if (title == null || hint == null || labelRoot == null)
                CreateCanvas();

            var info = FindFirstObjectByType<CampusPlaceInfo>();
            if (title != null && info != null)
                title.text = $"{info.Piso}  ·  {info.Nombre}";

            if (hint != null)
                hint.text = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Campus_Recepcion"
                    ? "Camina hacia la izquierda. Detente en INGRESAR para entrar a la clínica. E también permite entrar. Esc menú"
                    : "Detente en INGRESAR o SALIR para cambiar de lugar. E también permite interactuar. Esc menú";

            if (hint != null && Application.isMobilePlatform)
            {
                hint.text = "IZQ / DER: caminar · USAR: interactuar · PAUSA: menú";
                hint.rectTransform.anchoredPosition += new Vector2(0f, 25f);
            }

            if (labelRoot == null || font == null)
                return;

            foreach (var point in FindObjectsByType<CampusHotspot>(FindObjectsSortMode.None))
                CreateLabel(point.transform, point.Label);

            foreach (var elevator in FindObjectsByType<CampusElevator>(FindObjectsSortMode.None))
                CreateLabel(elevator.transform, elevator.Label);
        }

        void CreateCanvas()
        {
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null)
                font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            var canvasGo = new GameObject("HUD");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            canvasGo.AddComponent<GraphicRaycaster>();

            title = MakeText(canvasGo.transform, "Titulo", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(24f, -18f), 26, TextAnchor.UpperLeft);
            hint = MakeText(canvasGo.transform, "Pista", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 22f), 18, TextAnchor.LowerCenter);
            hint.color = new Color(1f, 0.95f, 0.75f);

            var labels = new GameObject("Etiquetas");
            labels.transform.SetParent(canvasGo.transform, false);
            labelRoot = labels.AddComponent<RectTransform>();
            labelRoot.anchorMin = Vector2.zero;
            labelRoot.anchorMax = Vector2.one;
            labelRoot.offsetMin = Vector2.zero;
            labelRoot.offsetMax = Vector2.zero;
        }

        Text MakeText(Transform parent, string name, Vector2 anchor, Vector2 pivot, Vector2 pos, int size, TextAnchor align)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var text = go.AddComponent<Text>();
            text.font = font;
            text.fontSize = size;
            text.color = Color.white;
            text.alignment = align;
            text.raycastTarget = false;
            var rt = text.rectTransform;
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = pivot;
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(900f, 40f);
            return text;
        }

        void CreateLabel(Transform target, string value)
        {
            var go = new GameObject(value);
            go.transform.SetParent(labelRoot, false);
            var text = go.AddComponent<Text>();
            text.font = font;
            text.fontSize = 16;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = new Color(1f, 0.96f, 0.9f);
            text.text = value;
            var rt = text.rectTransform;
            rt.sizeDelta = new Vector2(180f, 24f);
            var follow = go.AddComponent<CampusLabelFollow>();
            follow.Bind(target, new Vector3(0f, 0.7f, 0f));
        }
    }

    public class CampusLabelFollow : MonoBehaviour
    {
        Transform _target;
        Vector3 _offset;
        RectTransform _rect;

        public void Bind(Transform target, Vector3 offset)
        {
            _target = target;
            _offset = offset;
            _rect = GetComponent<RectTransform>();
        }

        void LateUpdate()
        {
            if (_target == null || _rect == null || Camera.main == null)
                return;

            Vector3 screen = Camera.main.WorldToScreenPoint(_target.position + _offset);
            _rect.position = screen;
        }
    }
}

