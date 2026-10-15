using UnityEngine;

namespace Piso7.Core
{
    /// <summary>
    /// Estado global mínimo de la partida (misión, vida, resistencia).
    /// </summary>
    public class GameSession : MonoBehaviour
    {
        public static GameSession Instance { get; private set; }

        [Header("Estado")]
        [SerializeField] float maxHealth = 100f;
        [SerializeField] float maxStamina = 100f;
        [SerializeField] string currentObjective = "Explora la entrada de UNIFRANZ.";

        public float Health { get; private set; }
        public float Stamina { get; private set; }
        public string CurrentObjective => currentObjective;

        public event System.Action<string> ObjectiveChanged;
        public event System.Action<float, float> VitalsChanged;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            Health = maxHealth;
            Stamina = maxStamina;
        }

        public void SetObjective(string objective)
        {
            currentObjective = objective;
            ObjectiveChanged?.Invoke(currentObjective);
        }

        public void ApplyDamage(float amount)
        {
            Health = Mathf.Max(0f, Health - amount);
            VitalsChanged?.Invoke(Health, Stamina);
        }

        public bool TrySpendStamina(float amount)
        {
            if (Stamina < amount)
                return false;

            Stamina -= amount;
            VitalsChanged?.Invoke(Health, Stamina);
            return true;
        }

        public void RecoverStamina(float amount)
        {
            Stamina = Mathf.Min(maxStamina, Stamina + amount);
            VitalsChanged?.Invoke(Health, Stamina);
        }
    }
}
