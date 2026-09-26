using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Crowfox.Audio
{
    public class VolumeSliders : MonoBehaviour
    {
        public static event Action<VolumeType, float> OnVolumeUpdate;

        [SerializeField] VolumeType _volumeType;
        [SerializeField] TextMeshProUGUI tmp_VolumeText;
        Slider _slider;
        const string VOLUME_KEY_PREFIX = "Volume_";

        private void OnEnable()
        {
            AudioManager.Action_OnResetVolumes += OnResetVolumes;
        }

        private void OnDisable()
        {
            AudioManager.Action_OnResetVolumes -= OnResetVolumes;
        }

        private void Awake()
        {
            if (!TryGetComponent(out _slider))
            {
                Debug.LogError($"VolumeSliders: No Slider component found on {gameObject.name}. Disabling script.");
                enabled = false;
            }

            if (tmp_VolumeText == null)
                Debug.LogWarning($"VolumeSliders: tmp_VolumeText is not assigned on {gameObject.name}. Volume percentage will not be displayed.");
        }

        private void Start()
        {
            if (PlayerPrefs.HasKey(VOLUME_KEY_PREFIX + _volumeType))
            {
                _slider.value = PlayerPrefs.GetFloat(VOLUME_KEY_PREFIX + _volumeType);
                tmp_VolumeText.SetText(((int)(_slider.value / _slider.maxValue * 100f)).ToString());
            }
        }

        public void OnSliderValueChanged()
        {
            float volumePercentage = _slider.value / _slider.maxValue * 100f;
            tmp_VolumeText.SetText(((int)volumePercentage).ToString());
            PlayerPrefs.SetFloat(VOLUME_KEY_PREFIX + _volumeType, volumePercentage);

            OnVolumeUpdate?.Invoke(_volumeType, volumePercentage);
        }

        void OnResetVolumes()
        {
            var value = PlayerPrefs.GetFloat(VOLUME_KEY_PREFIX + _volumeType);
            tmp_VolumeText.SetText(((int)value).ToString());
            _slider.SetValueWithoutNotify(value);
        }
    }
}
