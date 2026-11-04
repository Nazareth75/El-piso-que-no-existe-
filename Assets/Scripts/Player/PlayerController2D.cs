using UnityEngine;
using UnityEngine.InputSystem;

namespace Piso7.Player
{
    /// <summary>
    /// Movimiento lateral 2D: caminar, correr, saltar.
    /// Controles: WASD/flechas, Shift correr, Espacio saltar.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CapsuleCollider2D))]
    public class PlayerController2D : MonoBehaviour
    {
        [Header("Movimiento")]
        [SerializeField] float walkSpeed = 4.5f;
        [SerializeField] float runSpeed = 7.5f;
        [SerializeField] float jumpForce = 11f;
        [SerializeField] float groundedRememberTime = 0.1f;
        [SerializeField] float jumpBufferTime = 0.1f;

        [Header("Suelo")]
        [SerializeField] Transform groundCheck;
        [SerializeField] float groundCheckRadius = 0.12f;
        [SerializeField] LayerMask groundLayers;

        [Header("Resistencia al correr")]
        [SerializeField] float runStaminaCostPerSecond = 12f;
        [SerializeField] float staminaRecoverPerSecond = 8f;

        [SerializeField] bool faceLeftOnStart;
        Rigidbody2D _rb;
        SpriteRenderer _sprite;
        float _moveInput;
        bool _jumpPressed;
        bool _runHeld;
        float _groundedTimer;
        float _jumpBufferTimer;
        bool _facingRight = true;
        readonly Collider2D[] _groundHits = new Collider2D[12];
        CapsuleCollider2D _capsule;

        public bool IsGrounded { get; private set; }
        public bool IsRunning { get; private set; }
        public float MoveSpeedAbs => Mathf.Abs(_rb != null ? _rb.linearVelocity.x : 0f);
        public bool FacingRight => _facingRight;

        void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _capsule = GetComponent<CapsuleCollider2D>();
            _sprite = GetComponentInChildren<SpriteRenderer>();
            _facingRight = !faceLeftOnStart;
            if (_sprite != null) _sprite.flipX = faceLeftOnStart;
            _rb.freezeRotation = true;
            _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            _rb.interpolation = RigidbodyInterpolation2D.Interpolate;

            if (groundCheck == null)
            {
                var go = new GameObject("GroundCheck");
                go.transform.SetParent(transform);
                go.transform.localPosition = new Vector3(_capsule.offset.x, _capsule.offset.y - _capsule.size.y * 0.5f, 0f);
                groundCheck = go.transform;
            }
        }

        void Update()
        {
            ReadInput();
            UpdateFacing();
            UpdateGroundedMemory();
            TryJump();
            UpdateStamina();
        }

        void FixedUpdate()
        {
            if (!_rb.simulated) return;
            float targetSpeed = IsRunning ? runSpeed : walkSpeed;
            _rb.linearVelocity = new Vector2(_moveInput * targetSpeed, _rb.linearVelocity.y);
        }

        void ReadInput()
        {
            var keyboard = Keyboard.current;
            var gamepad = Gamepad.current;

            _moveInput = 0f;
            _runHeld = false;
            _jumpPressed = false;

            if (Core.GameMenu.BloqueaJuego)
                return;

            if (keyboard != null)
            {
                if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
                    _moveInput -= 1f;
                if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
                    _moveInput += 1f;

                _runHeld = keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed;
                _jumpPressed = keyboard.spaceKey.wasPressedThisFrame;
            }

            if (gamepad != null)
            {
                float stick = gamepad.leftStick.x.ReadValue();
                if (Mathf.Abs(stick) > 0.2f)
                    _moveInput = stick;

                if (gamepad.buttonEast.isPressed || gamepad.leftShoulder.isPressed)
                    _runHeld = true;

                if (gamepad.buttonSouth.wasPressedThisFrame)
                    _jumpPressed = true;
            }

            if (Mathf.Abs(Core.MobileControls.Move) > 0f) _moveInput = Core.MobileControls.Move;
            _runHeld |= Core.MobileControls.Run;
            _jumpPressed |= Core.MobileControls.Jump;
            _moveInput = Mathf.Clamp(_moveInput, -1f, 1f);

            if (_jumpPressed)
                _jumpBufferTimer = jumpBufferTime;
        }

        void UpdateGroundedMemory()
        {
            var filter = new ContactFilter2D { useTriggers = false };
            filter.SetLayerMask(groundLayers);
            int count = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, filter, _groundHits);
            IsGrounded = false;
            for (int i = 0; i < count; i++)
                if (_groundHits[i].attachedRigidbody != _rb && _groundHits[i].bounds.max.y <= _capsule.bounds.min.y + groundCheckRadius)
                    IsGrounded = true;
            if (IsGrounded)
                _groundedTimer = groundedRememberTime;
            else
                _groundedTimer -= Time.deltaTime;

            _jumpBufferTimer -= Time.deltaTime;
        }

        void TryJump()
        {
            bool canJump = _groundedTimer > 0f && _jumpBufferTimer > 0f;
            if (!canJump)
                return;

            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, jumpForce);
            _groundedTimer = 0f;
            _jumpBufferTimer = 0f;
        }

        void UpdateStamina()
        {
            bool wantsRun = _runHeld && Mathf.Abs(_moveInput) > 0.1f && IsGrounded;
            IsRunning = false;

            var session = Core.GameSession.Instance;
            if (wantsRun)
            {
                if (session == null || session.TrySpendStamina(runStaminaCostPerSecond * Time.deltaTime))
                    IsRunning = true;
            }
            else
            {
                session?.RecoverStamina(staminaRecoverPerSecond * Time.deltaTime);
            }
        }

        void UpdateFacing()
        {
            if (Mathf.Abs(_moveInput) < 0.05f)
                return;

            bool right = _moveInput > 0f;
            if (right == _facingRight)
                return;

            _facingRight = right;
            if (_sprite != null)
                _sprite.flipX = !_facingRight;
            else
                transform.localScale = new Vector3(_facingRight ? 1f : -1f, 1f, 1f);
        }

        void OnDrawGizmosSelected()
        {
            if (groundCheck == null)
                return;
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}

