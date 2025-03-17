using TMPro;
using UnityEngine;

public class EvidenceInspect : MonoBehaviour
{
    MysteryNode currentNode;

    [Header("Base Objects")]
    [SerializeField] Animator anim;
    [SerializeField] GameObject descriptionSection;
    [SerializeField] GameObject locationSection;
    [SerializeField] GameObject timeSection;
    [SerializeField] GameObject characterSection;
    [SerializeField] GameObject hiddenDetailsSection;

    [Header("Text Objects")]
    [SerializeField] TMP_Text type;
    [SerializeField] TMP_Text category;
    [SerializeField] TMP_Text content;
    [SerializeField] TMP_Text description;
    [SerializeField] TMP_Text location;
    [SerializeField] TMP_Text time;
    [SerializeField] TMP_Text characters;
    [SerializeField] TMP_Text hiddenDetails;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ActivateInspect(MysteryNode node)
    {
        Debug.Log("Activate Recieved");
        currentNode = node;
        anim.SetTrigger("activate");
        anim.SetBool("active", true);
    }

    public void DeactivateInspect()
    {
        anim.SetTrigger("activate");
        anim.SetBool("active", false);
    }

    public void FillInspect()
    {
        type.text = currentNode.Type;
        category.text = currentNode.Category;
        content.text = currentNode.Content;
        
        //check description
        if (string.IsNullOrEmpty(currentNode.Description))
        {
            descriptionSection.SetActive(false);
        }
        else
        {
            descriptionSection.SetActive(true);
            description.text = currentNode.Description;
        }

        //check location
        if (string.IsNullOrEmpty(currentNode.Location))
        {
            locationSection.SetActive(false);
        }
        else
        {
            locationSection.SetActive(true);
            location.text = currentNode.Location;
        }

        //check time
        if (currentNode.Time == null)
        {
            timeSection.SetActive(false);
        }
        else
        {
            timeSection.SetActive(true);
            time.text = currentNode.Time.ToString();
        }

        //check associated characters
        if (currentNode.Characters == null)
        {
            characterSection.SetActive(false);
        }
        else
        {
            characterSection.SetActive(true);
            characters.text = "";
            foreach (var character in currentNode.Characters)
            {
                characters.text += character.ToString() + "\n"; 
            }
        }

        //check hidden details
        if (currentNode.HiddenDetails == null)
        {
            hiddenDetailsSection.SetActive(false);
        }
        else
        {
            hiddenDetailsSection.SetActive(true);
            hiddenDetails.text = "";
            foreach (var detail in currentNode.HiddenDetails)
            {
                hiddenDetails.text += detail.ToString() + "\n";
            }
        }
    }
}
