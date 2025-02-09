using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NodeControl : MonoBehaviour
{
    public List<Node> allNodes;
    public List<GameObject> visualNodeStorage;
    private RectTransform canvasRect;

    [SerializeField] GameObject infoPrefab;
    [SerializeField] GameObject evidencePrefab;
    [SerializeField] GameObject connectionPrefab;
    [SerializeField] RectTransform contentPanel;

    [SerializeField] float scrollSpeed = 2000f;
    [SerializeField] float minY = -2000f;//adjust to something sensible later
    [SerializeField] float maxY = 2000f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        contentPanel.anchoredPosition = Vector2.zero;
    }

    // Update is called once per frame
    void Update()
    {
        HandleScroll();
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

    //recieves the nodes from the MysteryGen
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
    }

    public void CreateVisualNode(Node node)
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
    }
}