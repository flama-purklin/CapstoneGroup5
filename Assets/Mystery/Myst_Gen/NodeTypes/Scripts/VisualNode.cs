
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
        connections = new List<GameObject>();
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
        // Debug.Log("Visual Node " + nodeKey + " Updated");

        //fill in the information stored in the currentNode
        header.text = currentNode.Type;
        otherInfo.text = currentNode.Content;

        if (!currentNode.Discovered)
            gameObject.SetActive(false);
    }

    public void OnPointerDown(PointerEventData pointEventData)
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) && control.theoryMode == TheoryMode.Addition)
        {
            //send a call to node control to tell it this object has been clicked
            control.NodeClick(this);
        }
        if (Input.GetKey(KeyCode.Mouse1) && control.theoryMode == TheoryMode.None)
        {
            Debug.Log("Pointer Down at " + gameObject.name);
            GameObject.FindFirstObjectByType<EvidenceInspect>().ActivateInspect(currentNode);
        }
    }

    public void OnPointerUp(PointerEventData pointerEventData)
    {
        //Debug.Log("Pointer Released at " + gameObject.name);
    }

    public void OnDrag(PointerEventData pointEventData)
    {
        if (mysteryCam == null)
            mysteryCam = GameObject.FindWithTag("MysteryCam").GetComponent<Camera>();

        if (Input.GetKey(KeyCode.Mouse0) && control.theoryMode == TheoryMode.None)
        {
            Vector2 mousePos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                transform.parent as RectTransform, Input.mousePosition,
                        mysteryCam,
                                out mousePos);
            Debug.Log("Camera ortho size: " + mysteryCam.orthographicSize);
            transform.localPosition = new Vector3(mousePos.x, mousePos.y, 0);
        }
    }
}
