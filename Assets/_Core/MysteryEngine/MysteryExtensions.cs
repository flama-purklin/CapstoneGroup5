using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Extends the Mystery class with train layout data and helper methods.
/// </summary>
public static class MysteryExtensions
{
    /// <summary>
    /// Extracts TrainLayout from a Mystery object, parsing from environment data if needed.
    /// </summary>
    public static TrainLayout GetTrainLayout(this Mystery mystery)
    {
        // First, check if train_layout already exists directly in the mystery
        if (mystery.TrainLayout != null && mystery.TrainLayout.Cars != null && mystery.TrainLayout.Cars.Count > 0)
        {
            return mystery.TrainLayout;
        }
        
        // If we don't have a TrainLayout yet, try to extract it from the environment data
        if (mystery.Environment != null)
        {
            TrainLayout layout = ExtractTrainLayoutFromEnvironment(mystery.Environment);
            if (layout != null)
            {
                // Cache the extracted layout
                mystery.TrainLayout = layout;
                return layout;
            }
        }
        
        // If we still don't have a layout, create a default one
        Debug.LogWarning("No train layout found in mystery data. Creating a default layout.");
        return CreateDefaultTrainLayout();
    }
    
    /// <summary>
    /// Attempt to extract train layout information from the environment data.
    /// </summary>
    private static TrainLayout ExtractTrainLayoutFromEnvironment(MysteryEnvironment environment)
    {
        // Try to find train layout data in environment properties
        if (environment.TrainData != null)
        {
            try
            {
                // Convert JObject to TrainLayout
                TrainLayout layout = environment.TrainData.ToObject<TrainLayout>();
                if (layout != null && layout.Cars != null && layout.Cars.Count > 0)
                {
                    return layout;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error extracting train layout from environment: {e.Message}");
            }
        }
        
        // If we don't have direct train data, try to parse from environment details
        List<TrainCarDefinition> cars = new List<TrainCarDefinition>();
        
        // Check for train or car related properties in the environment
        if (environment.Properties != null)
        {
            foreach (var prop in environment.Properties)
            {
                if (prop.Key.ToLower().Contains("car") || prop.Key.ToLower().Contains("train"))
                {
                    // Parse car information from property
                    TrainCarDefinition car = ParseCarFromProperty(prop.Key, prop.Value);
                    if (car != null)
                    {
                        cars.Add(car);
                    }
                }
            }
        }
        
        if (cars.Count > 0)
        {
            return new TrainLayout
            {
                Cars = cars,
                Connections = new Dictionary<string, CarConnection>()
            };
        }
        
        return null;
    }
    
    /// <summary>
    /// Try to parse a car definition from an environment property.
    /// </summary>
    private static TrainCarDefinition ParseCarFromProperty(string key, string value)
    {
        // Basic parsing logic - can be expanded for more complex formats
        string carId = key;
        string carType = "passenger"; // Default type
        
        // Try to extract type from the value
        if (value.ToLower().Contains("dining"))
        {
            carType = "dining";
        }
        else if (value.ToLower().Contains("kitchen"))
        {
            carType = "kitchen";
        }
        else if (value.ToLower().Contains("bar"))
        {
            carType = "bar";
        }
        else if (value.ToLower().Contains("storage"))
        {
            carType = "storage";
        }
        else if (value.ToLower().Contains("engine"))
        {
            carType = "engine";
        }
        
        return new TrainCarDefinition
        {
            CarId = carId,
            CarType = carType,
            CarClass = "standard",
            Properties = new Dictionary<string, string>
            {
                { "description", value }
            },
            AvailableLocations = new List<string> { "center", "front", "back" }
        };
    }
    
    /// <summary>
    /// Creates a default train layout for testing or fallback.
    /// </summary>
    private static TrainLayout CreateDefaultTrainLayout()
    {
        List<TrainCarDefinition> cars = new List<TrainCarDefinition>
        {
            new TrainCarDefinition
            {
                CarId = "engine_01",
                CarType = "engine",
                CarClass = "standard",
                Properties = new Dictionary<string, string>(),
                AvailableLocations = new List<string> { "control_room", "engine_area" }
            },
            new TrainCarDefinition
            {
                CarId = "passenger_01",
                CarType = "passenger",
                CarClass = "first",
                Properties = new Dictionary<string, string>(),
                AvailableLocations = new List<string> { "seat_1", "seat_2", "aisle" }
            },
            new TrainCarDefinition
            {
                CarId = "dining_01",
                CarType = "dining",
                CarClass = "standard",
                Properties = new Dictionary<string, string>(),
                AvailableLocations = new List<string> { "table_1", "table_2", "bar" }
            },
            new TrainCarDefinition
            {
                CarId = "passenger_02",
                CarType = "passenger",
                CarClass = "second",
                Properties = new Dictionary<string, string>(),
                AvailableLocations = new List<string> { "seat_1", "seat_2", "aisle" }
            }
        };
        
        Dictionary<string, CarConnection> connections = new Dictionary<string, CarConnection>
        {
            { 
                "engine_01", 
                new CarConnection { ConnectedTo = "passenger_01", ConnectionType = "door" } 
            },
            { 
                "passenger_01", 
                new CarConnection { ConnectedTo = "dining_01", ConnectionType = "door" } 
            },
            { 
                "dining_01", 
                new CarConnection { ConnectedTo = "passenger_02", ConnectionType = "door" } 
            }
        };
        
        return new TrainLayout
        {
            Cars = cars,
            Connections = connections
        };
    }
    
    /// <summary>
    /// Gets all character initial locations from the mystery.
    /// </summary>
    public static Dictionary<string, string> GetCharacterInitialLocations(this Mystery mystery)
    {
        Dictionary<string, string> locations = new Dictionary<string, string>();
        
        if (mystery.Characters == null)
        {
            return locations;
        }
        
        foreach (var characterEntry in mystery.Characters)
        {
            string characterId = characterEntry.Key;
            MysteryCharacter character = characterEntry.Value;
            
            if (!string.IsNullOrEmpty(character.InitialLocation))
            {
                locations[characterId] = character.InitialLocation;
            }
            else
            {
                // Try to extract location from core data
                string location = ExtractLocationFromCharacterCore(character);
                if (!string.IsNullOrEmpty(location))
                {
                    locations[characterId] = location;
                }
                else
                {
                    // Default to first car as a fallback
                    TrainLayout layout = mystery.GetTrainLayout();
                    if (layout != null && layout.Cars.Count > 0)
                    {
                        locations[characterId] = layout.Cars[0].CarId;
                    }
                }
            }
        }
        
        return locations;
    }
    
    /// <summary>
    /// Try to extract location from character core data (like whereabouts).
    /// </summary>
    private static string ExtractLocationFromCharacterCore(MysteryCharacter character)
    {
        if (character.Core?.Whereabouts != null && character.Core.Whereabouts.Count > 0)
        {
            // Use the first whereabout as the initial location
            var firstWhereabout = character.Core.Whereabouts[0];
            if (firstWhereabout.WhereaboutData != null)
            {
                if (!string.IsNullOrEmpty(firstWhereabout.WhereaboutData.Location))
                {
                    return firstWhereabout.WhereaboutData.Location;
                }
                else if (!string.IsNullOrEmpty(firstWhereabout.WhereaboutData.Circumstance))
                {
                    // Try to extract location from circumstance
                    string circumstance = firstWhereabout.WhereaboutData.Circumstance.ToLower();
                    
                    // Look for car-related terms
                    if (circumstance.Contains("dining") || circumstance.Contains("dining car"))
                    {
                        return "dining_01";
                    }
                    else if (circumstance.Contains("passenger") || circumstance.Contains("passenger car"))
                    {
                        return "passenger_01";
                    }
                    else if (circumstance.Contains("engine") || circumstance.Contains("engine car"))
                    {
                        return "engine_01";
                    }
                    // Add more location extraction logic as needed
                }
            }
        }
        
        return string.Empty;
    }
}