using UnityEngine;
using Newtonsoft.Json.Linq;
using UnityEditor;
using System.IO;

namespace UnityMCP.Editor.Commands
{
    /// <summary>
    /// Stub implementation of material-related commands
    /// The actual Material functionality has been removed to eliminate URP dependencies
    /// </summary>
    public static class MaterialCommandHandler
    {
        /// <summary>
        /// Stub implementation that returns a friendly error message.
        /// The actual implementation required URP and has been removed.
        /// </summary>
        public static object SetMaterial(JObject @params)
        {
            string objectName = (string)@params["object_name"];
            var obj = GameObject.Find(objectName);
            
            if (obj == null)
            {
                return new { success = false, error = $"Object '{objectName}' not found." };
            }
            
            var renderer = obj.GetComponent<Renderer>();
            if (renderer == null)
            {
                return new { success = false, error = $"Object '{objectName}' has no renderer." };
            }
            
            return new { 
                success = false, 
                error = "Material functionality has been removed to eliminate URP dependencies. " +
                        "Please use the standard Unity editor to manage materials or implement a custom " +
                        "material handler compatible with your rendering pipeline."
            };
        }
    }
}