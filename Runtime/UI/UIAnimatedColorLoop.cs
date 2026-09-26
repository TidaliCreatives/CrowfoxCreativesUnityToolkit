using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Crowfox.Util
{
    public sealed class UIAnimatedColorLoop : MonoBehaviour
    {
        private enum LoopMode
        {
            Off,
            AlphaFade,
            Rainbow,
            RainbowWithAlphaFade
        }

        [Header("Targets")]
        [Tooltip("Also animate Image/TMP_Text on direct children (one level).")]
        [SerializeField] private bool includeDirectChildren = false;

        [Tooltip("Also animate Image/TMP_Text on all descendants. If true, this overrides 'includeDirectChildren'.")]
        [SerializeField] private bool includeAllChildren = false;

        [Header("Mode")]
        [SerializeField] private LoopMode mode = LoopMode.RainbowWithAlphaFade;

        [Header("Common")]
        [SerializeField, Min(0.0001f)] private float loopDuration = 5f;

        [Tooltip("Use unscaled time (ignores Time.timeScale).")]
        [SerializeField] private bool useUnscaledTime = false;

        [Header("Alpha Fade")]
        [SerializeField] private bool lerpAlpha = true;

        [Tooltip("If true: starts at alpha 0. If false: starts at alpha 1.")]
        [SerializeField] private bool startTransparent = true;

        [Tooltip("Phase offset inside the 0..1 loop.")]
        [SerializeField, Range(0f, 1f)] private float alphaStartOffset01 = 0f;

        [Header("Rainbow")]
        [SerializeField] private bool startWithRandomHue = true;

        [Tooltip("Phase offset inside the 0..1 loop.")]
        [SerializeField, Range(0f, 1f)] private float hueStartOffset01 = 0f;

        [SerializeField, Range(0f, 1f)] private float saturation = 1f;
        [SerializeField, Range(0f, 1f)] private float value = 1f;

        // Collected targets (current + optional children).
        private readonly List<Image> _images = new(16);
        private readonly List<TMP_Text> _texts = new(16);

        private float _alphaPhaseOffset01;
        private float _huePhaseOffset01;

        private void Awake()
        {
            _alphaPhaseOffset01 = Mathf.Clamp01(alphaStartOffset01);
            _huePhaseOffset01 = Mathf.Clamp01(hueStartOffset01);

            CollectTargetsOnce();
        }

        private void Start()
        {
            if (startWithRandomHue)
                _huePhaseOffset01 = Random.value;

            // If you start opaque, shift alpha phase by half a cycle (0 -> 1).
            if (!startTransparent)
                _alphaPhaseOffset01 = Mathf.Repeat(_alphaPhaseOffset01 + 0.5f, 1f);
        }

        private void Update()
        {
            if (mode == LoopMode.Off)
                return;

            if (loopDuration <= 0f)
                return;

            float t = useUnscaledTime ? Time.unscaledTime : Time.time;
            float phase01 = Mathf.Repeat(t / loopDuration, 1f);

            bool doRainbow =
                mode == LoopMode.Rainbow ||
                mode == LoopMode.RainbowWithAlphaFade;

            bool doAlpha =
                mode == LoopMode.AlphaFade ||
                mode == LoopMode.RainbowWithAlphaFade;

            Color baseColor;

            if (doRainbow)
            {
                float hue = Mathf.Repeat(phase01 + _huePhaseOffset01, 1f);
                baseColor = Color.HSVToRGB(hue, Mathf.Clamp01(saturation), Mathf.Clamp01(value));
            }
            else
            {
                // Alpha-only mode: preserve current RGB from the first available target.
                baseColor = GetAnyCurrentColorOrWhite();
            }

            float alpha = 1f;
            if (doAlpha)
            {
                float aPhase01 = Mathf.Repeat(phase01 + _alphaPhaseOffset01, 1f);

                if (lerpAlpha)
                {
                    // Smooth 0..1..0
                    alpha = (Mathf.Sin(aPhase01 * Mathf.PI * 2f) + 1f) * 0.5f;
                }
                else
                {
                    // Hard 0/1 toggling style (still loops)
                    alpha = Mathf.Round(Mathf.PingPong(aPhase01 * 2f, 1f));
                }
            }

            baseColor.a = alpha;
            ApplyColor(baseColor);
        }

        private void CollectTargetsOnce()
        {
            _images.Clear();
            _texts.Clear();

            // Always include current object.
            TryGetComponent(out Image img);
            if (img != null) _images.Add(img);

            TryGetComponent(out TMP_Text txt);
            if (txt != null) _texts.Add(txt);

            if (includeAllChildren)
            {
                // Uses your provided non-alloc helper to collect all descendants (and current by default).
                // We already added current above, so we call with doIncludeCurrent = false to avoid duplicates.
                transform.TryGetComponentsInChildrenNonAlloc(_images, doIncludeCurrent: false);
                transform.TryGetComponentsInChildrenNonAlloc(_texts, doIncludeCurrent: false);

                return;
            }

            if (includeDirectChildren)
            {
                // Only one level: direct children.
                int childCount = transform.childCount;
                for (int i = 0; i < childCount; i++)
                {
                    Transform child = transform.GetChild(i);

                    if (child.TryGetComponent(out Image childImg))
                        _images.Add(childImg);

                    if (child.TryGetComponent(out TMP_Text childTxt))
                        _texts.Add(childTxt);
                }
            }
        }

        private Color GetAnyCurrentColorOrWhite()
        {
            if (_images.Count > 0 && _images[0] != null)
                return _images[0].color;

            if (_texts.Count > 0 && _texts[0] != null)
                return _texts[0].color;

            return Color.white;
        }

        private void ApplyColor(Color c)
        {
            for (int i = 0; i < _images.Count; i++)
            {
                if (_images[i] != null)
                    _images[i].color = c;
            }

            for (int i = 0; i < _texts.Count; i++)
            {
                if (_texts[i] != null)
                    _texts[i].color = c;
            }
        }

        // Optional simple public controls
        public void SetModeAlphaFade() => mode = LoopMode.AlphaFade;
        public void SetModeRainbow() => mode = LoopMode.Rainbow;
        public void SetModeRainbowWithAlpha() => mode = LoopMode.RainbowWithAlphaFade;
        public void SetModeOff() => mode = LoopMode.Off;
    }
}