using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Audio;
using TMPro;

public class MainMenu : MonoBehaviour
{
    public Animator animator;
    private string levelToLoad;
    public GameObject mainMenu;
    public GameObject optionsMenu;
    public AudioMixer audioMixer;
    public TMP_Dropdown resolutionDropdown;

    public Slider volumeSlider;
    public AudioSource uiAudioSource;
    public AudioClip hoverSound;
    public AudioClip clickSound;
    private float lastHoverTime = -1f;
    private float hoverCooldown = 0.1f;

    Resolution[] resolutions;

    void Start()
    {
        resolutions = Screen.resolutions;
        //resolutionDropdown.ClearOptions();
        List<string> options = new List<string>();

        int currentResolutionIndex=0;
        for(int i=0; i<resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height + " @ " + resolutions[i].refreshRateRatio +"hz";
            options.Add(option);

            if(resolutions[i].width == Screen.width && resolutions[i].height == Screen.height && resolutions[i].refreshRateRatio.Equals(Screen.currentResolution.refreshRateRatio))
            {
                currentResolutionIndex=i;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value=currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();

        if (resolutionDropdown == null)
    {
        Debug.LogError("NOTHING");
        return; // Exit Start() early to avoid crashing
    }
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }

    public void PlayHoverSound()
    {
        if (hoverSound != null && uiAudioSource != null && Time.time - lastHoverTime > hoverCooldown)
        {
            uiAudioSource.PlayOneShot(hoverSound);
            lastHoverTime = Time.time;
        }
    }

    public void PlayClickSound()
    {
        if (clickSound != null && uiAudioSource != null)
        {
            uiAudioSource.PlayOneShot(clickSound);
        }
    }
    public void PlayGame()
    {
        FadeToLevel("SystemsTest");  
    }

    public void OptionsPage()
    {
        mainMenu.SetActive(false);
        optionsMenu.SetActive(true);
        animator.SetTrigger("FadeIn");
    }

    public void Quit(){
        
        Application.Quit();
    }

    public void FadeToLevel(string levelName){
        levelToLoad=levelName;
        animator.SetTrigger("FadeOut");
    }

    public void OnFadeComplete(){
        SceneManager.LoadScene(levelToLoad);
    }

    public void SetVolume(float volume)
    {
        if(volume < 1) { volume = 0.001f; }
        RefreshSlider(volume);
        audioMixer.SetFloat("volume", Mathf.Log10(volume / 100) * 20f);
    }
    public void RefreshSlider(float value)
    {
        volumeSlider.value = value;
    }

    public void BackToMainMenu()
    {
        optionsMenu.SetActive(false);
        mainMenu.SetActive(true);
    }

    public void SetQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }
    
}
