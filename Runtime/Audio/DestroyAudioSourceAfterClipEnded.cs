using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class DestroyAudioSourceAfterClipEnded : MonoBehaviour
{
    AudioSource audioSource;
    bool wasPlaying = false;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        audioSource.playOnAwake = false;
    }

    public void PlayAudioInDDOLScene(float delay = -1f)
    {
        transform.SetParent(null);
        DontDestroyOnLoad(audioSource);

        if (delay <= 0f)
        {
            audioSource.Play();
            wasPlaying = true;
        }
        else
        {
            StartCoroutine(PlayAudioAfterDelay(delay));
        }
    }

    void Update()
    {
        if (wasPlaying && !audioSource.isPlaying)
        {
            Destroy(gameObject);
        }
    }

    IEnumerator PlayAudioAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        audioSource.Play();
        wasPlaying = true;
    }
}
