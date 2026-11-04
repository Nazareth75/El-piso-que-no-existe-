using UnityEngine;

namespace Piso7.Core
{
    /// <summary>
    /// Placeholder de menú: carga Level1_Entrada al pulsar Play si se asigna en un botón UI.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] string firstLevelScene = "Level1_Entrada";

        public void StartGame()
        {
            if (string.IsNullOrWhiteSpace(firstLevelScene))
                return;
            UnityEngine.SceneManagement.SceneManager.LoadScene(firstLevelScene);
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
