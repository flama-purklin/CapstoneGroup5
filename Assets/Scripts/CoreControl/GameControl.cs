using UnityEngine;

public enum GameState
{
    DEFAULT,
    DIALOGUE
}

public class GameControl : MonoBehaviour
{

    public static GameControl GameController;

    public GameState currentState;
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
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
