using Crowfox.Util;
using UnityEngine;

namespace Crowfox.Audio
{
    [RequireComponent(typeof(AudioSource))]
    public class SimpleAmbientPlayer : MonoBehaviour
    {
        public static SimpleAmbientPlayer Instance { get; private set; }


        AudioSource _audioSource;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
                return;
            }
            Instance = this;
            transform.SetParent(null); // Detach from any parent to avoid being destroyed with it
            DontDestroyOnLoad(this.gameObject);
        }

        void Start()
        {
            _audioSource = GetComponent<AudioSource>();
            _audioSource.volume = 0f; // Start with volume 0
            _audioSource.FadeAudioSource(1f, 1f);
        }
    }
}