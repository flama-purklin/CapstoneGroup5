using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using LLMUnity;

public class DialogueControl : MonoBehaviour
{
    [Header("Core References")]
    [SerializeField] private LLMDialogueManager llmDialogueManager;
    [SerializeField] private GameObject dialogueCanvas;
    [SerializeField] private GameObject defaultHud;
    [SerializeField] private RectTransform dialoguePanel;

    [Header("Canvas Components")]
    [SerializeField] Image characterProf;
    [SerializeField] TMP_Text characterName;

    [SerializeField] private Animator anim;

    private bool isTransitioning = false;
    private bool shutdown = false;

    private void Start()
    {
        if (!llmDialogueManager)
        {
            Debug.LogError("LLMDialogueManager reference not set in DialogueControl!!");
            enabled = false;
            return;
        }

        if (!dialogueCanvas)
        {
            Debug.LogError("DialogueCanvas reference not set!");
            enabled = false;
            return;
        }

        if (!anim)
        {
            Debug.LogError("Animator reference not set!");
            enabled = false;
            return;
        }

        dialogueCanvas.SetActive(false);
    }

    public void Activate(GameObject npcObject)
    {
        if (isTransitioning) return;

        Debug.Log($"Attempting to activate dialogue with {npcObject.name}");

        Character character = npcObject.GetComponent<Character>();
        if (character == null)
        {
            Debug.LogError($"No Character component found on NPC: {npcObject.name}");
            return;
        }

        var llmCharacter = character.GetLLMCharacter();
        if (llmCharacter == null)
        {
            Debug.LogError($"Failed to get LLMCharacter for {character.GetCharacterName()}");
            return;
        }

        // Re-enable conversation loading now that native cache restore is disabled
        bool enableConversationLoading = true;
        
        // Perform safety checks and load previous conversation state
        if (enableConversationLoading) 
        {
            if (string.IsNullOrEmpty(llmCharacter.save))
            {
                Debug.LogWarning($"Character has empty 'save' property. Cannot load conversation state for: {character.GetCharacterName()}");
            }
            else
            {
                try
                {
                    // First check if the file exists and is valid
                    string saveFilePath = System.IO.Path.Combine(
                        UnityEngine.Application.persistentDataPath, 
                        llmCharacter.save + ".json");
                        
                    Debug.Log($"Checking for save file at: {saveFilePath}");
                    
                    if (System.IO.File.Exists(saveFilePath))
                    {
                        // File exists, but let's validate it's not corrupted
                        try {
                            // Try to read it as a string
                            string jsonContent = System.IO.File.ReadAllText(saveFilePath);
                            if (string.IsNullOrEmpty(jsonContent) || jsonContent.Length < 10) {
                                Debug.LogWarning($"Save file for {llmCharacter.save} appears empty or too small, skipping load");
                            }
                            else {
                                Debug.Log($"ATTEMPTING to load conversation state for {llmCharacter.save}");
                                // The await here ensures we wait for load to complete and catch any errors
                                _ = llmCharacter.Load(llmCharacter.save);
                                Debug.Log($"Successfully initiated load for conversation state: {llmCharacter.save}");
                            }
                        }
                        catch (System.Exception fileReadEx) {
                            Debug.LogError($"Error reading save file for {llmCharacter.save}: {fileReadEx.Message}");
                        }
                    }
                    else 
                    {
                        Debug.Log($"No existing save file found for {llmCharacter.save}, skipping load");
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"CRITICAL ERROR loading conversation for {llmCharacter.save}: {e.Message}");
                    Debug.LogError($"Stack trace: {e.StackTrace}");
                    // Continue even if load fails - the character will just have no memory
                }
            }
        }
        else 
        {
            Debug.Log($"Conversation loading is temporarily disabled to prevent crashes");
        }

        llmDialogueManager.SetCharacter(llmCharacter);
        GameControl.GameController.currentState = GameState.DIALOGUE;

        if (defaultHud)
        {
            defaultHud.SetActive(false);
        }

        //set the character name
        characterName.text = npcObject.GetComponentInChildren<LLMCharacter>().AIName;

        StartCoroutine(ActivateDialogue());
    }

    private IEnumerator ActivateDialogue()
    {
        isTransitioning = true;
        dialogueCanvas.SetActive(true);
        anim.Play("DialogueActivate");

        // wait for animation to start
        yield return null;

        // wait for animation to complete
        while (anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1)
        {
            yield return null;
        }

        llmDialogueManager.InitializeDialogue();
        isTransitioning = false;

        //return null; // for now...
    }

    private IEnumerator DeactivateDialogue()
    {
        Debug.Log("deactivating dialogue");
        isTransitioning = true;

        // --- SAVE FIRST ---
        // Save conversation state for the character BEFORE resetting the dialogue manager
        LLMCharacter characterToSave = llmDialogueManager.CurrentCharacter; 
        if (characterToSave != null)
        {
            Debug.Log($"[DialogueControl.Deactivate] Character to save: {characterToSave.save}, Current chat count: {characterToSave.chat.Count}");
            if (string.IsNullOrEmpty(characterToSave.save))
            {
                Debug.LogWarning($"Character has empty 'save' property. Cannot save conversation state.");
            }
            else
            {
                try
                {
                    Debug.Log($"ATTEMPTING to save conversation state for '{characterToSave.save}'");
                    // Use LLMUnity's built-in save. The 'save' field should already hold the character name.
                    _ = characterToSave.Save(characterToSave.save); // Use characterToSave here
                    Debug.Log($"Successfully initiated save for conversation state: '{characterToSave.save}'");
                    
                    // Optionally also update CharacterManager's state
                    try
                    {
                        CharacterManager characterManager = FindFirstObjectByType<CharacterManager>();
                        if (characterManager != null)
                        {
                            // Note: CharacterManager.SaveCharacterConversation also calls character.Save()
                            // This might be redundant, but let's keep it for now unless issues arise.
                            characterManager.SaveCharacterConversation(characterToSave.save); // Use characterToSave here
                            Debug.Log($"Successfully notified CharacterManager about saved conversation for '{characterToSave.save}'");
                        }
                        else
                        {
                            Debug.LogWarning("Could not find CharacterManager to update about saved conversation.");
                        }
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"Error updating CharacterManager: {e.Message}");
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"CRITICAL ERROR saving conversation for '{characterToSave.save}': {e.Message}");
                    Debug.LogError($"Stack trace: {e.StackTrace}");
                }
            }
        }
        else
        {
            Debug.LogWarning("Could not get active LLMCharacter reference to save conversation.");
        }

        // --- THEN RESET ---
        // Start the reset process AFTER saving
        Debug.Log("Resetting dialogue manager state...");
        var resetTask = llmDialogueManager.ResetDialogue();
        while (!resetTask.IsCompleted)
        {
            yield return null;
        }
        Debug.Log("Dialogue manager reset complete.");
        
        // --- THEN ANIMATE ---
        anim.Rebind();
        anim.Update(0f);
        anim.Play("DialogueDeactivate");
        
        

        // Wait for animation to complete
        while (anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1)
        {
            yield return null;
        }

        dialogueCanvas.SetActive(false);

        if (GameControl.GameController.currentState == GameState.DIALOGUE)
        {
            defaultHud.SetActive(true);
            GameControl.GameController.currentState = GameState.DEFAULT;
        }

        isTransitioning = false;
    }

    private void Update()
    {
        if (isTransitioning) return;

        if ((Input.GetKeyDown(KeyCode.Escape) && GameControl.GameController.currentState == GameState.DIALOGUE) ||
            (GameControl.GameController.currentState == GameState.FINAL && !shutdown))
        {
            if (GameControl.GameController.currentState == GameState.FINAL)
            {
                shutdown = true;
            }
            Deactivate();
        }
    }

    public void Deactivate()
    {
        if (isTransitioning || !dialogueCanvas.activeInHierarchy) return;
        StartCoroutine(DeactivateDialogue());
    }
}
