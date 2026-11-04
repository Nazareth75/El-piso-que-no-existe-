using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Piso7.Campus
{
    /// <summary>
    /// Ascensor de planta baja, en el mismo lugar que en la visita:
    /// a la derecha del muro de colores y antes de la escalera.
    /// </summary>
    public class CampusElevator : MonoBehaviour, Interaction.IInteractable
    {
        public string Label => "Ascensor";
        public string Prompt => "Ascensor";
        public bool CanInteract => true;

        public void Interact(Interaction.PlayerInteractor interactor)
        {
            Activar();
        }

        public void Activar()
        {
            CampusFloorPanel.Toggle();
        }
    }

    public class CampusFloorPanel : MonoBehaviour
    {
        static readonly (string piso, string escena)[] Pisos =
        {
            ("Subsuelo", "Campus_Subsuelo"),
            ("Planta baja", "Campus_Recepcion"),
            ("Mezanine", "Campus_Mezanine"),
            ("Piso 1", "Campus_PasilloPiso1"),
            ("Piso 2", "Campus_Piso2"),
            ("Piso 3", "Campus_Piso3"),
            ("Piso 4", "Campus_Piso4"),
            ("Piso 5", "Campus_Piso5"),
        };

        static CampusFloorPanel _open;

        public static void Toggle()
        {
            if (_open != null)
            {
                Destroy(_open.gameObject);
                _open = null;
                return;
            }

            var go = new GameObject("PanelAscensor");
            _open = go.AddComponent<CampusFloorPanel>();
            _open.Build();
        }

        void Build()
        {
            if (FindFirstObjectByType<EventSystem>() == null)
            {
                var events = new GameObject("EventSystem");
                events.AddComponent<EventSystem>();
                events.AddComponent<InputSystemUIInputModule>();
            }

            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null)
                font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 200;
            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            gameObject.AddComponent<GraphicRaycaster>();

            var panel = new GameObject("Lista");
            panel.transform.SetParent(transform, false);
            var image = panel.AddComponent<Image>();
            image.color = new Color(0.08f, 0.1f, 0.12f, 0.88f);
            var rect = image.rectTransform;
            rect.anchorMin = new Vector2(0f, 0.5f);
            rect.anchorMax = new Vector2(0f, 0.5f);
            rect.pivot = new Vector2(0f, 0.5f);
            rect.anchoredPosition = new Vector2(28f, 40f);
            rect.sizeDelta = new Vector2(280f, 530f);

            var title = CreateText(panel.transform, font, "ASCENSOR", 22, FontStyle.Bold);
            var titleRect = title.rectTransform;
            titleRect.anchorMin = new Vector2(0f, 1f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0f, -12f);
            titleRect.sizeDelta = new Vector2(-24f, 32f);

            for (int i = 0; i < Pisos.Length; i++)
            {
                var (piso, escena) = Pisos[i];
                var buttonGo = new GameObject(piso);
                buttonGo.transform.SetParent(panel.transform, false);
                var buttonImage = buttonGo.AddComponent<Image>();
                buttonImage.color = new Color(0.16f, 0.18f, 0.22f, 1f);
                var button = buttonGo.AddComponent<Button>();
                var buttonRect = buttonImage.rectTransform;
                buttonRect.anchorMin = new Vector2(0f, 1f);
                buttonRect.anchorMax = new Vector2(1f, 1f);
                buttonRect.pivot = new Vector2(0.5f, 1f);
                buttonRect.anchoredPosition = new Vector2(0f, -56f - i * 52f);
                buttonRect.sizeDelta = new Vector2(-28f, 44f);

                var label = CreateText(buttonGo.transform, font, piso, 20, FontStyle.Normal);
                var labelRect = label.rectTransform;
                labelRect.anchorMin = Vector2.zero;
                labelRect.anchorMax = Vector2.one;
                labelRect.offsetMin = Vector2.zero;
                labelRect.offsetMax = Vector2.zero;

                string sceneName = escena;
                button.onClick.AddListener(() =>
                {
                    _open = null;
                    CampusTransition.Load(sceneName);
                });
            }
        }

        static Text CreateText(Transform parent, Font font, string value, int size, FontStyle style)
        {
            var go = new GameObject("Texto");
            go.transform.SetParent(parent, false);
            var text = go.AddComponent<Text>();
            text.font = font;
            text.fontSize = size;
            text.fontStyle = style;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.text = value;
            text.raycastTarget = false;
            return text;
        }
    }
}
