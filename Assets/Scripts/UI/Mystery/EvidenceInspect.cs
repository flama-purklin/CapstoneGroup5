using System.Collections;
using TMPro;
using UnityEngine;
using System.Collections.Generic;

public class EvidenceInspect : MonoBehaviour
{
    VisualNode nodeObj;
    MysteryNode nodeInfo;

    [Header("Base Objects")]
    [SerializeField] Animator anim;
    [SerializeField] GameObject descriptionSection;
    [SerializeField] GameObject locationSection;
    [SerializeField] GameObject leadSection;
    [SerializeField] GameObject leadHolder;
    [SerializeField] GameObject charSection;
    [SerializeField] GameObject charHolder;

    [Header("Text Objects")]
    [SerializeField] TMP_Text header;
    [SerializeField] TMP_Text subheader;
    [SerializeField] TMP_Text description;
    [SerializeField] TMP_Text location;
    [SerializeField] TMP_Text leadTitle;

    [Header("Prefabs")]
    [SerializeField] GameObject leadPrefab;
    [SerializeField] GameObject characterPrefab;

    List<GameObject> currentLeads = new List<GameObject>();
    List<GameObject> currentChars = new List<GameObject>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ActivateInspect(VisualNode node)
    {
        nodeObj = node;
        nodeInfo = node.currentNode;
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
        header.text = nodeInfo.Title;
        subheader.text = nodeInfo.Type;
        description.text = nodeInfo.Description;

        //deal with additional data if it is evidence
        if (nodeInfo.Type == "EVIDENCE")
        {
            //subtype
            subheader.text += " - " + nodeInfo.Subtype;

            //location
            locationSection.SetActive(true);
            //TODO - replace this with the name of the car rather than the id
            string carID = nodeInfo.CarId;
            string carName = GameControl.GameController.coreMystery.Environment.Cars[carID].Name;
            location.text = carName;
        }
        else
        {
            //subtype

            //location
            locationSection.SetActive(false);
        }

        //leads
        StartCoroutine(LeadAttach());

        //characters
        StartCoroutine(CharacterAttach());
    }

    IEnumerator CharacterAttach()
    {
        //disable if there is nothing to attach
        if (nodeObj.charsRevealed.Count > 0)
        {
            charSection.SetActive(true);
            //lead title
            //leadCharTitle.text = "Relevant Characters";

            ClearCharPanel();

            //add all revealed characters to the detailed view here
            foreach (string charName in nodeObj.charsRevealed)
            {
                GameObject newChar = Instantiate(characterPrefab, charHolder.transform);
                currentChars.Add(newChar);

                //fill the text in the lead
                newChar.GetComponentInChildren<TMP_Text>().text = charName;
            }
        }
        else
        {
            charSection.SetActive(false);
        }

        //add all revealed characters to the detailed view here
        yield return null;
    }

    IEnumerator LeadAttach()
    {
        //disable if there is nothing to attach
        if (nodeObj.terminalLeads.Count > 0)
        {
            leadSection.SetActive(true);
            //lead title
            //leadCharTitle.text = "Relevant Leads";

            ClearLeadPanel();

            //add all revealed leads to the detailed view here
            foreach (MysteryLead lead in nodeObj.terminalLeads)
            {
                if (lead.Discovered)
                {
                    GameObject newLead = Instantiate(leadPrefab, leadHolder.transform);
                    currentLeads.Add(newLead);

                    //fill the text in the lead
                    string questionText = null;
                    if (lead.Solved)
                        questionText = "<s>";
                    questionText += lead.Question;
                    newLead.GetComponentInChildren<TMP_Text>().text = questionText;
                }
            }
        }
        else
        {
            leadSection.SetActive(false);
        }


        yield return null;
    }

    public void ClearLeadPanel()
    {
        for (int i = currentLeads.Count - 1; i >= 0; i--)
        {
            Destroy(currentLeads[i]);
            currentLeads.RemoveAt(i);
        }
    }

    public void ClearCharPanel()
    {
        for (int i = currentChars.Count - 1; i >= 0; i--)
        {
            Destroy(currentChars[i]);
            currentChars.RemoveAt(i);
        }
    }
}
