using UnityEngine;
using TMPro;
using System.Text;
using System.Globalization;

public class CurrentCarDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI carNameText;
    private CarDetection playerCarDetection;
    private TextInfo textInfo = new CultureInfo("en-US", false).TextInfo; // For title casing

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

        // Remove "Traincar #" prefix if present
        string cleanId = id;
        if (cleanId.StartsWith("Traincar"))
        {
            int indexOfSeparator = cleanId.IndexOf(' ', 9); // Look for space after potential number
            if (indexOfSeparator > 0)
            {
                cleanId = cleanId.Substring(indexOfSeparator + 1);
            }
        }

        // Replace underscores with spaces
        string spacedId = cleanId.Replace('_', ' ');

        // Apply title case
        string titleCasedId = textInfo.ToTitleCase(spacedId);

        return titleCasedId;
    }
}
