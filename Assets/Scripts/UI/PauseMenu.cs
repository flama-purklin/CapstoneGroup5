using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.EventSystems; // Ensure input works in UI

public class PauseMenu : MonoBehaviour
{
    public Animator animator; // Reference to the fade screen Animator
    private string levelToLoad;
    public static bool GameIsPaused = false;
    public GameObject pauseMenuUI;
    public GameObject[] checkInactive;
    public AudioControl audioControl;

    //store the current game state here before pause
    GameState prevState;

    void Start()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f; 
        GameIsPaused = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if(GameIsPaused)
            {
                audioControl.PlaySFX_Select();
                Resume();
            }
            // Only pause when in DEFAULT state - allow ConstellationToggle to handle Escape key in MYSTERY state
            else if(GameControl.GameController.currentState == GameState.DEFAULT && CanPause())
            {
                audioControl.PlaySFX_Select();
                Pause();
            }
        }
    }

    private bool CanPause()//you can only pause if all game objects in the array are deactivated
    {
        for(int i = 0; i < checkInactive.Length; i++)
        {
            if (checkInactive[i].activeSelf) { return false; }
        }
        return true;
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        GameIsPaused = false;

        //game state adjustments
        GameControl.GameController.currentState = prevState;
    }

    public void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        GameIsPaused = true;
        
        //game state adjustments
        prevState = GameControl.GameController.currentState;
        GameControl.GameController.currentState = GameState.PAUSE;
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
