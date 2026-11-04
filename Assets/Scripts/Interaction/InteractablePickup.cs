using Piso7.Core;
using UnityEngine;

namespace Piso7.Interaction
{
    /// <summary>
    /// Objeto recolectable (nota, llave, tarjeta).
    /// </summary>
    public class InteractablePickup : MonoBehaviour, IInteractable
    {
        [SerializeField] string itemId = "nota_entrada";
        [SerializeField] string displayName = "Nota";
        [SerializeField] string prompt = "Presiona E para recoger";
        [SerializeField] string objectiveAfterPickup = "Encuentra el acceso al edificio.";
        [SerializeField] bool destroyOnPickup = true;

        bool _taken;

        public string Prompt => prompt;
        public bool CanInteract => !_taken;

        public void Interact(PlayerInteractor interactor)
        {
            if (_taken)
                return;

            _taken = true;
            Debug.Log($"[Piso7] Recogido: {displayName} ({itemId})");

            if (GameSession.Instance != null && !string.IsNullOrWhiteSpace(objectiveAfterPickup))
                GameSession.Instance.SetObjective(objectiveAfterPickup);

            if (destroyOnPickup)
                Destroy(gameObject);
            else
                gameObject.SetActive(false);
        }
    }
}
