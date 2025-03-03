using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class CharacterSelector : MonoBehaviour
{
    [Header("UI References")]
    public Button characterButton;
    public DemoDialogueManager dialogueManager;

    [Header("Configuration")]
    public Color buttonColor = new Color(0.2f, 0.2f, 0.2f);
    public Color disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

    [Header("References")]
    public CharacterManager characterManager;

    private string[] characterNames;
    private int currentCharacterIndex = -1;
    private bool isReady = false;

    void Start()
    {
        InitializeButton();
        characterManager.OnInitializationComplete += OnCharacterManagerInitialized;
    }

    void OnDestroy()
    {
        if (characterManager != null)
        {
            characterManager.OnInitializationComplete -= OnCharacterManagerInitialized;
        }
    }

    private void OnCharacterManagerInitialized()
    {
        characterNames = characterManager.GetAvailableCharacters();
        isReady = true;
        SetButtonState(true);
        UpdateDialogueManager();
    }

    private void InitializeButton()
    {
        if (characterButton)
        {
            TMP_Text buttonText = characterButton.GetComponentInChildren<TMP_Text>();
            if (buttonText) buttonText.text = "CHANGE CHARACTER";

            characterButton.onClick.AddListener(OnCharacterButtonClick);
            SetButtonState(false);
            characterButton.image.color = buttonColor;
        }
        else
        {
            Debug.LogError("Character button not assigned!");
        }
    }

    private void SetButtonState(bool enabled)
    {
        if (characterButton)
        {
            characterButton.interactable = enabled;
            Color newColor = buttonColor;
            newColor.a = enabled ? 1f : 0.5f;
            characterButton.image.color = newColor;
        }
    }

    private async void OnCharacterButtonClick()
    {
        if (!isReady || characterNames.Length == 0)
        {
            Debug.LogWarning("Character selector not ready or no characters available");
            return;
        }

        SetButtonState(false);
        currentCharacterIndex = (currentCharacterIndex + 1) % characterNames.Length;
        string characterName = characterNames[currentCharacterIndex];

        Debug.Log($"Selector attempting to switch to character: {characterName}");

        var character = await characterManager.SwitchToCharacter(characterName);
        if (character != null)
        {
            dialogueManager.SetCharacter(character);
            dialogueManager.SetCurrentCharacter(characterName);
            await dialogueManager.ResetDialogue();
            dialogueManager.UpdateCharacterStatus($"Talking to {characterName}");
            Debug.Log($"Successfully switched to character: {characterName}");

            // Re-initialize dialogue and enable input
            dialogueManager.InitializeDialogue();
            dialogueManager.OnWarmupComplete(); // This will re-enable input
        }
        else
        {
            dialogueManager.UpdateCharacterStatus("Failed to switch character");
            Debug.LogError($"Failed to switch to character: {characterName}");
        }

        SetButtonState(true);
    }

    private void UpdateDialogueManager()
    {
        if (currentCharacterIndex >= 0 && currentCharacterIndex < characterNames.Length)
        {
            dialogueManager.UpdateCharacterStatus($"Ready to talk with any character");
            characterButton.image.color = buttonColor;
        }
        else
        {
            dialogueManager.UpdateCharacterStatus("No character selected");
            characterButton.image.color = buttonColor;
        }
    }
}