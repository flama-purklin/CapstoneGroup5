using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.EventSystems; // Ensure input works in UI

public class NotebookManager : MonoBehaviour
{
    public GameObject notebookCanvas;  // Assign the Notebook UI
    public TMP_Text notebookText;      // Assign the text element in the notebook

    private bool isNotebookOpen = false;
    private int offsetIndex = 0; // Tracks scrolling position (only for first page)
    private int currentPage = 0; // Tracks the current page
    private const int maxVisibleEntries = 5; // Number of entries visible per page
    private const int totalPages = 2; // Adjust based on total pages needed

    private float scrollCooldown = 0.1f; // Reduced delay for smoother scrolling
    private float nextScrollTime = 0f;  

    private List<string> notebookEntries = new List<string>
    {
        "- Talk to Nova Winchester",
        "- Talk to NPC #2",
        "- Talk to NPC #3",
        "- Found the Wrench",
        "- Found the Key",
        "- Unlocked the Door",
        "- Talk to Officer Carter",
        "- Talk to Timmy",
        "- Found the Suitcase"
    };

    void Start()
    {
        // Ensure notebook starts hidden
        notebookCanvas.SetActive(false);
        isNotebookOpen = false;
    }

    void Update()
    {
        // Open/Close notebook with "N" but only if not typing in a dialogue field
        if (Input.GetKeyDown(KeyCode.N) && !IsTypingInDialogue())
        {
            ToggleNotebook();
        }

        if (isNotebookOpen && EventSystem.current.currentSelectedGameObject == null)
        {
            // Scrolling only on first page
            if (currentPage == 0 && Time.time >= nextScrollTime)
            {
                if (Input.GetKey(KeyCode.K) && offsetIndex < notebookEntries.Count - maxVisibleEntries)
                {
                    offsetIndex++;
                    UpdateNotebookDisplay();
                    nextScrollTime = Time.time + scrollCooldown;
                }

                if (Input.GetKey(KeyCode.I) && offsetIndex > 0)
                {
                    offsetIndex--;
                    UpdateNotebookDisplay();
                    nextScrollTime = Time.time + scrollCooldown;
                }
            }

            // Page navigation: "L" for next, "J" for previous
            if (Input.GetKeyDown(KeyCode.L) && currentPage < totalPages - 1)
            {
                currentPage++;
                offsetIndex = 0; // Reset scrolling when changing pages
                UpdateNotebookDisplay();
            }

            if (Input.GetKeyDown(KeyCode.J) && currentPage > 0)
            {
                currentPage--;
                offsetIndex = 0; // Reset scrolling when changing pages
                UpdateNotebookDisplay();
            }
        }

        // Example: Modify specific lines when different keys are pressed
        if (Input.GetKeyDown(KeyCode.Alpha1)) 
            ReplaceNotebookEntry(0, "- T̶a̶l̶k̶ ̶t̶o̶ ̶N̶o̶v̶a̶ ̶W̶i̶n̶c̶h̶e̶s̶t̶e̶r̶");

        if (Input.GetKeyDown(KeyCode.Alpha2)) 
            ReplaceNotebookEntry(1, "- T̶a̶l̶k̶ ̶t̶o̶ ̶N̶P̶C̶ ̶#̶2̶");

        if (Input.GetKeyDown(KeyCode.Alpha3)) 
            ReplaceNotebookEntry(2, "- T̶a̶l̶k̶ ̶t̶o̶ ̶N̶P̶C̶ ̶#̶3̶");
    }

    void ToggleNotebook()
    {
        isNotebookOpen = !isNotebookOpen;
        notebookCanvas.SetActive(isNotebookOpen);

        if (isNotebookOpen)
        {
            currentPage = 0;  // Reset to first page
            offsetIndex = 0;  // Reset scrolling
            UpdateNotebookDisplay(); // Ensure objectives show immediately
        }
    }

    void UpdateNotebookDisplay()
    {
        if (currentPage == 0)
        {
            // Page 1: Display scrollable objectives
            notebookText.text = string.Join("\n", notebookEntries.GetRange(offsetIndex, Mathf.Min(maxVisibleEntries, notebookEntries.Count - offsetIndex)));
        }
        else if (currentPage == 1)
        {
            // Page 2: Character Bio
            notebookText.text = "📖 Character Bio 📖\n- Nova Winchester\n- Stuff about Nova Winchester\n";
        }
    }

    public void ReplaceNotebookEntry(int index, string newText)
    {
        if (index >= 0 && index < notebookEntries.Count)
        {
            notebookEntries[index] = newText;
            UpdateNotebookDisplay();
        }
        else
        {
            Debug.LogWarning("Invalid index: " + index);
        }
    }
    
    private bool IsTypingInDialogue()
    {
        // Check if an input field is currently selected/focused
        if (EventSystem.current.currentSelectedGameObject != null)
        {
            TMP_InputField inputField = EventSystem.current.currentSelectedGameObject.GetComponent<TMP_InputField>();
            if (inputField != null && inputField.isFocused)
            {
                return true;
            }
            
            // Also check for legacy InputField if used
            InputField legacyInput = EventSystem.current.currentSelectedGameObject.GetComponent<InputField>();
            if (legacyInput != null && legacyInput.isFocused)
            {
                return true;
            }
        }
        
        // Check if we're in dialogue state
        return GameControl.GameController.currentState == GameState.DIALOGUE;
    }
}