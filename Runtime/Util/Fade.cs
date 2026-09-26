using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Crowfox.Util
{
    /// <summary>
    /// Static helper class for fading UI elements and audio sources.
    /// Provides methods to fade CanvasGroups, TextMeshProUGUI components, and AudioSources over a specified duration.
    /// Also includes functionality to create a full-screen fade overlay on demand.
    /// </summary>
    public static class Fade
    {
        /// <summary>
        /// This helper component is added to any GameObject that is currently being faded,
        /// to track the currently running fade coroutine and allow it to be stopped if a new fade is initiated on the same object.
        /// </summary>
        private sealed class Tracker : MonoBehaviour
        {
            public Coroutine Coroutine_CurrentlyRunningFade;
        }

        // =========================
        // CanvasGroup
        // =========================

        /// <summary>
        /// Fade any canvas group to a target alpha over a duration.
        /// If doDestroyOnEnd is true, the GameObject will be destroyed when the fade finishes and the alpha is near 0.
        /// </summary>
        public static Coroutine FadeCanvasGroup(this CanvasGroup canvasGroup, float targetAlpha, float duration, bool doUseUnscaledTime = true, bool doDestroyOnEnd = false, float interactableThreshold = 0.2f)
        {
            // Validate input
            if (canvasGroup == null)
            {
                Debug.LogError("CanvasGroup is null. Cannot perform fade operation.");
                return null;
            }

            // Validate Tracker component
            var tracker = GetOrAddTracker(canvasGroup.gameObject);

            // Stop running fade if any
            StopRunningFadeIfAny(tracker);

            // Start new fade coroutine
            return tracker.Coroutine_CurrentlyRunningFade = tracker.StartCoroutine(FadeCanvasGroupRoutine(canvasGroup, targetAlpha, duration, tracker, doUseUnscaledTime, doDestroyOnEnd, interactableThreshold));
        }

        /// <summary>
        /// Fade any canvas group to a target alpha over a duration.
        /// If doDestroyOnEnd is true, the GameObject will be destroyed when the fade finishes and the alpha is near 0.
        /// Can be yield returned from a coroutine to wait for the fade to complete.
        /// </summary>
        public static IEnumerator FadeCanvasGroupCoroutine(this CanvasGroup canvasGroup, float targetAlpha, float duration, bool doUseUnscaledTime = true, bool doDestroyOnEnd = false, float interactableThreshold = 0.2f)
        {
            // Validate input
            if (canvasGroup == null)
            {
                Debug.LogError("CanvasGroup is null. Cannot perform fade operation.");
                yield break;
            }

            // Validate Tracker component
            var tracker = GetOrAddTracker(canvasGroup.gameObject);

            // Stop running fade if any
            StopRunningFadeIfAny(tracker);

            // Start new fade coroutine and wait for it
            tracker.Coroutine_CurrentlyRunningFade = tracker.StartCoroutine(FadeCanvasGroupRoutine(canvasGroup, targetAlpha, duration, tracker, doUseUnscaledTime, doDestroyOnEnd, interactableThreshold));
            yield return tracker.Coroutine_CurrentlyRunningFade;
        }

        // Internal routine (contains Tracker param, so keep it private!)
        private static IEnumerator FadeCanvasGroupRoutine(CanvasGroup canvasGroup, float targetAlpha, float duration, Tracker tracker, bool doUseUnscaledTime = true, bool doDestroyOnEnd = false, float interactableThreshold = 0.2f)
        {
            // Clamp target alpha for safety
            targetAlpha = Mathf.Clamp01(targetAlpha);

            // If duration is zero or negative, snap to target immediately
            if (duration <= 0f)
            {
                canvasGroup.alpha = targetAlpha;
                canvasGroup.interactable = targetAlpha > 0f; // Enable interaction if visible at all
                canvasGroup.blocksRaycasts = targetAlpha > 0f; // Enable interaction if visible at all
                tracker.Coroutine_CurrentlyRunningFade = null; // Fade finished
                yield break;
            }

            // While the current alpha is not approximately equal to the target, keep fading
            while (!Mathf.Approximately(canvasGroup.alpha, targetAlpha))
            {
                var deltaTime = doUseUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, targetAlpha, deltaTime / duration);

                // Interaction should be based on CURRENT alpha, not the target alpha
                canvasGroup.interactable = canvasGroup.alpha > interactableThreshold;
                canvasGroup.blocksRaycasts = canvasGroup.alpha > interactableThreshold;

                yield return null;
            }

            // Snap to target to avoid precision issues (normally redundant due to moveTowards)
            canvasGroup.alpha = targetAlpha;
            canvasGroup.interactable = targetAlpha > 0f; // Enable interaction if visible at all
            canvasGroup.blocksRaycasts = targetAlpha > 0f; // Enable interaction if visible at all

            if (doDestroyOnEnd && Mathf.Approximately(canvasGroup.alpha, 0f))
            {
                MonoBehaviour.Destroy(canvasGroup.gameObject);
            }

            tracker.Coroutine_CurrentlyRunningFade = null; // Fade finished
        }

        // =========================
        // TMP_Text
        // =========================

        /// <summary>
        /// Fade any TextMeshProUGUI to a target alpha over a duration.
        /// If doDestroyOnEnd is true, the GameObject will be destroyed when the fade finishes and the alpha is near 0.
        /// </summary>
        public static Coroutine FadeTMPText(this TMP_Text tmp, float targetAlpha, float duration, bool doUseUnscaledTime = true, bool doDestroyOnEnd = false)
        {
            // Validate input
            if (tmp == null)
            {
                Debug.LogError("TMP_Text is null. Cannot perform fade operation.");
                return null;
            }

            // Validate Tracker component
            var tracker = GetOrAddTracker(tmp.gameObject);

            // Stop running fade if any
            StopRunningFadeIfAny(tracker);

            // Start new fade coroutine
            return tracker.Coroutine_CurrentlyRunningFade = tracker.StartCoroutine(FadeTMPTextRoutine(tmp, tracker, targetAlpha, duration, doUseUnscaledTime, doDestroyOnEnd));
        }

        /// <summary>
        /// Fade any TextMeshProUGUI to a target alpha over a duration.
        /// If doDestroyOnEnd is true, the GameObject will be destroyed when the fade finishes and the alpha is near 0.
        /// Can be yield returned from a coroutine to wait for the fade to complete.
        /// </summary>
        public static IEnumerator FadeTMPTextCoroutine(TMP_Text tmp, float targetAlpha, float duration, bool doUseUnscaledTime = true, bool doDestroyOnEnd = false)
        {
            // Validate input
            if (tmp == null)
            {
                Debug.LogError("TMP_Text is null. Cannot perform fade operation.");
                yield break;
            }

            // Validate Tracker component
            var tracker = GetOrAddTracker(tmp.gameObject);

            // Stop running fade if any
            StopRunningFadeIfAny(tracker);

            // Start new fade coroutine and wait for it
            tracker.Coroutine_CurrentlyRunningFade = tracker.StartCoroutine(FadeTMPTextRoutine(tmp, tracker, targetAlpha, duration, doUseUnscaledTime, doDestroyOnEnd));
            yield return tracker.Coroutine_CurrentlyRunningFade;
        }

        // Internal routine (contains Tracker param, so keep it private!)
        private static IEnumerator FadeTMPTextRoutine(TMP_Text tmp, Tracker tracker, float targetAlpha, float duration, bool doUseUnscaledTime = true, bool doDestroyOnEnd = false)
        {
            // Clamp target alpha for safety
            targetAlpha = Mathf.Clamp01(targetAlpha);

            // If duration is zero or negative, snap to target immediately
            if (duration <= 0f)
            {
                tmp.alpha = targetAlpha;

                if (doDestroyOnEnd && Mathf.Approximately(targetAlpha, 0f))
                {
                    MonoBehaviour.Destroy(tmp.gameObject);
                }

                tracker.Coroutine_CurrentlyRunningFade = null; // Fade finished
                yield break;
            }

            // While the current alpha is not approximately equal to the target, keep fading
            while (!Mathf.Approximately(tmp.alpha, targetAlpha))
            {
                var deltaTime = doUseUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                tmp.alpha = Mathf.MoveTowards(tmp.alpha, targetAlpha, deltaTime / duration);
                yield return null;
            }

            // Snap to target to avoid precision issues (normally redundant due to moveTowards)
            tmp.alpha = targetAlpha;

            if (doDestroyOnEnd && Mathf.Approximately(tmp.alpha, 0f))
            {
                MonoBehaviour.Destroy(tmp.gameObject);
            }

            tracker.Coroutine_CurrentlyRunningFade = null; // Fade finished
        }



        /// <summary>
        /// Fade any TextMeshProUGUI to a target alpha and back over a duration.
        /// If doDestroyOnEnd is true, the GameObject will be destroyed when the fade finishes and the alpha is near 0.
        /// </summary>
        public static Coroutine FadeTMPTextToAndBack(this TMP_Text tmp, float targetAlpha, float fadeToDuration, float waitDuration, float fadeBackDuration, bool doUseUnscaledTime = true)
        {
            // Validate input
            if (tmp == null)
            {
                Debug.LogError("TMP_Text is null. Cannot perform fade operation.");
                return null;
            }

            // Validate Tracker component
            var tracker = GetOrAddTracker(tmp.gameObject);

            // Stop running fade if any
            StopRunningFadeIfAny(tracker);

            // Start new fade coroutine
            return tracker.Coroutine_CurrentlyRunningFade = tracker.StartCoroutine(FadeTMPTextToAndBackRoutine(tmp, tracker, targetAlpha, fadeToDuration, waitDuration, fadeBackDuration, doUseUnscaledTime));
        }

        // Internal routine (contains Tracker param, so keep it private!)
        private static IEnumerator FadeTMPTextToAndBackRoutine(TMP_Text tmp, Tracker tracker, float targetAlpha, float fadeToDuration, float waitDuration, float fadeBackDuration, bool doUseUnscaledTime = true)
        {
            var initialAlpha = tmp.alpha;

            yield return FadeTMPTextRoutine(tmp, tracker, targetAlpha, fadeToDuration, doUseUnscaledTime);

            if (waitDuration > 0f)
            {
                if (doUseUnscaledTime)
                    yield return new WaitForSecondsRealtime(waitDuration);
                else
                    yield return new WaitForSeconds(waitDuration);
            }

            yield return FadeTMPTextRoutine(tmp, tracker, initialAlpha, fadeBackDuration, doUseUnscaledTime);

            tracker.Coroutine_CurrentlyRunningFade = null; // Fade finished
        }

        // Count TMP number to another number over a duration
        public static Coroutine CountTMPTextTo(this TMP_Text tmp, int targetNumber, float duration, bool doUseUnscaledTime = true)
        {
            // Validate input
            if (tmp == null)
            {
                Debug.LogError("TMP_Text is null. Cannot perform count operation.");
                return null;
            }
            // Validate Tracker component
            var tracker = GetOrAddTracker(tmp.gameObject);
            // Stop running fade if any
            StopRunningFadeIfAny(tracker);
            // Start new count coroutine
            return tracker.Coroutine_CurrentlyRunningFade = tracker.StartCoroutine(CountTMPTextToRoutine(tmp, tracker, targetNumber, duration, doUseUnscaledTime));
        }

        // Internal routine (contains Tracker param, so keep it private!)
        private static IEnumerator CountTMPTextToRoutine(TMP_Text tmp, Tracker tracker, int targetNumber, float duration, bool doUseUnscaledTime = true)
        {
            int initialNumber = 0;
            if (int.TryParse(tmp.text, out int parsedNumber))
                initialNumber = parsedNumber;

            if (initialNumber == targetNumber)
            {
                tracker.Coroutine_CurrentlyRunningFade = null; // Count finished
                yield break;
            }
            float currentNumber = initialNumber;
            var directionIsPositive = targetNumber > initialNumber;
            while (initialNumber != targetNumber)
            {
                var deltaTime = doUseUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                currentNumber = Mathf.MoveTowards(currentNumber, targetNumber, deltaTime * Mathf.Abs(targetNumber - initialNumber) / duration);
                tmp.SetText(((int)currentNumber).ToString());

                if (directionIsPositive && currentNumber >= targetNumber
                    || directionIsPositive && currentNumber < initialNumber
                    || !directionIsPositive && currentNumber <= targetNumber
                    || !directionIsPositive && currentNumber > initialNumber)
                {
                    break;
                }

                yield return null;
            }
            tmp.SetText(targetNumber.ToString()); // Ensure the final number is set
            tracker.Coroutine_CurrentlyRunningFade = null; // Count finished
        }


        // =========================
        // Overlay Creation
        // =========================

        /// <summary>
        /// Creates a Canvas overlay on the specified GameObject if one does not exist and animates its alpha value to
        /// the specified target over the given duration.
        /// </summary>
        /// <remarks>If the GameObject does not already have a CanvasGroup, one is created automatically.
        /// This method is typically used to create temporary UI overlays that fade in or out. The method is an
        /// extension method and should be called on a GameObject instance.</remarks>
        /// <param name="callerObject">The GameObject to which the Canvas overlay is attached and faded.</param>
        /// <param name="targetAlpha">The target alpha value for the CanvasGroup. Valid values are between 0.0 (fully transparent) and 1.0 (fully opaque).</param>
        /// <param name="duration">The duration, in seconds, over which the fade animation occurs. Must be non-negative.</param>
        /// <param name="destroy">true to destroy the Canvas overlay after the fade completes; otherwise, false.</param>
        public static void CreateAndFadeCanvasGroup(this GameObject callerObject, float targetAlpha, float duration, bool doUseUnscaledTime = true, bool doDestroyOnEnd = false, float interactableThreshold = 0.2f)
        {
            // Validate input
            if (callerObject == null)
            {
                Debug.LogError("Caller object is null. Cannot create overlay.");
                return;
            }

            // Create a new CanvasGroup if it doesn't exist
            if (!callerObject.TryGetComponent(out CanvasGroup canvasGroup))
            {
                canvasGroup = CreateOverlay(callerObject);
            }

            // Start the fade coroutine
            FadeCanvasGroup(canvasGroup, targetAlpha, duration, doUseUnscaledTime, doDestroyOnEnd, interactableThreshold);
        }

        private static CanvasGroup CreateOverlay(GameObject callerObject)
        {
            // Try get UI layer
            var layer = LayerMask.NameToLayer("UI");

            // === Canvas ===
            GameObject go_Canvas = new("OverlayCanvas")
            {
                layer = Mathf.Max(0, layer), // If UI layer doesn't exist, default to 0
            };

            go_Canvas.transform.SetParent(callerObject.transform, false);

            // Add the canvas to the callerObject
            Canvas canvas = go_Canvas.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            // Ensure the canvas scaler is set up correctly
            if (!go_Canvas.TryGetComponent(out CanvasScaler scaler))
            {
                scaler = go_Canvas.AddComponent<CanvasScaler>();
            }

            // Set the CanvasScaler properties
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            scaler.referencePixelsPerUnit = 100f; // Set the reference pixels per unit

            // Ensure the canvas has a GraphicRaycaster for UI interaction
            if (!go_Canvas.TryGetComponent(out GraphicRaycaster _))
            {
                go_Canvas.AddComponent<GraphicRaycaster>();
            }

            // Add a CanvasGroup for fading
            var canvasGroup = go_Canvas.AddComponent<CanvasGroup>();

            // === Background Image ===
            GameObject imageGO = new("Background");
            imageGO.transform.SetParent(go_Canvas.transform, false);

            Image img = imageGO.AddComponent<Image>();
            img.color = Color.black;
            img.raycastTarget = true; // For blocking raycasts behind the overlay

            if (!imageGO.TryGetComponent(out RectTransform rect))
            {
                rect = imageGO.AddComponent<RectTransform>();
            }

            // Set the RectTransform to cover the entire canvas
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            return canvasGroup;
        }

        // =========================
        // AudioSource
        // =========================

        /// <summary>
        /// Fade any AudioSource to a target volume over a duration.
        /// If doStopAtEnd is true, the AudioSource will be stopped when the fade finishes and the volume is at 0.
        /// </summary>
        public static Coroutine FadeAudioSource(this AudioSource audioSource, float targetVolume, float duration, bool doUseUnscaledTime = true, bool doStopAtEnd = false, bool doDestroyAtEnd = false)
        {
            // Validate input
            if (audioSource == null)
            {
                Debug.LogError("AudioSource is null. Cannot perform fade operation.");
                return null;
            }

            // Validate Tracker component
            var tracker = GetOrAddTracker(audioSource.gameObject);

            // Stop running fade if any
            StopRunningFadeIfAny(tracker);

            // Start new fade coroutine
            return tracker.Coroutine_CurrentlyRunningFade = tracker.StartCoroutine(FadeAudioSourceRoutine(audioSource, targetVolume, duration, tracker, doUseUnscaledTime, doStopAtEnd, doDestroyAtEnd));
        }

        /// <summary>
        /// Fade any AudioSource to a target volume over a duration.
        /// If doStopAtEnd is true, the AudioSource will be stopped when the fade finishes and the volume is at 0.
        /// If destroyAtEnd is true, the AudioSource's GameObject will be destroyed when the fade finishes and the volume is at 0.
        /// Can be yield returned from a coroutine to wait for the fade to complete.
        /// </summary>
        public static IEnumerator FadeAudioSourceCoroutine(this AudioSource audioSource, float targetVolume, float duration, bool doUseUnscaledTime = true, bool doStopAtEnd = false, bool doDestroyAtEnd = false)
        {
            // Validate input
            if (audioSource == null)
            {
                Debug.LogError("AudioSource is null. Cannot perform fade operation.");
                yield break;
            }

            // Validate Tracker component
            var tracker = GetOrAddTracker(audioSource.gameObject);

            // Stop running fade if any
            StopRunningFadeIfAny(tracker);

            // Start new fade coroutine and wait for it
            tracker.Coroutine_CurrentlyRunningFade = tracker.StartCoroutine(FadeAudioSourceRoutine(audioSource, targetVolume, duration, tracker, doUseUnscaledTime, doStopAtEnd, doDestroyAtEnd));
            yield return tracker.Coroutine_CurrentlyRunningFade;
        }

        // Internal routine (contains Tracker param, so keep it private!)
        private static IEnumerator FadeAudioSourceRoutine(AudioSource audioSource, float targetVolume, float duration, Tracker tracker, bool doUseUnscaledTime = true, bool doStopAtEnd = false, bool doDestroyAtEnd = false)
        {
            // Clamp target volume for safety
            targetVolume = Mathf.Clamp01(targetVolume);

            // If duration is zero or negative, snap to target immediately
            if (duration <= 0f)
            {
                audioSource.volume = targetVolume;

                if (doDestroyAtEnd && Mathf.Approximately(targetVolume, 0f))
                {
                    MonoBehaviour.Destroy(audioSource.gameObject);
                }
                else if (doStopAtEnd && Mathf.Approximately(targetVolume, 0f))
                {
                    audioSource.Stop();
                }

                tracker.Coroutine_CurrentlyRunningFade = null; // Fade finished
                yield break;
            }

            // If fading in and not already playing, start the audio at 0 volume
            if (targetVolume > 0f && !audioSource.isPlaying)
            {
                audioSource.volume = 0f; // Start from 0 when fading in
                audioSource.Play();
            }

            // While the current volume is not approximately equal to the target, keep fading
            while (!Mathf.Approximately(audioSource.volume, targetVolume))
            {
                var deltaTime = doUseUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                audioSource.volume = Mathf.MoveTowards(audioSource.volume, targetVolume, deltaTime / duration);
                yield return null;
            }

            // Snap to target to avoid precision issues (normally redundant due to moveTowards)
            audioSource.volume = targetVolume;

            if (doDestroyAtEnd && Mathf.Approximately(targetVolume, 0f))
            {
                MonoBehaviour.Destroy(audioSource.gameObject);
            }
            else if (doStopAtEnd && Mathf.Approximately(targetVolume, 0f))
            {
                audioSource.Stop();
            }

            tracker.Coroutine_CurrentlyRunningFade = null; // Fade finished
        }

        // =========================
        // Utilities
        // =========================

        private static Tracker GetOrAddTracker(GameObject go)
        {
            // Validate Tracker component
            if (!go.TryGetComponent<Tracker>(out var tracker))
            {
                tracker = go.AddComponent<Tracker>();
            }

            return tracker;
        }

        private static void StopRunningFadeIfAny(Tracker tracker)
        {
            // Stop running fade if any
            if (tracker.Coroutine_CurrentlyRunningFade != null)
            {
                tracker.StopCoroutine(tracker.Coroutine_CurrentlyRunningFade);
            }

            tracker.Coroutine_CurrentlyRunningFade = null;
        }
    }
}
