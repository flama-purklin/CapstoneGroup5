using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;

public class MysteryNode
{
    [JsonProperty("type")]
    public string Type { get; set; }

    [JsonProperty("category")]
    public string Category { get; set; }

    [JsonProperty("content")]
    public string Content { get; set; }

    [JsonProperty("discovered")]
    public bool Discovered { get; set; }

    [JsonProperty("location")]
    public string Location { get; set; }

    [JsonProperty("time")]
    public int? Time { get; set; }  // Nullable because some nodes might not have time

    [JsonProperty("characters")]
    public List<string> Characters { get; set; }

    [JsonProperty("description")]
    public string Description { get; set; }

    [JsonProperty("hidden_details")]
    public List<string> HiddenDetails { get; set; }

    [JsonProperty("can_pickup")]
    public bool? CanPickup { get; set; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
