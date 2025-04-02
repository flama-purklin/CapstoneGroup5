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

    [Header("Prefabs")]
    [SerializeField] GameObject infoPrefab;
    [SerializeField] GameObject evidencePrefab;
    [SerializeField] GameObject connectionPrefab;

    [Header("UI Elements")]
    [SerializeField] RectTransform contentPanel;
    [SerializeField] TMP_Text instructions;
    [SerializeField] Button addTheoryButton;
    [SerializeField] Button removeTheoryButton;
    [SerializeField] Button simButton;


    [SerializeField] float scrollSpeed = 2000f;
    [SerializeField] float minY = -2000f;//adjust to something sensible later
    [SerializeField] float maxY = 2000f;

    bool loaded = false;

    public TheoryMode theoryMode = TheoryMode.None;

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
    }

    // Update is called once per frame
    void Update()
    {
        //HandleScroll();
    }

    void HandleScroll()
    {
        float scrollInput = Input.GetAxis("Mouse ScrollWheel") * -1f;
        if (scrollInput != 0f && contentPanel != null)
        {
            Vector2 newPos = contentPanel.anchoredPosition;
            newPos.y += scrollInput * scrollSpeed * Time.deltaTime;
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

        //create a node object for every parsed node in the constellation
        foreach (var nodePair in GameControl.GameController.coreConstellation.Nodes)
        {
            //create the node
            GameObject newNode = InstantiateNode(nodePair.Value);

            //assign data to the node
            newNode.GetComponent<VisualNode>().AssignNode(nodePair.Key, nodePair.Value);

            //store the node in the dictionary for easy access
            visualNodes.Add(nodePair.Key, newNode);
        }

        Debug.Log(visualNodes.Count + " visual nodes created");

        //create a connection for each parsed connection in the constellation
        foreach (var parsedConnection in GameControl.GameController.coreConstellation.Connections)
        {
            //create a connection object
            GameObject newConn = Instantiate(connectionPrefab, contentPanel);
            newConn.GetComponent<Connection>().ConnectionSpawn(visualNodes[parsedConnection.Source], visualNodes[parsedConnection.Target], parsedConnection);

            //store a link to it in both sides, so when they are discovered, they will turn it on if necessary
            visualNodes[parsedConnection.Source].GetComponent<VisualNode>().connections.Add(newConn);
            visualNodes[parsedConnection.Target].GetComponent<VisualNode>().connections.Add(newConn);
        }
    }

    public void UnlockVisualNode(string nodeKey)
    {
        GameObject unlockedNode = visualNodes[nodeKey];
        unlockedNode.SetActive(true);
        unlockedNode.GetComponent<VisualNode>().DiscoverNode();
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

    public GameObject InstantiateNode(MysteryNode mystNode)
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
    }

    //all button handlers here

    //called by the addition button
    public void TheoryAdd()
    {
        //turn off the Addition mode
        if (theoryMode == TheoryMode.Addition)
        { 
            theoryMode = TheoryMode.None;
            instructions.text = baseInstructions;
        }
        //turn on the Addition mode
        else
        {
            theoryMode = TheoryMode.Addition;
            instructions.text = "Left click on first node to begin a theory | Right click anywhere to cancel";
        }
    }

    //called by the removal butotn
    public void TheoryRemove()
    {
        //turn off the Addition mode
        if (theoryMode == TheoryMode.Addition)
        {
            theoryMode = TheoryMode.None;
            instructions.text = baseInstructions;
        }
        //turn on the Addition mode
        else
        {
            theoryMode = TheoryMode.Addition;
            instructions.text = "Left click on any theory to remove it | Right click anywhere to cancel";
        }
    }

    //called by the simulation button
    public void RunSimulation()
    {
        theoryMode = TheoryMode.Simulation;
        instructions.text = "Running simulation...";
        StartCoroutine(Simulation());
    }

    IEnumerator Simulation()
    {
        yield return new WaitForSeconds(5f);
        theoryMode = TheoryMode.None;
        instructions.text = baseInstructions;
    }
}