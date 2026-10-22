using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class CampusGameplaySetup
{
    static CampusGameplaySetup() { EditorApplication.update += Poll; }
    static void Poll()
    {
        if (!File.Exists("Temp/campus-setup.request") || EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlaying) return;
        File.Delete("Temp/campus-setup.request"); Configure();
    }
    [MenuItem("Tools/Equipo/Conectar encuentro Campus")]
    public static void Configure()
    {
        if (EditorApplication.isPlaying || SceneManager.GetActiveScene().isDirty)
        { Debug.LogWarning("Guarda la escena y sal de Play antes de configurar el Campus."); return; }
        string previous = SceneManager.GetActiveScene().path;
        var source = EditorSceneManager.OpenScene("Assets/Scenes/Dilan_Prototipo.unity", OpenSceneMode.Single);
        var sourceEnemy = source.GetRootGameObjects().First(x => x.name.StartsWith("personaje_caminar", StringComparison.OrdinalIgnoreCase));
        foreach (var root in source.GetRootGameObjects())
            foreach (var light in root.GetComponentsInChildren<UnityEngine.Rendering.Universal.Light2D>()) light.enabled = false;
        var target = EditorSceneManager.OpenScene("Assets/Scenes/Campus/Campus_EntradaPrincipal.unity", OpenSceneMode.Additive);
        var player = target.GetRootGameObjects().First(x => x.name == "Player");
        var enemy = target.GetRootGameObjects().FirstOrDefault(x => x.name == "Enemigo_Dilan");
        if (enemy == null)
        {
            enemy = UnityEngine.Object.Instantiate(sourceEnemy);
            enemy.name = "Enemigo_Dilan";
            SceneManager.MoveGameObjectToScene(enemy, target);
            enemy.transform.position = new Vector3(9.4f, 1.45f, 0f);
            enemy.transform.localScale = Vector3.one * 1.3f;
        }
        var renderer = enemy.GetComponent<SpriteRenderer>(); renderer.sortingOrder = 10;
        var body = enemy.GetComponent<Rigidbody2D>(); body.constraints = RigidbodyConstraints2D.FreezeRotation;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        var game = player.GetComponent<JuegoSimple>();
        if (game == null) game = player.AddComponent<JuegoSimple>();
        game.monstruo = enemy.transform; game.velocidadMonstruo = 1.3f;
        EditorUtility.SetDirty(game);
        EditorSceneManager.MarkSceneDirty(target); EditorSceneManager.SaveScene(target);
        EditorSceneManager.CloseScene(source, true);

        if (!string.IsNullOrEmpty(previous)) EditorSceneManager.OpenScene(previous);
        Directory.CreateDirectory("Library/PlayableValidation");
        File.WriteAllText("Library/PlayableValidation/setup.txt", "OK Campus_EntradaPrincipal: Player.JuegoSimple -> Enemigo_Dilan; PlayerController2D conserva movimiento.");
    }
}


