using UnityEngine;

public class VisualEvidenceNode : VisualNode
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    protected override void UpdateInformation()
    {
        //cast the node to the specific type to retrieve more specific information
        //EvidenceNode tempNode = (EvidenceNode)currentNode;

        //TODO - more fill in the information stored in the currentNode (number indicator, subtype, etc.)
        header.text = currentNode.Type;
        title.text = currentNode.Title;


        base.UpdateInformation();
    }
}
