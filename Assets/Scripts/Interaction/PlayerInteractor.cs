using UnityEngine;
using UnityEngine.InputSystem;

namespace Piso7.Interaction
{
    /// <summary>
    /// Detecta interactuables cercanos. Tecla E / gamepad West.
    /// </summary>
    public class PlayerInteractor : MonoBehaviour
    {
        [SerializeField] float radius = 1.2f;
        [SerializeField] LayerMask interactableMask = ~0;
        [SerializeField] Transform origin;

        IInteractable _current;

        public string CurrentPrompt => _current != null && _current.CanInteract ? _current.Prompt : string.Empty;

        void Awake()
        {
            if (origin == null)
                origin = transform;
        }

        void Update()
        {
            if (Piso7.Core.GameMenu.BloqueaJuego) { _current = null; return; }
            _current = FindClosest();

            bool pressed = false;
            if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
                pressed = true;
            if (Gamepad.current != null && Gamepad.current.buttonWest.wasPressedThisFrame)
                pressed = true;

            if (pressed && _current != null && _current.CanInteract)
                _current.Interact(this);
        }

        IInteractable FindClosest()
        {
            var hits = Physics2D.OverlapCircleAll(origin.position, radius, interactableMask);
            IInteractable best = null;
            float bestDist = float.MaxValue;

            foreach (var hit in hits)
            {
                var interactable = hit.GetComponent<IInteractable>() ?? hit.GetComponentInParent<IInteractable>();
                if (interactable == null || !interactable.CanInteract)
                    continue;

                float d = Vector2.Distance(origin.position, hit.transform.position);
                if (d < bestDist)
                {
                    bestDist = d;
                    best = interactable;
                }
            }

            return best;
        }

        void OnDrawGizmosSelected()
        {
            var p = origin != null ? origin.position : transform.position;
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(p, radius);
        }
    }
}
