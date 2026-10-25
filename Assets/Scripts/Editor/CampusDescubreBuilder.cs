#if UNITY_EDITOR
using System.IO;
using System.Linq;
using Piso7.Campus;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace Piso7.EditorTools
{
    /// <summary>
    /// Menú: Piso7 → 4. Crear recorrido Descubre el campus.
    /// Arma los fondos laterales y los puntos de avance. No coloca al personaje.
    /// </summary>
    public static class CampusDescubreBuilder
    {
        const string SceneFolder = "Assets/Scenes/Campus";
        const string FontResource = "LegacyRuntime.ttf";

        struct PointDef
        {
            public string Sprite;
            public float X;
            public string TargetScene;
            public string Label;
        }

        struct PlaceDef
        {
            public string SceneName;
            public string Background;
            public string Piso;
            public string Nombre;
            public float CameraX;
            public PointDef[] Points;
        }

        [MenuItem("Piso7/4. Crear recorrido Descubre el campus")]
        public static void CreateTour()
        {
            if (!EditorUtility.DisplayDialog("Recrear recorrido del campus",
                    "Esto BORRA y vuelve a crear Campus_EntradaPrincipal, Recepcion, HallPrincipal, Escaleras, PasilloPiso1 y LabHistologia. Se pierden los cambios hechos a mano en esas escenas.",
                    "Recrear", "Cancelar"))
                return;

            ConfigureCampusImports();
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

            Directory.CreateDirectory(SceneFolder);
            var places = BuildPlaces();
            foreach (var place in places)
                CreatePlace(place);

            var first = $"{SceneFolder}/{places[0].SceneName}.unity";
            EditorSceneManager.OpenScene(first);
            Debug.Log("[Piso7] Recorrido Descubre listo, sin personaje. Abre Campus_EntradaPrincipal y pulsa Play. A/D mira el lugar. Clic en un punto para avanzar.");
        }

        public static void ConfigureCampusImports()
        {
            var fondos = "Assets/Art/Campus/Descubre/Fondos";
            var puntos = "Assets/Art/Campus/Descubre/Puntos";
            foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", new[] { fondos }))
                ConfigureTexture(AssetDatabase.GUIDToAssetPath(guid));

            foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", new[] { puntos }))
                ConfigureTexture(AssetDatabase.GUIDToAssetPath(guid));

            AssetDatabase.Refresh();
        }

        static PlaceDef[] BuildPlaces()
        {
            return new[]
            {
                Place("Campus_EntradaPrincipal", "entrada_principal.png", "Planta baja", "Entrada principal", 12f,
                    Point("punto_avanzar.png", 12.16f, "Campus_Recepcion", "Recepción")),
                Place("Campus_Recepcion", "recepcion.png", "Planta baja", "Recepción", 9f,
                    Point("punto_atras.png", 1.64f, "Campus_EntradaPrincipal", "Entrada"),
                    Point("punto_avanzar.png", 16.36f, "Campus_HallPrincipal", "Hall")),
                Place("Campus_HallPrincipal", "hall_principal.png", "Planta baja", "Hall principal", 9f,
                    Point("punto_atras.png", 1.64f, "Campus_Recepcion", "Recepción"),
                    Point("punto_avanzar.png", 16.36f, "Campus_Escaleras", "Escaleras")),
                Place("Campus_Escaleras", "escaleras.png", "Planta baja", "Escaleras", 8f,
                    Point("punto_atras.png", 1.64f, "Campus_HallPrincipal", "Hall"),
                    Point("punto_subir.png", 8f, "Campus_PasilloPiso1", "Piso 1")),
                Place("Campus_PasilloPiso1", "pasillo_piso1.png", "Piso 1", "Pasillo principal", 9f,
                    Point("punto_bajar.png", 1.64f, "Campus_Escaleras", "Escaleras"),
                    Point("punto_avanzar.png", 16.4f, "Campus_LabHistologia", "Laboratorio")),
                Place("Campus_LabHistologia", "lab_histologia.png", "Piso 1", "Lab. de Histología", 7f,
                    Point("punto_atras.png", 1.64f, "Campus_PasilloPiso1", "Pasillo"))
            };
        }

        static PlaceDef Place(string scene, string background, string piso, string nombre, float cameraX, params PointDef[] points)
        {
            return new PlaceDef
            {
                SceneName = scene,
                Background = background,
                Piso = piso,
                Nombre = nombre,
                CameraX = cameraX,
                Points = points
            };
        }

        static PointDef Point(string sprite, float x, string target, string label)
        {
            return new PointDef { Sprite = sprite, X = x, TargetScene = target, Label = label };
        }

        static void CreatePlace(PlaceDef place)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            camGo.AddComponent<AudioListener>();
            cam.orthographic = true;
            cam.orthographicSize = CampusMetrics.CameraSize;
            cam.backgroundColor = new Color(0.45f, 0.62f, 0.78f);
            cam.transform.position = new Vector3(place.CameraX, CampusMetrics.CameraY, -10f);
            camGo.AddComponent<CampusPanCamera>().Configure(place.CameraX);

            var lightGo = new GameObject("Global Light 2D");
            var light = lightGo.AddComponent<UnityEngine.Rendering.Universal.Light2D>();
            light.lightType = UnityEngine.Rendering.Universal.Light2D.LightType.Global;
            light.intensity = 1f;

            var info = new GameObject("Lugar");
            info.AddComponent<CampusPlaceInfo>().Configure(place.Piso, place.Nombre);

            var bg = new GameObject("Fondo");
            var bgRenderer = bg.AddComponent<SpriteRenderer>();
            bgRenderer.sprite = LoadSprite($"Assets/Art/Campus/Descubre/Fondos/{place.Background}");
            bgRenderer.sortingOrder = -20;
            bg.transform.position = Vector3.zero;

            CreateGround();
            CreateWall("Limite_Izquierdo", -0.4f);
            CreateWall("Limite_Derecho", CampusMetrics.WorldWidth + 0.4f);

            foreach (var point in place.Points)
                CreatePoint(point);

            CreateOverlay();

            var path = $"{SceneFolder}/{place.SceneName}.unity";
            EditorSceneManager.SaveScene(scene, path);
            AddSceneToBuildSettings(path);
        }

        static void CreateGround()
        {
            var ground = new GameObject("Suelo");
            var col = ground.AddComponent<BoxCollider2D>();
            col.size = new Vector2(CampusMetrics.WorldWidth, 0.4f);
            ground.transform.position = new Vector3(
                CampusMetrics.WorldWidth * 0.5f,
                CampusMetrics.FloorY - 0.2f,
                0f);
        }

        static void CreateWall(string name, float x)
        {
            var wall = new GameObject(name);
            wall.transform.position = new Vector3(x, CampusMetrics.FloorY + 2f, 0f);
            var col = wall.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.4f, 6f);
        }

        static void CreatePoint(PointDef point)
        {
            var go = new GameObject($"Punto_{point.Label}");
            go.transform.position = new Vector3(
                point.X,
                CampusMetrics.FloorY + CampusMetrics.PointSize * 0.5f,
                0f);

            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = LoadSprite($"Assets/Art/Campus/Descubre/Puntos/{point.Sprite}");
            renderer.sortingOrder = 8;

            var col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.95f, 0.95f);

            go.AddComponent<CampusHotspot>().Configure(point.TargetScene, point.Label);
        }

        static void CreateOverlay()
        {
            var canvasGo = new GameObject("HUD");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            canvasGo.AddComponent<GraphicRaycaster>();

            var font = Resources.GetBuiltinResource<Font>(FontResource);
            if (font == null)
                font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            Text Make(string name, Vector2 anchor, Vector2 pivot, Vector2 pos, int size, TextAnchor align)
            {
                var go = new GameObject(name);
                go.transform.SetParent(canvasGo.transform, false);
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

            var title = Make("Titulo", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(24f, -18f), 26, TextAnchor.UpperLeft);
            var hint = Make("Pista", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 22f), 18, TextAnchor.LowerCenter);
            hint.color = new Color(1f, 0.95f, 0.75f);

            var labels = new GameObject("Etiquetas");
            labels.transform.SetParent(canvasGo.transform, false);
            var labelRect = labels.AddComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            var overlay = canvasGo.AddComponent<CampusOverlay>();
            var so = new SerializedObject(overlay);
            so.FindProperty("title").objectReferenceValue = title;
            so.FindProperty("hint").objectReferenceValue = hint;
            so.FindProperty("labelRoot").objectReferenceValue = labelRect;
            so.FindProperty("font").objectReferenceValue = font;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static Sprite LoadSprite(string path)
        {
            return AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().FirstOrDefault()
                   ?? AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        static void ConfigureTexture(string path)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
                return;

            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = CampusMetrics.PixelsPerUnit;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
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
}
#endif
