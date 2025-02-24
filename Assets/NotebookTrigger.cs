using UnityEngine;

public class NotebookTrigger : MonoBehaviour
{
    public GameObject NotebookCanvas;  // Assign in Inspector
    public GameObject DimBackground;   // Assign in Inspector

    void Start()
    {
        if (NotebookCanvas != null) NotebookCanvas.SetActive(false);
        if (DimBackground != null) DimBackground.SetActive(false);
    }

    void Update()
    {
        // Prevent notebook from opening if the game is paused
        if (PauseMenu.GameIsPaused) return;

        if (Input.GetKeyDown(KeyCode.N)) // Detect 'N' key press
        {
            bool isActive = !NotebookCanvas.activeSelf;
            NotebookCanvas.SetActive(isActive);
            if (DimBackground != null) DimBackground.SetActive(isActive);
        }
    }
}
