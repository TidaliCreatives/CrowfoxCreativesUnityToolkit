using UnityEngine;

namespace Crowfox.Util
{
    public sealed class EasyMovements : MonoBehaviour
    {
        [System.Flags]
        private enum Axis
        {
            None = 0,
            X = 1 << 0,
            Y = 1 << 1,
            Z = 1 << 2,
        }

        [SerializeField] bool doUseUnscaledTime = false;

        [Header("Rotation")]
        [SerializeField] private bool rotationIsActive = false;
        [SerializeField] private float degreePerSecond = 10f;
        [SerializeField] private Axis rotationAxis = Axis.Y;
        [SerializeField] private bool clockwise = true;
        [Space]

        [Header("Directional Movement")]
        [SerializeField] private bool directionalMovementIsActive = false;
        [SerializeField] private bool movementIsLocal = true;
        [SerializeField] private bool doTeleportInsteadOfLoop = false;
        [SerializeField, Range(0f, 0.999f)]
        float teleportStartValue = 0f;
        [SerializeField] private bool doLerpLoop = false;
        [SerializeField] private float directionalLoopRadius = 1f;
        [SerializeField] private bool doChooseRandomRadius = false;
        [SerializeField] private Vector2 directionalLoopRadiusMinMax = new(0f, 2f);
        [SerializeField] private float directionalLoopDuration = 1.5f;
        [SerializeField] private Axis directionalLoopAxis = Axis.Y;
        [SerializeField] private bool directionalLoopStartsPositive = true;
        [Space]

        [Header("Material Loop")]
        [SerializeField] private bool materialLoopIsActive = false;
        [SerializeField] private bool doLerpMaterialAlpha = false;
        [SerializeField] private float materialLoopDuration = 1.5f;
        [Space]

        [Header("Light Fade")]
        [SerializeField] private bool lightFadeIsActive = false;
        [SerializeField] private bool doLerpLightValues = true;
        [SerializeField] private float lightFadeDuration = 1.5f;
        [SerializeField] private bool includeLightIntensity = true;
        [SerializeField] private bool includeLightRange = true;
        [SerializeField] private bool includeLightColor = true;

        [Tooltip("Loop start point inside the 0..1 cycle (phase offset).")]
        [SerializeField, Range(0f, 1f)] private float lightFadeStartIndex01 = 0f;

        [Tooltip("Light intensity at the start of the cycle.")]
        [SerializeField] private float lightStartIntensity = 1f;

        [Tooltip("Light intensity at the end of the cycle.")]
        [SerializeField] private float lightEndIntensity = 0f;

        [Tooltip("Light range at the start of the cycle.")]
        [SerializeField] private float lightStartRange = 10f;

        [Tooltip("Light range at the end of the cycle.")]
        [SerializeField] private float lightEndRange = 0f;

        [Tooltip("Light color at the start of the cycle.")]
        [SerializeField] private Color lightStartColor = Color.white;

        [Tooltip("Light color at the end of the cycle.")]
        [SerializeField] private Color lightEndColor = Color.black;

        private Vector3 startWorldPos;
        private Vector3 startLocalPos;
        private float startTime;
        private Renderer cachedRenderer;
        private MaterialPropertyBlock mpb;
        private static readonly int ColorId = Shader.PropertyToID("_Color");

        private Light cachedLight;

        private bool warnedNoRenderer;
        private bool warnedNoLight;

        private void Awake()
        {
            startWorldPos = transform.position;
            startLocalPos = transform.localPosition;
            startTime = doUseUnscaledTime ? Time.unscaledTime : Time.time;

            if (transform.TryGetComponentInChildren(out Renderer renderer))
                cachedRenderer = renderer;
            else if (materialLoopIsActive)
                Debug.LogError("EasyMovements: No Renderer component found in children for MaterialLoop option!", transform);

            if (transform.TryGetComponentInChildren(out Light light))
                cachedLight = light;
            else if (lightFadeIsActive)
                Debug.LogError("EasyMovements: No Light component found in children for LightFade option!", transform);

            mpb = new MaterialPropertyBlock();

            if (doChooseRandomRadius)
                directionalLoopRadius = Random.Range(directionalLoopRadiusMinMax.x, directionalLoopRadiusMinMax.y);
        }

        private void LateUpdate()
        {
            if (rotationIsActive) Rotate();
            if (directionalMovementIsActive) DirectionalMove();
            if (materialLoopIsActive) MaterialLoop();
            if (lightFadeIsActive) LightFade();
        }

        private void Rotate()
        {
            Vector3 axisVector = AxisToVector(rotationAxis);
            if (axisVector.sqrMagnitude < 0.0001f) axisVector = Vector3.up;
            axisVector.Normalize();

            float direction = clockwise ? 1f : -1f;
            var time = doUseUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            transform.Rotate(axisVector, degreePerSecond * direction * time, Space.Self);
        }

        void DirectionalMove()
        {
            float duration = Mathf.Max(0.0001f, directionalLoopDuration);
            float elapsed = doUseUnscaledTime ? Time.unscaledTime - startTime : Time.time - startTime;

            Vector3 direction = Vector3.zero;

            if (movementIsLocal)
            {
                if ((directionalLoopAxis & Axis.X) != 0) direction += transform.right;
                if ((directionalLoopAxis & Axis.Y) != 0) direction += transform.up;
                if ((directionalLoopAxis & Axis.Z) != 0) direction += transform.forward;
            }
            else
            {
                if ((directionalLoopAxis & Axis.X) != 0) direction += Vector3.right;
                if ((directionalLoopAxis & Axis.Y) != 0) direction += Vector3.up;
                if ((directionalLoopAxis & Axis.Z) != 0) direction += Vector3.forward;
            }

            if (direction.sqrMagnitude < 0.0001f)
                direction = movementIsLocal ? transform.forward : Vector3.forward;

            direction.Normalize();

            float sign = directionalLoopStartsPositive ? 1f : -1f;
            Vector3 offset = direction * (directionalLoopRadius * sign);

            Vector3 basePos = movementIsLocal ? startLocalPos : startWorldPos;

            float t;

            if (doTeleportInsteadOfLoop)
            {
                float sv = Mathf.Clamp01(teleportStartValue);
                float phase = Mathf.Repeat((elapsed / duration) + sv, 1f);

                Vector3 snapStart = basePos - (offset * sv);
                Vector3 pos = snapStart + (offset * phase);

                if (movementIsLocal)
                    transform.localPosition = pos;
                else
                    transform.position = pos;

                return;
            }
            else if (doLerpLoop)
            {
                t = (Mathf.Sin((elapsed / duration) * Mathf.PI * 2f) + 1f) * 0.5f;
            }
            else
            {
                t = Mathf.PingPong((elapsed / duration) * 2f, 1f);
            }

            Vector3 currentOffset = offset * t;

            if (movementIsLocal)
                transform.localPosition = basePos + currentOffset;
            else
                transform.position = basePos + currentOffset;
        }

        private void MaterialLoop()
        {
            if (cachedRenderer == null)
            {
                if (!warnedNoRenderer)
                {
                    Debug.LogWarning(
                        "MaterialLoop option requires a Renderer component in the child hierarchy to function properly!",
                        transform);
                    warnedNoRenderer = true;
                }
                return;
            }

            float duration = Mathf.Max(0.0001f, materialLoopDuration);
            var time = doUseUnscaledTime ? Time.unscaledTime : Time.time;
            float alpha = EvaluateLoopFactor(time - startTime, duration, doLerpMaterialAlpha);

            cachedRenderer.GetPropertyBlock(mpb);

            Color baseColor = Color.white;
            if (cachedRenderer.sharedMaterial != null && cachedRenderer.sharedMaterial.HasProperty(ColorId))
                baseColor = cachedRenderer.sharedMaterial.GetColor(ColorId);

            baseColor.a = alpha;

            mpb.SetColor(ColorId, baseColor);
            cachedRenderer.SetPropertyBlock(mpb);
        }

        private void LightFade()
        {
            if (cachedLight == null)
            {
                if (!warnedNoLight)
                {
                    Debug.LogWarning(
                        "LightFade option requires a Light component in the child hierarchy to function properly!",
                        transform);
                    warnedNoLight = true;
                }
                return;
            }

            float duration = Mathf.Max(0.0001f, lightFadeDuration);
            float elapsed = doUseUnscaledTime ? Time.unscaledTime - startTime : Time.time - startTime;

            // Phase offset (0..1) across the cycle.
            float phase = Mathf.Repeat((elapsed / duration) + Mathf.Clamp01(lightFadeStartIndex01), 1f);

            // Match the feel of other loops: either sine (smooth) or pingpong (linear 0..1..0).
            float t;
            if (doLerpLightValues)
            {
                // Smooth 0..1..0
                t = (Mathf.Sin(phase * Mathf.PI * 2f) + 1f) * 0.5f;
            }
            else
            {
                // Linear 0..1..0 with phase
                t = Mathf.PingPong(phase * 2f, 1f);
            }

            if (includeLightIntensity)
                cachedLight.intensity = Mathf.Lerp(lightStartIntensity, lightEndIntensity, t);

            if (includeLightRange)
                cachedLight.range = Mathf.Lerp(lightStartRange, lightEndRange, t);

            if (includeLightColor)
                cachedLight.color = Color.Lerp(lightStartColor, lightEndColor, t);
        }

        private static float EvaluateLoopFactor(float elapsed, float duration, bool useSine)
        {
            if (useSine)
            {
                float sine = Mathf.Sin((elapsed / duration) * Mathf.PI * 2f);
                return (sine + 1f) * 0.5f;
            }

            return Mathf.PingPong((elapsed / duration) * 2f, 1f);
        }

        private static Vector3 AxisToVector(Axis axis)
        {
            Vector3 v = Vector3.zero;
            if ((axis & Axis.X) != 0) v.x = 1f;
            if ((axis & Axis.Y) != 0) v.y = 1f;
            if ((axis & Axis.Z) != 0) v.z = 1f;
            return v;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            degreePerSecond = Mathf.Max(0f, degreePerSecond);
            directionalLoopRadius = Mathf.Max(0f, directionalLoopRadius);
            directionalLoopDuration = Mathf.Max(0.0001f, directionalLoopDuration);
            materialLoopDuration = Mathf.Max(0.0001f, materialLoopDuration);

            lightFadeDuration = Mathf.Max(0.0001f, lightFadeDuration);
            lightStartRange = Mathf.Max(0f, lightStartRange);
            lightEndRange = Mathf.Max(0f, lightEndRange);
            lightStartIntensity = Mathf.Max(0f, lightStartIntensity);
            lightEndIntensity = Mathf.Max(0f, lightEndIntensity);
        }
#endif
    }
}
