using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

[InitializeOnLoad]
public static class DesktopBuild
{
    static DesktopBuild() { EditorApplication.update += Poll; }
    static void Poll()
    {
        const string request = "Temp/windows-build.request";
        if (!File.Exists(request) || EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode) return;
        string output = File.ReadAllText(request).Trim();
        File.Delete(request);
        EditorApplication.delayCall += () => {
            try { Build(output); File.WriteAllText("Library/windows-build-result.txt", "SUCCESS " + output); }
            catch (Exception ex) { File.WriteAllText("Library/windows-build-result.txt", ex.ToString()); Debug.LogException(ex); }
        };
    }
    public static void Build(string output)
    {
        var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
        if (scenes.Length == 0 || scenes[0] != "Assets/Scenes/MainMenu.unity" || scenes.Any(s => !File.Exists(s))) throw new Exception("Escenas de build incompletas");
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(output)));
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions { scenes = scenes, locationPathName = output, target = BuildTarget.StandaloneWindows64, options = BuildOptions.None });
        if (report.summary.result != BuildResult.Succeeded) throw new Exception("Windows: " + report.summary.result + ", errores: " + report.summary.totalErrors);
    }
}
