using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System.Threading.Tasks;
using System.IO;
using LLMUnity;

public class CharacterSelector : MonoBehaviour
{
    [Header("UI References")]
    public Button characterButton;
    public UIController uiController;

    [Header("Configuration")]
    public string charactersFolder = "Characters";
    public Color buttonColor = new Color(0.2f, 0.2f, 0.2f);
    public Color disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

    [Header("LLM Reference")]
    public LLMCharacter llmCharacter;

    private string[] characterFiles;
    private int currentCharacterIndex = -1;
    private bool isLLMReady = false;

    void Start()
    {
        InitializeButton();
        LoadCharacterFiles();
        StartCoroutine(WaitForLLM());
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
        characterButton.interactable = enabled;
        Color newColor = buttonColor;
        newColor.a = enabled ? 1f : 0.5f;
        characterButton.image.color = newColor;
    }

    private void LoadCharacterFiles()
    {
        string characterPath = Path.Combine(Application.streamingAssetsPath, charactersFolder);
        Debug.Log($"Looking for characters in: {characterPath}");

        if (Directory.Exists(characterPath))
        {
            characterFiles = Directory.GetFiles(characterPath, "*.json")
                                    .Select(path => Path.GetFileNameWithoutExtension(path))
                                    .ToArray();
            Debug.Log($"Found {characterFiles.Length} characters: {string.Join(", ", characterFiles)}");
        }
        else
        {
            Debug.LogError($"Characters folder not found at: {characterPath}");
            characterFiles = new string[0];
        }

        UpdateUIController();
    }

    private System.Collections.IEnumerator WaitForLLM()
    {
        uiController.UpdateLLMStatus("Loading LLM...");
        SetButtonState(false);

        Debug.Log("Waiting for LLM to be ready...");
        var waitTask = llmCharacter.llm.WaitUntilReady();
        while (!waitTask.IsCompleted)
        {
            yield return null;
        }

        isLLMReady = true;
        Debug.Log("LLM is ready - Character selection enabled");

        uiController.UpdateLLMStatus("LLM Ready");
        uiController.UpdateCharacterStatus("No character loaded");
        SetButtonState(true);
    }

    private void OnCharacterButtonClick()
    {
        if (!isLLMReady || characterFiles.Length == 0) return;

        SetButtonState(false);
        currentCharacterIndex = (currentCharacterIndex + 1) % characterFiles.Length;
        Debug.Log($"Switching to character index {currentCharacterIndex}: {characterFiles[currentCharacterIndex]}");
        StartCoroutine(LoadCharacterCoroutine(currentCharacterIndex));
    }

    private IEnumerator LoadCharacterCoroutine(int index)
    {
        if (index < 0 || index >= characterFiles.Length)
        {
            SetButtonState(true);
            yield break;
        }

        string characterName = characterFiles[index];
        uiController.UpdateCharacterStatus($"Loading {characterName}...");
        uiController.ResetChat(); // Clear the chat UI

        string jsonPath = Path.Combine(Application.streamingAssetsPath, charactersFolder, $"{characterName}.json");
        if (!File.Exists(jsonPath))
        {
            Debug.LogError($"Character file not found at: {jsonPath}");
            uiController.UpdateCharacterStatus("Character file missing");
            SetButtonState(true);
            yield break;
        }

        // Handle file reading with a task
        string jsonContent = "";
        var readTask = File.ReadAllTextAsync(jsonPath);
        while (!readTask.IsCompleted) yield return null;

        try
        {
            jsonContent = readTask.Result;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error reading character file: {e.Message}");
            uiController.UpdateCharacterStatus("Error reading character file");
            SetButtonState(true);
            yield break;
        }

        string systemPrompt = CharacterPromptGenerator.GenerateSystemPrompt(jsonContent);
        if (string.IsNullOrEmpty(systemPrompt))
        {
            Debug.LogError($"Failed to generate prompt for character: {characterName}");
            uiController.UpdateCharacterStatus("Failed to load character");
            SetButtonState(true);
            yield break;
        }

        // Set the prompt
        llmCharacter.SetPrompt(systemPrompt);

        // Warm up the model with the new character's system prompt
        var warmupTask = llmCharacter.Warmup();
        while (!warmupTask.IsCompleted) yield return null;

        // Check if warmup succeeded
        if (warmupTask.Exception != null)
        {
            Debug.LogError($"Error during character warmup: {warmupTask.Exception.Message}");
            uiController.UpdateCharacterStatus("Error initializing character");
            SetButtonState(true);
            yield break;
        }

        uiController.SetCurrentCharacter(characterName);
        uiController.UpdateCharacterStatus($"Playing as {characterName}");

        Debug.Log($"Successfully switched to character: {characterName}");
        SetButtonState(true);
    }

    private void UpdateUIController()
    {
        if (currentCharacterIndex >= 0 && currentCharacterIndex < characterFiles.Length)
        {
            uiController.UpdateCharacterStatus($"Talking to {characterFiles[currentCharacterIndex]}");
            characterButton.image.color = buttonColor;
        }
        else
        {
            uiController.UpdateCharacterStatus("No character loaded");
            characterButton.image.color = buttonColor;
        }
    }
}