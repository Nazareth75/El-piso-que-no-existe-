using UnityEngine;

namespace Piso7.Player
{
    /// <summary>
    /// Reproduce la animación Walk del spritesheet / frames del personaje.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class PlayerWalkAnimator : MonoBehaviour
    {
        [SerializeField] Sprite[] walkFrames;
        [SerializeField] Sprite idleFrame;
        [SerializeField] float framesPerSecond = 10f;
        [SerializeField] float moveThreshold = 0.15f;

        const string ChoiceKey = "piso7.personaje";
        const float SecondScale = 1.73f;

        SpriteRenderer _renderer;
        PlayerController2D _controller;
        float _timer;
        int _index;
        Sprite[] _originalFrames;
        Sprite _originalIdle;
        Vector3 _originalScale;
        bool _cached;

        public static int Selected => Mathf.Clamp(PlayerPrefs.GetInt(ChoiceKey, 0), 0, 1);

        public static void Select(int index)
        {
            PlayerPrefs.SetInt(ChoiceKey, index == 1 ? 1 : 0);
            PlayerPrefs.Save();
            var all = Object.FindObjectsByType<PlayerWalkAnimator>(FindObjectsSortMode.None);
            foreach (var anim in all)
                anim.ApplySelected();
        }

        void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
            _controller = GetComponentInParent<PlayerController2D>();
            if (idleFrame == null && walkFrames != null && walkFrames.Length > 0)
                idleFrame = walkFrames[0];
            CacheOriginal();
            ApplySelected();
        }

        void CacheOriginal()
        {
            if (_cached)
                return;
            _cached = true;
            _originalFrames = walkFrames;
            _originalIdle = idleFrame;
            _originalScale = transform.localScale;
        }

        public void ApplySelected()
        {
            CacheOriginal();
            if (Selected == 1 && TrySecond(out var frames))
            {
                walkFrames = frames;
                idleFrame = frames[0];
                transform.localScale = new Vector3(SecondScale, SecondScale, SecondScale);
            }
            else
            {
                walkFrames = _originalFrames;
                idleFrame = _originalIdle;
                transform.localScale = _originalScale;
            }

            if (_renderer == null)
                _renderer = GetComponent<SpriteRenderer>();
            if (_renderer != null && idleFrame != null)
                _renderer.sprite = idleFrame;
            _timer = 0f;
            _index = 0;
        }

        static bool TrySecond(out Sprite[] frames)
        {
            frames = new Sprite[10];
            for (int i = 0; i < frames.Length; i++)
            {
                frames[i] = Resources.Load<Sprite>($"Player/Personaje2/frame_{i:00}");
                if (frames[i] == null)
                    return false;
            }

            return true;
        }

        void Update()
        {
            if (_renderer == null || walkFrames == null || walkFrames.Length == 0)
                return;

            bool moving = _controller != null && _controller.IsGrounded && _controller.MoveSpeedAbs > moveThreshold;
            if (!moving)
            {
                _renderer.sprite = idleFrame != null ? idleFrame : walkFrames[0];
                _timer = 0f;
                _index = 0;
                return;
            }

            float speedMul = _controller != null && _controller.IsRunning ? 1.35f : 1f;
            _timer += Time.deltaTime * framesPerSecond * speedMul;
            if (_timer >= 1f)
            {
                _timer -= 1f;
                _index = (_index + 1) % walkFrames.Length;
                _renderer.sprite = walkFrames[_index];
            }
        }

        public void SetFrames(Sprite[] frames)
        {
            walkFrames = frames;
            if (frames != null && frames.Length > 0)
                idleFrame = frames[0];
        }
    }
}
