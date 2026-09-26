using UnityEngine;

namespace Crowfox.Util
{
    [RequireComponent(typeof(CanvasGroup))]
    public class FadeHelper : MonoBehaviour
    {
        CanvasGroup canvasGroup;

        private void Awake()
        {
            if (!TryGetComponent(out canvasGroup))
            {
                Debug.LogError($"FadeHelper: No CanvasGroup component found on {gameObject.name}. Disabling script.");
                enabled = false;
            }
        }

        /// <summary>
        /// Fade in a canvas group.
        /// Called from a button to fade in the canvas group this script is attached to.
        /// </summary>
        public void FadeInCanvasGroup(float duration)
        {
            Fade.FadeCanvasGroup(canvasGroup, 1f, duration);
        }

        /// <summary>
        /// Fade out a canvas group.
        /// Called from a button to fade out the canvas group this script is attached to.
        /// </summary>
        /// <param name="duration"></param>
        public void FadeOutCanvasGroup(float duration)
        {
            Fade.FadeCanvasGroup(canvasGroup, 0f, duration);
        }
    }
}
