using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Audio;

public class MainMenuMusic : MonoBehaviour
{
    public static MainMenuMusic instance;
    public AudioSource audioSource;
    public float fadeDuration = 1.5f;

    public AudioMixer audioMixer;

    void Awake()
    {
        // Singleton pattern to keep music playing across scenes
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        if (audioSource && !audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }

    public void OnButtonClick()
    {
        StartCoroutine(FadeOutMusic());
    }

    IEnumerator FadeOutMusic()
    {
        float startVolume = audioSource.volume;

        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            audioSource.volume = Mathf.Lerp(startVolume, 0, t / fadeDuration);
            yield return null;
        }

        audioSource.volume = 0;
        // Instead of stopping, just lower the volume so it can fade back in later
    }

    public void FadeInMusic()
    {
        StartCoroutine(FadeInMusicCoroutine());
    }

    IEnumerator FadeInMusicCoroutine()
    {
        float targetVolume = 1.0f;
        float startVolume = audioSource.volume;

        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            audioSource.volume = Mathf.Lerp(startVolume, targetVolume, t / fadeDuration);
            yield return null;
        }

        audioSource.volume = targetVolume;
    }
}
