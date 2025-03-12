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

        //create a core mystery object with all information stored within
        Mystery fullMystery = JsonConvert.DeserializeObject<Mystery>(jsonContent);
        MysteryConstellation constellation = fullMystery.Constellation;
        
        //output all node ids - WORKS
        foreach (var node in constellation.Nodes)
        {
            Debug.Log("Node Id:" + node.Key);
        }
    }
}
