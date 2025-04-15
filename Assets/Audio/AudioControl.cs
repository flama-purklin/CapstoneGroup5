using UnityEngine;

public class AudioControl : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip sfx_enter;
    public AudioClip sfx_select;
    public AudioClip sfx_exit;
    public AudioClip sfx_hover;
    public AudioClip sfx_click;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void RandomizePitch()
    {
        audioSource.volume = 0.65f + Random.Range(0f, 0.05f);
        audioSource.pitch = 0.95f + Random.Range(0f, 0.1f);
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
}