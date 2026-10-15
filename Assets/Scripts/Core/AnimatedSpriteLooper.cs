using UnityEngine;

namespace Piso7.Core
{
    /// <summary>
    /// Reproduce un fondo animado desde frames individuales (p. ej. intro UNIFRANZ 5 frames).
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class AnimatedSpriteLooper : MonoBehaviour
    {
        [SerializeField] Sprite[] frames;
        [SerializeField] float framesPerSecond = 6f;
        [SerializeField] bool playOnAwake = true;

        SpriteRenderer _renderer;
        float _timer;
        int _index;
        bool _playing;

        void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
            _playing = playOnAwake;
            if (frames != null && frames.Length > 0)
                _renderer.sprite = frames[0];
        }

        void Update()
        {
            if (!_playing || frames == null || frames.Length <= 1)
                return;

            _timer += Time.deltaTime * framesPerSecond;
            if (_timer < 1f)
                return;

            _timer -= 1f;
            _index = (_index + 1) % frames.Length;
            _renderer.sprite = frames[_index];
        }

        public void SetFrames(Sprite[] newFrames)
        {
            frames = newFrames;
            _index = 0;
            if (_renderer != null && frames != null && frames.Length > 0)
                _renderer.sprite = frames[0];
        }
    }
}
