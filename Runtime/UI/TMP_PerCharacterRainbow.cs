using TMPro;
using UnityEngine;

namespace Crowfox.Util
{
    [RequireComponent(typeof(TMP_Text))]
    public sealed class TMP_PerCharacterRainbow : MonoBehaviour
    {
        [SerializeField] private bool isActive = true;
        [SerializeField] private bool useUnscaledTime = false;

        [Header("Rainbow")]
        [SerializeField, Min(0.0001f)] private float colorCycleDuration = 3f;
        [SerializeField, Min(0f)] private float hueOffsetPerCharacter = 0.08f;
        [SerializeField, Range(0f, 1f)] private float saturation = 1f;
        [SerializeField, Range(0f, 1f)] private float value = 1f;

        [Header("Per-Character Size (Optional)")]
        [SerializeField] private bool animateCharacterSize = false;

        [Tooltip("How fast the size wave moves (seconds for a full cycle).")]
        [SerializeField, Min(0.0001f)] private float sizeCycleDuration = 1.5f;

        [Tooltip("Wave phase offset per character (bigger = more variation across letters).")]
        [SerializeField, Min(0f)] private float sizeOffsetPerCharacter = 0.35f;

        [Tooltip("Amplitude of the size wave. Example: 0.25 = up to +25% and down to -25% around 1.")]
        [SerializeField, Range(0f, 1f)] private float sizeAmplitude = 0.2f;

        [Tooltip("Clamp the final per-character scale multiplier.")]
        [SerializeField, Min(0.01f)] private float sizeMinMultiplier = 0.6f;

        [SerializeField, Min(0.01f)] private float sizeMaxMultiplier = 1.8f;

        [Header("Options")]
        [SerializeField] private bool animate = true;

        private TMP_Text _tmp;

        private void Awake()
        {
            _tmp = GetComponent<TMP_Text>();
        }

        private void OnEnable()
        {
            _tmp.ForceMeshUpdate();
            ApplyColorsAndOptionalSize();
        }

        private void Update()
        {
            if (!isActive)
                return;

            if (!animate)
                return;

            ApplyColorsAndOptionalSize();
        }

        private void ApplyColorsAndOptionalSize()
        {
            if (_tmp == null)
                return;

            // Rebuild mesh info so we always start from a clean base (prevents vertex drift).
            _tmp.ForceMeshUpdate();

            TMP_TextInfo textInfo = _tmp.textInfo;
            int charCount = textInfo.characterCount;
            if (charCount == 0)
                return;

            float t = useUnscaledTime ? Time.unscaledTime : Time.time;

            float baseHue = (colorCycleDuration <= 0f) ? 0f : Mathf.Repeat(t / colorCycleDuration, 1f);
            float sizePhaseBase = (sizeCycleDuration <= 0f) ? 0f : (t / sizeCycleDuration) * Mathf.PI * 2f;

            // Modify per character
            for (int i = 0; i < charCount; i++)
            {
                TMP_CharacterInfo c = textInfo.characterInfo[i];
                if (!c.isVisible)
                    continue;

                int matIndex = c.materialReferenceIndex;
                int vIndex = c.vertexIndex;

                // ----- Color -----
                float hue = Mathf.Repeat(baseHue + (i * hueOffsetPerCharacter), 1f);
                Color32 col = Color.HSVToRGB(hue, saturation, value);

                Color32[] colors = textInfo.meshInfo[matIndex].colors32;
                colors[vIndex + 0] = col;
                colors[vIndex + 1] = col;
                colors[vIndex + 2] = col;
                colors[vIndex + 3] = col;

                // ----- Size (vertex scaling) -----
                if (animateCharacterSize)
                {
                    Vector3[] verts = textInfo.meshInfo[matIndex].vertices;

                    // Quad vertices:
                    // 0 bottom-left, 1 top-left, 2 top-right, 3 bottom-right (TMP convention)
                    Vector3 bl = verts[vIndex + 0];
                    Vector3 tl = verts[vIndex + 1];
                    Vector3 tr = verts[vIndex + 2];
                    Vector3 br = verts[vIndex + 3];

                    // Center of the quad
                    Vector3 center = (bl + tl + tr + br) * 0.25f;

                    // Smooth wave: scale = 1 + sin(...) * amplitude
                    float phase = sizePhaseBase + (i * sizeOffsetPerCharacter);
                    float wave = Mathf.Sin(phase); // -1..1
                    float scaleMul = 1f + (wave * sizeAmplitude);
                    scaleMul = Mathf.Clamp(scaleMul, sizeMinMultiplier, sizeMaxMultiplier);

                    // Scale vertices around the center
                    verts[vIndex + 0] = center + (bl - center) * scaleMul;
                    verts[vIndex + 1] = center + (tl - center) * scaleMul;
                    verts[vIndex + 2] = center + (tr - center) * scaleMul;
                    verts[vIndex + 3] = center + (br - center) * scaleMul;
                }
            }

            // Push updated data back to TMP meshes
            for (int m = 0; m < textInfo.meshInfo.Length; m++)
            {
                TMP_MeshInfo mi = textInfo.meshInfo[m];

                mi.mesh.vertices = mi.vertices;
                mi.mesh.colors32 = mi.colors32;

                _tmp.UpdateGeometry(mi.mesh, m);
            }
        }
    }
}