using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

public class MysteryConstellation
{

    [JsonProperty("nodes")]
    public Dictionary<string, MysteryNode> Nodes { get; set; }

    [JsonProperty("connections")]
    public List<MysteryConnection> Connections { get; set; }

    [JsonProperty("mini_mysteries")]
    public Dictionary<string, MiniMystery> MiniMysteries { get; set; }

    [JsonProperty("scripted-events")]
    public Dictionary<string, MysteryEvent> ScriptedEvents { get; set; }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    
    public MysteryNode DiscoverNode(string nodeKey)
    {
        //check if the node exists in the parsed dict of nodes
        bool nodeExist = Nodes.ContainsKey(nodeKey);
        if (!nodeExist)
        {
            if (string.IsNullOrEmpty(nodeKey))
                Debug.Log("Node Key is not defined");
            else
                Debug.Log("No node found that correlates to Key " + nodeKey);
            return null;
        }

        bool prevVal = Nodes[nodeKey].Discover();
        if (prevVal)
        {
            Debug.Log("Node already discovered");
            return null;
        }
        else
        {
            Debug.Log("Node " + nodeKey + " has been discovered");

            //start the node notification here
            GameObject.FindFirstObjectByType<NodeNotif>().NodeUnlock();

            //Unlock the Visual Node GameObject
            GameObject.FindFirstObjectByType<NodeControl>().UnlockVisualNode(nodeKey);

            return Nodes[nodeKey];
        }

        
            
    }
}
