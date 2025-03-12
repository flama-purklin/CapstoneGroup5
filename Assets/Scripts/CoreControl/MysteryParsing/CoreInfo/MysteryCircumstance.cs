using Newtonsoft.Json;
using UnityEngine;

public class MysteryCircumstance : MonoBehaviour
{
    [JsonProperty("location")]
    public string Location { get; set; }

    [JsonProperty("time_minutes")]
    public int TimeMinutes { get; set; }

    [JsonProperty("details")]
    public string Details { get; set; }
}
