using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public Animator animator; // Reference to the fade screen Animator
    private string levelToLoad;
    public static bool GameIsPaused = false;
    public GameObject pauseMenuUI;

    void Start()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f; 
        GameIsPaused = false;
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if(GameIsPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        GameIsPaused = false;
    }

    public void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        GameIsPaused = true;
    }

    public void LoadMenu()
    {
        Time.timeScale=1f;
        FadeToLevel("MainMenu");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
    public void BackFromOptionsPage()
    {
        FadeToLevel("MainMenu");
    }

    public void FadeToLevel(string levelName)
    {
        levelToLoad = levelName;
        animator.SetTrigger("FadeOut"); // Triggers the fade-to-black animation
    }

    // This function will be called via an Animation Event when the fade is complete
    public void OnFadeComplete()
    {
        SceneManager.LoadScene(levelToLoad);
    }
}
