using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The main Mystery data class, containing all information for a mystery.
/// Enhanced with TrainLayout property and additional helper methods.
/// </summary>
public class Mystery
{
    // Metadata
    [JsonProperty("metadata")]
    public MysteryMetadata Metadata { get; set; }

    // Core data
    [JsonProperty("core")]
    public MysteryCore Core { get; set; }

    // Character data
    [JsonProperty("characters")]
    public Dictionary<string, MysteryCharacter> Characters { get; set; }

    // Environmental data
    [JsonProperty("environment")]
    public MysteryEnvironment Environment { get; set; }

    // Constellation
    [JsonProperty("constellation")]
    public MysteryConstellation Constellation { get; set; }
    
    // Train layout (may be parsed from environment data)
    [JsonProperty("train_layout")]
    public TrainLayout TrainLayout { get; set; }
    
    /// <summary>
    /// Gets a character by ID.
    /// </summary>
    public MysteryCharacter GetCharacter(string characterId)
    {
        if (Characters != null && Characters.TryGetValue(characterId, out MysteryCharacter character))
        {
            return character;
        }
        
        return null;
    }
    
    /// <summary>
    /// Gets a node by ID.
    /// </summary>
    public MysteryNode GetNode(string nodeId)
    {
        if (Constellation?.Nodes != null && Constellation.Nodes.TryGetValue(nodeId, out MysteryNode node))
        {
            return node;
        }
        
        return null;
    }
}

/// <summary>
/// Represents the train layout in the mystery.
/// </summary>
[System.Serializable]
public class TrainLayout
{
    [JsonProperty("cars")]
    public List<TrainCarDefinition> Cars { get; set; }
    
    [JsonProperty("connections")]
    public Dictionary<string, CarConnection> Connections { get; set; }
}

/// <summary>
/// Defines a single train car in the layout.
/// </summary>
[System.Serializable]
public class TrainCarDefinition
{
    [JsonProperty("car_id")]
    public string CarId { get; set; }
    
    [JsonProperty("car_type")]
    public string CarType { get; set; }
    
    [JsonProperty("car_class")]
    public string CarClass { get; set; }
    
    [JsonProperty("properties")]
    public Dictionary<string, string> Properties { get; set; }
    
    [JsonProperty("available_locations")]
    public List<string> AvailableLocations { get; set; }
}