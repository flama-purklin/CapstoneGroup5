using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;
using static TrainManager;

public class MysteryEnvironment
{
    [JsonProperty("cars")]
    public Dictionary<string, TrainCar> Cars { get; set; }

    [JsonProperty("layout_order")]
    public List<string> LayoutOrder { get; set; }
}

public class TrainCar
{
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("description")]
    public string Description { get; set; }

    // Handle optional points_of_interest in new JSON, default to empty if missing
    [JsonProperty("points_of_interest", NullValueHandling = NullValueHandling.Ignore)]
    public Dictionary<string, PointOfInterest> PointsOfInterest { get; set; } = new Dictionary<string, PointOfInterest>();
}

public class PointOfInterest
{
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("description")]
    public string Description { get; set; }

    [JsonProperty("evidence_items")]
    public List<string> EvidenceItems { get; set; }
}
