using UnityEngine;

public class MysteryGenRunner : MonoBehaviour
{
    // This is a legacy test mystery generator, disabled to prevent interference
    // with the primary mystery generation pipeline using transformed-mystery.json
    [SerializeField] private bool useTestGenerator = false;
    
    void Start()
    {
        // Skip legacy mystery generation unless explicitly enabled
        if (!useTestGenerator)
        {
            Debug.Log("MysteryGenRunner: Test generator is disabled. Using only the standard mystery pipeline.");
            return;
        }
        
        Debug.Log("MysteryGenRunner: Warning - Test generator is enabled. This will create a secondary test mystery.");
        MysteryGenBeta mysteryGen = new MysteryGenBeta();
        mysteryGen.GenerateMystery();
    }
}