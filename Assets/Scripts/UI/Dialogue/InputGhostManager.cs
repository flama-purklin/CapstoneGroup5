using TMPro;
using UnityEngine;

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
        ClearGhost();
    }

    private void Start()
    {
        Debug.Log($"[InputGhostManager] Start - GameObject active: {gameObject.activeInHierarchy}, ghostText active: {ghostText?.gameObject.activeInHierarchy}");
        
        // Force GameObject active - this is for testing only
        if (!gameObject.activeInHierarchy)
        {
            gameObject.SetActive(true);
            Debug.Log($"[InputGhostManager] Start - Forced GameObject active: {gameObject.activeInHierarchy}");
        }
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

        // Store the message for later use
        lastPlayerMessage = playerInput;
        
        Debug.Log($"[InputGhostManager] Setting ghost text to: '{playerInput}'");
        
        if (ghostText != null)
        {
            // Make sure this GameObject is active
            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
                Debug.Log("[InputGhostManager] Activated self GameObject");
            }
            
            // Set the text (the TextMeshProUGUI component is directly on this GameObject)
            ghostText.text = prefix + playerInput + suffix;
            
            // Make text visible by setting alpha to 1
            Color textColor = ghostText.color;
            textColor.a = 1f;
            ghostText.color = textColor;
            
            // Ensure the TextMeshProUGUI component is enabled
            ghostText.enabled = true;
            
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
            
            // Hide by setting alpha to 0 instead of deactivating GameObject
            Color textColor = ghostText.color;
            textColor.a = 0f;
            ghostText.color = textColor;
            
            Debug.Log("[InputGhostManager] Ghost text cleared and alpha set to 0");
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
            // Make sure GameObject is active
            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
                Debug.Log("[InputGhostManager] Activated InputGhost GameObject");
            }
            
            // Set text
            ghostText.text = prefix + lastPlayerMessage + suffix;
            
            // Ensure text is visible with alpha = 1
            Color textColor = ghostText.color;
            textColor.a = 1f;
            ghostText.color = textColor;
            
            // Make sure TextMeshProUGUI component is enabled
            ghostText.enabled = true;
            
            Debug.Log($"[InputGhostManager] Force-showed ghost text: '{prefix + lastPlayerMessage + suffix}'");
            Debug.Log($"[InputGhostManager] Text color alpha: {ghostText.color.a}, enabled: {ghostText.enabled}");
        }
        else
        {
            Debug.LogWarning($"[InputGhostManager] Cannot force show ghost text. lastPlayerMessage empty: {string.IsNullOrEmpty(lastPlayerMessage)}, ghostText null: {ghostText == null}");
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
}
