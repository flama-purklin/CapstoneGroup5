using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;

#nullable enable
public class MysteryNode
{
    [JsonProperty("type")]
    public string Type { get; set; } = default!;

#nullable enable
    [JsonProperty("subtype")]
<<<<<<< Updated upstream
    public string Subtype { get; set; } = default!;
=======
    public string? Subtype { get; set; }
#nullable disable
>>>>>>> Stashed changes

    [JsonProperty("title")]
    public string Title { get; set; } = default!;

    [JsonProperty("description")]
    public string Description { get; set; } = default!;

    [JsonProperty("car_id")]
    public string? CarId { get; set; }

    [JsonProperty("coords")]
    public int[]? Coords { get; set; }

    [JsonProperty("solution")]
    public string? Solution { get; set; }

    [JsonProperty("contains")]
    public string[]? Contains { get; set; }

    [JsonProperty("locked_by")]
    public string? LockedBy { get; set; }

    public bool Discovered { get; set; }

    public bool Discover()
    {
        bool previousVal = Discovered;
        Discovered = true;
        return previousVal;
    }
}
#nullable disable
