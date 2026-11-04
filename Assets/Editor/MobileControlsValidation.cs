using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Piso7.Core;
using Piso7.Player;

// Integration check in Play Mode. This is not a substitute for testing the APK.
[InitializeOnLoad]
public static class MobileControlsValidation
{
    const string StateKey = "MobileControlsValidation.State";
    const string Report = "Library/mobile-controls-validation.txt";
    static double deadline;
    static int stage;
    static float startX;
    static float startY;
    static MobileHoldButton right;
    static PointerEventData first, second;

    static MobileControlsValidation() { EditorApplication.update += Tick; }

    [MenuItem("Tools/Equipo/Probar controles tactiles en editor")]
    public static void Run()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling)
            throw new InvalidOperationException("Sal de Play Mode y espera la compilación.");
        if (SceneManager.GetActiveScene().isDirty)
            throw new InvalidOperationException("Guarda primero la escena abierta.");
        SessionState.SetString("MobileControlsValidation.Scene", SceneManager.GetActiveScene().path);
        File.WriteAllText(Report, "Prueba de integración en editor Unity " + Application.unityVersion + "\n");
        EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");
        SessionState.SetInt(StateKey, 1);
        EditorApplication.isPlaying = true;
    }

    static void Check(bool condition, string label)
    {
        File.AppendAllText(Report, (condition ? "PASS " : "FAIL ") + label + "\n");
        if (!condition) throw new Exception(label);
    }

    static void Tick()
    {
        if (File.Exists("Temp/mobile-controls-validation.request") && !BuildPipeline.isBuildingPlayer &&
            !EditorApplication.isCompiling && !EditorApplication.isUpdating)
        {
            File.Delete("Temp/mobile-controls-validation.request");
            Run(); return;
        }
        int state = SessionState.GetInt(StateKey, 0);
        if (state == 0 || EditorApplication.isCompiling) return;
        if (state == 9 && !EditorApplication.isPlayingOrWillChangePlaymode)
        {
            SessionState.SetInt(StateKey, 0);
            var scene = SessionState.GetString("MobileControlsValidation.Scene", "");
            if (File.Exists(scene)) EditorSceneManager.OpenScene(scene);
            return;
        }
        if (!EditorApplication.isPlaying) return;
        try
        {
            if (state == 1)
            {
                if (Time.timeSinceLevelLoad < 0.3f) return;
                if (UnityEngine.Object.FindFirstObjectByType<MobileControls>() == null) return;
                Check(GameObject.Find("Acciones") == null, "Acciones ocultas en MainMenu");
                SceneManager.LoadScene("Campus_EntradaPrincipal");
                SessionState.SetInt(StateKey, 2); deadline = EditorApplication.timeSinceStartup + 1;
                stage = 0; return;
            }
            if (state != 2 || EditorApplication.timeSinceStartup < deadline) return;
            var player = UnityEngine.Object.FindFirstObjectByType<PlayerController2D>();
            Check(player != null, "Jugador disponible, etapa " + stage);
            switch (stage++)
            {
                case 0:
                    Check(GameObject.Find("Acciones") != null, "Acciones visibles en juego");
                    right = GameObject.Find("DER").GetComponent<MobileHoldButton>();
                    first = new PointerEventData(EventSystem.current) { pointerId = 41 };
                    second = new PointerEventData(EventSystem.current) { pointerId = 42 };
                    startX = player.transform.position.x;
                    right.OnPointerDown(first); right.OnPointerDown(second);
                    deadline = EditorApplication.timeSinceStartup + 0.35; break;
                case 1:
                    Check(player.transform.position.x > startX + 0.1f, "Botón DER mueve al jugador real");
                    right.OnPointerUp(first);
                    Check(right.Held, "Soltar un dedo conserva el segundo");
                    right.OnPointerExit(second);
                    Check(!right.Held, "Salir del botón libera el movimiento");
                    startY = player.transform.position.y;
                    GameObject.Find("SALTAR").GetComponent<Button>().onClick.Invoke();
                    deadline = EditorApplication.timeSinceStartup + 0.15; break;
                case 2:
                    Check(!MobileControls.Jump, "Salto no queda retenido después de un toque");
                    Check(player.transform.position.y > startY + 0.1f, "SALTAR eleva al jugador real");
                    right.OnPointerDown(first);
                    GameObject.Find("PAUSA").GetComponent<Button>().onClick.Invoke();
                    deadline = EditorApplication.timeSinceStartup + 0.25; break;
                case 3:
                    Check(GameMenu.BloqueaJuego && Time.timeScale == 0f, "PAUSA abre menú y detiene juego");
                    Check(MobileControls.Move == 0 && !right.Held, "Pausa libera dirección y dedos");
                    GameMenu.Instance.SendMessage("OnPlay");
                    deadline = EditorApplication.timeSinceStartup + 0.25; break;
                case 4:
                    Check(!GameMenu.BloqueaJuego && Time.timeScale == 1f, "Reanudar devuelve el control");
                    right.OnPointerDown(first);
                    UnityEngine.Object.FindFirstObjectByType<MobileControls>().SendMessage("OnApplicationFocus", false);
                    Check(!right.Held && MobileControls.Move == 0f, "Pérdida de foco libera controles");
                    File.AppendAllText(Report, "FIN: pruebas editor completadas. APK en dispositivo pendiente.\n");
                    SessionState.SetInt(StateKey, 9); EditorApplication.isPlaying = false; break;
            }
        }
        catch (Exception exception)
        {
            File.AppendAllText(Report, "ERROR " + exception + "\n");
            SessionState.SetInt(StateKey, 9); EditorApplication.isPlaying = false;
            Debug.LogException(exception);
        }
    }
}
