using UnityEngine;

public class MinigameControl : MonoBehaviour
{
    //Minigame References
    [SerializeField] LuggageControl luggage;
    [SerializeField] EvidenceControl evidence;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //for debugging purposes
        if (GameControl.GameController.currentState == GameState.DEFAULT && Input.GetKeyDown(KeyCode.T))
            LuggageInit("123");
    }

    public void LuggageInit(string combination)
    {
        luggage.gameObject.SetActive(true);
        luggage.SetCombo(combination);
    }

    //create a serializableObj for each piece of evidence, pass this instead of mysterynode
    public void EvidenceReveal(EvidenceData revealedData)
    {
        evidence.gameObject.SetActive(true);
        evidence.Reveal(revealedData);
    }
}
