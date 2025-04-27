using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

public class MysteryEvent
{
    [JsonProperty("id")]
    public string Id { get; set; }

    // Updated to parse array of triggers instead of single trigger string
    [JsonProperty("triggers")]
    public List<string> Triggers { get; set; }
}
