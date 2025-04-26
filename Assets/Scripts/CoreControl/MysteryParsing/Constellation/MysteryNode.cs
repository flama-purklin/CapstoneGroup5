using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;

public class MysteryNode
{
    [JsonProperty("type")]
    public string Type { get; set; }

    [JsonProperty("subtype")]
    public string Subtype { get; set; }

    [JsonProperty("title")]
    public string Title { get; set; }

    [JsonProperty("description")]
    public string Description { get; set; }

    //Nullable strings to only use if evidence is physical
    #nullable enable
    [JsonProperty("car_id")]
    public string? CarId { get; set; }

    [JsonProperty("coords")]
    public int[]? Coords { get; set; }

    //Nullable strings to use only if a barrier or blocked by a barrier
    [JsonProperty("solution")]
    public string? Solution {  get; set; }

    [JsonProperty("contains")]
    public string[]? Contains { get; set; }

    [JsonProperty("locked_by")]
    public string? LockedBy { get; set; }
    #nullable disable

    public bool Discovered { get; set; }

    public bool Discover()
    {
        bool previousVal = Discovered;
        Discovered = true;
        return previousVal;
    }
}
