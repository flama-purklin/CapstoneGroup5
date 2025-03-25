using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace MysteryEngine.Implementation
{
    /// <summary>
    /// Represents the layout of a train in the mystery.
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
    /// Data class for car connections in train layout.
    /// </summary>
    [System.Serializable]
    public class CarConnection
    {
        [JsonProperty("connected_to")]
        public string ConnectedTo { get; set; }
        
        [JsonProperty("connection_type")]
        public string ConnectionType { get; set; } = "door";
    }

    // Extensions for Mystery class
    public static class MysteryTrainExtensions
    {
        /// <summary>
        /// Gets the train layout from a mystery.
        /// </summary>
        public static TrainLayout GetTrainLayout(this Mystery mystery)
        {
            // First check if direct train_layout is available
            if (mystery.TrainLayout != null && mystery.TrainLayout.Cars != null && mystery.TrainLayout.Cars.Count > 0)
            {
                return mystery.TrainLayout;
            }
            
            // Fall back to default layout
            Debug.LogWarning("No train layout found in mystery. Creating default layout.");
            return CreateDefaultTrainLayout();
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
                    CarId = "engine_car",
                    CarType = "engine",
                    CarClass = "standard",
                    Properties = new Dictionary<string, string>(),
                    AvailableLocations = new List<string> { "control_room", "engine_area" }
                },
                new TrainCarDefinition
                {
                    CarId = "passenger_car",
                    CarType = "passenger",
                    CarClass = "first",
                    Properties = new Dictionary<string, string>(),
                    AvailableLocations = new List<string> { "seat_1", "seat_2", "aisle" }
                },
                new TrainCarDefinition
                {
                    CarId = "dining_car",
                    CarType = "dining",
                    CarClass = "standard",
                    Properties = new Dictionary<string, string>(),
                    AvailableLocations = new List<string> { "table_1", "table_2", "bar" }
                }
            };
            
            Dictionary<string, CarConnection> connections = new Dictionary<string, CarConnection>
            {
                { 
                    "engine_car", 
                    new CarConnection { ConnectedTo = "passenger_car", ConnectionType = "door" } 
                },
                { 
                    "passenger_car", 
                    new CarConnection { ConnectedTo = "dining_car", ConnectionType = "door" } 
                }
            };
            
            return new TrainLayout
            {
                Cars = cars,
                Connections = connections
            };
        }
        
        /// <summary>
        /// Gets character initial locations from the mystery.
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
                    // Default to first car as a fallback
                    TrainLayout layout = mystery.GetTrainLayout();
                    if (layout != null && layout.Cars.Count > 0)
                    {
                        locations[characterId] = layout.Cars[0].CarId;
                    }
                }
            }
            
            return locations;
        }
        
        /// <summary>
        /// Gets a character from the mystery by ID.
        /// </summary>
        public static MysteryCharacter GetCharacter(this Mystery mystery, string characterId)
        {
            if (mystery.Characters != null && mystery.Characters.TryGetValue(characterId, out MysteryCharacter character))
            {
                return character;
            }
            
            return null;
        }
    }
}