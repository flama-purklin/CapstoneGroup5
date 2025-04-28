using Newtonsoft.Json;
using UnityEngine;

public class MysteryLead
{
    //the id that will be called upon for access to the lead
    [JsonProperty("id")]
    public string Id { get; set; }

    //the question corresponding to this lead
    [JsonProperty("question")]
    public string Question { get; set; }

    //the node that houses the lead
    [JsonProperty("inside")]
    public string Inside { get; set; }

    //the node that provides and answer to the question posed here
    [JsonProperty("answer")] // Fixed capitalization to match JSON convention
    public string Answer { get; set; }

    //the base node that this lead corresponds with
    [JsonProperty("terminal")]
    public string Terminal { get; set; }

    public bool Discovered = false;
    public bool Solved = false;
}
