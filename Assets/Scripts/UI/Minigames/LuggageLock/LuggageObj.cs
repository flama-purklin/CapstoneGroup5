using UnityEngine;

public class LuggageObj : MinigameObj
{
    string combination;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Awake()
    {
        base.Awake();
        NewCombination();
    }

    public override void Interact()
    {
        GameObject.FindFirstObjectByType<MinigameControl>().LuggageInit(combination);
        base.Interact();
    }

    public void NewCombination()
    {
        combination = Random.Range(0, 10).ToString() + Random.Range(0, 10).ToString() + Random.Range(0, 10).ToString();
    }
}
