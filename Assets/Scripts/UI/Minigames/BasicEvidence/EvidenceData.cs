using UnityEngine;

[CreateAssetMenu(fileName = "evidenceData", menuName = "ScriptableObjects/EvidenceData", order = 1)]
public class EvidenceData : ScriptableObject
{
    public Sprite evidenceArt;
    //maybe a 3d model here if we go that route
    public string evidenceTitle;
    public string evidenceDescription;
    public string nodeKey;
}
