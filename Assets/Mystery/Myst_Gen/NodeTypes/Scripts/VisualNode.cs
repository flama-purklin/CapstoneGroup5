
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;

public class VisualNode : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    //MysteryNode parsedNode;
    

    //assigned node
    public MysteryNode currentNode;
    public string nodeKey;

    //associated connections
    public List<GameObject> connections;

    //associated leads
    public List<MysteryLead> terminalLeads;
    public List<MysteryLead> storedLeads;

    //associated chars
    public List<string> charsRevealed;

    //all components of the visual object
    [Header("UI Components")]
    [SerializeField] protected TMP_Text idDisplay;
    [SerializeField] protected TMP_Text header;
    [SerializeField] protected TMP_Text subheader;
    [SerializeField] protected TMP_Text title;
    [SerializeField] protected GameObject iconPanel;
    [SerializeField] protected Image background;

    [Header("Variables and Prefabs")]
    [SerializeField] protected Color evidenceColor;
    [SerializeField] protected Color infoColor;
    [SerializeField] protected GameObject visualLead;
    [SerializeField] protected GameObject characterIndicator;


    //other object refs
    [SerializeField] Camera mysteryCam;
    NodeControl control;



    //movement shit


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        //connections = new List<GameObject>();
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AssignNode(string newKey, MysteryNode associatedNode)
    {
        //instantiate all lists
        connections = new List<GameObject>();
        terminalLeads = new List<MysteryLead>();
        storedLeads = new List<MysteryLead>();
        charsRevealed = new List<string>();

        control = GameObject.FindFirstObjectByType<NodeControl>();
        currentNode = associatedNode;
        nodeKey = newKey;
        
        //Debug.Log("Mystery Cam " + mysteryCam.name);
        

        UpdateInformation();
        //parent = transform.parent.GetComponent<RectTransformUtility>();
    }

    //call to enable the node and check whether connections also need to be unlocked
    public void DiscoverNode()
    {
        Debug.Log("Visual Node" + nodeKey + " Discovered");

        transform.localPosition = Random.insideUnitCircle * 300f;

        //might not be necessary anymore
        /*
        if (connections.Count > 0)
        {
            foreach (var connection in connections)
            {
                connection.GetComponent<Connection>().DiscoveryCheck();
            }
        }*/
        Debug.Log(terminalLeads.Count + " impostors among us");
        if (terminalLeads.Count > 0)
        {
            foreach (var lead in terminalLeads)
            {
                //unlock the lead if the revealing node is the same as the terminal node
                if (lead.Inside == lead.Terminal)
                {
                    lead.Discovered = true;
                }

                if (lead.Discovered)
                {
                    AddLeadVisual();
                }
            }
        }
    }

    public void AddLeadVisual()
    {
        GameObject newLead = Instantiate(visualLead, iconPanel.transform);
    }

    protected virtual void UpdateInformation()
    {
        // Debug.Log("Visual Node " + nodeKey + " Updated");

        //TODO - fix this and fill in the information stored in the currentNode
        header.text = currentNode.Type;
        subheader.text = currentNode.Subtype;
        title.text = currentNode.Title;
        idDisplay.text = GameControl.GameController.coreConstellation.Nodes.Keys.ToList().IndexOf(nodeKey).ToString();

        //update the color to match the type
        if (currentNode.Type == "EVIDENCE")
            background.color = evidenceColor;
        else
            background.color = infoColor;

        if (!currentNode.Discovered)
            gameObject.SetActive(false);
    }

    // Store the mouse position on click to determine if this is a click or drag
    private Vector2 pointerDownPos;
    private bool isDragging = false;
    private const float dragThreshold = 5f; // Pixels of movement before considered a drag
    
    public void OnPointerDown(PointerEventData pointEventData)
    {
        // Store the initial position to distinguish between click and drag
        pointerDownPos = pointEventData.position;
        isDragging = false;
        
        // Handle left clicks based on mode
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            if (control.theoryMode == TheoryMode.Addition)
            {
                // In addition mode, send the node to be connected
                control.NodeClick(this);
                
                // Provide visual feedback
                transform.localScale = new Vector3(1.1f, 1.1f, 1.1f); // Temporarily scale up
                StartCoroutine(ResetScale(0.2f)); // Reset after slight delay
            }
            else if (control.theoryMode == TheoryMode.Removal)
            {
                // Nothing happens when clicking on nodes in removal mode
                // But we could add a small animation to indicate the click was recognized
                transform.localScale = new Vector3(0.95f, 0.95f, 0.95f); // Slightly scale down
                StartCoroutine(ResetScale(0.1f)); // Reset after slight delay
            }
            else if (control.theoryMode == TheoryMode.Simulation)
            {
                // In simulation mode, left click to add this node to a theory
                // Similar to Addition mode behavior
                control.NodeClick(this);
                
                // Visual feedback indicates the node is part of simulation
                transform.localScale = new Vector3(1.1f, 1.1f, 1.1f); // Temporarily scale up
                StartCoroutine(ResetScale(0.2f)); // Reset after slight delay
            }
            // Normal mode left click deferred to OnPointerUp to distinguish from drag
        }
    }
    
    public void OnPointerUp(PointerEventData pointerEventData)
    {
        // Only handle as a click if we're not dragging
        if (!isDragging && Input.GetMouseButtonUp(0) && 
            control.theoryMode == TheoryMode.None &&
            Vector2.Distance(pointerDownPos, pointerEventData.position) < dragThreshold)
        {
            Debug.Log("Inspecting node via LEFT click: " + currentNode.Title);
            
            // Visual feedback for inspection
            background.color = new Color(
                background.color.r * 1.2f, 
                background.color.g * 1.2f, 
                background.color.b * 1.2f
            );
            
            // Show the inspection panel
            EvidenceInspect inspector = GameObject.FindFirstObjectByType<EvidenceInspect>();
            if (inspector != null)
            {
                inspector.ActivateInspect(this);
                
                // Reset color after a delay
                StartCoroutine(ResetColor(0.5f));
            }
        }
    }
    
    // Helper coroutines for visual feedback
    IEnumerator ResetScale(float delay)
    {
        yield return new WaitForSeconds(delay);
        transform.localScale = Vector3.one;
    }
    
    IEnumerator ResetColor(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // Only reset if we're still using the evidence type color
        if (currentNode.Type == "EVIDENCE")
            background.color = evidenceColor;
        else
            background.color = infoColor;
    }

    public void OnDrag(PointerEventData pointEventData)
    {
        // Once we exceed the drag threshold, mark as dragging to prevent OnPointerUp from treating as a click
        if (!isDragging && Vector2.Distance(pointerDownPos, pointEventData.position) > dragThreshold)
        {
            isDragging = true;
        }
        
        if (mysteryCam == null)
            mysteryCam = GameObject.FindWithTag("MysteryCam").GetComponent<Camera>();

        if (Input.GetKey(KeyCode.Mouse0) && 
            (control.theoryMode == TheoryMode.None || control.theoryMode == TheoryMode.Simulation))
        {
            Vector2 mousePos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                transform.parent as RectTransform, Input.mousePosition,
                        mysteryCam,
                                out mousePos);
            
            transform.localPosition = new Vector3(mousePos.x, mousePos.y, 0);
        }
    }
}
