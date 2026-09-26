using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using Crowfox.Util;

namespace Crowfox.Audio
{
    /// <summary>
    /// Volume Types for any simple to slightly complex game audio system.
    /// For more immersive or complex games, we'll want to expand this.
    /// </summary>
    public enum VolumeType
    {
        // This is only adjusted in script for specific behaviour to mute all sound (e.g. on pause). It is not user adjustable.
        Master,

        // The master volume the player will be able to control. Called Master in settings menu.
        PlayerMaster,

        // Sounds like button clicks or hovers and non-gameplay SFX.
        UI,

        // All gameplay sound effects that are not covered by other categories. e.g. Footsteps, gunshots, item pickups, etc.
        SFX,

        // Usually all (2D) music. (A radio might be ambient or SFX).
        Music,

        // Ambient sounds, generally sounds the player has no control over. e.g. wind, birds, traffic, machinery, etc. that are background only.
        Ambient,

        // All spoken or written dialogue.
        Dialogue,
    }

    public sealed class AudioManager : MonoBehaviour
    {
        public static event Action Action_OnResetVolumes;

        public static AudioManager Instance;

        [Header("Mixer")]
        public AudioMixer MasterAudioMixer;

        const float MIN_DECIBEL = -80f;
        const string VOLUME_KEY_PREFIX = "Volume_";

        [Header("Behaviour")]
        [SerializeField] bool muteOnPause = true;

        [Header("Menu Sounds")]
        [SerializeField] List<AudioClip> uiHoverSounds;
        [SerializeField] List<AudioClip> uiClickSounds;

        [Header("In-Game Sounds")]
        [SerializeField] float uiVolume = 0.35f;

        [Space]
        [Header("PlayerPrefs")]
        [Space]
        private const float DEFAULT_VOLUME_MASTER = 100f;
        private const float DEFAULT_VOLUME_PLAYERMASTER = 50f;
        private const float DEFAULT_VOLUME_SFX = 50f;
        private const float DEFAULT_VOLUME_MUSIC = 50f;
        private const float DEFAULT_VOLUME_AMBIENT = 50f;
        private const float DEFAULT_VOLUME_DIALOGUE = 50f;
        private const float DEFAULT_VOLUME_UI = 50f;

        // Cache
        bool lastPausedState = false;
        Transform tr_HoverSFX;
        Transform tr_ClickSFX;


        string GetMixerChannel(VolumeType type) => VOLUME_KEY_PREFIX + type;

        float GetDefaultValue(VolumeType type) => type switch
        {
            VolumeType.Master => DEFAULT_VOLUME_MASTER,
            VolumeType.PlayerMaster => DEFAULT_VOLUME_PLAYERMASTER,
            VolumeType.SFX => DEFAULT_VOLUME_SFX,
            VolumeType.Music => DEFAULT_VOLUME_MUSIC,
            VolumeType.Ambient => DEFAULT_VOLUME_AMBIENT,
            VolumeType.Dialogue => DEFAULT_VOLUME_DIALOGUE,
            VolumeType.UI => DEFAULT_VOLUME_UI,
            _ => 50f
        };

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this; // Set the singleton instance
            transform.SetParent(null); // Detach from any parent to avoid hierarchy issues
            DontDestroyOnLoad(gameObject); // Ensure this instance persists across scenes

            if (MasterAudioMixer == null)
                Debug.LogError("AudioManager: MasterAudioMixer is not assigned in the inspector.", this);

            SetDefaultVolumeValues();
            ConfigureAudioSources();
        }

        private void OnEnable()
        {
            UISounds.OnHovered += PlayHoverSound;
            UISounds.OnClicked += PlayClickSound;
            VolumeSliders.OnVolumeUpdate += OnVolumeChanged;
            MenuButtons.Action_ResetVolumes += ResetVolumes;
        }

        private void OnDisable()
        {
            UISounds.OnHovered -= PlayHoverSound;
            UISounds.OnClicked -= PlayClickSound;
            VolumeSliders.OnVolumeUpdate -= OnVolumeChanged;
            MenuButtons.Action_ResetVolumes -= ResetVolumes;
        }

        private void Start()
        {
            SetMixerValues();

            // Initialize pause state once
            lastPausedState = Time.timeScale == 0f;
            if (muteOnPause)
                ApplyPauseMute(lastPausedState);
        }

        void Update()
        {
            // Update mute state based on time scale, if enabled (event-like: only on state changes)
            if (!muteOnPause || MasterAudioMixer == null)
                return;

            bool paused = Time.timeScale == 0f;
            if (paused == lastPausedState)
                return;

            lastPausedState = paused;
            ApplyPauseMute(paused);
        }

        void ApplyPauseMute(bool paused)
        {
            // Master is not user controlled in your setup, so we can safely toggle it between 0 and MIN_DECIBEL.
            MasterAudioMixer.SetFloat(GetMixerChannel(VolumeType.Master), paused ? MIN_DECIBEL : 0f);
        }

        public void ResetVolumes()
        {
            foreach (VolumeType type in Enum.GetValues(typeof(VolumeType)))
            {
                float defaultValue = GetDefaultValue(type);
                PlayerPrefs.SetFloat(GetMixerChannel(type), defaultValue);
                OnVolumeChanged(type, defaultValue);
            }

            PlayerPrefs.Save();
            Action_OnResetVolumes?.Invoke();
        }

        public AudioMixerGroup GetAudioMixerGroupByType(VolumeType type)
        {
            if (MasterAudioMixer == null)
            {
                Debug.LogError("AudioManager: MasterAudioMixer is null.", this);
                return null;
            }

            // IMPORTANT: This must match your AudioMixerGroup names.
            // Example group name: "Volume_SFX", "Volume_UI", ...
            string groupName = type.ToString();

            var groups = MasterAudioMixer.FindMatchingGroups(groupName);
            if (groups == null || groups.Length == 0)
            {
                Debug.LogError($"AudioManager: No AudioMixerGroup found for '{groupName}'. Ensure a group with that exact name exists in the MasterAudioMixer.", this);
                return null;
            }

            return groups[0];
        }

        void SetDefaultVolumeValues()
        {
            // Set default values for volume IF THEY DONT EXIST
            foreach (VolumeType type in Enum.GetValues(typeof(VolumeType)))
            {
                string key = GetMixerChannel(type);

                if (!PlayerPrefs.HasKey(key))
                {
                    float defaultValue = GetDefaultValue(type);
                    PlayerPrefs.SetFloat(key, defaultValue);
                }
            }

            PlayerPrefs.Save();
        }

        void OnVolumeChanged(VolumeType type, float volumePercentage)
        {
            if (MasterAudioMixer == null)
            {
                Debug.LogError("AudioManager: MasterAudioMixer is null. Cannot apply volume changes.", this);
                return;
            }

            float volumeDb = VolumePercentToDb(volumePercentage);

            string mixerName = GetMixerChannel(type);
            MasterAudioMixer.SetFloat(mixerName, volumeDb);
        }

        static float VolumePercentToDb(float volumePercent)
        {
            if (volumePercent <= 0f) return MIN_DECIBEL;
            return Mathf.Clamp(20f * Mathf.Log10(volumePercent / 100f), MIN_DECIBEL, 0f);
        }

        public AudioClip PlayAnySoundFromList(List<AudioClip> list, AudioClip lastPlayedSound = null, VolumeType volumeType = VolumeType.SFX, float volume = 1f, float pitch = 1f, bool doLoop = false, bool doPlayOnAwake = false)
        {
            if (list == null || list.Count == 0)
            {
                Debug.LogError("No audio clips provided to play.", this);
                return null;
            }

            return EasyAudio.SmartPlayListReturnClip(list, transform, lastPlayedSound, volumeType, volume, pitch, doLoop, doPlayOnAwake, 0f, 0f, 0f);
        }

        public void PlayAnySound(AudioClip soundToPlay, VolumeType volumeType = VolumeType.SFX, float volume = 1f, float pitch = 1f, bool doLoop = false, bool doPlayOnAwake = false)
        {
            if (soundToPlay == null)
            {
                Debug.LogError("No audio clips provided to play.", this);
                return;
            }

            EasyAudio.SmartPlayClip(soundToPlay, transform, volumeType, volume, pitch, doLoop, doPlayOnAwake, 0f, 0f, 0f);
        }

        void PlayHoverSound()
        {
            if (uiHoverSounds == null || uiHoverSounds.Count == 0)
                return;

            if (tr_HoverSFX == null)
            {
                tr_HoverSFX = new GameObject("HoverSFX").transform;
                tr_HoverSFX.SetParent(transform);
            }

            EasyAudio.SmartPlayListReturnClip(uiHoverSounds, tr_HoverSFX, null, VolumeType.UI, uiVolume, 1f, false, false, 0f, 0f, 0f, 1);
        }

        void PlayClickSound()
        {
            if (uiClickSounds == null || uiClickSounds.Count == 0)
                return;

            if (tr_ClickSFX == null)
            {
                tr_ClickSFX = new GameObject("ClickSFX").transform;
                tr_ClickSFX.SetParent(transform);
            }

            EasyAudio.SmartPlayListReturnClip(uiClickSounds, tr_ClickSFX, null, VolumeType.UI, uiVolume, 1f, false, false, 0f, 0f, 0f, 1);
        }

        void ConfigureAudioSources()
        {
            foreach (AudioSource audioSource in GetComponentsInChildren<AudioSource>(true))
            {
                audioSource.loop = false;
                audioSource.reverbZoneMix = 0f;
                audioSource.dopplerLevel = 0f;
                audioSource.spatialBlend = 0f;
            }
        }

        void SetMixerValues()
        {
            // Apply saved values to the AudioMixer
            foreach (VolumeType type in Enum.GetValues(typeof(VolumeType)))
            {
                string key = GetMixerChannel(type);
                float savedPercent = PlayerPrefs.GetFloat(key, GetDefaultValue(type));
                OnVolumeChanged(type, savedPercent);
            }
        }

        public void UpdatePlayerPref(VolumeType type, float value, bool saveImmediately = false)
        {
            // Save percent value (0..100) into PlayerPrefs
            PlayerPrefs.SetFloat(GetMixerChannel(type), value);

            if (saveImmediately)
                PlayerPrefs.Save();

            OnVolumeChanged(type, value);
        }
    }
}
