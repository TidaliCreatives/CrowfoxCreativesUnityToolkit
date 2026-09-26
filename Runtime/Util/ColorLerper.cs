using UnityEngine;
using UnityEngine.UI;

namespace Crowfox.Util
{
    public sealed class ColorLerper : MonoBehaviour
    {
        public enum PlayMode
        {
            Single,
            Loop,
            PingPong
        }

        [Header("Target")]
        SpriteRenderer _spriteRenderer;
        Image _image;

        [Header("Playback")]
        [SerializeField] bool _playOnStart = true;
        [SerializeField] PlayMode _playMode = PlayMode.Single;
        [SerializeField] bool _rainbow = false;

        [Header("Timing")]
        [SerializeField] float _duration = 1f;
        [SerializeField] bool _doUseUnscaledTime = false;

        [Header("Curve")]
        [SerializeField]
        AnimationCurve _curve =
            AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Header("Colors")]
        [SerializeField] Color _startColor = Color.white;
        [SerializeField] Color _endColor = Color.white;

        float _elapsedTime;

        bool _isRunning;
        bool _destroyOnComplete;

        void Awake()
        {
            if (!TryGetComponent(out _spriteRenderer))
                TryGetComponent(out _image);
        }

        void Start()
        {
            if (_playOnStart)
                Play();
        }

        void Update()
        {
            if (!_isRunning)
                return;

            float deltaTime = _doUseUnscaledTime
                ? Time.unscaledDeltaTime
                : Time.deltaTime;

            _elapsedTime += deltaTime;

            float linearT = GetPlaybackT();

            float t = _curve != null
                ? _curve.Evaluate(linearT)
                : linearT;

            ApplyColor(GetColor(t));

            if (_playMode == PlayMode.Single && _elapsedTime >= _duration)
                Complete();
        }

        public void Initialize(
            Color start,
            Color end,
            float lerpDuration,
            bool destroyAfter = false,
            bool doUseUnscaledTime = false,
            AnimationCurve animationCurve = null)
        {
            _startColor = start;
            _endColor = end;

            _duration = Mathf.Max(0.0001f, lerpDuration);
            _destroyOnComplete = destroyAfter;
            _doUseUnscaledTime = doUseUnscaledTime;

            if (animationCurve != null)
                _curve = animationCurve;

            Play();
        }

        public void Play()
        {
            _duration = Mathf.Max(0.0001f, _duration);

            _elapsedTime = 0f;
            _isRunning = true;

            ApplyColor(GetColor(0f));
        }

        public void Stop()
        {
            _isRunning = false;
        }

        float GetPlaybackT()
        {
            float time = _elapsedTime / _duration;

            switch (_playMode)
            {
                case PlayMode.Single:
                    return Mathf.Clamp01(time);

                case PlayMode.Loop:
                    return Mathf.Repeat(time, 1f);

                case PlayMode.PingPong:
                    return Mathf.PingPong(time, 1f);

                default:
                    return 0f;
            }
        }

        Color GetColor(float t)
        {
            if (!_rainbow)
                return Color.LerpUnclamped(_startColor, _endColor, t);

            Color rainbowColor = Color.HSVToRGB(
                Mathf.Repeat(t, 1f),
                1f,
                1f
            );

            // Still allow alpha fading through Start/End Color.
            rainbowColor.a = Mathf.LerpUnclamped(
                _startColor.a,
                _endColor.a,
                t
            );

            return rainbowColor;
        }

        void Complete()
        {
            _isRunning = false;

            // Make sure Single always ends exactly where expected.
            ApplyColor(GetColor(1f));

            if (_destroyOnComplete)
                Destroy(gameObject);
        }

        void ApplyColor(Color color)
        {
            if (_spriteRenderer != null)
            {
                _spriteRenderer.color = color;
                return;
            }

            if (_image != null)
            {
                _image.color = color;
                return;
            }

            Debug.LogWarning(
                $"{nameof(ColorLerper)} on {gameObject.name} has no SpriteRenderer or Image.",
                this
            );

            _isRunning = false;
        }
    }
}