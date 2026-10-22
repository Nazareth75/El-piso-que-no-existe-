using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class EntryBackgroundSetup
{
    static EntryBackgroundSetup() { EditorApplication.update += Poll; }
    static void Poll()
    {
        if (File.Exists("Temp/entry-background-view.request") && !EditorApplication.isPlaying && !EditorApplication.isCompiling && !EditorApplication.isUpdating)
        {
            try { File.Delete("Temp/entry-background-view.request"); } catch (IOException) { return; }
            if (SceneManager.GetActiveScene().isDirty) return;
            EditorSceneManager.OpenScene("Assets/Scenes/Campus/Campus_EntradaPrincipal.unity");
            var view = EditorWindow.GetWindow<SceneView>(); view.in2DMode = true;
            view.Frame(new Bounds(new Vector3(9f, 5.6f, 0f), new Vector3(18f, 11.2f, 1f)), true); view.Focus();
            return;
        }
        if (!File.Exists("Temp/entry-background.request") || EditorApplication.isPlaying || EditorApplication.isCompiling || EditorApplication.isUpdating) return;
        try { File.Delete("Temp/entry-background.request"); } catch (IOException) { return; } Apply();
    }
    [MenuItem("Tools/Equipo/Aplicar fondo de entrada")]
    public static void Apply()
    {
        if (EditorApplication.isPlaying || SceneManager.GetActiveScene().isDirty) { Debug.LogWarning("Guarda los cambios de escena antes de aplicar el fondo."); return; }
        const string path = "Assets/Art/Backgrounds/Entrada/entrada_referencia.png";
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = 100; importer.filterMode = FilterMode.Point; importer.mipmapEnabled = false;
        importer.maxTextureSize = 2048; importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.SaveAndReimport();
        var source = EditorSceneManager.OpenScene("Assets/Scenes/Dilan_Prototipo.unity");
        var square = source.GetRootGameObjects().First(x => x.name == "Square").GetComponent<SpriteRenderer>().sprite;
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/Campus/Campus_EntradaPrincipal.unity");
        var roots = scene.GetRootGameObjects();
        var background = roots.First(x => x.name == "Fondo").GetComponent<SpriteRenderer>();
        background.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        float scale = 18f / background.sprite.bounds.size.x;
        background.transform.localScale = Vector3.one * scale;
        background.transform.position = new Vector3(9f, 0.64f + background.sprite.bounds.size.y * scale * 0.5f, 0f);
        var ground = roots.First(x => x.name == "Suelo");
        var floor = ground.transform.Find("SueloVisual");
        if (floor == null) { floor = new GameObject("SueloVisual").transform; floor.SetParent(ground.transform, false); }
        var renderer = floor.GetComponent<SpriteRenderer>(); if (renderer == null) renderer = floor.gameObject.AddComponent<SpriteRenderer>();
        renderer.sprite = square; renderer.sharedMaterial = background.sharedMaterial;
        renderer.color = new Color(0.52f, 0.52f, 0.54f); renderer.sortingOrder = 2;
        floor.localPosition = Vector3.zero; floor.localScale = new Vector3(18f / square.bounds.size.x, 0.4f / square.bounds.size.y, 1);
        var camera = roots.First(x => x.name == "Main Camera").GetComponent<Camera>();
        camera.orthographicSize = 5.65f; camera.backgroundColor = new Color(0.08f, 0.1f, 0.13f);
        camera.transform.position = new Vector3(9f, 5.65f, -10f);
        var settings = new SerializedObject(camera.GetComponent<Piso7.Campus.CampusPanCamera>());
        settings.FindProperty("cameraSize").floatValue = 5.65f;
        settings.FindProperty("worldWidth").floatValue = 18f;
        settings.FindProperty("startX").floatValue = 9f;
        settings.FindProperty("y").floatValue = 5.65f;
        settings.ApplyModifiedPropertiesWithoutUndo();
        roots.First(x => x.name == "Punto_Recepcion").transform.position = new Vector3(13.2f, 1f, 0f);
        EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
        File.WriteAllText("Library/entry-background.txt", "OK imagen original, proporcion conservada, suelo gris y conexion a recepcion.");
    }
}

