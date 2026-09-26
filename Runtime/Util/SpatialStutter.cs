// Btw this was 100% written by ChaptGPT 5.2 at 21.02.2026

using Sortify;
using UnityEngine;

namespace Crowfox.Util
{
    /// <summary>
    /// Makes an object appear to "stutter" / jitter in space in a controllable way.
    /// Focus: position stutter (with optional rotation stutter + scale stutter).
    /// Works with local/world space, unscaled time, deterministic seed, pulses, smoothing, and drift correction.
    /// </summary>
    public sealed class SpatialStutter : MonoBehaviour
    {
        [System.Flags]
        private enum Axis
        {
            None = 0,
            X = 1 << 0,
            Y = 1 << 1,
            Z = 1 << 2,
        }

        private enum SpaceMode
        {
            Local,
            World
        }

        private enum StepWave
        {
            Random,
            Oscillate
        }

        [SerializeField] private bool doUseUnscaledTime = false;

        [BetterHeader("Master")]
        [SerializeField] private bool isActive = true;

        [Tooltip("Where the stutter is applied for the parent.")]
        [SerializeField] private SpaceMode spaceMode = SpaceMode.Local;

        [Tooltip("Writes in LateUpdate to win against most animation/follow scripts.")]
        [SerializeField] private bool writeInLateUpdate = true;

        [Space]
        [BetterHeader("Determinism")]
        [SerializeField] private bool useDeterministicSeed = true;
        [SerializeField] private int deterministicSeed = 1337;

        [Tooltip("If deterministic is OFF, a random seed is chosen on Awake.")]
        [SerializeField] private bool rerollSeedOnEnable = false;

        // -----------------------------
        // Parent Stutter: Position
        // -----------------------------
        [Space]
        [BetterHeader("Parent Position Stutter")]
        [SerializeField] private bool positionIsActive = true;
        [SerializeField] private Axis positionAxes = Axis.X | Axis.Y | Axis.Z;

        [SerializeField, Min(0f)] private float positionAmplitude = 0.05f;
        [SerializeField] private Vector3 positionAxisMultiplier = Vector3.one;

        [SerializeField, Min(0.01f)] private float positionStepsPerSecond = 12f;
        [SerializeField] private StepWave positionWave = StepWave.Random;

        [Tooltip("Only used when wave = Oscillate.")]
        [SerializeField, Min(0f)] private float positionOscHz = 3f;

        [Tooltip("Only used when wave = Oscillate.")]
        [SerializeField] private Vector3 positionOscPhase = Vector3.zero;

        [SerializeField] private bool positionUseSmoothing = true;
        [SerializeField, Min(0f)] private float positionSmoothing = 25f;

        [Tooltip("Grid-snap the generated offset (makes discrete 'steppy' motion).")]
        [SerializeField] private bool positionQuantize = false;
        [SerializeField, Min(0.0001f)] private float positionQuantizeStep = 0.01f;

        [Tooltip("Clamp Max offset length (0 = disabled).")]
        [SerializeField, Min(0f)] private float positionMaxMagnitude = 0.25f;

        // -----------------------------
        // Parent Stutter: Rotation
        // -----------------------------
        [Space]
        [BetterHeader("Parent Rotation Stutter")]
        [SerializeField] private bool rotationIsActive = false;
        [SerializeField] private Axis rotationAxes = Axis.Y;

        [SerializeField, Min(0f)] private float rotationAmplitudeDegrees = 2.5f;
        [SerializeField, Min(0.01f)] private float rotationStepsPerSecond = 10f;

        [SerializeField] private StepWave rotationWave = StepWave.Random;
        [SerializeField, Min(0f)] private float rotationOscHz = 2f;
        [SerializeField] private Vector3 rotationOscPhase = Vector3.zero;

        [SerializeField] private bool rotationUseSmoothing = true;
        [SerializeField, Min(0f)] private float rotationSmoothing = 20f;

        // -----------------------------
        // Parent Stutter: Scale
        // -----------------------------
        [Space]
        [BetterHeader("Parent Scale Stutter")]
        [SerializeField] private bool scaleIsActive = false;
        [SerializeField] private Axis scaleAxes = Axis.X | Axis.Y | Axis.Z;

        [SerializeField, Min(0f)] private float scaleAmplitude = 0.02f;
        [SerializeField, Min(0.01f)] private float scaleStepsPerSecond = 8f;

        [SerializeField] private StepWave scaleWave = StepWave.Random;
        [SerializeField, Min(0f)] private float scaleOscHz = 2f;
        [SerializeField] private Vector3 scaleOscPhase = Vector3.zero;

        [SerializeField] private bool scaleUseSmoothing = true;
        [SerializeField, Min(0f)] private float scaleSmoothing = 18f;

        [Tooltip("Prevents scale from reaching/going below zero.")]
        [SerializeField, Min(0.0001f)] private float minimumScaleComponent = 0.0001f;

        // -----------------------------
        // Child compensation (SpriteMask use-case)
        // -----------------------------
        [Space]
        [BetterHeader("Child Compensation (Keep Visual Stable)")]
        [SerializeField] private bool childCompensationIsActive = false;

        [Tooltip("Assign the visual child (e.g. sprite behind SpriteMask) that should stay stable in world space.")]
        [SerializeField] private Transform compensatedChild = null;

        [Tooltip("If ON, the child is kept stable in world position.")]
        [SerializeField] private bool compensateChildPosition = true;

        [Tooltip("If ON, the child is kept stable in world rotation.")]
        [SerializeField] private bool compensateChildRotation = true;

        [Tooltip("If ON, the child is kept stable in world scale.")]
        [SerializeField] private bool compensateChildScale = true;

        [Space]
        [BetterHeader("Child Extra Motion (still allowed)")]
        [Tooltip("Extra LOCAL position applied on top of the stable baseline (in child's stable frame).")]
        [SerializeField] private Vector3 childExtraLocalPosition = Vector3.zero;

        [Tooltip("Extra LOCAL rotation (Euler) applied on top of the stable baseline.")]
        [SerializeField] private Vector3 childExtraLocalEuler = Vector3.zero;

        [Tooltip("Extra scale multiplier applied on top of the stable baseline.")]
        [SerializeField] private Vector3 childExtraScaleMultiplier = Vector3.one;

        [Tooltip("Optional smoothing for child correction (0 = off). Useful if you want less harsh correction.")]
        [SerializeField, Min(0f)] private float childCompensationSmoothing = 0f;

        // -----------------------------
        // Internal cached rest pose
        // -----------------------------
        private Vector3 _restWorldPos;
        private Vector3 _restLocalPos;
        private Quaternion _restWorldRot;
        private Quaternion _restLocalRot;
        private Vector3 _restLocalScale;

        // Child baseline (world) captured on start/enable.
        private Matrix4x4 _childBaselineWorldMtx;
        private bool _hasChildBaseline;

        // RNG state (LCG).
        private int _rngState;

        // Step state
        private float _posTimer;
        private Vector3 _posHeld;
        private Vector3 _posSmoothed;

        private float _rotTimer;
        private Vector3 _rotHeldEuler;
        private Vector3 _rotSmoothedEuler;

        private float _scaleTimer;
        private Vector3 _scaleHeldMul;     // multiplicative around 1
        private Vector3 _scaleSmoothedMul; // multiplicative around 1

        private void Awake()
        {
            CacheRestPose();

            if (!useDeterministicSeed)
                deterministicSeed = Random.Range(int.MinValue, int.MaxValue);

            _rngState = deterministicSeed;

            _posHeld = Vector3.zero;
            _posSmoothed = Vector3.zero;

            _rotHeldEuler = Vector3.zero;
            _rotSmoothedEuler = Vector3.zero;

            _scaleHeldMul = Vector3.one;
            _scaleSmoothedMul = Vector3.one;

            CaptureChildBaselineWorld();
        }

        private void OnEnable()
        {
            if (rerollSeedOnEnable)
            {
                if (!useDeterministicSeed)
                    deterministicSeed = Random.Range(int.MinValue, int.MaxValue);

                _rngState = deterministicSeed;
            }

            CacheRestPose();
            CaptureChildBaselineWorld();
        }

        private void Update()
        {
            if (!writeInLateUpdate)
                TickAndApply();
        }

        private void LateUpdate()
        {
            if (writeInLateUpdate)
                TickAndApply();
        }

        private void TickAndApply()
        {
            if (!isActive)
                return;

            float dt = NowDeltaTime();

            Vector3 posOffset = positionIsActive ? TickPosition(dt) : Vector3.zero;
            Quaternion rotOffset = rotationIsActive ? TickRotation(dt) : Quaternion.identity;
            Vector3 scaleMul = scaleIsActive ? TickScale(dt) : Vector3.one;

            ApplyParentStutter(posOffset, rotOffset, scaleMul);

            if (childCompensationIsActive)
                ApplyChildCompensation(dt);
        }

        private void CacheRestPose()
        {
            _restWorldPos = transform.position;
            _restLocalPos = transform.localPosition;
            _restWorldRot = transform.rotation;
            _restLocalRot = transform.localRotation;
            _restLocalScale = transform.localScale;
        }

        private void CaptureChildBaselineWorld()
        {
            _hasChildBaseline = false;

            if (!childCompensationIsActive)
                return;

            if (compensatedChild == null)
                return;

            // Capture the child's current world pose as the baseline "stay here" target.
            _childBaselineWorldMtx = Matrix4x4.TRS(
                compensatedChild.position,
                compensatedChild.rotation,
                compensatedChild.lossyScale
            );

            _hasChildBaseline = true;
        }

        private void ApplyParentStutter(Vector3 posOffset, Quaternion rotOffset, Vector3 scaleMul)
        {
            if (spaceMode == SpaceMode.Local)
            {
                transform.localPosition = _restLocalPos + posOffset;
                transform.localRotation = _restLocalRot * rotOffset;
                transform.localScale = new Vector3(
                    _restLocalScale.x * scaleMul.x,
                    _restLocalScale.y * scaleMul.y,
                    _restLocalScale.z * scaleMul.z
                );
            }
            else
            {
                transform.position = _restWorldPos + posOffset;
                transform.rotation = _restWorldRot * rotOffset;

                // World-scale isn't directly settable; localScale is used.
                // This is still fine for most SpriteMask setups because parent is usually the relevant scaler.
                transform.localScale = new Vector3(
                    _restLocalScale.x * scaleMul.x,
                    _restLocalScale.y * scaleMul.y,
                    _restLocalScale.z * scaleMul.z
                );
            }
        }

        // -----------------------------
        // Position
        // -----------------------------
        private Vector3 TickPosition(float dt)
        {
            float interval = 1f / Mathf.Max(0.01f, positionStepsPerSecond);
            _posTimer += dt;

            while (_posTimer >= interval)
            {
                _posTimer -= interval;

                Vector3 raw = positionWave == StepWave.Random
                    ? NextOffsetRandom()
                    : OffsetOscillate(positionOscHz, positionOscPhase);

                raw = ApplyAxisMask(raw, positionAxes);
                raw = ScaleVector(raw, positionAxisMultiplier);

                Vector3 target = raw * positionAmplitude;

                if (positionQuantize)
                    target = Quantize(target, positionQuantizeStep);

                if (positionMaxMagnitude > 0f)
                    target = Vector3.ClampMagnitude(target, positionMaxMagnitude);

                _posHeld = target;
            }

            if (positionUseSmoothing)
            {
                float k = positionSmoothing;
                _posSmoothed = Vector3.Lerp(_posSmoothed, _posHeld, 1f - Mathf.Exp(-k * dt));
            }
            else
            {
                _posSmoothed = _posHeld;
            }

            return _posSmoothed;
        }

        // -----------------------------
        // Rotation
        // -----------------------------
        private Quaternion TickRotation(float dt)
        {
            float interval = 1f / Mathf.Max(0.01f, rotationStepsPerSecond);
            _rotTimer += dt;

            while (_rotTimer >= interval)
            {
                _rotTimer -= interval;

                Vector3 raw = rotationWave == StepWave.Random
                    ? NextOffsetRandom()
                    : OffsetOscillate(rotationOscHz, rotationOscPhase);

                raw = ApplyAxisMask(raw, rotationAxes);

                // Use signed amplitude per axis (raw already in -1..1).
                _rotHeldEuler = raw * rotationAmplitudeDegrees;
            }

            if (rotationUseSmoothing)
            {
                float k = rotationSmoothing;
                _rotSmoothedEuler = Vector3.Lerp(_rotSmoothedEuler, _rotHeldEuler, 1f - Mathf.Exp(-k * dt));
            }
            else
            {
                _rotSmoothedEuler = _rotHeldEuler;
            }

            return Quaternion.Euler(_rotSmoothedEuler);
        }

        // -----------------------------
        // Scale
        // -----------------------------
        private Vector3 TickScale(float dt)
        {
            float interval = 1f / Mathf.Max(0.01f, scaleStepsPerSecond);
            _scaleTimer += dt;

            while (_scaleTimer >= interval)
            {
                _scaleTimer -= interval;

                Vector3 raw = scaleWave == StepWave.Random
                    ? NextOffsetRandom()
                    : OffsetOscillate(scaleOscHz, scaleOscPhase);

                raw = ApplyAxisMask(raw, scaleAxes);

                // Multiplicative around 1.0
                Vector3 mul = Vector3.one + (raw * scaleAmplitude);

                mul.x = Mathf.Max(minimumScaleComponent, mul.x);
                mul.y = Mathf.Max(minimumScaleComponent, mul.y);
                mul.z = Mathf.Max(minimumScaleComponent, mul.z);

                _scaleHeldMul = mul;
            }

            if (scaleUseSmoothing)
            {
                float k = scaleSmoothing;
                _scaleSmoothedMul = Vector3.Lerp(_scaleSmoothedMul, _scaleHeldMul, 1f - Mathf.Exp(-k * dt));
            }
            else
            {
                _scaleSmoothedMul = _scaleHeldMul;
            }

            return _scaleSmoothedMul;
        }

        // -----------------------------
        // Child compensation (automatic, no reference)
        // -----------------------------
        private void ApplyChildCompensation(float dt)
        {
            if (compensatedChild == null)
                return;

            if (!_hasChildBaseline)
            {
                // If user toggled the option at runtime, recover.
                CaptureChildBaselineWorld();
                if (!_hasChildBaseline)
                    return;
            }

            // Build desired world pose = baseline world pose + extra motion.
            // Extras are applied in the baseline "stable" frame.
            DecomposeTRS(_childBaselineWorldMtx, out Vector3 basePos, out Quaternion baseRot, out Vector3 baseScale);

            Vector3 desiredPos = basePos;
            Quaternion desiredRot = baseRot;
            Vector3 desiredScale = baseScale;

            if (compensateChildRotation)
                desiredRot = baseRot * Quaternion.Euler(childExtraLocalEuler);
            else
                desiredRot = compensatedChild.rotation; // let it inherit parent effects if you want

            if (compensateChildPosition)
                desiredPos = basePos + (baseRot * childExtraLocalPosition);
            else
                desiredPos = compensatedChild.position;

            if (compensateChildScale)
                desiredScale = Vector3.Scale(baseScale, childExtraScaleMultiplier);
            else
                desiredScale = compensatedChild.lossyScale;

            // Compute required local TRS so that child's world TRS becomes desired TRS under current parent.
            // localMtx = parentWorldToLocal * desiredWorldMtx
            Matrix4x4 desiredWorld = Matrix4x4.TRS(desiredPos, desiredRot, desiredScale);
            Matrix4x4 parentWorldToLocal = transform.worldToLocalMatrix;
            Matrix4x4 localMtx = parentWorldToLocal * desiredWorld;

            DecomposeTRS(localMtx, out Vector3 targetLocalPos, out Quaternion targetLocalRot, out Vector3 targetLocalScale);

            if (childCompensationSmoothing > 0f)
            {
                float k = childCompensationSmoothing;
                float a = 1f - Mathf.Exp(-k * dt);

                compensatedChild.localPosition = Vector3.Lerp(compensatedChild.localPosition, targetLocalPos, a);
                compensatedChild.localRotation = Quaternion.Slerp(compensatedChild.localRotation, targetLocalRot, a);
                compensatedChild.localScale = Vector3.Lerp(compensatedChild.localScale, targetLocalScale, a);
            }
            else
            {
                compensatedChild.localPosition = targetLocalPos;
                compensatedChild.localRotation = targetLocalRot;
                compensatedChild.localScale = targetLocalScale;
            }
        }

        // -----------------------------
        // Helpers
        // -----------------------------
        private float NowDeltaTime() => doUseUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

        private float Next01()
        {
            unchecked { _rngState = _rngState * 1664525 + 1013904223; }
            uint u = (uint)_rngState;
            return (u & 0x00FFFFFF) / 16777215f;
        }

        private float NextSigned() => (Next01() * 2f) - 1f;

        private Vector3 NextOffsetRandom() => new Vector3(NextSigned(), NextSigned(), NextSigned());

        private Vector3 OffsetOscillate(float hz, Vector3 phase)
        {
            if (hz <= 0f)
                return Vector3.zero;

            float t = doUseUnscaledTime ? Time.unscaledTime : Time.time;
            float w = hz * Mathf.PI * 2f;

            float x = Mathf.Sin((t * w) + phase.x);
            float y = Mathf.Sin((t * w) + phase.y);
            float z = Mathf.Sin((t * w) + phase.z);

            return new Vector3(x, y, z);
        }

        private static Vector3 ApplyAxisMask(Vector3 v, Axis axes)
        {
            if ((axes & Axis.X) == 0) v.x = 0f;
            if ((axes & Axis.Y) == 0) v.y = 0f;
            if ((axes & Axis.Z) == 0) v.z = 0f;
            return v;
        }

        private static Vector3 ScaleVector(Vector3 v, Vector3 axisMultiplier)
        {
            v.x *= axisMultiplier.x;
            v.y *= axisMultiplier.y;
            v.z *= axisMultiplier.z;
            return v;
        }

        private static Vector3 Quantize(Vector3 v, float step)
        {
            float s = Mathf.Max(0.0001f, step);
            v.x = Mathf.Round(v.x / s) * s;
            v.y = Mathf.Round(v.y / s) * s;
            v.z = Mathf.Round(v.z / s) * s;
            return v;
        }

        /// <summary>
        /// Decomposes a matrix into TRS. Assumes no shear (Unity typical transform matrices).
        /// Handles negative scale approximately (best effort).
        /// </summary>
        private static void DecomposeTRS(Matrix4x4 m, out Vector3 pos, out Quaternion rot, out Vector3 scale)
        {
            pos = m.GetColumn(3);

            Vector3 x = m.GetColumn(0);
            Vector3 y = m.GetColumn(1);
            Vector3 z = m.GetColumn(2);

            float sx = x.magnitude;
            float sy = y.magnitude;
            float sz = z.magnitude;

            if (sx < 0.000001f) sx = 0.000001f;
            if (sy < 0.000001f) sy = 0.000001f;
            if (sz < 0.000001f) sz = 0.000001f;

            // Normalize axes to extract rotation.
            Vector3 xn = x / sx;
            Vector3 yn = y / sy;
            Vector3 zn = z / sz;

            // Fix handedness if needed (negative determinant).
            // If determinant is negative, flip one axis scale.
            float det = Vector3.Dot(Vector3.Cross(xn, yn), zn);
            if (det < 0f)
            {
                sx = -sx;
                xn = -xn;
            }

            rot = Quaternion.LookRotation(zn, yn);
            scale = new Vector3(sx, sy, sz);
        }

        /// <summary>
        /// Increases the scale stutter amplitude by the given amount.
        /// Positive values increase intensity, negative values reduce it.
        /// </summary>
        public void AddScaleAmplitude(float amount)
        {
            scaleAmplitude += amount;

            if (scaleAmplitude < 0f)
                scaleAmplitude = 0f;
        }

        public void AddScaleStepsPerSecond(float amount)
        {
            scaleStepsPerSecond += amount;
            if (scaleStepsPerSecond < 0.01f)
                scaleStepsPerSecond = 0.01f;
        }



#if UNITY_EDITOR
        private void OnValidate()
        {
            positionStepsPerSecond = Mathf.Max(0.01f, positionStepsPerSecond);
            rotationStepsPerSecond = Mathf.Max(0.01f, rotationStepsPerSecond);
            scaleStepsPerSecond = Mathf.Max(0.01f, scaleStepsPerSecond);

            positionAmplitude = Mathf.Max(0f, positionAmplitude);
            rotationAmplitudeDegrees = Mathf.Max(0f, rotationAmplitudeDegrees);
            scaleAmplitude = Mathf.Max(0f, scaleAmplitude);

            positionQuantizeStep = Mathf.Max(0.0001f, positionQuantizeStep);
            positionMaxMagnitude = Mathf.Max(0f, positionMaxMagnitude);

            positionOscHz = Mathf.Max(0f, positionOscHz);
            rotationOscHz = Mathf.Max(0f, rotationOscHz);
            scaleOscHz = Mathf.Max(0f, scaleOscHz);

            positionSmoothing = Mathf.Max(0f, positionSmoothing);
            rotationSmoothing = Mathf.Max(0f, rotationSmoothing);
            scaleSmoothing = Mathf.Max(0f, scaleSmoothing);

            minimumScaleComponent = Mathf.Max(0.0001f, minimumScaleComponent);
            childCompensationSmoothing = Mathf.Max(0f, childCompensationSmoothing);

            childExtraScaleMultiplier.x = Mathf.Max(0.0001f, childExtraScaleMultiplier.x);
            childExtraScaleMultiplier.y = Mathf.Max(0.0001f, childExtraScaleMultiplier.y);
            childExtraScaleMultiplier.z = Mathf.Max(0.0001f, childExtraScaleMultiplier.z);
        }
#endif

        // Optional: API so other scripts can drive the child's independent motion cleanly.
        public void SetChildExtraEuler(Vector3 euler) => childExtraLocalEuler = euler;
        public void SetChildExtraLocalPosition(Vector3 localPos) => childExtraLocalPosition = localPos;
        public void SetChildExtraScaleMultiplier(Vector3 mul) => childExtraScaleMultiplier = mul;

        /// <summary>
        /// Re-capture the child's current world pose as the new "stable baseline".
        /// Call this if you intentionally reposition the child and want that as the new lock point.
        /// </summary>
        public void RebaselineChildNow() => CaptureChildBaselineWorld();
    }
}
