using UnityEngine;
using UnityEngine.UI;

namespace Piso7.Core
{
    /// <summary>
    /// HUD simple: objetivo + vida/resistencia. Funciona con Text o TMP.
    /// </summary>
    public class HudController : MonoBehaviour
    {
        [SerializeField] Text objectiveText;
        [SerializeField] Text healthText;
        [SerializeField] Text staminaText;
        [SerializeField] Text promptText;
        [SerializeField] Interaction.PlayerInteractor interactor;

        void OnEnable()
        {
            if (GameSession.Instance != null)
            {
                GameSession.Instance.ObjectiveChanged += OnObjective;
                GameSession.Instance.VitalsChanged += OnVitals;
                OnObjective(GameSession.Instance.CurrentObjective);
                OnVitals(GameSession.Instance.Health, GameSession.Instance.Stamina);
            }
        }

        void OnDisable()
        {
            if (GameSession.Instance != null)
            {
                GameSession.Instance.ObjectiveChanged -= OnObjective;
                GameSession.Instance.VitalsChanged -= OnVitals;
            }
        }

        void Update()
        {
            if (promptText == null)
                return;

            if (interactor == null)
                interactor = FindFirstObjectByType<Interaction.PlayerInteractor>();

            promptText.text = interactor != null ? interactor.CurrentPrompt : string.Empty;
        }

        void OnObjective(string objective)
        {
            if (objectiveText != null)
                objectiveText.text = objective;
        }

        void OnVitals(float health, float stamina)
        {
            if (healthText != null)
                healthText.text = $"Vida: {Mathf.RoundToInt(health)}";
            if (staminaText != null)
                staminaText.text = $"Resistencia: {Mathf.RoundToInt(stamina)}";
        }
    }
}
