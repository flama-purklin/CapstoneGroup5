using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NodeControl : MonoBehaviour
{
    public List<Node> allNodes;

    public List<GameObject> visualNodeStorage;

    [SerializeField] GameObject infoPrefab;
    [SerializeField] GameObject evidencePrefab;

    [SerializeField] GameObject connectionPrefab;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //recieves the nodes from the MysteryGen
    public void CreateConstellation(List<Node> generatedNodes)
    {
        allNodes = generatedNodes;

        //instantiate visualNodeStorage (used for quick retrieval of nodes for connections)
        visualNodeStorage = new List<GameObject>();

        //begins generation of all nodes
        foreach (Node node in allNodes)
        {
            CreateVisualNode(node);
        }

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

    public void CheckConnections(Node node)
    {
        if (node.requirement != null)
        {
            for (int i = 0; i < node.requirement.Length; i++)
            {
                GameObject newConnect = Instantiate(connectionPrefab);
                newConnect.transform.SetParent(transform, false);
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
