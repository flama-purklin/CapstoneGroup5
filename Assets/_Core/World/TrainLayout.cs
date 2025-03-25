using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

/// <summary>
/// Represents the layout of a train in the mystery.
/// Because you can't have a proper murder mystery without a train.
/// Agatha Christie would be proud.
/// </summary>
[System.Serializable]
public class TrainLayout
{
    [JsonProperty("cars")]
    public List<TrainCarDefinition> Cars { get; set; }
    
    [JsonProperty("connections")]
    public Dictionary<string, CarConnection> Connections { get; set; }
    
    /// <summary>
    /// Gets a car definition by its ID.
    /// </summary>
    public TrainCarDefinition GetCarById(string carId)
    {
        if (Cars == null) return null;
        
        return Cars.Find(car => car.CarId == carId);
    }
    
    /// <summary>
    /// Gets all car IDs in the layout.
    /// </summary>
    public List<string> GetAllCarIds()
    {
        if (Cars == null) return new List<string>();
        
        List<string> carIds = new List<string>();
        foreach (var car in Cars)
        {
            carIds.Add(car.CarId);
        }
        
        return carIds;
    }
    
    /// <summary>
    /// Gets all cars of a specific type.
    /// </summary>
    public List<TrainCarDefinition> GetCarsByType(string carType)
    {
        if (Cars == null) return new List<TrainCarDefinition>();
        
        return Cars.FindAll(car => car.CarType.ToLower() == carType.ToLower());
    }
}

/// <summary>
/// Defines a car in the train layout.
/// The blueprint for train cars, before they become real (in a digital sense).
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
    
    /// <summary>
    /// Gets a property value.
    /// </summary>
    public string GetProperty(string key, string defaultValue = "")
    {
        if (Properties != null && Properties.TryGetValue(key, out string value))
        {
            return value;
        }
        
        return defaultValue;
    }
    
    /// <summary>
    /// Checks if the car has a specific property.
    /// </summary>
    public bool HasProperty(string key)
    {
        return Properties != null && Properties.ContainsKey(key);
    }
}

/// <summary>
/// Add TrainLayout property to the Mystery class.
/// </summary>
public partial class Mystery
{
    [JsonProperty("train_layout")]
    public TrainLayout TrainLayout { get; set; }
}

/// <summary>
/// Add PhysicalEvidence and Location properties to MysteryNode.
/// </summary>
public partial class MysteryNode
{
    [JsonProperty("physical_evidence")]
    public bool PhysicalEvidence { get; set; }
    
    [JsonProperty("location")]
    public string Location { get; set; }
    
    [JsonProperty("properties")]
    public Dictionary<string, string> Properties { get; set; }
}
