using UnityEngine;

namespace Crowfox.Util
{
    [RequireComponent(typeof(Renderer))]
    public sealed class MaterialColorLerper : MonoBehaviour
    {
        private static readonly int BaseColorID = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorID = Shader.PropertyToID("_Color");

        [Header("Timing")]
        [SerializeField] private bool _doUseUnscaledTime = false;

        [Header("Curve")]
        [SerializeField] private AnimationCurve curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        private Renderer targetRenderer;
        private MaterialPropertyBlock propertyBlock;

        private Color startColor;
        private Color endColor;
        private float duration;
        private float elapsedTime;
        private bool isRunning;
        private bool destroyOnComplete;

        // Cache which property exists (avoid guessing every frame)
        private int activeColorPropertyID = -1;

        private void Awake()
        {
            targetRenderer = GetComponent<Renderer>();
            propertyBlock = new MaterialPropertyBlock();
            CacheColorProperty();
        }

        public void Initialize(Color start, Color end, float lerpDuration, bool destroyAfter = false, bool doUseUnscaledTime = false, AnimationCurve animationCurve = null)
        {
            startColor = start;
            endColor = end;
            duration = Mathf.Max(0.0001f, lerpDuration);
            destroyOnComplete = destroyAfter;

            _doUseUnscaledTime = doUseUnscaledTime;
            if (animationCurve != null) curve = animationCurve;

            elapsedTime = 0f;
            isRunning = true;

            // In case Initialize is called before Awake (rare, but can happen)
            if (targetRenderer == null) targetRenderer = GetComponent<Renderer>();
            if (propertyBlock == null) propertyBlock = new MaterialPropertyBlock();
            if (activeColorPropertyID == -1) CacheColorProperty();

            ApplyColor(startColor);
        }

        private void Update()
        {
            if (!isRunning)
                return;

            float dt = _doUseUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

            elapsedTime += dt;
            float linearT = Mathf.Clamp01(elapsedTime / duration);

            // Curve maps 0..1 -> (usually) 0..1 (but can overshoot if user wants)
            float curvedT = curve != null ? curve.Evaluate(linearT) : linearT;
            curvedT = Mathf.Clamp01(curvedT);

            ApplyColor(Color.Lerp(startColor, endColor, curvedT));

            if (linearT >= 1f)
            {
                isRunning = false;

                if (destroyOnComplete)
                    Destroy(gameObject);
            }
        }

        private void CacheColorProperty()
        {
            // Validate input
            if (targetRenderer == null)
                return;

            // Best effort: URP/HDRP commonly use _BaseColor, built-in often uses _Color
            var mat = targetRenderer.sharedMaterial;
            if (mat == null)
            {
                activeColorPropertyID = BaseColorID; // fallback default
                return;
            }

            if (mat.HasProperty(BaseColorID)) activeColorPropertyID = BaseColorID;
            else if (mat.HasProperty(ColorID)) activeColorPropertyID = ColorID;
            else activeColorPropertyID = BaseColorID; // fallback
        }

        private void ApplyColor(Color color)
        {
            // Validate input
            if (targetRenderer == null)
                return;

            targetRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor(activeColorPropertyID, color);
            targetRenderer.SetPropertyBlock(propertyBlock);
        }
    }
}
