using System.Collections;
using UnityEngine;

public enum GameState
{
    DEFAULT,
    DIALOGUE,
    PAUSE,
    FINAL,
    WIN,
    LOSE,
    MINIGAME,
    MYSTERY
}

public class GameControl : MonoBehaviour
{

    public static GameControl GameController;

    public GameState currentState;

    public Mystery coreMystery;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if (GameController == null)
        {
            GameController = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        //StartCoroutine(TimerUpdate());
    }

    // Update is called once per frame
    void Update()
    {

    }

    //OUTMODED - replace this basic logic with Jorge's timer function when that is finished
    IEnumerator TimerUpdate()
    {
        yield return new WaitForSeconds(300);
        Debug.Log("Final State Activated");
        currentState = GameState.FINAL;
    }
}
