using UnityEngine;

public class ResultsControl : MonoBehaviour
{
    [SerializeField] GameObject winHolder;
    [SerializeField] GameObject lossHolder;


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
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
