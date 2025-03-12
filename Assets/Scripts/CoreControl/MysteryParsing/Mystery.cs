using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

public class Mystery
{
    //metadata
    [JsonProperty("metadata")]
    public MysteryMetadata Metadata { get; set; }

    //core data
    [JsonProperty("core")]
    public Core Core { get; set; }

    //character data
    [JsonProperty("characters")]
    public Dictionary<string, Character> Characters { get; set; }

    //environmental data
    [JsonProperty("environment")]
    public Environment Environment { get; set; }

    //constellation
    [JsonProperty("constellation")]
    public MysteryConstellation Constellation { get; set; }


    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
