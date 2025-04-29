using Unity.VisualScripting;
using UnityEngine;

public class EvidenceObj : MinigameObj
{
    //MysteryNode associatedNode;

    //serializable object with a predetermined set of information to reveal to the player on reveal
    [SerializeField] public EvidenceData data;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Awake()
    {
        base.Awake();
        nodeKey = data.nodeKey;
    }

    public override void Interact()
    {
        base.Interact();
        GameObject.FindFirstObjectByType<MinigameControl>().EvidenceReveal(data);
    }
}
