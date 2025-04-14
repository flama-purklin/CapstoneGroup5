using JetBrains.Annotations;
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

    //scripted events should be a flexible system - triggered by the unlocking of nodes, which has function calls to do specific things
    //reveal hidden details on existing nodes, kill off certain characters, reveal new piece of evidence, move a character, etc.
    [JsonProperty("scripted-events")]
    public Dictionary<string, MysteryEvent> ScriptedEvents { get; set; }
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [Header("MysteryCompletion")]
    public int completeMysteryCount = 0;
    public int currentMysteryCount = 0;
    
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

    public void CompleteMysteryCalc()
    {
        completeMysteryCount = Connections.Count + Nodes.Count;

        //current count starts at one because of the initial node
        currentMysteryCount = 1;
    }

    public float ConfidenceScore()
    {
        return (float)currentMysteryCount / completeMysteryCount;
    }
}

/*
 * "appearance": {
 *  "base": "woman1.png",
 *  "hair": "black_hair.png",
 *  "dress": "bruh.png",
 *  "shoes": "airjordans.png",
 *  "eyes": "green.png",
 *  "nose": "schnauz2.png",
 *  "mouth": "lusciouslips.png"
 * 
 * }
 * 
 * 
 */
