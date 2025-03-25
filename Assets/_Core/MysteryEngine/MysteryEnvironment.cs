using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents the environment data in a mystery.
/// Enhanced to support train layout data.
/// </summary>
public class MysteryEnvironment
{
    [JsonProperty("cars")]
    public Dictionary<string, TrainCar> Cars { get; set; }

    [JsonProperty("layout_order")]
    public List<string> LayoutOrder { get; set; }
    
    [JsonProperty("properties")]
    public Dictionary<string, string> Properties { get; set; }
    
    [JsonProperty("train_data")]
    public JObject TrainData { get; set; }
    
    /// <summary>
    /// Gets a car by ID.
    /// </summary>
    public TrainCar GetCar(string carId)
    {
        if (Cars != null && Cars.TryGetValue(carId, out TrainCar car))
        {
            return car;
        }
        
        return null;
    }
    
    /// <summary>
    /// Gets the ordered car IDs based on layout order.
    /// </summary>
    public List<string> GetOrderedCarIds()
    {
        if (LayoutOrder != null && LayoutOrder.Count > 0)
        {
            return LayoutOrder;
        }
        
        if (Cars != null)
        {
            return new List<string>(Cars.Keys);
        }
        
        return new List<string>();
    }
}

/// <summary>
/// Represents a train car within the environment.
/// </summary>
public class TrainCar
{
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("description")]
    public string Description { get; set; }

    [JsonProperty("points_of_interest")]
    public Dictionary<string, PointOfInterest> PointsOfInterest { get; set; }
    
    [JsonProperty("car_type")]
    public string CarType { get; set; }
    
    [JsonProperty("car_class")]
    public string CarClass { get; set; }
    
    [JsonProperty("properties")]
    public Dictionary<string, string> Properties { get; set; }
    
    /// <summary>
    /// Gets a point of interest by ID.
    /// </summary>
    public PointOfInterest GetPointOfInterest(string poiId)
    {
        if (PointsOfInterest != null && PointsOfInterest.TryGetValue(poiId, out PointOfInterest poi))
        {
            return poi;
        }
        
        return null;
    }
    
    /// <summary>
    /// Gets all points of interest that contain a specific evidence item.
    /// </summary>
    public List<PointOfInterest> GetPointsOfInterestWithEvidence(string evidenceId)
    {
        List<PointOfInterest> result = new List<PointOfInterest>();
        
        if (PointsOfInterest != null)
        {
            foreach (var poi in PointsOfInterest.Values)
            {
                if (poi.EvidenceItems != null && poi.EvidenceItems.Contains(evidenceId))
                {
                    result.Add(poi);
                }
            }
        }
        
        return result;
    }
}

/// <summary>
/// Represents a specific location or feature within a train car.
/// </summary>
public class PointOfInterest
{
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("description")]
    public string Description { get; set; }

    [JsonProperty("evidence_items")]
    public List<string> EvidenceItems { get; set; }
    
    [JsonProperty("properties")]
    public Dictionary<string, string> Properties { get; set; }
    
    /// <summary>
    /// Determines whether this point of interest contains the specified evidence.
    /// </summary>
    public bool HasEvidence(string evidenceId)
    {
        return EvidenceItems != null && EvidenceItems.Contains(evidenceId);
    }
}