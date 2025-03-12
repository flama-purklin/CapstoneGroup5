using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

public class MysteryConstellation
{

    [JsonProperty("nodes")]
    public Dictionary<string, MysteryNode> Nodes { get; set; }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
