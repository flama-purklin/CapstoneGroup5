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
            // Since the TextMeshProUGUI is on the same GameObject, try to get component directly
            ghostText = GetComponent<TextMeshProUGUI>();
            Debug.Log($"[InputGhostManager] Awake - Found ghostText on self: {(ghostText != null)}");
            
            // If still not found, look in children
            if (ghostText == null)
            {
                ghostText = GetComponentInChildren<TextMeshProUGUI>();
                Debug.Log($"[InputGhostManager] Awake - Found ghostText in children: {(ghostText != null)}");
            }
        }
        
        // Make sure we start clean
        ClearGhost();
    }

    private void Start()
    {
        Debug.Log($"[InputGhostManager] Start - GameObject active: {gameObject.activeInHierarchy}, ghostText active: {ghostText?.gameObject.activeInHierarchy}");
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
        Debug.Log($"[InputGhostManager] Setting ghost text to: '{playerInput}'");
        
        if (ghostText != null)
        {
            // Make sure text is empty before setting new text - fixes stuck text issues
            ghostText.text = string.Empty;
            
            // Ensure this GameObject is active
            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
                Debug.Log("[InputGhostManager] Activated ghost GameObject");
            }
            
            // Set the text with prefix and suffix
            ghostText.text = prefix + playerInput + suffix;
            
            // Ensure text is visible
            ghostText.enabled = true;
            Color textColor = ghostText.color;
            textColor.a = 1f;
            ghostText.color = textColor;
            
            // Force layout rebuild to ensure proper sizing
            Canvas.ForceUpdateCanvases(); // Force immediate update of all canvases
            if (transform.parent is RectTransform parentRect)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
                LayoutRebuilder.ForceRebuildLayoutImmediate(parentRect);
            }
            
            Debug.Log($"[InputGhostManager] Ghost text set to: '{prefix + playerInput + suffix}'");
            Debug.Log($"[InputGhostManager] Text color alpha: {ghostText.color.a}, enabled: {ghostText.enabled}");
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
            ghostText.text = string.Empty;
            
            // Hide by disabling the component and setting alpha to 0
            ghostText.enabled = false;
            Color textColor = ghostText.color;
            textColor.a = 0f;
            ghostText.color = textColor;
            
            Debug.Log("[InputGhostManager] Ghost text cleared and hidden");
        }
        else
        {
            Debug.LogWarning("[InputGhostManager] ghostText reference is null! Cannot clear.");
        }
    }
    
    /// <summary>
    /// Forces the ghost text to be visible if there's a stored message.
    /// </summary>
    public void ForceShowGhostText()
    {
        Debug.Log($"[InputGhostManager] ForceShowGhostText called. lastPlayerMessage: '{lastPlayerMessage}'");
        
        if (!string.IsNullOrEmpty(lastPlayerMessage) && ghostText != null)
        {
            // Clear text first to ensure refresh
            ghostText.text = string.Empty;
            
            // Make sure GameObject is active
            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
                Debug.Log("[InputGhostManager] Activated InputGhost GameObject");
            }
            
            // Set text
            ghostText.text = prefix + lastPlayerMessage + suffix;
            
            // Ensure text is visible
            ghostText.enabled = true;
            Color textColor = ghostText.color;
            textColor.a = 1f;
            ghostText.color = textColor;
            
            // Force layout rebuild for all canvases to ensure visibility
            Canvas.ForceUpdateCanvases(); // Force immediate update of all canvases
            if (transform.parent is RectTransform parentRect)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
                LayoutRebuilder.ForceRebuildLayoutImmediate(parentRect);
            }
            
            Debug.Log($"[InputGhostManager] Force-showed ghost text: '{prefix + lastPlayerMessage + suffix}'");
            Debug.Log($"[InputGhostManager] Text color alpha: {ghostText.color.a}, enabled: {ghostText.enabled}");
        }
    }
    
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
