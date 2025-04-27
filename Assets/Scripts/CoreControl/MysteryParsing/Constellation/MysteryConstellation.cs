using JetBrains.Annotations;
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

public class MysteryConstellation
{

    [JsonProperty("nodes")]
    public Dictionary<string, MysteryNode> Nodes { get; set; }

    //connections no longer exist
    /*    [JsonProperty("connections")]
        public List<MysteryConnection> Connections { get; set; }*/

    [JsonProperty("leads")]
    public List<MysteryLead> Leads { get; set; }

    [JsonProperty("mini_mysteries")]
    public Dictionary<string, MiniMystery> MiniMysteries { get; set; } = new Dictionary<string, MiniMystery>();

    //scripted events should be a flexible system - triggered by the unlocking of nodes, which has function calls to do specific things
    //reveal hidden details on existing nodes, kill off certain characters, reveal new piece of evidence, move a character, etc.
    // Removed scripted_events property from Constellation; now parsed at root Mystery level

    [Header("MysteryCompletion")]
    public int completeMysteryCount = 0;
    public int currentMysteryCount = 0;

    public List<string> foundEvidence = new List<string>();
    
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
            //TODO - add a clause here for if node is revealed by a different character


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

            if (Nodes[nodeKey].Type == "EVIDENCE")
                foundEvidence.Add(nodeKey);
            //Debug.Log("Found Evidence Count: " + foundEvidence.Count);

            //TODO - Check for scripted events here

            return Nodes[nodeKey];
        }

        
            
    }

    public void CompleteMysteryCalc()
    {
        completeMysteryCount = Nodes.Count + Leads.Count;

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
