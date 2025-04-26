using UnityEngine;

public class VisualInfoNode : VisualNode
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
        //InfoNode tempNode = (InfoNode)currentNode;

        //fill in the information stored in the currentNode
        header.text = currentNode.Type;
        title.text = currentNode.Title;
        //otherInfo.text = tempNode.info;
        //idDisplay.text = tempNode.id.ToString();

        //set opacity based on whether node is reached yet or not (replace this with a better indicator eventually)
        /*if (tempNode.reached)
            background.color = new Color(background.color.r, background.color.g, background.color.b, 1f);
        else
            background.color = new Color(background.color.r, background.color.g, background.color.b, 0.5f);
        */

        base.UpdateInformation();
    }
}
