using UnityEngine;
using TMPro;

// This script updates the NotebookText to show Mystery Map instructions
public class NotebookTextUpdater : MonoBehaviour
{
    void Awake()
    {
        // Find all TextMeshProUGUI components in child objects named "NotebookText"
        TextMeshProUGUI[] texts = GetComponentsInChildren<TextMeshProUGUI>(true);
        foreach (var text in texts)
        {
            if (text.gameObject.name == "NotebookText")
            {
                // Change the text to show Mystery Map instructions
                text.text = "Press M for Mystery Map";
            }
        }
    }
}