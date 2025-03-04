using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class VisualNode : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    //

    //assigned node
    public Node currentNode;

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
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AssignNode(Node associatedNode)
    {
        currentNode = associatedNode;
        UpdateInformation();
    }

    protected virtual void UpdateInformation()
    {
        Debug.Log("Visual Node " + currentNode.id + " Updated");
    }

    public void OnPointerDown(PointerEventData pointEventData)
    {
        Debug.Log("Pointer Down at " + gameObject.name);
    }

    public void OnPointerUp(PointerEventData pointerEventData)
    {
        Debug.Log("Pointer Released at " + gameObject.name);
    }

    public void OnDrag(PointerEventData pointEventData)
    {
        if (Input.GetKey(KeyCode.Mouse0))
        {
            Debug.Log("Dragging this one " + gameObject.name);
            transform.position = Input.mousePosition;
        }
    }
}
