using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Extension methods for the Mystery class to simplify working with train layouts
/// and character locations.
/// </summary>
public static class MysteryExtensions
{
    /// <summary>
    /// Gets the train layout from the mystery. If the mystery doesn't have an explicit
    /// train layout, attempts to generate one from environment data.
    /// </summary>
    public static TrainLayout GetTrainLayout(this Mystery mystery)
    {
        // Check if we already have a train layout
        if (mystery.TrainLayout != null && mystery.TrainLayout.Cars != null && mystery.TrainLayout.Cars.Count > 0)
        {
            return mystery.TrainLayout;
        }
        
        // Otherwise, try to generate one from environment data
        return GenerateTrainLayoutFromEnvironment(mystery);
    }
    
    /// <summary>
    /// Gets initial locations for all characters in the mystery.
    /// </summary>
    public static Dictionary<string, string> GetCharacterInitialLocations(this Mystery mystery)
    {
        Dictionary<string, string> locations = new Dictionary<string, string>();
        
        if (mystery.Characters == null)
        {
            Debug.LogWarning("Mystery has no characters");
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
                Debug.LogWarning($"Character {characterId} has no initial location specified");
            }
        }
        
        return locations;
    }
    
    /// <summary>
    /// Gets a character by ID from the mystery.
    /// </summary>
    public static MysteryCharacter GetCharacter(this Mystery mystery, string characterId)
    {
        if (mystery.Characters == null)
        {
            return null;
        }
        
        if (mystery.Characters.TryGetValue(characterId, out MysteryCharacter character))
        {
            return character;
        }
        
        return null;
    }
    
    /// <summary>
    /// Gets evidence locations from the mystery's constellation.
    /// </summary>
    public static Dictionary<string, string> GetEvidenceLocations(this Mystery mystery)
    {
        Dictionary<string, string> locations = new Dictionary<string, string>();
        
        if (mystery.Constellation == null || mystery.Constellation.Nodes == null)
        {
            Debug.LogWarning("Mystery has no constellation or nodes");
            return locations;
        }
        
        foreach (var nodeEntry in mystery.Constellation.Nodes)
        {
            string nodeId = nodeEntry.Key;
            MysteryNode node = nodeEntry.Value;
            
            if (node.PhysicalEvidence && !string.IsNullOrEmpty(node.Location))
            {
                locations[nodeId] = node.Location;
            }
        }
        
        return locations;
    }
    
    /// <summary>
    /// Generates a train layout from the mystery's environment data.
    /// This is a fallback if no explicit train layout is defined.
    /// </summary>
    private static TrainLayout GenerateTrainLayoutFromEnvironment(Mystery mystery)
    {
        TrainLayout layout = new TrainLayout
        {
            Cars = new List<TrainCarDefinition>(),
            Connections = new Dictionary<string, CarConnection>()
        };
        
        // Check if we have environment data
        if (mystery.Environment == null)
        {
            Debug.LogError("Mystery has no environment data to generate train layout from");
            
            // Create a default layout with one passenger car
            layout.Cars.Add(new TrainCarDefinition
            {
                CarId = "default_car",
                CarType = "passenger",
                CarClass = "economy",
                AvailableLocations = new List<string> { "entrance", "seat_1", "seat_2", "walkway" }
            });
            
            return layout;
        }
        
        // Try to get train data from environment
        if (mystery.Environment.TrainData != null && mystery.Environment.TrainData.Cars != null)
        {
            // Convert environment train data to train layout
            foreach (var carData in mystery.Environment.TrainData.Cars)
            {
                TrainCarDefinition carDef = new TrainCarDefinition
                {
                    CarId = carData.ID,
                    CarType = carData.Type,
                    CarClass = carData.Class,
                    Properties = carData.Properties,
                    AvailableLocations = carData.Locations
                };
                
                layout.Cars.Add(carDef);
                
                // Set up connections if defined
                if (carData.ConnectedTo != null && carData.ConnectedTo.Count > 0)
                {
                    foreach (string connectedCar in carData.ConnectedTo)
                    {
                        layout.Connections[carData.ID] = new CarConnection
                        {
                            ConnectedTo = connectedCar,
                            ConnectionType = "door"
                        };
                    }
                }
            }
        }
        else
        {
            Debug.LogWarning("Mystery environment has no train data, creating default layout");
            
            // Create a minimal layout based on needed locations
            HashSet<string> requiredLocations = new HashSet<string>();
            
            // Add character locations
            foreach (var character in mystery.Characters.Values)
            {
                if (!string.IsNullOrEmpty(character.InitialLocation))
                {
                    string carId = ParseCarIdFromLocation(character.InitialLocation);
                    requiredLocations.Add(carId);
                }
            }
            
            // Add evidence locations
            if (mystery.Constellation != null && mystery.Constellation.Nodes != null)
            {
                foreach (var node in mystery.Constellation.Nodes.Values)
                {
                    if (node.PhysicalEvidence && !string.IsNullOrEmpty(node.Location))
                    {
                        string carId = ParseCarIdFromLocation(node.Location);
                        requiredLocations.Add(carId);
                    }
                }
            }
            
            // Ensure we have at least one car
            if (requiredLocations.Count == 0)
            {
                requiredLocations.Add("main_car");
            }
            
            // Create cars for all required locations
            int index = 0;
            foreach (string carId in requiredLocations)
            {
                string carType = DetermineCarType(carId);
                
                TrainCarDefinition carDef = new TrainCarDefinition
                {
                    CarId = carId,
                    CarType = carType,
                    CarClass = "standard",
                    AvailableLocations = GenerateDefaultLocations(carType)
                };
                
                layout.Cars.Add(carDef);
                
                // Connect adjacent cars
                if (index > 0)
                {
                    string prevCarId = layout.Cars[index - 1].CarId;
                    
                    layout.Connections[prevCarId] = new CarConnection
                    {
                        ConnectedTo = carId,
                        ConnectionType = "door"
                    };
                }
                
                index++;
            }
        }
        
        return layout;
    }
    
    /// <summary>
    /// Parses a car ID from a location string.
    /// </summary>
    private static string ParseCarIdFromLocation(string location)
    {
        if (string.IsNullOrEmpty(location))
        {
            return "main_car";
        }
        
        // If the location contains a dot, the part before the dot is the car ID
        if (location.Contains("."))
        {
            return location.Split('.')[0];
        }
        
        // If no dot, the whole string is the car ID
        return location;
    }
    
    /// <summary>
    /// Determines an appropriate car type based on its ID.
    /// </summary>
    private static string DetermineCarType(string carId)
    {
        string lowerId = carId.ToLower();
        
        if (lowerId.Contains("dining") || lowerId.Contains("restaurant"))
        {
            return "dining";
        }
        else if (lowerId.Contains("bar") || lowerId.Contains("lounge"))
        {
            return "bar";
        }
        else if (lowerId.Contains("kitchen"))
        {
            return "kitchen";
        }
        else if (lowerId.Contains("storage") || lowerId.Contains("cargo"))
        {
            return "storage";
        }
        else if (lowerId.Contains("engine") || lowerId.Contains("locomotive"))
        {
            return "engine";
        }
        
        // Default to passenger car
        return "passenger";
    }
    
    /// <summary>
    /// Generates default locations for a car type.
    /// </summary>
    private static List<string> GenerateDefaultLocations(string carType)
    {
        switch (carType.ToLower())
        {
            case "dining":
                return new List<string>
                {
                    "entrance",
                    "table_1",
                    "table_2",
                    "table_3",
                    "table_4",
                    "counter",
                    "walkway"
                };
                
            case "bar":
                return new List<string>
                {
                    "entrance",
                    "bar_counter",
                    "table_1",
                    "table_2",
                    "lounge_area",
                    "walkway"
                };
                
            case "kitchen":
                return new List<string>
                {
                    "entrance",
                    "cooking_area",
                    "prep_area",
                    "storage_area",
                    "walkway"
                };
                
            case "storage":
                return new List<string>
                {
                    "entrance",
                    "shelf_1",
                    "shelf_2",
                    "crate_area",
                    "walkway"
                };
                
            case "engine":
                return new List<string>
                {
                    "entrance",
                    "control_panel",
                    "engine_room",
                    "walkway"
                };
                
            case "passenger":
            default:
                return new List<string>
                {
                    "entrance",
                    "seat_1",
                    "seat_2",
                    "seat_3",
                    "seat_4",
                    "window_area",
                    "walkway"
                };
        }
    }
}