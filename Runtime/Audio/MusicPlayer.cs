using Crowfox.Audio;
using Crowfox.Util;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicPlayer : MonoBehaviour
{
    public static MusicPlayer Instance { get; private set; }
    AudioSource _audioSource;
    float _initialVolume = 1f;

    //AudioSource _audioSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);

        if (TryGetComponent(out _audioSource))
            _initialVolume = _audioSource.volume;
    }

    void OnEnable()
    {
        VolumeToggle.Action_OnMuteToggled += OnMuteToggled;
        MenuButtons.Action_OnGameReset += OnGameReset;
    }

    void OnDisable()
    {
        VolumeToggle.Action_OnMuteToggled -= OnMuteToggled;
        MenuButtons.Action_OnGameReset -= OnGameReset;
    }

    void OnGameReset()
    {
        Instance = null;
        _audioSource.FadeAudioSource(0f, 3f, doDestroyAtEnd: true);
    }

    void OnMuteToggled(VolumeType volumeType, bool isMuted)
    {
        if (volumeType == VolumeType.Music)
            _audioSource.volume = isMuted ? 0f : _initialVolume;
    }
}
