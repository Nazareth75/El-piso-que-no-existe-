using UnityEngine;
using UnityEngine.InputSystem;

namespace Piso7.Campus
{
    /// <summary>
    /// Recorre el fondo de lado a lado sin personaje.
    /// A y D mueven la vista. Un clic corto activa un punto.
    /// </summary>
    public class CampusPanCamera : MonoBehaviour
    {
        [SerializeField] float startX = 9f;
        [SerializeField] float y = CampusMetrics.CameraY;
        [SerializeField] float panSpeed = 7f;

        [SerializeField] float cameraSize = CampusMetrics.CameraSize;
        [SerializeField] float worldWidth = CampusMetrics.WorldWidth;
        Camera _camera;
        float _minX = 4.8f;
        float _maxX = 13.2f;
        Vector2 _pressPosition;
        bool _pressing;
        bool _moved;

        public void Configure(float cameraX)
        {
            startX = cameraX;
            ApplyFrame();
        }

        void Awake()
        {
            _camera = GetComponent<Camera>();
            if (_camera != null)
            {
                _camera.orthographic = true;
                _camera.orthographicSize = cameraSize;
            }
        }

        void Start()
        {
            ApplyFrame();
        }

        void ApplyFrame()
        {
            if (_camera == null)
                _camera = GetComponent<Camera>();

            float half = Mathf.Min(VisibleHalfWidth(), worldWidth * 0.5f);
            _minX = half;
            _maxX = Mathf.Max(half, worldWidth - half);

            var p = transform.position;
            p.x = Mathf.Clamp(startX, _minX, _maxX);
            p.y = y;
            p.z = -10f;
            transform.position = p;
        }

        void Update()
        {
            if (PlayerAlive())
                return;

            float axis = ReadAxis();
            if (Mathf.Abs(axis) > 0.01f)
            {
                var p = transform.position;
                p.x = Mathf.Clamp(p.x + axis * panSpeed * Time.deltaTime, _minX, _maxX);
                p.y = y;
                transform.position = p;
            }
        }

        void LateUpdate()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                ReadClick();
                return;
            }

            var p = transform.position;
            p.x = Mathf.Clamp(player.transform.position.x, _minX, _maxX);
            p.y = y;
            transform.position = p;
            ReadClick();
        }

        static bool PlayerAlive()
        {
            return GameObject.FindGameObjectWithTag("Player") != null;
        }

        float VisibleHalfWidth()
        {
            var cam = _camera != null ? _camera : Camera.main;
            float aspect = cam != null ? cam.aspect : 16f / 9f;
            return cameraSize * aspect;
        }

        static float ReadAxis()
        {
            float axis = 0f;
            var keyboard = Keyboard.current;
            if (keyboard == null)
                return 0f;

            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
                axis -= 1f;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
                axis += 1f;
            return axis;
        }

        void ReadClick()
        {
            var mouse = Mouse.current;
            if (Piso7.Core.GameMenu.BloqueaJuego)
                return;

            if (mouse == null || _camera == null)
                return;

            if (mouse.leftButton.wasPressedThisFrame)
            {
                _pressing = true;
                _moved = false;
                _pressPosition = mouse.position.ReadValue();
            }

            if (!_pressing)
                return;

            if (Vector2.Distance(_pressPosition, mouse.position.ReadValue()) > 8f)
                _moved = true;

            if (!mouse.leftButton.wasReleasedThisFrame)
                return;

            _pressing = false;
            if (_moved)
                return;

            Vector3 screen = mouse.position.ReadValue();
            screen.z = Mathf.Abs(_camera.transform.position.z);
            Vector2 world = _camera.ScreenToWorldPoint(screen);
            var hit = Physics2D.OverlapPoint(world);
            if (hit == null)
                return;

            var point = hit.GetComponent<CampusHotspot>();
            if (point != null)
                point.Activar();

            var elevator = hit.GetComponent<CampusElevator>();
            if (elevator != null)
                elevator.Activar();
        }
    }
}

