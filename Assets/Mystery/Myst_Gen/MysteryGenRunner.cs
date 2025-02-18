using UnityEngine;

public class MysteryGenRunner : MonoBehaviour
{
    void Start()
    {
        MysteryGenBeta mysteryGen = new MysteryGenBeta();
        mysteryGen.GenerateMystery();
    }
}