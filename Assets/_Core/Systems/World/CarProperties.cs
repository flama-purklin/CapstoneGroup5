using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Component to store custom properties for a train car.
/// </summary>
public class CarProperties : MonoBehaviour
{
    private Dictionary<string, string> properties = new Dictionary<string, string>();
    
    /// <summary>
    /// Sets all properties at once.
    /// </summary>
    public void SetProperties(Dictionary<string, string> props)
    {
        properties = new Dictionary<string, string>(props);
    }
    
    /// <summary>
    /// Gets a property value by key.
    /// </summary>
    public string GetProperty(string key, string defaultValue = "")
    {
        if (properties.TryGetValue(key, out string value))
        {
            return value;
        }
        
        return defaultValue;
    }
    
    /// <summary>
    /// Checks if a property exists.
    /// </summary>
    public bool HasProperty(string key)
    {
        return properties.ContainsKey(key);
    }
    
    /// <summary>
    /// Sets a single property.
    /// </summary>
    public void SetProperty(string key, string value)
    {
        properties[key] = value;
    }
    
    /// <summary>
    /// Gets all property keys.
    /// </summary>
    public List<string> GetPropertyKeys()
    {
        return new List<string>(properties.Keys);
    }
    
    /// <summary>
    /// Gets all properties as a dictionary.
    /// </summary>
    public Dictionary<string, string> GetAllProperties()
    {
        return new Dictionary<string, string>(properties);
    }
}
