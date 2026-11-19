using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Piso7.Core
{
    // Secuencia cinematográfica y protocolo de controles, antes de cargar el campus.
    public class IntroTutorial : MonoBehaviour
    {
        readonly Color paper = new Color(0.9f, 0.89f, 0.84f);
        readonly Color muted = new Color(0.57f, 0.63f, 0.64f);
        readonly Color red = new Color(0.83f, 0.24f, 0.2f);
        readonly string[] chapter = { "01   /   PRIMER DÍA", "02   /   EL SILENCIO", "03   /   FUERA DEL PLANO" };
        readonly string[] titles = { "Un nuevo\ncomienzo.", "Ya no queda\nnadie.", "Hay un piso\nque no existe." };
        readonly string[] story = {
            "Llegaste a UNIFRANZ para comenzar una nueva etapa.\nSolo tenías que encontrar tu aula.",
            "Los pasillos se vaciaron. Las voces desaparecieron.\nCuando intentaste salir, la puerta ya no se abrió.",
            "Algo se mueve entre las sombras.\nSigue las pistas. Descubre qué esconde la universidad."
        };
        Text heading, message, eyebrow, progress, index, nextLabel;
        Text[] keys = new Text[3], descriptions = new Text[3];
        CanvasGroup content, actions;
        RectTransform safeRect, artwork;
        Image black;
        Button next, back, skip;
        Action finished;
        Coroutine sequence;
        AudioSource atmosphere;
        AudioClip drone;
        Texture2D grain;
        int page;
        bool tutorial, closing;
        float elapsed, veil = 1f;

        public void Begin(Action onFinished)
        {
            finished = onFinished;
            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 600;
            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1600, 900);
            scaler.matchWidthOrHeight = 0.5f;
            gameObject.AddComponent<GraphicRaycaster>();
            Surface(transform, "Negro", Color.black, Vector2.zero, Vector2.one);
            var image = new GameObject("Pasillo imposible", typeof(RectTransform), typeof(RawImage));
            image.transform.SetParent(transform, false);
            artwork = (RectTransform)image.transform;
            Stretch(artwork, Vector2.zero, Vector2.one);
            var raw = image.GetComponent<RawImage>();
            raw.texture = Resources.Load<Texture2D>("Intro/pasillo_cinematico");
            raw.raycastTarget = false;
            raw.color = Color.white;
            black = Surface(transform, "Revelado desde negro", Color.black, Vector2.zero, Vector2.one);
            MakeGrain();
            var safe = new GameObject("Contenido seguro", typeof(RectTransform));
            safe.transform.SetParent(transform, false);
            safeRect = (RectTransform)safe.transform;
            UpdateSafeArea();
            var brand = Label(safeRect, "Identidad", "EL PISO QUE NO EXISTE", 19, new Vector2(0.075f, 0.88f), new Vector2(0.6f, 0.94f));
            brand.color = muted;
            Label(safeRect, "Edición", "UNIFRANZ  /  ARCHIVO 07", 14, new Vector2(0.075f, 0.84f), new Vector2(0.6f, 0.88f)).color = muted;
            var textRoot = new GameObject("Relato", typeof(RectTransform), typeof(CanvasGroup));
            textRoot.transform.SetParent(safeRect, false);
            Stretch((RectTransform)textRoot.transform, new Vector2(0.075f, 0.24f), new Vector2(0.65f, 0.79f));
            content = textRoot.GetComponent<CanvasGroup>();
            content.blocksRaycasts = false;
            eyebrow = Label(textRoot.transform, "Capítulo", "", 18, new Vector2(0, 0.88f), Vector2.one);
            eyebrow.color = red;
            heading = Label(textRoot.transform, "Título", "", 66, new Vector2(0, 0.34f), new Vector2(1, 0.86f));
            heading.fontStyle = FontStyle.Bold;
            message = Label(textRoot.transform, "Narración", "", 25, Vector2.zero, new Vector2(0.95f, 0.3f));
            message.color = paper;
            index = Label(safeRect, "Número de archivo", "07", 170, new Vector2(0.77f, 0.14f), new Vector2(0.94f, 0.4f));
            index.color = new Color(0.8f, 0.3f, 0.25f, 0.13f);
            progress = Label(safeRect, "Progreso", "", 16, new Vector2(0.075f, 0.095f), new Vector2(0.5f, 0.15f));
            progress.color = muted;
            next = MakeButton("CONTINUAR  >", new Vector2(0.68f, 0.07f), new Vector2(0.94f, 0.16f), Advance);
            nextLabel = next.GetComponentInChildren<Text>();
            back = MakeButton("<  ANTERIOR", new Vector2(0.47f, 0.07f), new Vector2(0.67f, 0.16f), Previous);
            back.gameObject.SetActive(false);
            skip = MakeButton("OMITIR RELATO", new Vector2(0.72f, 0.87f), new Vector2(0.94f, 0.95f), SkipIntro);
            BuildActions();
            MenuAudio.PauseMusic();
            CreateAtmosphere();
            ShowStory();
        }

        void BuildActions()
        {
            var root = new GameObject("Guía de controles", typeof(RectTransform), typeof(CanvasGroup));
            root.transform.SetParent(safeRect, false);
            Stretch((RectTransform)root.transform, new Vector2(0.075f, 0.25f), new Vector2(0.64f, 0.54f));
            actions = root.GetComponent<CanvasGroup>();
            actions.alpha = 0f;
            actions.blocksRaycasts = false;
            root.SetActive(false);
            for (int i = 0; i < 3; i++)
            {
                float top = 1f - i / 3f;
                keys[i] = Label(root.transform, "Control " + i, "", 19, new Vector2(0, top - 0.27f), new Vector2(0.25f, top));
                keys[i].color = red;
                keys[i].fontStyle = FontStyle.Bold;
                descriptions[i] = Label(root.transform, "Acción " + i, "", 23, new Vector2(0.3f, top - 0.27f), new Vector2(1, top));
            }
        }

        void Update()
        {
            if (safeRect == null) return;
            UpdateSafeArea();
            elapsed += Time.unscaledDeltaTime;
            artwork.localScale = Vector3.one * (1.035f + Mathf.Sin(elapsed * 0.055f) * 0.015f);
            artwork.anchoredPosition = new Vector2(Mathf.Sin(elapsed * 0.045f) * 9f, 0f);
            float target = tutorial ? 0.32f : page == 0 ? 0.86f : page == 1 ? 0.59f : 0.37f;
            if (closing) target = 1f;
            veil = Mathf.MoveTowards(veil, target, Time.unscaledDeltaTime * 0.25f);
            black.color = new Color(0, 0, 0, veil);
            if (atmosphere != null) atmosphere.volume = MenuAudio.SfxVolume * (closing ? 0f : 0.12f);
            var keyboard = Keyboard.current;
            if (keyboard != null && (keyboard.enterKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame)) Advance();
            else if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame && !tutorial) SkipIntro();
        }
        void UpdateSafeArea()
        {
            var area = Screen.safeArea;
            Stretch(safeRect, new Vector2(area.xMin / Mathf.Max(1, Screen.width), area.yMin / Mathf.Max(1, Screen.height)),
                new Vector2(area.xMax / Mathf.Max(1, Screen.width), area.yMax / Mathf.Max(1, Screen.height)));
        }
        void ShowStory()
        {
            eyebrow.text = chapter[page];
            heading.text = titles[page];
            message.text = story[page];
            progress.text = "RELATO   /   " + (page + 1).ToString("00") + " — 03";
            sequence = StartCoroutine(StoryFade());
        }
        IEnumerator StoryFade()
        {
            yield return Fade(0f, 1f, 1.3f);
            yield return new WaitForSecondsRealtime(5f);
            yield return Fade(1f, 0f, 1f);
            sequence = null;
            NextStory();
        }
        IEnumerator Fade(float from, float to, float duration)
        {
            content.alpha = from;
            if (tutorial) actions.alpha = from;
            float time = 0f;
            while (time < duration)
            {
                time += Time.unscaledDeltaTime;
                content.alpha = Mathf.Lerp(from, to, Mathf.SmoothStep(0, 1, time / duration));
                if (tutorial) actions.alpha = content.alpha;
                yield return null;
            }
            content.alpha = to;
            if (tutorial) actions.alpha = to;
        }
        void StopSequence() { if (sequence != null) StopCoroutine(sequence); sequence = null; }
        void NextStory() { if (++page < story.Length) ShowStory(); else ShowTutorial(); }
        void SkipIntro() { if (tutorial || closing) return; StopSequence(); ShowTutorial(); }
        void Advance()
        {
            if (closing) return;
            MenuAudio.Click();
            if (!tutorial) { StopSequence(); NextStory(); return; }
            if (page < 2) { page++; RenderTutorial(); return; }
            closing = true;
            next.interactable = back.interactable = false;
            StartCoroutine(Finish());
        }
        void Previous() { if (!tutorial || closing || page == 0) return; page--; RenderTutorial(); }
        void ShowTutorial()
        {
            tutorial = true;
            page = 0;
            skip.gameObject.SetActive(false);
            actions.gameObject.SetActive(true);
            Stretch(heading.rectTransform, new Vector2(0, 0.6f), new Vector2(1, 0.86f));
            heading.resizeTextMaxSize = 50;
            Stretch(message.rectTransform, new Vector2(0, -0.12f), new Vector2(0.95f, -0.02f));
            message.resizeTextMaxSize = 19;
            RenderTutorial();
        }
        void RenderTutorial()
        {
            StopSequence();
            bool mobile = Application.isMobilePlatform;
            eyebrow.text = "PROTOCOLO DE INGRESO   /   " + (page + 1).ToString("00");
            heading.text = page == 0 ? "Aprende a moverte." : page == 1 ? "Lee las señales." : "No te detengas.";
            message.text = page == 0 ? "Tu primera decisión es seguir avanzando." : page == 1 ? "Una pista puede cambiar el rumbo de tu historia." : "La entidad no se combate. Se evita.";
            string[] codes = page == 0 ? (mobile ? new[] { "IZQ / DER", "CORRER", "SALTAR" } : new[] { "A / D", "SHIFT", "ESPACIO" })
                : page == 1 ? (mobile ? new[] { "USAR", "FLECHAS", "PAUSA" } : new[] { "E", "FLECHAS", "ESC" })
                : new[] { "RESISTENCIA", "ZONA VERDE", "REINTENTAR" };
            string[] hints = page == 0 ? new[] { mobile ? "Mantén una dirección para caminar." : "Camina. También puedes usar las flechas.", "Mantén junto a una dirección para correr.", "Salta para superar el peligro." }
                : page == 1 ? new[] { "Acércate para leer e interactuar.", "Detente junto a ellas para cambiar de sala.", "Consulta la ayuda o toma un descanso." }
                : new[] { "Correr la consume; caminar la recupera.", "En las escaleras, llega al refugio para escapar.", "Si te atrapan, vuelve a intentarlo." };
            for (int i = 0; i < 3; i++) { keys[i].text = codes[i]; descriptions[i].text = hints[i]; }
            progress.text = "GUÍA DE SUPERVIVENCIA   /   " + (page + 1).ToString("00") + " — 03";
            index.text = (page + 1).ToString("00");
            back.gameObject.SetActive(page > 0);
            nextLabel.text = page == 2 ? "ENTRAR A UNIFRANZ  >" : "SIGUIENTE  >";
            sequence = StartCoroutine(Fade(0, 1, 0.45f));
        }
        IEnumerator Finish()
        {
            yield return Fade(content.alpha, 0f, 1f);
            var callback = finished;
            finished = null;
            callback?.Invoke();
            Destroy(gameObject);
        }
        void CreateAtmosphere()
        {
            const int rate = 22050;
            var samples = new float[rate * 4];
            for (int i = 0; i < samples.Length; i++)
            {
                float t = (float)i / rate;
                samples[i] = (Mathf.Sin(2 * Mathf.PI * 48 * t) * 0.55f + Mathf.Sin(2 * Mathf.PI * 72.25f * t) * 0.22f)
                    * (0.6f + 0.4f * Mathf.Cos(2 * Mathf.PI * t / 4));
            }
            drone = AudioClip.Create("Ambiente del piso inexistente", samples.Length, 1, rate, false);
            drone.SetData(samples, 0);
            atmosphere = gameObject.AddComponent<AudioSource>();
            atmosphere.clip = drone; atmosphere.loop = true; atmosphere.volume = MenuAudio.SfxVolume * 0.12f;
            atmosphere.Play();
        }
        void MakeGrain()
        {
            grain = new Texture2D(128, 128, TextureFormat.RGBA32, false);
            var pixels = new Color32[128 * 128];
            var random = new System.Random(7);
            for (int i = 0; i < pixels.Length; i++) { byte v = (byte)random.Next(50, 220); pixels[i] = new Color32(v, v, v, 12); }
            grain.SetPixels32(pixels); grain.wrapMode = TextureWrapMode.Repeat; grain.Apply();
            var go = new GameObject("Grano de película", typeof(RectTransform), typeof(RawImage));
            go.transform.SetParent(transform, false);
            Stretch((RectTransform)go.transform, Vector2.zero, Vector2.one);
            var raw = go.GetComponent<RawImage>(); raw.texture = grain; raw.uvRect = new Rect(0, 0, 10, 6); raw.raycastTarget = false;
        }
        void OnDestroy() { if (drone != null) Destroy(drone); if (grain != null) Destroy(grain); }
        static void Stretch(RectTransform rect, Vector2 min, Vector2 max)
        { rect.anchorMin = min; rect.anchorMax = max; rect.offsetMin = rect.offsetMax = Vector2.zero; }
        static Image Surface(Transform parent, string name, Color color, Vector2 min, Vector2 max)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image)); go.transform.SetParent(parent, false);
            var image = go.GetComponent<Image>(); image.color = color; Stretch(image.rectTransform, min, max); return image;
        }
        Text Label(Transform parent, string name, string value, int size, Vector2 min, Vector2 max)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text)); go.transform.SetParent(parent, false);
            var text = go.GetComponent<Text>(); text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.text = value; text.fontSize = size; text.resizeTextForBestFit = true; text.resizeTextMinSize = 14; text.resizeTextMaxSize = size;
            text.alignment = TextAnchor.MiddleLeft; text.color = paper; text.raycastTarget = false; text.supportRichText = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap; text.verticalOverflow = VerticalWrapMode.Truncate;
            Stretch(text.rectTransform, min, max); return text;
        }
        Button MakeButton(string label, Vector2 min, Vector2 max, UnityEngine.Events.UnityAction clicked)
        {
            var image = Surface(safeRect, label, new Color(0, 0, 0, 0.12f), min, max);
            var button = image.gameObject.AddComponent<Button>(); button.targetGraphic = image;
            var colors = button.colors;
            colors.normalColor = Color.white; colors.highlightedColor = new Color(1, 0.5f, 0.4f); colors.pressedColor = new Color(0.7f, 0.18f, 0.12f); colors.fadeDuration = 0.2f; button.colors = colors;
            button.onClick.AddListener(clicked);
            var text = Label(image.transform, "Texto", label, 19, new Vector2(0.05f, 0.05f), new Vector2(0.95f, 0.95f));
            text.alignment = TextAnchor.MiddleCenter; return button;
        }
    }
}
