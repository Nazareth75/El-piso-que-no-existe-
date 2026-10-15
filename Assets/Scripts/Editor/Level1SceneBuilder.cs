#if UNITY_EDITOR
using System.IO;
using System.Linq;
using Piso7.CameraSystem;
using Piso7.Core;
using Piso7.Interaction;
using Piso7.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace Piso7.EditorTools
{
    /// <summary>
    /// Menú: Piso7 → Crear escena Nivel 1 (Entrada)
    /// Configura pixel art, jugador, fondo, suelo, HUD e interacción.
    /// </summary>
    public static class Level1SceneBuilder
    {
        const string ScenePath = "Assets/Scenes/Level1_Entrada.unity";
        const string PlayerSheet = "Assets/Art/Player/mujer_universitaria_1_sheet.png";
        const string PlayerFramesFolder = "Assets/Art/Player/Frames";
        const string BackgroundPath = "Assets/Art/Backgrounds/Entrada/entrada_universitaria.png";

        [MenuItem("Piso7/1. Configurar importación Pixel Art")]
        public static void ConfigurePixelArtImports()
        {
            ConfigureTexture(PlayerSheet, 100);
            foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", new[] { PlayerFramesFolder }))
                ConfigureTexture(AssetDatabase.GUIDToAssetPath(guid), 100);

            ConfigureTexture(BackgroundPath, 100);

            var introFolder = "Assets/Art/Backgrounds/IntroUNIFRANZ";
            foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", new[] { introFolder }))
                ConfigureTexture(AssetDatabase.GUIDToAssetPath(guid), 100);

            CampusDescubreBuilder.ConfigureCampusImports();

            AssetDatabase.Refresh();
            Debug.Log("[Piso7] Importación pixel art configurada (Filter=Point, Compression=None).");
        }

        [MenuItem("Piso7/2. Crear escena Nivel 1 (Entrada)")]
        public static void CreateLevel1()
        {
            ConfigurePixelArtImports();

            // Cierra cualquier escena abierta (evita 2 AudioListeners / plantillas URP).
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Cámara única
            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            camGo.AddComponent<AudioListener>();
            cam.orthographic = true;
            cam.orthographicSize = 5.5f;
            cam.backgroundColor = new Color(0.08f, 0.1f, 0.14f);
            cam.transform.position = new Vector3(0f, 2f, -10f);
            var follow = camGo.AddComponent<CameraFollow2D>();

            // Luz 2D (URP necesita al menos una)
            var lightGo = new GameObject("Global Light 2D");
            var light2d = lightGo.AddComponent<UnityEngine.Rendering.Universal.Light2D>();
            light2d.lightType = UnityEngine.Rendering.Universal.Light2D.LightType.Global;
            light2d.intensity = 1f;

            EnsureSingleAudioListener(camGo);

            // Fondo
            var bgSprite = LoadFirstSprite(BackgroundPath);
            var bg = new GameObject("Background_Entrada");
            var bgSr = bg.AddComponent<SpriteRenderer>();
            bgSr.sprite = bgSprite;
            bgSr.sortingOrder = -20;
            bg.transform.position = new Vector3(6f, 3.2f, 0f);
            float bgScale = 1.15f;
            bg.transform.localScale = Vector3.one * bgScale;

            // Suelo invisible ancho
            var ground = new GameObject("Ground");
            ground.layer = LayerMask.NameToLayer("Default");
            var groundCol = ground.AddComponent<BoxCollider2D>();
            groundCol.size = new Vector2(80f, 1f);
            ground.transform.position = new Vector3(10f, -0.5f, 0f);

            // Paredes límite
            CreateWall("Wall_Left", new Vector2(-2f, 3f), new Vector2(1f, 12f));
            CreateWall("Wall_Right", new Vector2(28f, 3f), new Vector2(1f, 12f));

            // Jugador
            var player = new GameObject("Player");
            player.tag = "Player";
            player.transform.position = new Vector3(1.5f, 1.2f, 0f);

            var rb = player.AddComponent<Rigidbody2D>();
            rb.gravityScale = 3f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            var capsule = player.AddComponent<CapsuleCollider2D>();
            capsule.size = new Vector2(0.9f, 2.2f);
            capsule.offset = new Vector2(0f, 0f);

            var visual = new GameObject("Visual");
            visual.transform.SetParent(player.transform);
            visual.transform.localPosition = Vector3.zero;
            var sr = visual.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 10;

            var walkSprites = LoadPlayerWalkSprites();
            if (walkSprites.Length > 0)
            {
                sr.sprite = walkSprites[0];
                // El sprite es grande (~4u). Lo escalamos a tamaño de personaje jugable.
                visual.transform.localScale = Vector3.one * 0.55f;
            }

            var controller = player.AddComponent<PlayerController2D>();
            var so = new SerializedObject(controller);
            so.FindProperty("groundLayers").intValue = ~0;
            so.ApplyModifiedPropertiesWithoutUndo();

            var animator = visual.AddComponent<PlayerWalkAnimator>();
            animator.SetFrames(walkSprites);

            player.AddComponent<PlayerInteractor>();

            follow.SetTarget(player.transform);

            // Nota interactuable
            var note = new GameObject("Nota_Bienvenida");
            note.transform.position = new Vector3(6f, 0.6f, 0f);
            var noteSr = note.AddComponent<SpriteRenderer>();
            noteSr.color = new Color(1f, 0.92f, 0.55f);
            noteSr.sprite = CreateFallbackSprite();
            noteSr.sortingOrder = 5;
            note.transform.localScale = new Vector3(0.6f, 0.8f, 1f);
            var noteCol = note.AddComponent<BoxCollider2D>();
            noteCol.isTrigger = true;
            noteCol.size = new Vector2(1.2f, 1.2f);
            note.AddComponent<InteractablePickup>();

            // Puerta
            var door = new GameObject("Puerta_Entrada");
            door.transform.position = new Vector3(18f, 1.2f, 0f);
            var doorSr = door.AddComponent<SpriteRenderer>();
            doorSr.color = new Color(0.45f, 0.7f, 0.95f, 0.85f);
            doorSr.sprite = CreateFallbackSprite();
            doorSr.sortingOrder = 4;
            door.transform.localScale = new Vector3(1.2f, 2.4f, 1f);
            var doorCol = door.AddComponent<BoxCollider2D>();
            doorCol.isTrigger = true;
            doorCol.size = new Vector2(1f, 1f);
            door.AddComponent<InteractableDoor>();

            // GameSession + HUD
            var sessionGo = new GameObject("GameSession");
            sessionGo.AddComponent<GameSession>();

            CreateHud(player.GetComponent<PlayerInteractor>());

            // Instrucciones en escena
            var guide = new GameObject("_LEEME");
            guide.AddComponent<SceneGuideNotes>();

            Directory.CreateDirectory("Assets/Scenes");
            EditorSceneManager.SaveScene(scene, ScenePath);
            AddSceneToBuildSettings(ScenePath);

            Selection.activeGameObject = player;
            Debug.Log("[Piso7] Escena Level1_Entrada creada. Pulsa Play para probar. Controles: A/D mover, Shift correr, Espacio saltar, E interactuar.");
        }

        [MenuItem("Piso7/3. Crear escena Intro UNIFRANZ (fondo animado)")]
        public static void CreateIntroScene()
        {
            ConfigurePixelArtImports();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            var cam = Camera.main;
            cam.orthographic = true;
            cam.orthographicSize = 4.7f;
            cam.transform.position = new Vector3(0f, 0f, -10f);

            var frames = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Art/Backgrounds/IntroUNIFRANZ/Frames" })
                .Select(AssetDatabase.GUIDToAssetPath)
                .OrderBy(p => p)
                .SelectMany(AssetDatabase.LoadAllAssetsAtPath)
                .OfType<Sprite>()
                .ToArray();

            var go = new GameObject("Intro_UNIFRANZ");
            var sr = go.AddComponent<SpriteRenderer>();
            var looper = go.AddComponent<AnimatedSpriteLooper>();
            looper.SetFrames(frames);
            if (frames.Length > 0)
            {
                sr.sprite = frames[0];
                float worldHeight = frames[0].bounds.size.y;
                float target = cam.orthographicSize * 2f;
                float scale = target / worldHeight;
                go.transform.localScale = Vector3.one * scale;
            }

            var path = "Assets/Scenes/Intro_UNIFRANZ.unity";
            Directory.CreateDirectory("Assets/Scenes");
            EditorSceneManager.SaveScene(scene, path);
            AddSceneToBuildSettings(path);
            Debug.Log("[Piso7] Escena Intro_UNIFRANZ creada con animación de 5 frames.");
        }

        static void EnsureSingleAudioListener(GameObject keepOn)
        {
            var listeners = Object.FindObjectsByType<AudioListener>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var listener in listeners)
            {
                if (listener != null && listener.gameObject != keepOn)
                    Object.DestroyImmediate(listener);
            }

            if (keepOn.GetComponent<AudioListener>() == null)
                keepOn.AddComponent<AudioListener>();
        }

        static void CreateWall(string name, Vector2 pos, Vector2 size)
        {
            var wall = new GameObject(name);
            wall.transform.position = pos;
            var col = wall.AddComponent<BoxCollider2D>();
            col.size = size;
        }

        static void CreateHud(PlayerInteractor interactor)
        {
            var canvasGo = new GameObject("HUD");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasGo.AddComponent<GraphicRaycaster>();

            Text MakeText(string name, Vector2 anchorMin, Vector2 anchorMax, TextAnchor align, int size)
            {
                var go = new GameObject(name);
                go.transform.SetParent(canvasGo.transform, false);
                var text = go.AddComponent<Text>();
                text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                if (text.font == null)
                    text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                text.fontSize = size;
                text.color = Color.white;
                text.alignment = align;
                text.text = name;
                var rt = text.rectTransform;
                rt.anchorMin = anchorMin;
                rt.anchorMax = anchorMax;
                rt.offsetMin = new Vector2(16f, 8f);
                rt.offsetMax = new Vector2(-16f, -8f);
                return text;
            }

            var objective = MakeText("Objective", new Vector2(0f, 1f), new Vector2(1f, 1f), TextAnchor.UpperLeft, 22);
            objective.rectTransform.pivot = new Vector2(0.5f, 1f);
            objective.rectTransform.anchoredPosition = new Vector2(0f, -12f);
            objective.rectTransform.sizeDelta = new Vector2(-32f, 40f);

            var health = MakeText("Health", new Vector2(0f, 0f), new Vector2(0.5f, 0f), TextAnchor.LowerLeft, 18);
            health.rectTransform.anchoredPosition = new Vector2(12f, 28f);
            health.rectTransform.sizeDelta = new Vector2(280f, 28f);

            var stamina = MakeText("Stamina", new Vector2(0f, 0f), new Vector2(0.5f, 0f), TextAnchor.LowerLeft, 18);
            stamina.rectTransform.anchoredPosition = new Vector2(12f, 4f);
            stamina.rectTransform.sizeDelta = new Vector2(280f, 28f);

            var prompt = MakeText("Prompt", new Vector2(0f, 0f), new Vector2(1f, 0f), TextAnchor.LowerCenter, 20);
            prompt.rectTransform.anchoredPosition = new Vector2(0f, 60f);
            prompt.rectTransform.sizeDelta = new Vector2(-40f, 30f);
            prompt.color = new Color(1f, 0.95f, 0.6f);

            var hud = canvasGo.AddComponent<HudController>();
            var so = new SerializedObject(hud);
            so.FindProperty("objectiveText").objectReferenceValue = objective;
            so.FindProperty("healthText").objectReferenceValue = health;
            so.FindProperty("staminaText").objectReferenceValue = stamina;
            so.FindProperty("promptText").objectReferenceValue = prompt;
            so.FindProperty("interactor").objectReferenceValue = interactor;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static Sprite[] LoadPlayerWalkSprites()
        {
            var fromFrames = AssetDatabase.FindAssets("t:Texture2D", new[] { PlayerFramesFolder })
                .Select(AssetDatabase.GUIDToAssetPath)
                .OrderBy(p => p)
                .SelectMany(p => AssetDatabase.LoadAllAssetsAtPath(p).OfType<Sprite>())
                .ToArray();
            if (fromFrames.Length > 0)
                return fromFrames;

            return AssetDatabase.LoadAllAssetsAtPath(PlayerSheet).OfType<Sprite>().ToArray();
        }

        static Sprite LoadFirstSprite(string path)
        {
            return AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().FirstOrDefault()
                   ?? AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        static Sprite CreateFallbackSprite()
        {
            var tex = Texture2D.whiteTexture;
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 16f);
        }

        static void ConfigureTexture(string path, int ppu)
        {
            if (string.IsNullOrEmpty(path) || AssetDatabase.LoadMainAssetAtPath(path) == null)
                return;

            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
                return;

            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = ppu;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.SaveAndReimport();
        }

        static void AddSceneToBuildSettings(string scenePath)
        {
            var scenes = EditorBuildSettings.scenes.ToList();
            if (scenes.Any(s => s.path == scenePath))
                return;
            scenes.Add(new EditorBuildSettingsScene(scenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }

    public class SceneGuideNotes : MonoBehaviour
    {
        // Solo para que el objeto exista en la jerarquía.
    }
}
#endif
