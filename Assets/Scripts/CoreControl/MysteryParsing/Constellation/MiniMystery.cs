using Newtonsoft.Json;
using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class MiniMystery
{
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("description")]
    public string Description { get; set; }

    [JsonProperty("entry_points")]
    public List<string> EntryPoints { get; set; }

    [JsonProperty("revelation")]
    public string Revelation { get; set; }

    [JsonProperty("connects_to_main")]
    public List<string> ConnectsToMain { get; set; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
