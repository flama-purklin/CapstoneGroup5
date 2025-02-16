using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class CharacterSelector : MonoBehaviour
{
    [Header("UI References")]
    public Button characterButton;
    public UIController uiController;

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
        UpdateUIController();
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
            uiController.llmCharacter = character;
            uiController.SetCurrentCharacter(characterName);
            uiController.ResetChat();
            uiController.UpdateCharacterStatus($"Talking to {characterName}");
            Debug.Log($"Successfully switched to character: {characterName}");
        }
        else
        {
            uiController.UpdateCharacterStatus("Failed to switch character");
            Debug.LogError($"Failed to switch to character: {characterName}");
        }

        SetButtonState(true);
    }

    private void UpdateUIController()
    {
        if (currentCharacterIndex >= 0 && currentCharacterIndex < characterNames.Length)
        {
            uiController.UpdateCharacterStatus($"Ready to talk with any character");
            characterButton.image.color = buttonColor;
        }
        else
        {
            uiController.UpdateCharacterStatus("No character selected");
            characterButton.image.color = buttonColor;
        }
    }
}