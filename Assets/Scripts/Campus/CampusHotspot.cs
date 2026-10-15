using System.Collections;
using UnityEngine;

namespace Piso7.Campus
{
    /// <summary>
    /// Punto de avance. El personaje cambia de escena al llegar a la flecha.
    /// </summary>
    public class CampusHotspot : MonoBehaviour, Interaction.IInteractable
    {
        [SerializeField] string targetScene;
        [SerializeField] string label = "Avanzar";

        bool _checked;
        bool _blocked = true;
        float _dwell;

        const float DwellNeeded = 0.22f;
        const float PassSpeed = 0.55f;

        static int _bestFrame = -1;
        static CampusHotspot _best;
        static float _bestDist;

        public string TargetScene => targetScene;
        public string Label => label;
        public string Prompt => label;
        public bool CanInteract => !string.IsNullOrWhiteSpace(targetScene);

        public void Configure(string scene, string pointLabel)
        {
            targetScene = scene;
            label = pointLabel;
        }

        public void Interact(Interaction.PlayerInteractor interactor)
        {
            Activar();
        }

        public void Activar()
        {
            if (!CanInteract)
                return;

            CampusTransition.Load(targetScene);
        }

        IEnumerator Start()
        {
            yield return new WaitForFixedUpdate();
            _blocked = PlayerTouching();
            _checked = true;
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            TryPass(other);
        }

        void OnTriggerStay2D(Collider2D other)
        {
            TryPass(other);
        }

        void OnTriggerExit2D(Collider2D other)
        {
            if (!other.CompareTag("Player"))
                return;

            _blocked = false;
            _dwell = 0f;
        }

        void LateUpdate()
        {
            if (_best != this || _bestFrame != Time.frameCount)
                return;

            _best = null;
            Activar();
        }

        void TryPass(Collider2D other)
        {
            if (Core.GameMenu.BloqueaJuego)
                return;

            if (!_checked || _blocked || !other.CompareTag("Player"))
                return;

            var body = other.attachedRigidbody;
            float speed = body != null ? Mathf.Abs(body.linearVelocity.x) : 0f;
            if (speed > PassSpeed)
            {
                _dwell = 0f;
                return;
            }

            _dwell += Time.fixedDeltaTime;
            if (_dwell < DwellNeeded)
                return;

            float dist = Mathf.Abs(other.transform.position.x - transform.position.x);
            if (_best == null || _bestFrame != Time.frameCount || dist < _bestDist)
            {
                _bestFrame = Time.frameCount;
                _best = this;
                _bestDist = dist;
            }
        }

        bool PlayerTouching()
        {
            var col = GetComponent<Collider2D>();
            if (col == null)
                return false;

            var hits = Physics2D.OverlapBoxAll(col.bounds.center, col.bounds.size, 0f);
            foreach (var hit in hits)
            {
                if (hit != null && hit.CompareTag("Player"))
                    return true;
            }

            return false;
        }

        void Update()
        {
            float pulse = 1f + Mathf.Sin(Time.time * 3f + transform.position.x) * 0.045f;
            transform.localScale = new Vector3(pulse, pulse, 1f);
        }
    }
}
