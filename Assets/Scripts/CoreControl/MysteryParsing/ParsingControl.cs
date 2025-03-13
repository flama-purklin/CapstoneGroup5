using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using Newtonsoft.Json;




//This class will be used to parse the giant mystery json into usable game objects 
public class ParsingControl : MonoBehaviour
{
    public string mysteryFiles = "MysteryStorage";

    private void Awake()
    {
        ParseMystery();
    }

    public void ParseMystery()
    {
        //retrieve the mystery json from Streaming Assets
        string mysteryPath = Path.Combine(Application.streamingAssetsPath, mysteryFiles);
        if (!Directory.Exists(mysteryPath))
        {
            Debug.LogError($"Mystery folder not found at: {mysteryPath}");
            return;
        }

        
        //expand this later into a full mystery selection area
        var foundMysteries = Directory.GetFiles(mysteryPath, "*.json")
            .ToArray();

        string firstMystery = foundMysteries[0];

        Debug.Log("First Found Mystery at: " + firstMystery);

        //read json to a parsable string
        string jsonContent = File.ReadAllText(firstMystery);

        //create a core mystery object with all information stored within - stored in the game controller for easy access throughout the game
        GameControl.GameController.coreMystery = JsonConvert.DeserializeObject<Mystery>(jsonContent);
        GameControl.GameController.coreConstellation = GameControl.GameController.coreMystery.Constellation;
        
        //output all node ids - WORKS
        foreach (var node in GameControl.GameController.coreConstellation.Nodes)
        {
            Debug.Log("Node Id:" + node.Key);
        }

        foreach (var character in GameControl.GameController.coreMystery.Characters)
        {
            //check that all wherabouts are properly deserialized
            foreach (var whereabout in character.Value.Core.Whereabouts)
            {
                string value = whereabout.WhereaboutData.Circumstance ?? whereabout.WhereaboutData.Location;
                Debug.Log("Character Key: " + character.Key + " Whereabout #" + whereabout.Key + ": " + value);
            }

            //check that all relationships are properly deserialized
            foreach (var relationship in character.Value.Core.Relationships)
            {
                Debug.Log("Character Key: " + character.Key + " Relationship: " + relationship.CharName);
            }
            
        }
    }
}
