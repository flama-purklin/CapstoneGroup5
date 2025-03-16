
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.UI;

public class VisualNode : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    //MysteryNode parsedNode;
    

    //assigned node
    public MysteryNode currentNode;
    public string nodeKey;

    //associated connections
    public List<GameObject> connections;

    //all components of the visual object
    [SerializeField] protected TMP_Text idDisplay;
    [SerializeField] protected TMP_Text header;
    [SerializeField] protected TMP_Text character;
    [SerializeField] protected TMP_Text otherInfo;
    [SerializeField] protected Image background;



    //movement shit


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        connections = new List<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AssignNode(string newKey, MysteryNode associatedNode)
    {
        currentNode = associatedNode;
        nodeKey = newKey;
        UpdateInformation();
    }

    //call to enable the node and check whether connections also need to be unlocked
    public void DiscoverNode()
    {
        Debug.Log("Visual Node" + nodeKey + " Discovered");

        if (connections.Count > 0)
        {
            foreach (var connection in connections)
            {
                connection.GetComponent<Connection>().DiscoveryCheck();
            }
        }
    }

    protected virtual void UpdateInformation()
    {
        Debug.Log("Visual Node " + nodeKey + " Updated");
        if (!currentNode.Discovered)
            gameObject.SetActive(false);
    }

    public void OnPointerDown(PointerEventData pointEventData)
    {
        //Debug.Log("Pointer Down at " + gameObject.name);
    }

    public void OnPointerUp(PointerEventData pointerEventData)
    {
        //Debug.Log("Pointer Released at " + gameObject.name);
    }

    public void OnDrag(PointerEventData pointEventData)
    {
        if (Input.GetKey(KeyCode.Mouse0))
        {
            //Debug.Log("Dragging this one " + gameObject.name);
            transform.position = new Vector3(Input.mousePosition.x, Input.mousePosition.y, transform.position.z);
        }
    }
}
