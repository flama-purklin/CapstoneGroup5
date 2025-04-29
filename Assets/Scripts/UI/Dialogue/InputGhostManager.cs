using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InputGhostManager : MonoBehaviour
{
    [Tooltip("TextMeshProUGUI used to display the ghost text")]
    [SerializeField] private TextMeshProUGUI ghostText;
    [SerializeField] private string prefix = "You said: \"";
    [SerializeField] private string suffix = "\"";
    
    // Store the last message to persist it
    private string lastPlayerMessage = string.Empty;

    private void Awake()
    {
        if (ghostText == null)
        {
            // Attempt to find the TextMeshProUGUI component, preferably on this GameObject
            ghostText = GetComponent<TextMeshProUGUI>();
            if (ghostText == null)
            {
                ghostText = GetComponentInChildren<TextMeshProUGUI>();
                Debug.Log($"[InputGhostManager] Awake - Found ghostText in children: {(ghostText != null)}");
            } else {
                 Debug.Log($"[InputGhostManager] Awake - Found ghostText on self: {(ghostText != null)}");
            }

            if (ghostText == null) {
                Debug.LogError("[InputGhostManager] Awake - Failed to find ghostText component!");
            }
        }
        
        // Ensure the text component is enabled, but start with empty text
        if (ghostText != null) {
            ghostText.enabled = true; // Keep it enabled always
            ghostText.text = string.Empty; // Start empty
            Color textColor = ghostText.color;
            textColor.a = 1f; // Keep it fully opaque always
            ghostText.color = textColor;
            Debug.Log("[InputGhostManager] Awake - Ensured ghostText is enabled and cleared.");
        }
        
        // Clear the internal message state too
        lastPlayerMessage = string.Empty;
    }

    // Start() is likely unnecessary now, but keep for potential future debug
    private void Start()
    {
        Debug.Log($"[InputGhostManager] Start - GameObject active: {gameObject.activeInHierarchy}, ghostText active and enabled: {ghostText?.enabled}");
    }

    /// <summary>
    /// Shows the last player input in the ghost label.
    /// </summary>
    public void SetGhostText(string playerInput)
    {
        if (string.IsNullOrWhiteSpace(playerInput))
        {
            Debug.LogWarning("[InputGhostManager] SetGhostText called with empty text, clearing ghost");
            ClearGhost();
            return;
        }

        lastPlayerMessage = playerInput;
        // Debug.Log($"[InputGhostManager] Setting ghost text to: '{playerInput}'"); // Reduced verbosity
        
        if (ghostText != null)
        {
            // Directly set the text. Assume component and GameObject are always active & visible.
            ghostText.text = prefix + playerInput + suffix;
            Debug.Log($"[InputGhostManager] Ghost text set to: '{ghostText.text}'");
        }
        else
        {
            Debug.LogError("[InputGhostManager] ghostText reference is null! Cannot set text.");
        }
    }

    /// <summary>
    /// Clears and hides the ghost label.
    /// </summary>
    public void ClearGhost()
    {
        lastPlayerMessage = string.Empty;
        
        if (ghostText != null)
        {
            // Only clear the text content. Keep component enabled and visible.
            ghostText.text = string.Empty;
            Debug.Log("[InputGhostManager] Ghost text cleared");
        }
        else
        {
            Debug.LogWarning("[InputGhostManager] ghostText reference is null! Cannot clear.");
        }
    }
    
    // REMOVED ForceShowGhostText as it's no longer needed with the simplified always-visible approach.
    
    // Debug method to check state from outside
    public void LogCurrentState()
    {
        Debug.Log($"[InputGhostManager] STATE CHECK - gameObject active: {gameObject.activeInHierarchy}");
        Debug.Log($"[InputGhostManager] STATE CHECK - ghostText reference exists: {ghostText != null}");
        
        if (ghostText != null)
        {
            Debug.Log($"[InputGhostManager] STATE CHECK - ghostText.enabled: {ghostText.enabled}");
            Debug.Log($"[InputGhostManager] STATE CHECK - ghostText color alpha: {ghostText.color.a}");
        }
        
        Debug.Log($"[InputGhostManager] STATE CHECK - lastPlayerMessage: '{lastPlayerMessage}'");
        Debug.Log($"[InputGhostManager] STATE CHECK - current text: '{ghostText?.text}'");
    }

    /// <summary>
    /// Returns the current text being displayed in the ghost. Useful for debugging.
    /// </summary>
    public string GetCurrentText()
    {
        return ghostText != null ? ghostText.text : string.Empty;
    }
}
