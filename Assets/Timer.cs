using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.EventSystems; // Ensure input works in UI
using UnityEngine.SceneManagement;

public class Timer : MonoBehaviour
{
    public float timeRemaining = 60f;
    public bool timerRunning = false;
    public TextMeshProUGUI timerText;
    public GameObject gameOverPanel; // Assign in Inspector

    void Start()
    {
        timerRunning = true;
        gameOverPanel.SetActive(false); // Ensure "You Lose" screen is hidden at start
    }

    void Update()
    {
        if (timerRunning)
        {
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
                UpdateTimerDisplay(timeRemaining);
            }
            else
            {
                timeRemaining = 0;
                timerRunning = false;
                ShowGameOverScreen();
            }
        }
    }

    void UpdateTimerDisplay(float timeToDisplay)
    {
        int minutes = Mathf.FloorToInt(timeToDisplay / 60);
        int seconds = Mathf.FloorToInt(timeToDisplay % 60);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    void ShowGameOverScreen()
    {
        gameOverPanel.SetActive(true); // Show the "You Lose" screen
        Time.timeScale = 0f; // Pause game
    }

    public void RestartGame() // Attach this to Restart button
    {
        Time.timeScale = 1f; // Resume time
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Reload scene
    }
}
