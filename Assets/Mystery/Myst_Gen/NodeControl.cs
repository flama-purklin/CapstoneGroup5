using System.Collections.Generic;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum TheoryMode
{
    None,
    Addition,
    Removal,
    Simulation
}

public class NodeControl : MonoBehaviour
{
    //DO NOT USE THESE
    public List<Node> allNodes;
    public List<GameObject> visualNodeStorage;


    private RectTransform canvasRect;

    //new Visual Node storage with parsed MysteryNode hookup
    public Dictionary<string, GameObject> visualNodes;
    public Dictionary<string, GameObject> leadConnections;

    [Header("Prefabs")]
    //[SerializeField] GameObject infoPrefab;
    //[SerializeField] GameObject evidencePrefab;
    [SerializeField] GameObject connectionPrefab;
    [SerializeField] GameObject nodePrefab;
    [SerializeField] GameObject theoryPrefab;

    [Header("UI Elements")]
    [SerializeField] RectTransform contentPanel;
    [SerializeField] TMP_Text instructions;
    [SerializeField] Button addTheoryButton;
    [SerializeField] Button removeTheoryButton;
    [SerializeField] Button simButton;
    [SerializeField] TMP_Text theoryNotif;
    [SerializeField] TMP_Text confidenceScore;
    [SerializeField] TMP_Text simulationDisplayAmt;
    
    [Header("Simulation UI Elements")]
    [SerializeField] GameObject simulationIcon; // Icon shown during simulation mode
    [SerializeField] Image darkOverlay; // Overlay for darkening the board during simulation
    [SerializeField] float darkOverlayAlpha = 0.5f; // How dark the overlay gets
    [SerializeField] float darkTransitionDuration = 0.3f; // How fast the darkening happens

    [Header("Navigation Vars")]
    [SerializeField] float scrollSpeed = 2000f;
    [SerializeField] float minY = -2000f;//adjust to something sensible later
    [SerializeField] float maxY = 2000f;

    bool loaded = false;

    //theory stuff
    public TheoryMode theoryMode = TheoryMode.None;
    public GameObject currentTheory;
    public List<GameObject> untestedTheories;
    Coroutine currentNotif;
    private float simulationCost;

    string baseInstructions;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        contentPanel.anchoredPosition = Vector2.zero;
        if (!loaded)
        {
            NewConstellation();
        }
        baseInstructions = instructions.text;

        //instantiate theory list
        untestedTheories = new List<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        // Handle middle mouse panning (new navigation option)
        HandleMiddleMousePan();

        if (theoryMode != TheoryMode.None && Input.GetMouseButtonDown(1))
        {
            if (theoryMode == TheoryMode.Addition)
            {
                TheoryAdd();
            }
            else if (theoryMode == TheoryMode.Removal)
            {
                TheoryRemove();
            }
        }

        if (confidenceScore.isActiveAndEnabled)
        {
            confidenceScore.text = (GameControl.GameController.coreConstellation.ConfidenceScore()).ToString("P");
        }

        if (simulationDisplayAmt.isActiveAndEnabled)
        {
            simulationDisplayAmt.text = "-" + simulationCost.ToString("P");
        }

        if (Input.GetKeyDown(KeyCode.F10))
        {
            StartCoroutine(UnlockAllNodes());
        }
    }

    public void MapEnabled()
    {
        StartCoroutine(SimulationCalc());
    }

    // Removed vertical scroll handling as it conflicts with zoom functionality
    // Added Middle Mouse Button panning for more intuitive navigation
    void HandleMiddleMousePan()
    {
        if (Input.GetMouseButton(2) && contentPanel != null) // Middle mouse button
        {
            Vector2 mouseDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")) * scrollSpeed * Time.deltaTime;
            Vector2 newPos = contentPanel.anchoredPosition + mouseDelta;
            newPos.x = Mathf.Clamp(newPos.x, -2000f, 2000f); // Add horizontal clamping
            newPos.y = Mathf.Clamp(newPos.y, minY, maxY);
            contentPanel.anchoredPosition = newPos;
        }
    }

    //maybe inefficient way of getting node levels
    int GetNodeLevel(Node node)
    {
        if (node.requirement == null || node.requirement.Length == 0)
            return 0;
        int maxLevel = 0;
        foreach (var req in node.requirement)
        {
            int level = GetNodeLevel(req);
            if (level > maxLevel)
                maxLevel = level;
        }
        return maxLevel + 1;
    }

    /*recieves the nodes from the MysteryGen
    public void CreateConstellation(List<Node> generatedNodes)
    {
        allNodes = generatedNodes;

        //instantiate visualNodeStorage (used for quick retrieval of nodes for connections)
        visualNodeStorage = new List<GameObject>();

        //node spacing
        float verticalSpacing = 320f;
        float canvasWidth = 1920f;

        //determine a "level" for each node (0 for starting nodes, increasing upward)
        Dictionary<Node, int> levels = new Dictionary<Node, int>();
        foreach (var node in generatedNodes)
        {
            levels[node] = GetNodeLevel(node);
        }

        //group nodes by level
        Dictionary<int, List<Node>> nodesByLevel = new Dictionary<int, List<Node>>();
        foreach (var kvp in levels)
        {
            if (!nodesByLevel.ContainsKey(kvp.Value))
                nodesByLevel[kvp.Value] = new List<Node>();
            nodesByLevel[kvp.Value].Add(kvp.Key);
        }

        //begins generation of all nodes
        float totalHeight = 0f;
        foreach (var level in nodesByLevel.Keys)
        {
            List<Node> levelNodes = nodesByLevel[level];
            int count = levelNodes.Count;
            for (int i = 0; i < count; i++)
            {
                CreateVisualNode(levelNodes[i], (canvasWidth / (count + 1) * (i + 1)) - (canvasWidth / 2f), 300f - (verticalSpacing * level));
                totalHeight = Mathf.Max(totalHeight, Mathf.Abs(verticalSpacing * level));
            }
        }

        //adjust content panel size based on nodes
        contentPanel.sizeDelta = new Vector2(contentPanel.sizeDelta.x, totalHeight + 300f);

        //generate links between nodes
        Debug.Log("Now generating links");
        foreach (Node node in allNodes)
        {
            CheckConnections(node);
        }
    }*/

    public void NewConstellation()
    {
        visualNodes = new Dictionary<string, GameObject>();
        leadConnections = new Dictionary<string, GameObject>();

        GameControl.GameController.coreConstellation.CompleteMysteryCalc();

        //replace this if the first node has to be discovered, necessary to make it visible to player on constellation begin
        bool firstNode = true;
        string firstNodeKey = null;

        //create a node object for every parsed node in the constellation
        foreach (var nodePair in GameControl.GameController.coreConstellation.Nodes)
        {
            //create the node
            GameObject newNode = Instantiate(nodePrefab, contentPanel);

            //assign data to the node
            newNode.GetComponent<VisualNode>().AssignNode(nodePair.Key, nodePair.Value);

            //store the node in the dictionary for easy access
            visualNodes.Add(nodePair.Key, newNode);

            //if first node, make discovered
            if (firstNode)
            {
                firstNode = false;
                firstNodeKey = nodePair.Key;
            }
        }

        Debug.Log(visualNodes.Count + " visual nodes created");

        //TOOD - create a connection for each parsed lead in the constellation
        List<MysteryLead> leads = GameControl.GameController.coreConstellation.Leads;
        foreach (var lead in leads)
        {
            //store the lead to the associated terminal node
            visualNodes[lead.Terminal].GetComponent<VisualNode>().terminalLeads.Add(lead);

            //store the lead to the associated storage node
            visualNodes[lead.Inside].GetComponent<VisualNode>().storedLeads.Add(lead);

            //create a connection for each
            GameObject newConn = Instantiate(connectionPrefab, contentPanel);
            newConn.GetComponent<Connection>().ConnectionSpawn(visualNodes[lead.Terminal], visualNodes[lead.Answer], lead);
            visualNodes[lead.Terminal].GetComponent<VisualNode>().connections.Add(newConn);
            visualNodes[lead.Answer].GetComponent<VisualNode>().connections.Add(newConn);

            //add to the leadConn dict
            leadConnections.Add(lead.Id, newConn);
        }

        //discover the first node - necessary after leads have been assigned for the sake of revealing the lead
        GameControl.GameController.coreConstellation.DiscoverNode(firstNodeKey);

        Debug.Log(GameControl.GameController.coreConstellation.Leads.Count + " connections have been parsed");
    }

    public void UnlockVisualNode(string nodeKey)
    {
        if (!visualNodes[nodeKey].activeInHierarchy)
        {
            GameObject unlockedNode = visualNodes[nodeKey];
            unlockedNode.SetActive(true);
            unlockedNode.GetComponent<VisualNode>().DiscoverNode();

            //increment the currentmysterycount
            GameControl.GameController.coreConstellation.currentMysteryCount++;

            StartCoroutine(AutoConnect(nodeKey));
        }
        else
            Debug.Log("Visual Node already discovered");
        
    }

    IEnumerator AutoConnect(string nodeKey)
    {
        //store all connections where the unlocked node is needed as an answer
        List<string> requiredConns = new List<string>();
        foreach (var lead in leadConnections)
        {
            if (lead.Value.GetComponent<Connection>().leadInfo.Answer == nodeKey)
            {
                requiredConns.Add(lead.Value.GetComponent<Connection>().leadInfo.Inside);
            }
            yield return new WaitForEndOfFrame();
        }

        //unlock all leads/connections higher one higher in the hierarchy than that node
        foreach (var lead in leadConnections)
        {
            foreach (var prevNode in requiredConns)
            {
                if (lead.Value.GetComponent<Connection>().leadInfo.Answer == prevNode)
                {
                    lead.Value.GetComponent<Connection>().confirmed = true;
                    lead.Value.GetComponent<Connection>().DiscoveryCheck();
                    Debug.Log("Upstream Lead Unlocked!");
                }
                yield return new WaitForEndOfFrame();
            }
            yield return new WaitForEndOfFrame();
        }
            
    }

    /*public void CreateVisualNode(Node node)
    {
        //create the correct corresponding visual node
        GameObject newNode;
        if (node is InfoNode)
        {
            newNode = Instantiate(infoPrefab);
        }
        else if (node is EvidenceNode)
        {
            newNode = Instantiate(evidencePrefab);
        }
        else
        {
            newNode = null;
            Debug.Log("Error - node type not yet supported");
        }

        //assign as a child of the canvas
        newNode.transform.SetParent(transform, false);
        newNode.transform.localPosition = new Vector2(Random.Range(-500, 500), Random.Range(-500, 500));

        //assign the node to the visual node, fill it in
        newNode.GetComponent<VisualNode>().AssignNode(node);

        //store in the storage for easy access when making connections
        visualNodeStorage.Add(newNode);

        Debug.Log("Node " + node.id + " successfully created");
    }

    public void CreateVisualNode(Node node, float xPos, float yPos)
    {
        //create the correct corresponding visual node
        GameObject newNode;
        if (node is InfoNode)
        {
            newNode = Instantiate(infoPrefab, contentPanel);
        }
        else if (node is EvidenceNode)
        {
            newNode = Instantiate(evidencePrefab, contentPanel);
        }
        else
        {
            newNode = null;
            Debug.Log("Error - node type not yet supported");
            return;
        }

        newNode.transform.localPosition = new Vector2(xPos, yPos);

        //assign the node to the visual node, fill it in
        newNode.GetComponent<VisualNode>().AssignNode(node);

        //store in the storage for easy access when making connections
        visualNodeStorage.Add(newNode);

        Debug.Log("Node " + node.id + " successfully created");
    }

    public void CheckConnections(Node node)
    {
        if (node.requirement != null)
        {
            for (int i = 0; i < node.requirement.Length; i++)
            {
                GameObject newConnect = Instantiate(connectionPrefab);
                newConnect.transform.SetParent(contentPanel, false);
                //send the gameObjects associated with the origin and currentNode
                newConnect.GetComponent<Connection>().ConnectionSpawn(visualNodeStorage[node.requirement[i].id], visualNodeStorage[node.id]);
            }
        }
        else
        {
            Debug.Log("Node " + node.id + " has no requirements");
        }
    }*/

    /*public GameObject InstantiateNode(MysteryNode mystNode)
    {
        GameObject createdNode;
        //TODO - MORE NODE TYPES HERE
        switch (mystNode.Type)
        {
            case "EVIDENCE":
                createdNode = Instantiate(evidencePrefab, contentPanel);
                break;
            default:
                createdNode = Instantiate(infoPrefab, contentPanel);
                break;
        }


        return createdNode;
    }*/

    //all button handlers here

    //called by the addition button
    public void TheoryAdd()
    {
        //turn off the Addition mode
        if (theoryMode == TheoryMode.Addition)
        {
            theoryMode = TheoryMode.None;
            instructions.text = baseInstructions;
            
            // Visual cue - deactivate button highlight
            addTheoryButton.GetComponent<Image>().color = Color.white;
            
            // Reset cursor
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            
            // Hide simulation icon and dark overlay
            if (simulationIcon != null)
                simulationIcon.SetActive(false);
                
            // Fade out the dark overlay
            StartCoroutine(AnimateDarkOverlay(false));

            //reset the currentTheory
            if (currentTheory != null)
                currentTheory.GetComponent<Theory>().KillYourself();
            currentTheory = null;

            StartCoroutine(SimulationCalc());
        }
        //turn on the Addition mode
        else
        {
            theoryMode = TheoryMode.Addition;
            instructions.text = "Left click on first node to begin a theory | Right click anywhere to cancel";
            
            // Visual cue - highlight button
            addTheoryButton.GetComponent<Image>().color = new Color(0.8f, 1f, 0.8f); // Light green tint
            
            // Change cursor to indicate connection mode
            // Note: You would need a custom cursor texture for production use
            // For now, we're just changing the system cursor style
            Cursor.SetCursor(null, Vector2.zero, CursorMode.ForceSoftware);
            
            // Show simulation icon and dark overlay
            if (simulationIcon != null)
                simulationIcon.SetActive(true);
                
            // Fade in the dark overlay to highlight nodes
            StartCoroutine(AnimateDarkOverlay(true));

            //spawn a new theory
            // Instantiate theory as last child by default to ensure it renders on top
            // of both the nodes and the dark overlay
            currentTheory = Instantiate(theoryPrefab, contentPanel);
            // Removed SetAsFirstSibling() to fix rendering order
        }
    }

    public void NodeClick(VisualNode node)
    {
        //send a call to the current theory to assign the node to it
        currentTheory.GetComponent<Theory>().NodeAssign(node);
    }

    public void TheoryPlaced()
    {
        if (TheoryVerify())
        {
            untestedTheories.Add(currentTheory);
            currentTheory = null;
        }
        TheoryAdd();
    }

    public bool TheoryVerify()
    {
        Theory theoryTest = currentTheory.GetComponent<Theory>();
        //make sure that the starting and endingPos are not equal
        if (theoryTest.leadObj == theoryTest.answerObj)
        {
            NotifCall("Theory cannot connect to itself", Color.red);
            return false;
        }

        //make new list of all connections to check, include connections for the startObj of currenttheory
        List<GameObject> allConnects = new List<GameObject>(untestedTheories);
        allConnects.AddRange(theoryTest.leadObj.GetComponent<VisualNode>().connections);
        //make sure that no theory or connection currently exists with the same node pair
        foreach (GameObject connect in allConnects)
        {
            Connection connectTest = connect.GetComponent<Connection>();

            //don't check this one if the connection is not a theory and not confirmed
            if (!(connectTest is Theory || connectTest.confirmed))
            {
                Debug.Log("Skipping this one");
                continue;
            }

            if ((theoryTest.leadObj == connectTest.leadObj || theoryTest.leadObj == connectTest.answerObj) && 
                (theoryTest.answerObj == connectTest.leadObj || theoryTest.answerObj == connectTest.answerObj))
            {
                NotifCall("Connection or theory already exists", Color.red);
                return false;
            }
        }

        NotifCall("New Theory Added!", Color.green);
        return true;
    }

    //called by the removal button
    public void TheoryRemove()
    {
        //turn off the Removal mode
        if (theoryMode == TheoryMode.Removal)
        {
            theoryMode = TheoryMode.None;
            instructions.text = baseInstructions;
            
            // Visual cue - deactivate button highlight
            removeTheoryButton.GetComponent<Image>().color = Color.white;
            
            // Reset cursor
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            
            StartCoroutine(SimulationCalc());
        }
        //turn on the Removal mode
        else
        {
            theoryMode = TheoryMode.Removal;
            instructions.text = "Left click on any theory to remove it | Right click anywhere to cancel";
            
            // Visual cue - highlight button
            removeTheoryButton.GetComponent<Image>().color = new Color(1f, 0.8f, 0.8f); // Light red tint
            
            // Change cursor to indicate removal mode
            Cursor.SetCursor(null, Vector2.zero, CursorMode.ForceSoftware);
        }
    }

    public void TheoryRemoved(GameObject theory)
    {
        untestedTheories.Remove(theory);

        theory.GetComponent<Theory>().KillYourself();

        Debug.Log("There are now " + untestedTheories.Count + " untested theories");
        TheoryRemove();
    }

    //called by the simulation button

    //TODO add a simulation confirmation, where the user can see how much power each simulation will cost and has the opportunity to change before confirming things
    public void RunSimulation()
    {
        theoryMode = TheoryMode.Simulation;
        instructions.text = "Running simulation...";

        // Show the simulation icon
        if (simulationIcon != null)
            simulationIcon.SetActive(true);
            
        // Darken the board - animate transition
        StartCoroutine(AnimateDarkOverlay(true));

        //power decrement
        GameControl.GameController.powerControl.PowerDrain(simulationCost);

        StartCoroutine(Simulation());
    }
    
    // Animate darkening overlay for simulation mode
    private IEnumerator AnimateDarkOverlay(bool entering)
    {
        if (darkOverlay == null)
            yield break;
            
        float startAlpha = entering ? 0 : darkOverlayAlpha;
        float endAlpha = entering ? darkOverlayAlpha : 0;
        float elapsed = 0;
        
        // Enable the overlay if it's not already active
        darkOverlay.gameObject.SetActive(true);
        
        // Start with correct color
        Color overlayColor = darkOverlay.color;
        overlayColor.a = startAlpha;
        darkOverlay.color = overlayColor;
        
        while (elapsed < darkTransitionDuration)
        {
            float t = elapsed / darkTransitionDuration;
            // Use a smooth easing function
            t = t * t * (3f - 2f * t); // Smoothstep
            
            overlayColor.a = Mathf.Lerp(startAlpha, endAlpha, t);
            darkOverlay.color = overlayColor;
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Ensure we reach final state
        overlayColor.a = endAlpha;
        darkOverlay.color = overlayColor;
        
        // If exiting sim mode, disable the overlay completely
        if (!entering && darkOverlay.color.a <= 0.01f)
            darkOverlay.gameObject.SetActive(false);
    }

    IEnumerator SimulationCalc()
    {
        //retrieve all active visual nodes
        VisualNode[] visualNodes = GameObject.FindObjectsByType<VisualNode>(FindObjectsSortMode.None);

        float connectedNodes = 0;
        foreach (VisualNode visualNode in visualNodes)
        {
            foreach (GameObject connection in visualNode.connections) 
            {
                if (connection.GetComponent<Connection>().confirmed)
                {
                    connectedNodes++;
                    break;
                }
            }
        }

        float unconnectedNodes = visualNodes.Length - connectedNodes;
        Debug.Log(connectedNodes + " connected Nodes and " + unconnectedNodes + " unconnected Nodes");


        float theoryCount = untestedTheories.Count;
        float costPerTheory = Mathf.Pow(0.75f, -unconnectedNodes) / 100f;
        simulationCost = costPerTheory * theoryCount;
        Debug.Log("Total Sim Cost: " + simulationCost + " | Cost per Theory: " + costPerTheory);

        yield return null;
    }


    IEnumerator Simulation()
    {
        //start at the head node and progressively reveal whether connections are real or not, layer by layer
        Debug.Log("Running simulation on " + untestedTheories.Count + " untested theories");
        
        int totalTheories = untestedTheories.Count;
        int completedTheories = 0;
        
        // Show simulation progress in instructions
        instructions.text = "Running simulation... (0/" + totalTheories + ")";

        while (untestedTheories.Count > 0)
        {
            untestedTheories[0].GetComponent<Theory>().Reveal();
            untestedTheories.RemoveAt(0);
            
            // Update completion count
            completedTheories++;
            instructions.text = "Running simulation... (" + completedTheories + "/" + totalTheories + ")";
            
            // Reduced wait time for better responsiveness while still showing animation
            yield return new WaitForSecondsRealtime(0.75f);
        }

        untestedTheories.Clear();

        // End simulation mode
        theoryMode = TheoryMode.None;
        instructions.text = "Simulation complete!";
        
        // Hide simulation icon
        if (simulationIcon != null)
            simulationIcon.SetActive(false);
            
        // Return overlay to normal (fade out darkness)
        StartCoroutine(AnimateDarkOverlay(false));
        
        // Wait a moment to show the completion message before reverting
        yield return new WaitForSecondsRealtime(1f);
        
        instructions.text = baseInstructions;
        StartCoroutine(SimulationCalc());
    }

    private void NotifCall(string message, Color fill)
    {
        if (currentNotif != null)
            StopCoroutine(currentNotif);
        currentNotif = StartCoroutine(NotifActivate(message, fill));
    }

    IEnumerator NotifActivate(string message, Color fill)
    {
        theoryNotif.color = fill;
        theoryNotif.text = message;

        yield return new WaitForSecondsRealtime(3f);

        theoryNotif.text = string.Empty;
        currentNotif = null;
    }

    public void LeadReveal(string terminalId)
    {
        visualNodes[terminalId].GetComponent<VisualNode>().AddLeadVisual();
    }

    IEnumerator UnlockAllNodes()
    {
        foreach (var node in visualNodes)
        {
            GameControl.GameController.coreConstellation.DiscoverNode(node.Key);

            yield return new WaitForSecondsRealtime(0.25f);
        }
    }
}
