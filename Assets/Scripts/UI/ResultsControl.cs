using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ResultsControl : MonoBehaviour
{
    [SerializeField] GameObject winHolder;
    [SerializeField] GameObject lossHolder;
    [SerializeField] TMP_Text confidenceScore;
    [SerializeField] Button returnButton;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if (PlayerPrefs.GetInt("gameWin") == 1)
        {
            
            winHolder.SetActive(true);
            lossHolder.SetActive(false);
            Debug.Log("Player Won!");
        }
        else
        {
            winHolder.SetActive(false);
            lossHolder.SetActive(true);
            Debug.Log("Player Lost!");
        }

        //set the confidence score
        confidenceScore.text = GameControl.GameController.coreConstellation.ConfidenceScore().ToString("P");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ReturnToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
