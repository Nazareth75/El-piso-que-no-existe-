using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class CrearEscena
{
    public static void Preparar()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
            AssetDatabase.CreateFolder("Assets", "Scenes");

        var escena = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var camaraGo = new GameObject("Main Camera");
        camaraGo.tag = "MainCamera";
        var camara = camaraGo.AddComponent<Camera>();
        camara.orthographic = true;
        camara.orthographicSize = 10.4f;
        camara.clearFlags = CameraClearFlags.SolidColor;
        camara.backgroundColor = new Color(0.04f, 0.1f, 0.07f);
        camaraGo.transform.position = new Vector3(20f, 10f, -10f);
        camaraGo.AddComponent<AudioListener>();

        var director = new GameObject("Director");
        director.AddComponent<DirectorCampus>();
        director.AddComponent<InterfazCampus>();

        EditorSceneManager.SaveScene(escena, "Assets/Scenes/Campus.unity");
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene("Assets/Scenes/Campus.unity", true)
        };

        PlayerSettings.productName = "El piso que no existe 2.0";
        PlayerSettings.companyName = "PIXEL404";
        PlayerSettings.fullScreenMode = FullScreenMode.FullScreenWindow;
        PlayerSettings.defaultScreenWidth = 1280;
        PlayerSettings.defaultScreenHeight = 720;
        AssetDatabase.SaveAssets();
        EditorApplication.Exit(0);
    }
}
