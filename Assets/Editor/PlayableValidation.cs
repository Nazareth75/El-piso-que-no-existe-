using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;
using Piso7.Core;
using Piso7.Player;

[InitializeOnLoad]
public static class PlayableValidation
{
    const string StepKey = "PlayableValidation.Step";
    static float wait, startX, deathStart;
    static int gender, campusIndex;
    static string destination;
    static string[] campus;
    static string Report => "Library/PlayableValidation/report.txt";
    static PlayableValidation() { EditorApplication.update += Tick; Application.logMessageReceived += Log; }
    static void Write(string s) { Directory.CreateDirectory("Library/PlayableValidation"); File.AppendAllText(Report, s + "\n"); }
    static void Log(string s, string stack, LogType t) { if (SessionState.GetInt(StepKey, 0) > 0 && (t == LogType.Error || t == LogType.Exception || t == LogType.Assert)) Write("ERROR " + s + "\n" + stack); }
    static void Check(bool ok, string s) { Write((ok ? "PASS " : "ERROR ") + s); }
    static JuegoSimple Game => UnityEngine.Object.FindFirstObjectByType<JuegoSimple>();
    static void Key(params UnityEngine.InputSystem.Key[] keys) { InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState(keys)); InputSystem.Update(); }
    static Button Button(string name) => UnityEngine.Object.FindObjectsByType<Button>(FindObjectsSortMode.None).First(x => x.name == name && x.gameObject.activeInHierarchy);
    [MenuItem("Tools/Equipo/Probar flujo jugable")]
    public static void Run()
    {
        if (EditorApplication.isPlaying || EditorApplication.isCompiling || SceneManager.GetActiveScene().isDirty) return;
        Directory.CreateDirectory("Library/PlayableValidation"); File.WriteAllText(Report, "Unity " + Application.unityVersion + "\n");
        SessionState.SetInt("PlayableValidation.Choice", PlayerWalkAnimator.Selected);
        typeof(Editor).Assembly.GetType("UnityEditor.LogEntries")?.GetMethod("Clear")?.Invoke(null, null);
        SessionState.SetInt(StepKey, 1);
        foreach (var entry in EditorBuildSettings.scenes.Where(x => x.enabled))
        {
            Check(File.Exists(entry.path), "Build scene exists: " + entry.path);
            var scene = EditorSceneManager.OpenScene(entry.path);
            foreach (var root in scene.GetRootGameObjects())
            foreach (var t in root.GetComponentsInChildren<Transform>(true))
            {
                Check(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject) == 0, entry.path + "/" + t.name + " scripts");
                foreach (var c in t.GetComponents<Component>().Where(x => x != null))
                {
                    var it = new SerializedObject(c).GetIterator();
                    while (it.NextVisible(true))
                        if (it.propertyType == SerializedPropertyType.ObjectReference && it.objectReferenceValue == null && it.objectReferenceInstanceIDValue != 0) Write("ERROR broken reference " + entry.path + "/" + t.name + "/" + it.propertyPath);
                }
            }
        }
        EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");
        SessionState.SetInt("PlayableValidation.Gender", 1);
        wait = 0; gender = 1; EditorApplication.isPlaying = true;
    }
    static void Go(int step, float delay = 0.5f) { SessionState.SetInt(StepKey, step); wait = Time.time + delay; }
    static void Tick()
    {
        if (File.Exists("Temp/playable-validation.request") && !EditorApplication.isCompiling && !EditorApplication.isUpdating)
        { try { File.Delete("Temp/playable-validation.request"); } catch (IOException) { return; } Run(); return; }
        int step = SessionState.GetInt(StepKey, 0);
        if (step == 0 || EditorApplication.isCompiling) return;
        if (step == 99 && !EditorApplication.isPlaying)
        {
            SessionState.SetInt(StepKey, 0); PlayerWalkAnimator.Select(SessionState.GetInt("PlayableValidation.Choice", 0));
            EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity"); Write("COMPLETE"); return;
        }
        if (!EditorApplication.isPlaying || Time.time < wait) return;
        // Domain reload when entering Play resets static fields.
        if (gender != 0 && gender != 1) gender = 1;
        try
        {
            switch (step)
            {
                case 1:
                    gender = SessionState.GetInt("PlayableValidation.Gender", 1);
                    Button("BotonJugar").onClick.Invoke(); Go(2); break;
                case 2:
                    Check(SceneManager.GetActiveScene().name == "MainMenu" && Button("HOMBRE") != null && Button("MUJER") != null, "Jugar opens existing character selection");
                    Button(gender == 1 ? "HOMBRE" : "MUJER").onClick.Invoke(); Go(3); break;
                case 3:
                    Check(SceneManager.GetActiveScene().name == "Campus_EntradaPrincipal", "Choice -> Campus_EntradaPrincipal gender=" + gender);
                    Check(PlayerWalkAnimator.Selected == gender, "Saved choice gender=" + gender);
                    CheckVisual();
                    Check(Game.CampusCombat && Game.GetComponents<PlayerController2D>().Length == 1 && Game.monstruo != null, "One movement controller and connected Dilan encounter");
                    Game.velocidadMonstruo = 0f; Game.monstruo.GetComponent<Rigidbody2D>().position = new Vector2(14f, 1.4f);
                    startX = Game.transform.position.x; Key(UnityEngine.InputSystem.Key.D); Go(4); break;
                case 4:
                    Check(Game.transform.position.x > startX + 0.05f, "Movement gender=" + gender); Key();
                    Game.monstruo.GetComponent<Rigidbody2D>().position = Game.transform.position + new Vector3(1.45f, 0f, 0f);
                    Game.velocidadMonstruo = 1.3f; startX = Game.monstruo.position.x; Go(5, 0.15f); break;
                case 5:
                    Check(Game.monstruo.position.x < startX, "Pursuit gender=" + gender);
                    Game.velocidadMonstruo = 0f;
                    Game.monstruo.GetComponent<Rigidbody2D>().position = Game.transform.position + new Vector3(1.3f, 0f, 0f);
                    Physics2D.SyncTransforms(); Key(UnityEngine.InputSystem.Key.F); Game.SendMessage("Update"); Key();
                    Check(Game.EnemyHealth == 2, "F input attacks enemy gender=" + gender); Go(6); break;
                case 6:
                    Game.monstruo.GetComponent<Rigidbody2D>().position = Game.transform.position + new Vector3(1.3f, 0f, 0f);
                    Check(Game.TryAttack() && Game.EnemyHealth == 1, "Second hit damages enemy gender=" + gender); Go(7); break;
                case 7:
                    Game.monstruo.GetComponent<Rigidbody2D>().position = Game.transform.position + new Vector3(1.3f, 0f, 0f);
                    Check(Game.TryAttack(), "Third hit gender=" + gender); Go(8); break;
                case 8:
                    Check(Game.Ganaste && Game.Termino && Game.EnemyHealth == 0, "Enemy death and Victory gender=" + gender);
                    Button("Continuar").onClick.Invoke();
                    Check(Game.EncounterCleared && Game.GetComponent<Rigidbody2D>().simulated && !Game.monstruo.gameObject.activeSelf, "Continue resumes campus gender=" + gender);
                    var point = UnityEngine.Object.FindFirstObjectByType<Piso7.Campus.CampusHotspot>(); destination = point.TargetScene; point.Activar(); Go(9); break;
                case 9:
                    Check(SceneManager.GetActiveScene().name == destination && PlayerWalkAnimator.Selected == gender, "Transition preserves choice gender=" + gender); CheckVisual();
                    SceneManager.LoadScene("Campus_EntradaPrincipal"); Go(10); break;
                case 10:
                    deathStart = Time.time; Game.velocidadMonstruo = 0;
                    Game.GetComponent<Rigidbody2D>().position = Game.monstruo.position; Go(11); break;
                case 11:
                    Check(GameSession.Instance.Health < 100f && GameSession.Instance.Health > 0f, "Contact deals gradual damage gender=" + gender); Go(12); break;
                case 12:
                    if (!Game.Termino && Time.time - deathStart < 20f)
                    { Game.GetComponent<Rigidbody2D>().position = Game.monstruo.position; Go(12); break; }
                    Check(Game.Termino && !Game.Ganaste && GameSession.Instance.Health == 0f, "Player death and Game Over gender=" + gender);
                    Button("Reintentar").onClick.Invoke();
                    Check(!Game.Termino && GameSession.Instance.Health == 100f && Game.EnemyHealth == 3, "Retry resets health and enemy gender=" + gender);
                    if (gender == 1)
                    { SessionState.SetInt("PlayableValidation.Gender", 0); SceneManager.LoadScene("MainMenu"); Go(1); }
                    else
                    {
                        campus = EditorBuildSettings.scenes.Where(x => x.enabled && x.path.Contains("/Campus/") && !x.path.Contains("Campus_EntradaPrincipal")).Select(x => Path.GetFileNameWithoutExtension(x.path)).ToArray();
                        campusIndex = 0; Go(20);
                    }
                    break;
                case 20:
                    if (campusIndex >= campus.Length) { Key(); SceneManager.LoadScene("Campus_Clinica"); Go(30); break; }
                    SceneManager.LoadScene(campus[campusIndex++]); Go(21); break;
                case 21:
                    CheckVisual();
                    var hotspot = UnityEngine.Object.FindFirstObjectByType<Piso7.Campus.CampusHotspot>(); destination = hotspot.TargetScene;
                    Check(Application.CanStreamedLevelBeLoaded(destination), "Build target " + destination); hotspot.Activar(); Go(22); break;
                case 30:
                    Check(!MobileControls.Enabled, "Windows/editor standalone hides touch controls");
                    var boundaryPlayer = GameObject.Find("Player").GetComponent<Rigidbody2D>();
                    boundaryPlayer.position = new Vector2(25f, 1.7f); Go(31); break;
                case 31:
                    Check(GameObject.Find("Player").transform.position.x < 18f, "Floor edge clamps player inside walkway");
                    GameObject.Find("Player").GetComponent<Rigidbody2D>().position = new Vector2(6f, -20f); Go(32); break;
                case 32:
                    Check(GameObject.Find("Player").transform.position.y > -5f && GameMenu.BloqueaJuego, "Fall recovery returns player and opens options");
                    Button("Menú principal").onClick.Invoke(); Go(33); break;
                case 33:
                    Check(SceneManager.GetActiveScene().name == "MainMenu", "Fall menu returns to MainMenu without restart");
                    Key(); Go(99); EditorApplication.isPlaying = false; break;
                case 22:
                    Check(SceneManager.GetActiveScene().name == destination, "Campus transition " + destination); Go(20); break;
            }
        }
        catch (Exception ex) { Write("ERROR " + ex); Key(); Go(99); EditorApplication.isPlaying = false; }
    }
    static void CheckVisual()
    {
        var anim = UnityEngine.Object.FindFirstObjectByType<PlayerWalkAnimator>();
        var sprite = anim.GetComponent<SpriteRenderer>().sprite;
        string path = AssetDatabase.GetAssetPath(sprite);
        Check(sprite != null && (PlayerWalkAnimator.Selected == 1 ? path.Contains("Personaje2") : !path.Contains("Personaje2")), "Correct visual " + PlayerWalkAnimator.Selected + ": " + path);
    }
}
