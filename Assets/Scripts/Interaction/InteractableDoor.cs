using Piso7.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Piso7.Interaction
{
    /// <summary>
    /// Puerta / transición de zona. Puede requerir un ítem (placeholder).
    /// </summary>
    public class InteractableDoor : MonoBehaviour, IInteractable
    {
        [SerializeField] string prompt = "Presiona E para entrar";
        [SerializeField] string lockedPrompt = "Necesitas una tarjeta de acceso";
        [SerializeField] bool isLocked;
        [SerializeField] string requiredItemId;
        [SerializeField] string nextSceneName;
        [SerializeField] string objectiveWhenOpened = "Explora el interior de UNIFRANZ.";

        public string Prompt => isLocked ? lockedPrompt : prompt;
        public bool CanInteract => true;

        public void Interact(PlayerInteractor interactor)
        {
            if (isLocked)
            {
                Debug.Log($"[Piso7] Puerta bloqueada. Requiere: {requiredItemId}");
                return;
            }

            if (GameSession.Instance != null)
                GameSession.Instance.SetObjective(objectiveWhenOpened);

            if (!string.IsNullOrWhiteSpace(nextSceneName))
            {
                SceneManager.LoadScene(nextSceneName);
                return;
            }

            Debug.Log("[Piso7] Puerta abierta (sin escena destino configurada).");
        }

        public void Unlock()
        {
            isLocked = false;
        }
    }
}
