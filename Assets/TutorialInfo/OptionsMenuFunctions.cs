using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Audio;

public class OptionsMenuFunctions : MonoBehaviour
{
    public AudioMixer audioMixer;
    
    public void SetVolume(float volume)
    {
        audioMixer.SetFloat("volume", volume);

    }
}
