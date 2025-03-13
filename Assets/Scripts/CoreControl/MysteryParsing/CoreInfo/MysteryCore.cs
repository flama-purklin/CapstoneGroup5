using Newtonsoft.Json;
using UnityEngine;

public class MysteryCore
{
    [JsonProperty("type")]
    public string Type { get; set; }

    [JsonProperty("subtype")]
    public string Subtype { get; set; }

    [JsonProperty("theme")]
    public string Theme { get; set; }

    [JsonProperty("victim")]
    public string Victim { get; set; }

    [JsonProperty("culprit")]
    public string Culprit { get; set; }

    [JsonProperty("manipulator")]
    public string Manipulator { get; set; }

    [JsonProperty("method")]
    public string Method { get; set; }

    [JsonProperty("motive")]
    public string Motive { get; set; }

    [JsonProperty("circumstance")]
    public MysteryCircumstance Circumstance { get; set; }
}
