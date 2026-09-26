using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace Crowfox.Audio
{
    public static class EasyAudio
    {
        // Reuse list to avoid allocations (non-thread-safe, but fine for main thread Unity usage)
        static readonly List<AudioSource> tempSources = new(8);

        /// <summary>
        /// Plays a random sound from a list on an AudioSource component attached to the given transform and
        /// returns the played audio clip.
        /// You can specify the last played sound to avoid repeating it immediately.
        /// Creates a new audio source if none exist or all are playing.
        /// </summary>
        public static AudioClip SmartPlayListReturnClip(List<AudioClip> sounds, Transform transform, AudioClip lastPlayedSound = null, VolumeType volumeType = VolumeType.SFX,
            float volume = 1f, float pitch = 1f, bool doLoop = false, bool doPlayOnAwake = false, float spatialBlend = 0f, float reverbZoneMix = 0f, float dopplerLevel = 0f, int maxAudioSources = 30)
        {
            // Return if anything is invalid
            //
            // Sounds
            if (sounds == null || sounds.Count == 0) { Debug.LogWarning("Assigned sounds are null or empty", transform); return null; }
            //
            // Transform
            if (transform == null) { Debug.LogWarning("Assigned transform is null", transform); return null; }
            //
            // Mixer Group
            var mixerGroup = GetMixerGroup(volumeType, transform);
            if (mixerGroup == null) return null;

            // Initiate new sound to play
            AudioClip soundToPlay = PickRandomClip(sounds, lastPlayedSound);

            // Get a free source (or create one)
            var source = GetOrCreateFreeSource(transform, maxAudioSources);

            // Set audio source properties
            ConfigureAudioSource(source, doLoop, doPlayOnAwake, volume, pitch, spatialBlend, reverbZoneMix, dopplerLevel);

            // Play the sound on the audio source
            AssignAndPlay(source, soundToPlay, mixerGroup);

            return soundToPlay;
        }

        /// <summary>
        /// Plays a random sound from a list on an AudioSource component attached to the given transform and
        /// returns the audio source used.
        /// You can specify the last played sound to avoid repeating it immediately.
        /// Creates a new audio source if none exist or all are playing.
        /// </summary>
        public static AudioSource SmartPlayListReturnSource(List<AudioClip> sounds, Transform transform, AudioClip lastPlayedSound = null, VolumeType volumeType = VolumeType.SFX,
            float volume = 1f, float pitch = 1f, bool doLoop = false, bool doPlayOnAwake = false, float spatialBlend = 0f, float reverbZoneMix = 0f, float dopplerLevel = 0f, int maxAudioSources = 30)
        {
            // Return if anything is invalid
            //
            // Sounds
            if (sounds == null || sounds.Count == 0) { Debug.LogWarning("Assigned sounds are null or empty", transform); return null; }
            //
            // Transform
            if (transform == null) { Debug.LogWarning("Assigned transform is null", transform); return null; }
            //
            // Mixer Group
            var mixerGroup = GetMixerGroup(volumeType, transform);
            if (mixerGroup == null) return null;

            // Initiate new sound to play
            AudioClip soundToPlay = PickRandomClip(sounds, lastPlayedSound);

            // Get a free source (or create one)
            var source = GetOrCreateFreeSource(transform, maxAudioSources);

            // Set audio source properties
            ConfigureAudioSource(source, doLoop, doPlayOnAwake, volume, pitch, spatialBlend, reverbZoneMix, dopplerLevel);

            // Play the sound on the audio source
            AssignAndPlay(source, soundToPlay, mixerGroup);

            return source;
        }

        /// <summary>
        /// Plays a sound on an AudioSource component attached to the given transform and
        /// returns the audio source used.
        /// Creates a new audio source if none exist or all are playing.
        /// </summary>
        public static AudioSource SmartPlayClip(AudioClip soundToPlay, Transform transform, VolumeType volumeType = VolumeType.SFX,
            float volume = 1f, float pitch = 1f, bool doLoop = false, bool doPlayOnAwake = false, float spatialBlend = 1f, float reverbZoneMix = 1f, float dopplerLevel = 0f, int maxAudioSources = 30)
        {
            // Return if anything is invalid
            //
            // Sounds
            if (soundToPlay == null) { Debug.LogWarning("Assigned sound is null", transform); return null; }
            //
            // Transform
            if (transform == null) { Debug.LogWarning("Assigned transform is null", transform); return null; }
            //
            // Mixer Group
            var mixerGroup = GetMixerGroup(volumeType, transform);
            if (mixerGroup == null) return null;

            // Get a free source (or create one)
            var source = GetOrCreateFreeSource(transform, maxAudioSources);

            // Set audio source properties
            ConfigureAudioSource(source, doLoop, doPlayOnAwake, volume, pitch, spatialBlend, reverbZoneMix, dopplerLevel);

            // Play the sound on the audio source
            AssignAndPlay(source, soundToPlay, mixerGroup);

            return source;
        }

        // ----------------------------------------
        // Internals
        // ----------------------------------------

        static AudioMixerGroup GetMixerGroup(VolumeType volumeType, Transform context)
        {
            if (AudioManager.Instance == null)
            {
                Debug.LogError("SmartPlay: AudioManager.Instance is null.", context);
                return null;
            }

            var mixerGroup = AudioManager.Instance.GetAudioMixerGroupByType(volumeType);
            if (mixerGroup == null)
            {
                Debug.LogError($"SmartPlay: Mixer group '{volumeType}' not found.", context);
                return null;
            }

            return mixerGroup;
        }

        static AudioClip PickRandomClip(List<AudioClip> sounds, AudioClip lastPlayedSound)
        {
            // Select a random sound from the list, ensuring it's not the same as the last played sound
            if (sounds.Count == 1)
                return sounds[0];

            AudioClip soundToPlay;
            do
            {
                soundToPlay = sounds[Random.Range(0, sounds.Count)];
            }
            while (soundToPlay == lastPlayedSound);

            return soundToPlay;
        }

        static AudioSource GetOrCreateFreeSource(Transform transform, int maxAudioSources = 30)
        {
            tempSources.Clear();
            transform.GetComponents(tempSources);

            for (int i = 0; i < tempSources.Count; i++)
            {
                var s = tempSources[i];
                if (!s.isPlaying)
                    return s;
            }

            if (maxAudioSources <= tempSources.Count)
            {
                // If we've reached the max number of audio sources and all are playing, just return the first one (lazy fallback)
                return tempSources[0];
            }

            return transform.gameObject.AddComponent<AudioSource>();
        }

        // Play a clip on a source via specified AudioMixerGroup
        static void AssignAndPlay(AudioSource source, AudioClip clip, AudioMixerGroup mixerGroup)
        {
            source.clip = clip;
            source.outputAudioMixerGroup = mixerGroup;
            source.Play();
        }

        // Default settings for AudioSources
        static void ConfigureAudioSource(AudioSource source, bool doLoop = false, bool doPlayOnAwake = false, float volume = 1f, float pitch = 1f, float spatialBlend = 1f, float reverbZoneMix = 1f, float dopplerLevel = 0f)
        {
            source.volume = volume;
            source.pitch = pitch;
            source.loop = doLoop;
            source.playOnAwake = doPlayOnAwake;
            source.spatialBlend = spatialBlend;
            source.reverbZoneMix = reverbZoneMix;
            source.dopplerLevel = dopplerLevel;

            //if (spatialBlend > 0f)
            //{
            //    source.rolloffMode = AudioRolloffMode.Linear;
            //}
        }
    }
}
