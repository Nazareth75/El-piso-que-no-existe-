using UnityEngine;

namespace Piso7.CameraSystem
{
    /// <summary>
    /// Cámara lateral que sigue al jugador con suavizado y límites opcionales.
    /// </summary>
    public class CameraFollow2D : MonoBehaviour
    {
        [SerializeField] Transform target;
        [SerializeField] Vector3 offset = new Vector3(0f, 1.2f, -10f);
        [SerializeField] float smoothTime = 0.15f;
        [SerializeField] bool useBounds;
        [SerializeField] Vector2 minBounds = new Vector2(-20f, -2f);
        [SerializeField] Vector2 maxBounds = new Vector2(40f, 8f);

        Vector3 _velocity;

        public void SetTarget(Transform newTarget) => target = newTarget;

        void LateUpdate()
        {
            if (target == null)
                return;

            Vector3 desired = target.position + offset;
            if (useBounds)
            {
                desired.x = Mathf.Clamp(desired.x, minBounds.x, maxBounds.x);
                desired.y = Mathf.Clamp(desired.y, minBounds.y, maxBounds.y);
            }

            transform.position = Vector3.SmoothDamp(transform.position, desired, ref _velocity, smoothTime);
        }
    }
}
