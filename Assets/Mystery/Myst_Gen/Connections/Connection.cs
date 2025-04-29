using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Connection : MonoBehaviour
{
    [Header("UI Attributes")]
    [SerializeField] protected Image visualConn;
    [SerializeField] protected RectTransform rect;
    [SerializeField] protected Animator animControl;

    [Header("Object Refs")]
    public GameObject leadObj;
    public GameObject answerObj;

    //MysteryLead ref
    public MysteryLead leadInfo;

    [Header("Data")]
    //public string type;
    public bool discovered = false;
    public bool confirmed = false;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    public virtual void Update()
    {
        // Debug.Log("Updating");
        if (discovered)
            StartCoroutine(ConnectionUpdate());
    }

    void FixedUpdate()
    {
        //Debug.Log("FixedUpdating");

    }

    //associated node objects are passed from NodeControl
    public void ConnectionSpawn(GameObject origin, GameObject result, MysteryLead newLead)
    {
        leadObj = origin;
        answerObj = result;
        leadInfo = newLead;

        //assign the length to the connection obj
        //rect = GetComponent<RectTransform>();
        Vector2 hypotenuse = answerObj.transform.localPosition - leadObj.transform.localPosition;
        float dist = hypotenuse.magnitude;
        rect.sizeDelta = new Vector2(dist, 20f);

        //assign the position
        transform.localPosition = Vector2.Lerp(origin.transform.localPosition, result.transform.localPosition, 0.5f);

        //calculate and apply the rotation
        float x = hypotenuse.x;
        float y = hypotenuse.y;
        float theta = 2 * Mathf.PI + Mathf.Atan2(y, -x);
        //Debug.Log(x + " " + y + " " + theta);

        Vector2 direction = new Vector2(Mathf.Sin(theta), Mathf.Cos(theta));

        transform.localRotation = Quaternion.LookRotation(Vector3.forward, direction);

        //send to back
        transform.SetSiblingIndex(0);

        //connectionDesc.text = type;

        //Debug.Log("new connection successfully created");
        DiscoveryCheck();
    }

    protected IEnumerator ConnectionUpdate()
    {
        //assign the length to the connection obj
        //rect = GetComponent<RectTransform>();
        Vector2 hypotenuse = leadObj.transform.localPosition - answerObj.transform.localPosition;
        float dist = hypotenuse.magnitude;
        rect.sizeDelta = new Vector2(dist, 20f);

        //assign the position
        transform.localPosition = Vector2.Lerp(leadObj.transform.localPosition, answerObj.transform.localPosition, 0.5f);

        //calculate and apply the rotation
        float x = hypotenuse.x;
        float y = hypotenuse.y;
        float theta = 2 * Mathf.PI + Mathf.Atan2(y, -x);
        //Debug.Log(x + " " + y + " " + theta);

        Vector2 direction = new Vector2(Mathf.Sin(theta), Mathf.Cos(theta));

        transform.localRotation = Quaternion.LookRotation(Vector3.forward, direction);

        //Debug.Log("Updated connection");
        yield return null;
    }

    public void DiscoveryCheck()
    {
        Debug.Log("Checking Connection Discovery");
        //additional checks may be needed: GameControl.GameController.coreConstellation.Nodes[leadInfo.Terminal].Discovered
        //&& GameControl.GameController.coreConstellation.Nodes[leadInfo.Answer].Discovered
        //&&
        if (GameControl.GameController.coreConstellation.Nodes[leadInfo.Terminal].Discovered
        && GameControl.GameController.coreConstellation.Nodes[leadInfo.Answer].Discovered
        && confirmed)
        {

            visualConn.enabled = true;
            transform.SetAsFirstSibling();
            discovered = true;
            leadInfo.Solved = true;

            //check for any new leads to reveal
            NewLeadCheck();
            Debug.Log("DiscoveryCheck successful");

            //increment the currentmysterycount
            GameControl.GameController.coreConstellation.currentMysteryCount++;
        }
        else
        {
            visualConn.enabled = false;
            discovered = false;
            Debug.Log("DiscoveryCheck unsuccessful");
        }

        //add a clause here for a connection reveal without player simulation
    }

    public void NewLeadCheck()
    {

        for (int i = 0; i < answerObj.GetComponent<VisualNode>().storedLeads.Count; i++)
        {
            if (!answerObj.GetComponent<VisualNode>().storedLeads[i].Discovered)
            {
                answerObj.GetComponent<VisualNode>().storedLeads[i].Discovered = true;

                //add a visual indicator to the terminal node here - needs to go through NodeControl since that's where the terminals are stored
                GameObject.FindFirstObjectByType<NodeControl>().LeadReveal(answerObj.GetComponent<VisualNode>().storedLeads[i].Terminal);
            }
            else
                Debug.Log("Node was already discovered");

        }
    }
}