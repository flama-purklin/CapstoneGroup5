using UnityEngine;

public class LuggageObj : MinigameObj
{
    [SerializeField] public string combination;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Awake()
    {
        base.Awake();
        if (combination == null) { NewCombination(); }
    }

    public override void Interact()
    {
        GameObject.FindFirstObjectByType<MinigameControl>().LuggageInit(combination);
        base.Interact();
    }

    //TO Did - should be taken from the barrier evidence node, assigned at runtime when the prefab is instantitated and placed in the train
    public void NewCombination()
    {
        //random
        //combination = Random.Range(0, 10).ToString() + Random.Range(0, 10).ToString() + Random.Range(0, 10).ToString();

        //hardset for demo
        combination = "0451";
    }
}
