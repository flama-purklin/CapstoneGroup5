using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FinalSubmission : MonoBehaviour
{
    [SerializeField] GameObject finalCanvas;

    [SerializeField] Animator anim;

    bool activated = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        finalCanvas.SetActive(false);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (GameControl.GameController.currentState == GameState.FINAL && activated == false)
        {
            activated = true;
            StartCoroutine(ActivateFinalSub());
        }
    }

    IEnumerator ActivateFinalSub()
    {
        finalCanvas.SetActive(true);
        anim.Play("FinalSubActivate");
        yield return null;
    }


    //This function will check whether the player has chosen correctly and won or not
    public void FinishGame()
    {
        //TODO - replace the rng with actual mechanics for checking the game win state (requires the elements of mystery gen as objects that can be called upon)

        //for now, just randomly select either win or loss on submission press
        int selectedState = Random.Range(0, 2);
        if (selectedState == 0)
            GameControl.GameController.currentState = GameState.LOSE;
        else
            GameControl.GameController.currentState = GameState.WIN;

        LoadResultScreen();
    }

    private void LoadResultScreen()
    {
        SceneManager.LoadScene("ResultScreen");
    }
}
