using UnityEngine;

public class AudioControl : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip sfx_enter;
    public AudioClip sfx_select;
    public AudioClip sfx_exit;
    public AudioClip sfx_hover;
    public AudioClip sfx_click;
    public AudioClip[] sfx_footsteps;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void RandomizePitch(float f = 0.1f)
    {
        audioSource.volume = 0.65f + Random.Range(0f, 0.05f);
        audioSource.pitch = 1f - (f/2) + Random.Range(0f, f);
    }
    public void PlaySFX_Enter()
    {
        RandomizePitch();
        audioSource.PlayOneShot(sfx_enter);
    }
    public void PlaySFX_Select()
    {
        RandomizePitch();
        audioSource.PlayOneShot(sfx_select);
    }
    public void PlaySFX_Exit()
    {
        RandomizePitch();
        audioSource.PlayOneShot(sfx_exit);
    }
    public void PlaySFX_Hover()
    {
        RandomizePitch();
        audioSource.PlayOneShot(sfx_hover);
    }
    public void PlaySFX_Click()
    {
        RandomizePitch();
        audioSource.PlayOneShot(sfx_click);
    }
    public void PlaySFX_Footstep()
    {
        RandomizePitch(0.4f);
        audioSource.PlayOneShot(sfx_footsteps[Random.Range(0, sfx_footsteps.Length)]);
    }
}