using UnityEngine;

namespace Crowfox.Util
{
    public class JuicyAnimation : MonoBehaviour
    {
        [Header("Moving")]
        [SerializeField] private bool doMove = false;
        [SerializeField] private bool moveRight = false;
        [SerializeField] private bool hasGlobalBoundaries = false;
        [SerializeField] private float boundaryLeft = 50f;
        [SerializeField] private float boundaryRight = 70f;
        [SerializeField] private float moveSpeed = 1f;
        [SerializeField] private float maxMoveDistance = 40f;
        [Space]

        [Header("Scaling")]
        [SerializeField] private bool doScale = true;
        [SerializeField] private float bounceTime = 1.0f; // seconds per cycle
        [SerializeField] private float maxScaleAddition = 0.025f;
        [Space]

        [Header("Bounce")]
        [SerializeField] private bool doBounce = false;
        [SerializeField] private float elevationTime = 1.0f; // seconds per cycle
        [SerializeField] private float maxElevation = 0.025f;
        [SerializeField] private bool doRandomizeSeed = false;
        [Space]

        [Header("Sprite Rotation")]
        [SerializeField] private bool _doUpdateSpriteRotation = false;
        [SerializeField] private bool _doInvertFlipX = false;
        SpriteRenderer _spRenderer;
        float lastX = 0f;

        // General stuff
        private Vector3 startPos;
        private Vector3 startScale;

        private float scaleSeed;
        private float bounceSeed;

        private void Start()
        {
            startPos = transform.position;
            startScale = transform.localScale;

            if (doRandomizeSeed)
            {
                scaleSeed = Random.Range(0f, 10f);
                bounceSeed = Random.Range(0f, 10f);
            }

            if (!transform.TryGetComponentInChildren(out _spRenderer) && _doUpdateSpriteRotation)
            {
                Debug.LogWarning($"JuicyAnimation: No SpriteRenderer found in children of {gameObject.name}. Sprite rotation might not work as intended.", transform);
            }
        }

        private void Update()
        {
            if (doScale) { JuicyScale(); }
            if (doBounce) { JuicyBounce(); }
            if (doMove) { JuicyMove(); }
            if (_doUpdateSpriteRotation && _spRenderer != null) { UpdateSpriteRotation(); }
        }

        private void JuicyScale()
        {
            // Validate input
            if (bounceTime <= 0f) return;

            // Sinus in [-1..1]
            float t = (Time.time + scaleSeed) * (Mathf.PI * 2f) / bounceTime;
            float wave = Mathf.Sin(t);

            // Apply only to Y, but based on startScale (no drift)
            float y = startScale.y + wave * maxScaleAddition;
            transform.localScale = new Vector3(startScale.x, y, startScale.z);
        }

        private void JuicyBounce()
        {
            // Validate input
            if (elevationTime <= 0f) return;

            float t = (Time.time + bounceSeed) * (Mathf.PI * 2f) / elevationTime;
            float wave = Mathf.Sin(t);

            float y = startPos.y + wave * maxElevation;
            transform.position = new Vector3(transform.position.x, y, transform.position.z);
        }

        private void JuicyMove()
        {
            float dir = moveRight ? 1f : -1f;
            float x = transform.position.x + dir * moveSpeed * Time.deltaTime;

            // Reset x pos when reached global boundaries
            if (hasGlobalBoundaries)
            {
                float left = -Mathf.Abs(boundaryLeft);
                float right = boundaryRight;

                // Wrap around
                if (x < left) x = right;
                else if (x > right) x = left;

                transform.position = new Vector3(x, transform.position.y, transform.position.z);
                return;
            }

            // Reset x pos when reached max distance
            if (Mathf.Abs(x - startPos.x) > maxMoveDistance)
            {
                x = startPos.x;
            }

            transform.position = new Vector3(x, transform.position.y, transform.position.z);
        }

        private void UpdateSpriteRotation()
        {
            var posX = _spRenderer.transform.position;
            if (posX.x > lastX)
            {
                _spRenderer.flipX = _doInvertFlipX;
            }
            else if (posX.x < lastX)
            {
                _spRenderer.flipX = !_doInvertFlipX;
            }
            lastX = posX.x;
        }
    }
}
