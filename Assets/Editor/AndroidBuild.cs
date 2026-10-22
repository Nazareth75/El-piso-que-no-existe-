using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

[InitializeOnLoad]
public static class AndroidBuild
{
    static AndroidBuild() { EditorApplication.update += CheckRequest; }

    static void CheckRequest()
    {
        const string request = "Temp/android-build.request";
        if (!File.Exists(request) || EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)
            return;
        string output = File.ReadAllText(request).Trim();
        File.Delete(request);
        EditorApplication.delayCall += () =>
        {
            try
            {
                Environment.SetEnvironmentVariable("PISO_ANDROID_APK", output);
                Build();
                File.WriteAllText("Library/android-build-result.txt", "SUCCESS " + output);
            }
            catch (Exception exception)
            {
                File.WriteAllText("Library/android-build-result.txt", exception.ToString());
                Debug.LogException(exception);
            }
        };
    }

    [MenuItem("Tools/Equipo/Configurar Android horizontal")]
    public static void Configure()
    {
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
        PlayerSettings.allowedAutorotateToPortrait = false;
        PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
        PlayerSettings.allowedAutorotateToLandscapeLeft = true;
        PlayerSettings.allowedAutorotateToLandscapeRight = true;
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.pacus777.elpisoquenoexiste");
        PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel25;
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
        EditorUserBuildSettings.buildAppBundle = false;
        EditorUserBuildSettings.exportAsGoogleAndroidProject = false;
        AssetDatabase.SaveAssets();
        Debug.Log("Android configurado: horizontal, IL2CPP ARM64, APK.");
    }

    [MenuItem("Tools/Equipo/Generar APK Android")]
    public static void Build()
    {
        Configure();
        if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Android, BuildTarget.Android))
            throw new BuildFailedException("Instala Android Build Support, SDK, NDK y OpenJDK desde Unity Hub.");
        var scenes = EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(scene => scene.path).ToArray();
        if (scenes.Length == 0 || scenes[0] != "Assets/Scenes/MainMenu.unity" || scenes.Any(scene => !File.Exists(scene)))
            throw new BuildFailedException("Revisa las escenas incluidas; MainMenu debe ser la primera.");
        string output = Environment.GetEnvironmentVariable("PISO_ANDROID_APK");
        if (string.IsNullOrWhiteSpace(output)) output = "Builds/Android/ElPisoQueNoExiste.apk";
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(output)));
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = scenes, locationPathName = output, target = BuildTarget.Android,
            options = BuildOptions.None
        });
        if (report.summary.result != BuildResult.Succeeded)
            throw new BuildFailedException("Falló el APK: " + report.summary.result + "; errores: " + report.summary.totalErrors);
        Debug.Log("APK generado: " + Path.GetFullPath(output));
    }
}
