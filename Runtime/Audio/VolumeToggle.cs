using System;
using UnityEngine;
using UnityEngine.UI;

namespace Crowfox.Audio
{
    public class VolumeToggle : MonoBehaviour
    {
        public static event Action<VolumeType, bool> Action_OnMuteToggled; // _volumeType, isMuted

        [SerializeField] VolumeType _volumeType;
        [SerializeField] Image img_Icon;
        [SerializeField] Sprite sprite_Muted;
        [SerializeField] Sprite sprite_Unmuted;
        const string VOLUME_KEY_PREFIX = "IsMuted_";
        bool _isMuted = false;

        private void OnEnable()
        {
            AudioManager.Action_OnResetVolumes += OnResetVolumes;
        }

        private void OnDisable()
        {
            AudioManager.Action_OnResetVolumes -= OnResetVolumes;
        }

        private void Start()
        {
            _isMuted = PlayerPrefs.GetInt(VOLUME_KEY_PREFIX + _volumeType, 0) == 1;
            UpdateVisuals();
            Action_OnMuteToggled?.Invoke(_volumeType, _isMuted);
        }

        public void ToggleMute()
        {
            _isMuted = !_isMuted;
            PlayerPrefs.SetInt(VOLUME_KEY_PREFIX + _volumeType, _isMuted ? 1 : 0);
            UpdateVisuals();
            Action_OnMuteToggled?.Invoke(_volumeType, _isMuted);
        }

        void UpdateVisuals()
        {
            if (!img_Icon)
                return;

            img_Icon.sprite = _isMuted ? sprite_Muted : sprite_Unmuted;
        }

        void OnResetVolumes()
        {
            _isMuted = PlayerPrefs.GetInt(VOLUME_KEY_PREFIX + _volumeType, 0) == 1;
            UpdateVisuals();
        }
    }
}
