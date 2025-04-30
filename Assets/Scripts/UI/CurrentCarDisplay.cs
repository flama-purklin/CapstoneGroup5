using UnityEngine;
using TMPro;
using System.Text;
using System.Globalization;
using System.Text.RegularExpressions;

public class CurrentCarDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI carNameText;
    private CarDetection playerCarDetection;
    private TextInfo textInfo = new CultureInfo("en-US", false).TextInfo; // For title casing
    
    // Regex pattern that matches either:
    // 1. Any variation of "Traincar"/"Train Car" followed by optional digits and spaces
    // 2. Numbers at the start of the name followed by spaces
    private static readonly Regex prefixPattern = new Regex(@"^((?:traincar|train\s*car)\s*\d*\s*|\d+\s+)", RegexOptions.IgnoreCase);

    void Start()
    {
        // Find the Player object (assuming it has the "Player" tag)
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            playerCarDetection = playerObject.GetComponent<CarDetection>();
            if (playerCarDetection == null)
            {
                Debug.LogError("CurrentCarDisplay: Player object found, but CarDetection component is missing!");
            }
        }
        else
        {
            Debug.LogError("CurrentCarDisplay: Could not find GameObject with tag 'Player'!");
        }

        if (carNameText == null)
        {
            Debug.LogError("CurrentCarDisplay: Car Name TextMeshProUGUI reference not set in the inspector!");
        }
    }

    void Update()
    {
        if (playerCarDetection == null || carNameText == null)
        {
            // Don't proceed if setup failed
            return;
        }

        CarVisibility currentCar = playerCarDetection.GetCurrentCar();

        if (currentCar != null)
        {
            string carId = currentCar.gameObject.name;
            string formattedName = FormatCarName(carId);
            carNameText.text = formattedName;
        }
        else
        {
            // Optional: What to display when between cars or detection fails
            carNameText.text = ""; // Or "Transitioning..." or similar
        }
    }

    private string FormatCarName(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return "";
        }

        // Remove prefix pattern (Traincar #, Train Car #, or just # at the start)
        string cleanId = prefixPattern.Replace(id, "");
        
        // If the removal left nothing, return the original ID
        if (string.IsNullOrWhiteSpace(cleanId))
        {
            cleanId = id;
        }

        // Replace underscores with spaces
        string spacedId = cleanId.Replace('_', ' ');

        // Apply title case
        string titleCasedId = textInfo.ToTitleCase(spacedId);

        return titleCasedId.Trim();
    }
}
