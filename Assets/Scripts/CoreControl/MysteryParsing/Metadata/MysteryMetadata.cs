using Newtonsoft.Json;
using UnityEngine;

public class MysteryMetadata
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("version")]
    public string Version { get; set; }

    [JsonProperty("last_updated")]
    public string LastUpdated { get; set; }

    [JsonProperty("title")]
    public string Title { get; set; }
}
