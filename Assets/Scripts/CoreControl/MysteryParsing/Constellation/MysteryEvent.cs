using Newtonsoft.Json;
using UnityEngine;

public class MysteryEvent
{
    [JsonProperty("id")]
    public string Id {  get; set; }

    [JsonProperty("trigger")]
    public string Trigger { get; set; }

    [JsonProperty("action")]
    public string Action { get; set; }

    [JsonProperty("description")]
    public string Description { get; set; }

}
