using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

// Repeatable editor checks. Reports are local in Library/TeamValidation.
[InitializeOnLoad]
public static class TeamIntegrationValidation
{
    const string Key = "TeamValidation.Step";
    static double next;
    static float nextGameTime;
    static int index;
    static string[] scenes;
    static string Report => Path.GetFullPath("Library/TeamValidation/report.txt");
    static TeamIntegrationValidation()
    {
        EditorApplication.update += Tick;
        Application.logMessageReceived += Log;
    }
    static void Write(string s) { Directory.CreateDirectory(Path.GetDirectoryName(Report)); File.AppendAllText(Report, s + "\n"); }
    static void Log(string message, string stack, LogType type)
    {
        if (SessionState.GetInt(Key, 0) > 0 && (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)) Write("ERROR " + message + "\n" + stack);
    }
    [MenuItem("Tools/Equipo/Validar integracion")]
    public static void Run()
    {
        if (EditorApplication.isPlaying || EditorApplication.isCompiling) return;
        if (SceneManager.GetActiveScene().isDirty) { Debug.LogWarning("Guarda la escena antes de validar."); return; }
        next = 0; nextGameTime = 0;
        typeof(Editor).Assembly.GetType("UnityEditor.LogEntries")?.GetMethod("Clear", BindingFlags.Static | BindingFlags.Public)?.Invoke(null, null);
        Directory.CreateDirectory(Path.GetDirectoryName(Report)); File.WriteAllText(Report, "Unity " + Application.unityVersion + "\n");
        SessionState.SetString("TeamValidation.Scene", SceneManager.GetActiveScene().path);
        SessionState.SetInt(Key, 1);
        foreach (var entry in EditorBuildSettings.scenes.Where(x => x.enabled))
        {
            var scene = EditorSceneManager.OpenScene(entry.path);
            foreach (var root in scene.GetRootGameObjects())
            foreach (var t in root.GetComponentsInChildren<Transform>(true))
            {
                if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject) > 0) Write("ERROR Missing Script " + entry.path + " " + t.name);
                foreach (var c in t.GetComponents<Component>().Where(c => c != null))
                {
                    var it = new SerializedObject(c).GetIterator();
                    while (it.NextVisible(true))
                        if (it.propertyType == SerializedPropertyType.ObjectReference && it.objectReferenceValue == null && it.objectReferenceInstanceIDValue != 0)
                            Write("ERROR Broken reference " + entry.path + " " + t.name + " " + it.propertyPath);
                }
            }
            Write("EDIT " + entry.path + " roots=" + scene.rootCount);
        }
        EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");
        SessionState.SetInt(Key, 1); EditorApplication.isPlaying = true;
    }
    static void Check(bool ok, string label) { Write((ok ? "PASS " : "ERROR ") + label); }
    static void Tick()
    {
        if (File.Exists("Temp/team-validation.request") && !EditorApplication.isCompiling && !EditorApplication.isUpdating)
        { File.Delete("Temp/team-validation.request"); Run(); return; }
        int step = SessionState.GetInt(Key, 0);
        if (step == 0 || EditorApplication.isCompiling || EditorApplication.timeSinceStartup < next) return;
        if (step == 99 && !EditorApplication.isPlaying)
        {
            SessionState.SetInt(Key, 0);
            string original = SessionState.GetString("TeamValidation.Scene", "");
            if (!string.IsNullOrEmpty(original)) EditorSceneManager.OpenScene(original);
            Write("COMPLETE"); return;
        }
        if (!EditorApplication.isPlaying || Time.time < nextGameTime) return;
        next = EditorApplication.timeSinceStartup + 0.05;
        nextGameTime = Time.time + 0.35f;
        try
        {
            switch (step)
            {
                case 1:
                    var menu = UnityEngine.Object.FindFirstObjectByType<Piso7.Core.GameMenu>();
                    Check(menu != null && GameObject.Find("MenuCanvas") != null, "MainMenu runtime UI");
                    typeof(Piso7.Core.GameMenu).GetMethod("OnPlay", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(menu, null);
                    SessionState.SetInt(Key, 2); break;
                case 2:
                    Check(SceneManager.GetActiveScene().name == "Campus_EntradaPrincipal", "MainMenu Jugar -> Campus_EntradaPrincipal");
                    scenes = EditorBuildSettings.scenes.Where(x => x.enabled && x.path.Contains("/Campus/")).Select(x => Path.GetFileNameWithoutExtension(x.path)).ToArray();
                    index = 0; SessionState.SetInt(Key, 3); break;
                case 3:
                    if (index < scenes.Length) { SceneManager.LoadScene(scenes[index++]); SessionState.SetInt(Key, 4); }
                    else { SceneManager.LoadScene("Dilan_Prototipo"); SessionState.SetInt(Key, 7); }
                    break;
                case 4:
                    Check(GameObject.Find("Player") != null && Camera.main != null && GameObject.Find("Suelo") != null, SceneManager.GetActiveScene().name + " runtime objects");
                    KeyboardInput(true); SessionState.SetInt(Key, 5); break;
                case 5:
                    var player = GameObject.Find("Player");
                    Check(player.GetComponent<Rigidbody2D>().linearVelocity.x > 0.1f, SceneManager.GetActiveScene().name + " movement D");
                    KeyboardInput(false);
                    var point = UnityEngine.Object.FindFirstObjectByType<Piso7.Campus.CampusHotspot>();
                    if (point != null) { SessionState.SetString("TeamValidation.Target", point.TargetScene); point.Activar(); SessionState.SetInt(Key, 6); }
                    else SessionState.SetInt(Key, 3);
                    break;
                case 6:
                    Check(SceneManager.GetActiveScene().name == SessionState.GetString("TeamValidation.Target", ""), "CampusTransition target"); SessionState.SetInt(Key, 3); break;
                case 7:
                    var freshGame = UnityEngine.Object.FindFirstObjectByType<JuegoSimple>();
                    Check(freshGame != null, "Dilan component");
                    freshGame.Reiniciar();
                    freshGame.velocidadMonstruo = 0f;
                    freshGame.monstruo.GetComponent<Rigidbody2D>().position = new Vector2(30f, -1f);
                    KeyboardInput(true); SessionState.SetInt(Key, 8); break;
                case 8:
                    var game = UnityEngine.Object.FindFirstObjectByType<JuegoSimple>();
                    Check(game.GetComponent<Rigidbody2D>().linearVelocity.x > 0.1f, "Dilan movement D"); KeyboardInput(false);
                    game.velocidadMonstruo = 4f;
                    game.GetComponent<Rigidbody2D>().position = game.monstruo.GetComponent<Rigidbody2D>().position;
                    SessionState.SetInt(Key, 9); break;
                case 9:
                    game = UnityEngine.Object.FindFirstObjectByType<JuegoSimple>(); Check(game.Termino && !game.Ganaste, "Dilan Game Over collision");
                    game.Reiniciar(); Check(!game.Termino, "Dilan retry");
                    game.transform.position = GameObject.Find("ActivadorVictoria").transform.position;
                    game.GetComponent<Rigidbody2D>().position = game.transform.position;
                    Physics2D.SyncTransforms();
                    SessionState.SetInt(Key, 10); break;
                case 10:
                    game = UnityEngine.Object.FindFirstObjectByType<JuegoSimple>(); Check(game.Termino && game.Ganaste, "Dilan victory trigger");
                    Write("Dilan final: player=" + game.transform.position + " trigger=" + GameObject.Find("ActivadorVictoria").transform.position + " isTrigger=" + GameObject.Find("ActivadorVictoria").GetComponent<Collider2D>().isTrigger + " termino=" + game.Termino + " ganaste=" + game.Ganaste);
                    SessionState.SetInt(Key, 99); EditorApplication.isPlaying = false; break;
            }
        }
        catch (Exception ex) { Write("ERROR " + ex); KeyboardInput(false); SessionState.SetInt(Key, 99); EditorApplication.isPlaying = false; }
    }
    static void KeyboardInput(bool down)
    {
        if (Keyboard.current == null) throw new InvalidOperationException("No keyboard device");
        InputSystem.QueueStateEvent(Keyboard.current, down ? new KeyboardState(UnityEngine.InputSystem.Key.D) : new KeyboardState());
        InputSystem.Update();
    }
}



