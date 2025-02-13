using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VisualNode : MonoBehaviour
{
    //assigned node
    public Node currentNode;

    //all components of the visual object
    [SerializeField] protected TMP_Text idDisplay;
    [SerializeField] protected TMP_Text header;
    [SerializeField] protected TMP_Text character;
    [SerializeField] protected TMP_Text otherInfo;
    [SerializeField] protected Image background;

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
}
