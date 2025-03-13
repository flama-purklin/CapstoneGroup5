using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class MinigameCanvasControl : MonoBehaviour
{
    [SerializeField] protected Animator anim;

    [SerializeField] protected EvidenceData assignedData;

    [Header("Canvas Elements")]
    [SerializeField] protected TMP_Text title;
    [SerializeField] protected TMP_Text description;
    [SerializeField] protected Image image;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //Function to fill in the success data from evidenceData into canvas
    protected void FillEvidence(EvidenceData data)
    {
        title.text = assignedData.evidenceTitle + " discovered!";
        description.text = assignedData.evidenceDescription;
        image.sprite = assignedData.evidenceArt;
    }
}
