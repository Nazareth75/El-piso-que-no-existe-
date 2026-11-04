using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Piso7.Campus
{
    /// <summary>
    /// Fundido corto entre lugares, para que el cambio de escena se note.
    /// </summary>
    public class CampusTransition : MonoBehaviour
    {
        static CampusTransition _active;
        bool _busy;

        public static void Load(string sceneName)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
                return;

            if (_active != null && _active._busy)
                return;

            var go = new GameObject("TransicionCampus");
            _active = go.AddComponent<CampusTransition>();
            DontDestroyOnLoad(go);
            _active.StartCoroutine(_active.FadeAndLoad(sceneName));
        }

        IEnumerator FadeAndLoad(string sceneName)
        {
            _busy = true;
            var canvasGo = new GameObject("Fundido");
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 500;
            var imageGo = new GameObject("Negro");
            imageGo.transform.SetParent(canvasGo.transform, false);
            var image = imageGo.AddComponent<Image>();
            image.color = new Color(0f, 0f, 0f, 0f);
            var rt = image.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            yield return Fade(image, 0f, 1f, 0.18f);
            SceneManager.LoadScene(sceneName);
            yield return null;
            yield return Fade(image, 1f, 0f, 0.22f);

            _busy = false;
            _active = null;
            Destroy(gameObject);
        }

        static IEnumerator Fade(Image image, float from, float to, float duration)
        {
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / duration);
                var color = image.color;
                color.a = Mathf.Lerp(from, to, k);
                image.color = color;
                yield return null;
            }
        }
    }
}
