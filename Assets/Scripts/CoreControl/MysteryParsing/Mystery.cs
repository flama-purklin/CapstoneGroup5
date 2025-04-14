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
    public MysteryCore Core { get; set; }

    //character data
    [JsonProperty("character_profiles")] // Updated JSON property name
    public Dictionary<string, MysteryCharacter> Characters { get; set; }

    //environmental data
    [JsonProperty("environment")]
    public MysteryEnvironment Environment { get; set; }

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
