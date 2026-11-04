using UnityEditor;

// The original team check now runs the expanded playable-flow regression suite.
public static class TeamIntegrationValidation
{
    [MenuItem("Tools/Equipo/Validar integracion")]
    public static void Run() => PlayableValidation.Run();
}
