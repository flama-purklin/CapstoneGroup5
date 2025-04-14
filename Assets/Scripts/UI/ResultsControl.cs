using TMPro;
using UnityEngine;

public class ResultsControl : MonoBehaviour
{
    [SerializeField] GameObject winHolder;
    [SerializeField] GameObject lossHolder;
    [SerializeField] TMP_Text confidenceScore;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (GameControl.GameController.currentState == GameState.WIN)
        {
            winHolder.SetActive(true);
            lossHolder.SetActive(false);
        }
        else if (GameControl.GameController.currentState == GameState.LOSE)
        {
            winHolder.SetActive(false);
            lossHolder.SetActive(true);
        }

        //set the confidence score
        confidenceScore.text = GameControl.GameController.coreConstellation.ConfidenceScore().ToString("P");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
