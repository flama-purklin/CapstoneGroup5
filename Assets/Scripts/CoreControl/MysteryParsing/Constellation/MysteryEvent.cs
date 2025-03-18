using Newtonsoft.Json;
using UnityEngine;

public class MysteryEvent : MonoBehaviour
{
    [JsonProperty("character")]
    public string Character {  get; set; }

    [JsonProperty("trigger")]
    public string Trigger { get; set; }

    [JsonProperty("description")]
    public string Description { get; set; }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
