using Crowfox.Audio;
using System.Collections;
using UnityEngine;

namespace Crowfox.Util
{
    public sealed class UIWavePunch : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float punchAmount = 0.08f; // 0.08 = +8%
        [SerializeField] private bool useUnscaledTime = true;
        [SerializeField] AudioClip sfx_Punch;

        private Coroutine _running;
        private Vector3 _baseScale;

        private void Awake()
        {
            _baseScale = transform.localScale;
        }

        private void OnEnable()
        {
            // Re-capture in case layout/animations changed it before enable.
            _baseScale = transform.localScale;
        }

        public void DoWave(float duration, string name, int score, float delay = 0f, int rank = -1, float pitch = 1f)
        {
            if (_running != null)
                StopCoroutine(_running);

            _running = StartCoroutine(WaveCoroutine(duration, name, score, delay, rank, pitch));
        }

        private IEnumerator WaveCoroutine(float duration, string name, int score, float delay = 0f, int rank = -1, float pitch = 1f)
        {
            if (delay > 0f)
            {
                if (useUnscaledTime) yield return new WaitForSecondsRealtime(delay);
                else yield return new WaitForSeconds(delay);
            }

            var tr = transform.parent ? transform.parent : transform;

            if (sfx_Punch)
                EasyAudio.SmartPlayClip(sfx_Punch, tr, VolumeType.SFX, 0.6f, pitch, spatialBlend: 0f, reverbZoneMix: 0f, maxAudioSources: 1);

            // Always start from a known base to prevent stacking/drift.
            transform.localScale = _baseScale;

            float t = 0f;
            float inv = 1f / Mathf.Max(0.0001f, duration);

            while (t < duration)
            {
                t += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                float x = Mathf.Clamp01(t * inv);

                // Smooth punch curve: 0 -> 1 -> 0 (sine).
                float punch = Mathf.Sin(x * Mathf.PI);

                float scaleMul = 1f + punchAmount * punch;
                transform.localScale = _baseScale * scaleMul;

                yield return null;
            }

            transform.localScale = _baseScale;
            _running = null;
        }
    }
}