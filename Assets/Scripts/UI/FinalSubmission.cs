using LLMUnity;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Events;

public class FinalSubmission : MonoBehaviour
{
    [SerializeField] GameObject finalCanvas;

    [SerializeField] Animator anim;

    [SerializeField] GameObject buttonHolder;

    [SerializeField] GameObject buttonPrefab;

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
        StartCoroutine(CharButtonSpawn());
        yield return null;
    }

    //used to fill in all possible characters with a button - selecting one will end the game
    IEnumerator CharButtonSpawn()
    {
        //retrieve all instances of LLMChar
        GameObject[] allChars = GameObject.FindGameObjectsWithTag("Character");

        foreach (var character in GameControl.GameController.coreMystery.Characters)
        {
            /*//get the characterllm component
            LLMCharacter llmChar = character.GetComponentInChildren<LLMCharacter>();
            */

            GameObject newButton = Instantiate(buttonPrefab);
            newButton.transform.SetParent(buttonHolder.transform, false);

            //set char text to be the name of the character
            newButton.GetComponentInChildren<TMP_Text>().text = character.Value.MindEngine.Identity.Name;

            //attach a listener to each button
            newButton.GetComponent<Button>().onClick.AddListener(() => FinishGame(character.Key));
        }


        yield return null;
    }


    //This function will check whether the player has chosen correctly and won or not
    public void FinishGame(string charTag)
    {
        //TODO - replace the rng with actual mechanics for checking the game win state (requires the elements of mystery gen as objects that can be called upon)

        if (charTag != GameControl.GameController.coreMystery.Core.Culprit)
        {
            GameControl.GameController.currentState = GameState.LOSE;
            PlayerPrefs.SetInt("gameWin", 0);
        }
        else
        {
            GameControl.GameController.currentState = GameState.WIN;
            PlayerPrefs.SetInt("gameWin", 1);
        }
            
        LoadResultScreen();
    }

    private void LoadResultScreen()
    {
        SceneManager.LoadScene("ResultScreen");
    }
}
