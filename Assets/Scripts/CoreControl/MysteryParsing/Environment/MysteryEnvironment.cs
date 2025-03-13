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

    [JsonProperty("points_of_interest")]
    public Dictionary<string, PointOfInterest> PointsOfInterest { get; set; }
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
