using UnityEngine;
using System.Collections;
using System.Collections.Generic; // Added for List<>
using UnityEngine.UI;
using TMPro;
using LLMUnity;
using System; 
using System.IO; // Added for Path and File
using System.Threading.Tasks; // Added for Task

public class DialogueControl : MonoBehaviour
{
    [Header("Core References")]
    [SerializeField] private LLMDialogueManager llmDialogueManager;
    [SerializeField] private GameObject dialogueCanvas;
    [SerializeField] private GameObject defaultHud;
    [SerializeField] private RectTransform dialoguePanel;
    [SerializeField] private DialogueUIController dialogueUIController; // New DialogueUIController reference
    
    [Header("HUD Elements to Keep Active")]
    [SerializeField] private GameObject nodeUnlockNotifHud; // Keep this active during dialogue for function calls
    [SerializeField] private GameObject powerControlHud; // Keep this active during dialogue

    [Header("Canvas Components")]
    [SerializeField] Image characterProf;
    [SerializeField] TMP_Text characterName;

    [Header("BeepSpeak Integration")]
    [SerializeField] private BeepSpeak beepSpeak;
    [SerializeField] private TMPro.TextMeshProUGUI npcDialogueText;
    
    // Public getter for BeepSpeak for debugging purposes
    public BeepSpeak GetBeepSpeak() => beepSpeak;

    [SerializeField] private AudioControl audioControl;

    [SerializeField] private Animator anim;

    private bool isTransitioning = false;
    private bool shutdown = false;
    private Coroutine deactivationCoroutine = null; // Track the deactivation coroutine

    // Public property to check if the dialogue UI is currently active
    public bool IsDialogueCanvasActive => dialogueCanvas != null && dialogueCanvas.activeInHierarchy;

    // Public property to check if BeepSpeak is currently playing
    public bool IsBeepSpeakPlaying => beepSpeak != null && beepSpeak.IsPlaying;

    // Public property to check if we're using new UI
    public bool UseNewUI => dialogueUIController != null;

    private void Start()
    {
        if (!llmDialogueManager) { Debug.LogError("LLMDialogueManager reference not set!"); enabled = false; return; }
        if (!dialogueCanvas) { Debug.LogError("DialogueCanvas reference not set!"); enabled = false; return; }
        if (!anim) { Debug.LogError("Animator reference not set!"); enabled = false; return; }
        
        // Check for DialogueUIController reference
        if (!dialogueUIController) {
            dialogueUIController = GetComponentInChildren<DialogueUIController>();
            if (!dialogueUIController) {
                Debug.LogWarning("DialogueUIController reference not set! Falling back to legacy dialogue UI.");
            } else {
                Debug.Log("Found DialogueUIController in children!");
            }
        }

        // Connect BeepSpeak with the new UI's text component if available
        if (beepSpeak != null && dialogueUIController != null) {
            // Get the responseText TMP component from DialogueUIController
            TMP_Text responseText = dialogueUIController.ResponseTextComponent;
            if (responseText != null) {
                beepSpeak.newUIResponseText = responseText;
                Debug.Log("Successfully connected BeepSpeak to DialogueUIController's response text");
            } else {
                Debug.LogError("Failed to get responseText component from DialogueUIController");
            }
        } else if (dialogueUIController != null) {
            Debug.LogWarning("BeepSpeak component not found but DialogueUIController exists. Text animations may not work properly.");
        }

        dialogueCanvas.SetActive(false);
        llmDialogueManager.RegisterDialogueControl(this);

        // Subscribe to DialogueUIController's OnPlayerMessageSubmitted event if available
        if (dialogueUIController != null) {
            dialogueUIController.OnPlayerMessageSubmitted += HandlePlayerInput;
            Debug.Log("Successfully subscribed to DialogueUIController.OnPlayerMessageSubmitted");
        }
    }

    // Handle player input from the new DialogueUIController
    private void HandlePlayerInput(string input)
    {
        Debug.Log($"[DialogueControl] Received player input from new UI: {input}");
        
        // Forward the player's message to the LLM system
        // llmDialogueManager.SendPlayerMessage(input); // This method doesn't exist
        
        // Use the correct approach - simulate as if the input came from the input field
        if (llmDialogueManager != null && !string.IsNullOrEmpty(input))
        {
            // Get a reference to the input field used by LLMDialogueManager
            var inputField = llmDialogueManager.GetComponent<LLMDialogueManager>()?.GetComponentInChildren<TMP_InputField>();
            if (inputField != null)
            {
                // Set the text in the input field
                inputField.text = input;
                
                // Find and invoke the submit button's onClick event
                var submitButton = llmDialogueManager.GetComponent<LLMDialogueManager>()?.GetComponentInChildren<Button>();
                if (submitButton != null)
                {
                    submitButton.onClick.Invoke();
                }
                else
                {
                    Debug.LogWarning("Could not find submit button in LLMDialogueManager");
                }
            }
            else
            {
                Debug.LogWarning("Could not find input field in LLMDialogueManager");
            }
        }
    }

    public void DisplayNPCDialogue(string dialogue)
    {
        if (UseNewUI) {
            // When using the new UI, only let BeepSpeak handle the text display
            // The BeepSpeak component is already connected to the DialogueUIController's text
            if (beepSpeak != null) {
                beepSpeak.UpdateStreamingText(dialogue);
                Debug.Log($"[DialogueControl] BeepSpeak updating text for new UI: {dialogue.Substring(0, Mathf.Min(40, dialogue.Length))}...");
            } else {
                // Direct fallback if somehow BeepSpeak is missing
                dialogueUIController.SetNPCResponse(dialogue);
                Debug.Log($"[DialogueControl] Direct UI update (no BeepSpeak): {dialogue.Substring(0, Mathf.Min(40, dialogue.Length))}...");
            }
            return;
        }
        
        // Legacy path - used only when DialogueUIController is not available
        if (beepSpeak != null) {
            var dialogueEntries = new List<BeepSpeak.DialogueEntry> { new BeepSpeak.DialogueEntry { text = dialogue, speaker = beepSpeak } };
            beepSpeak.StartDialogue(dialogueEntries);
        } else {
            npcDialogueText.text = dialogue;
        }
    }

    // Completely redesigned streaming text handling to prevent animation interruptions
    public void DisplayNPCDialogueStreaming(string dialogue)
    {
        if (UseNewUI) {
            // For the new UI, only use BeepSpeak to handle typed text display
            if (beepSpeak != null) {
                beepSpeak.UpdateStreamingText(dialogue);
                Debug.Log($"[DialogueControl] BeepSpeak streaming for new UI: {dialogue}");
            } else {
                // Direct fallback if somehow BeepSpeak is missing
                dialogueUIController.AppendToNPCResponse(dialogue);
                Debug.Log($"[DialogueControl] Direct streaming (no BeepSpeak): {dialogue}");
            }
            return;
        }
        
        // Legacy path below - only used when DialogueUIController is not available
        Debug.Log($"[BEEP DEBUG] DisplayNPCDialogueStreaming called with text length: {dialogue.Length}");
        Debug.Log($"[BEEP DEBUG] First 40 chars: '{dialogue.Substring(0, Math.Min(40, dialogue.Length))}'");
        
        if (beepSpeak != null)
        {
            // SIMPLIFICATION: Always use UpdateStreamingText for consistency
            // This matches the behavior of the new UI path and reduces complexity
            // The BeepSpeak component will internally decide how to handle the new text
            Debug.Log($"[BEEP DEBUG] Calling UpdateStreamingText with text length: {dialogue.Length}");
            beepSpeak.UpdateStreamingText(dialogue);
            Debug.Log($"[DialogueControl] Using consistent BeepSpeak text handling approach");
        }
        else if (npcDialogueText != null)
        {
            // Fallback: update the text immediately if no BeepSpeak is assigned
            npcDialogueText.text = dialogue;
        }
    }

    // Made async to await Load
    public async void Activate(GameObject npcObject) 
    {
        float startTime = Time.realtimeSinceStartup;
        Debug.Log($"[TIMEDBG] Activate started at {startTime:F3}s");
        
        if (isTransitioning) return;
        Debug.Log($"Attempting to activate dialogue with {npcObject.name}");

        Character character = npcObject.GetComponent<Character>();
        if (character == null) { Debug.LogError($"No Character component found on NPC: {npcObject.name}"); return; }

        var llmCharacter = character.GetLLMCharacter();
        if (llmCharacter == null) { Debug.LogError($"Failed to get LLMCharacter for {character.GetCharacterName()}"); return; }

        // Load conversation state ONLY if the save file exists
        if (!string.IsNullOrEmpty(llmCharacter.save)) {
            string saveFilePath = Path.Combine(Application.persistentDataPath, llmCharacter.save + ".json");
            if (File.Exists(saveFilePath)) // Check if file exists BEFORE trying to load
            {
                try {
                    Debug.Log($"Save file found for {llmCharacter.save}. ATTEMPTING to load conversation state...");
                    await llmCharacter.Load(llmCharacter.save); // Await the load operation
                    Debug.Log($"Load operation completed for: {llmCharacter.save}");
                } catch (Exception e) {
                    Debug.LogError($"CRITICAL ERROR loading conversation for {llmCharacter.save}: {e.Message}\nStack trace: {e.StackTrace}");
                }
            } else {
                 Debug.Log($"No save file found for {llmCharacter.save}. Starting fresh conversation.");
                 // Optionally clear chat explicitly if Load doesn't handle it on file not found
                 // llmCharacter.ClearChat(); 
            }
        } else { Debug.LogWarning($"Character {character.GetCharacterName()} has empty 'save' property. Cannot load conversation state."); }
        
        llmDialogueManager.SetCharacter(llmCharacter);
        GameControl.GameController.currentState = GameState.DIALOGUE;
        
        // MODIFIED: Selectively deactivate HUD elements, preserving critical ones
        if (defaultHud) {
            // Instead of deactivating the entire HUD, iterate and selectively hide non-essential elements
            for (int i = 0; i < defaultHud.transform.childCount; i++) {
                Transform childTransform = defaultHud.transform.GetChild(i);
                GameObject childObject = childTransform.gameObject;
                
                // Skip our critical HUD elements
                bool isNodeUnlockNotif = nodeUnlockNotifHud != null && childObject == nodeUnlockNotifHud;
                bool isPowerControl = powerControlHud != null && childObject == powerControlHud;
                
                if (!isNodeUnlockNotif && !isPowerControl) {
                    // Only hide non-critical elements
                    childObject.SetActive(false);
                    Debug.Log($"[DialogueControl] Hiding HUD element during dialogue: {childObject.name}");
                } else {
                    // Ensure critical elements stay visible
                    childObject.SetActive(true);
                    Debug.Log($"[DialogueControl] Keeping HUD element visible during dialogue: {childObject.name}");
                }
            }
        }

        // Activate the new UI if available
        if (dialogueUIController != null) {
            // Get character portrait image from the NPC
            Sprite portrait = npcObject.GetComponentInChildren<NPCAnimManager>()?.anims?.profile;
            
            Debug.Log($"[DialogueControl] About to call dialogueUIController.ShowDialogue for {llmCharacter.AIName}");
            Debug.Log($"[DialogueControl] dialogueUIController.gameObject active: {dialogueUIController.gameObject.activeInHierarchy}");
            
            dialogueUIController.ShowDialogue(llmCharacter.AIName, portrait);
            Debug.Log($"[DialogueControl] Called dialogueUIController.ShowDialogue for {llmCharacter.AIName}");
            
            // Log state of DialogueUIController after activation
            dialogueUIController.LogRuntimeState();
        }
        else {
            Debug.LogError("[DialogueControl] dialogueUIController is NULL! Cannot show UI.");
        }

        //set the character name (legacy UI)
        characterName.text = llmCharacter.AIName;

        //set the character profile (legacy UI)
        characterProf.sprite = npcObject.GetComponentInChildren<NPCAnimManager>().anims.profile;

        StartCoroutine(ActivateDialogueAnimation()); 
    }

    private IEnumerator ActivateDialogueAnimation() 
    {
        float startTime = Time.realtimeSinceStartup;
        Debug.Log($"[TIMEDBG] ActivateDialogueAnimation started at {startTime:F3}s");
        
        audioControl.PlaySFX_Enter();
        isTransitioning = true;
        dialogueCanvas.SetActive(true);
        anim.Play("DialogueActivate");
        
        float animStartTime = Time.realtimeSinceStartup;
        yield return null;
        
        int frameCount = 0;
        while (anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1) 
        {
            frameCount++;
            if (frameCount % 60 == 0) // Log every ~1 second
            {
                Debug.Log($"[TIMEDBG] Animation progress: {anim.GetCurrentAnimatorStateInfo(0).normalizedTime:F2}, elapsed: {Time.realtimeSinceStartup - animStartTime:F1}s");
            }
            yield return null;
        }
        
        float beforeInitTime = Time.realtimeSinceStartup;
        Debug.Log($"[TIMEDBG] Animation completed after {beforeInitTime - animStartTime:F3}s, calling InitializeDialogue");
        
        llmDialogueManager.InitializeDialogue();
        
        float afterInitTime = Time.realtimeSinceStartup;
        Debug.Log($"[TIMEDBG] InitializeDialogue completed after {afterInitTime - beforeInitTime:F3}s");
        
        isTransitioning = false;
        Debug.Log($"[TIMEDBG] ActivateDialogueAnimation completed in {Time.realtimeSinceStartup - startTime:F3}s");
        
        // Log DialogueUIController state after animation completes
        if (dialogueUIController != null) {
            Debug.Log("[DialogueControl] Checking DialogueUIController state after animation completes:");
            dialogueUIController.LogRuntimeState();
        }
    }

    private IEnumerator DeactivateDialogue()
    {
        float startTime = Time.realtimeSinceStartup;
        Debug.Log($"[TIMEDBG] DeactivateDialogue Coroutine Started at {startTime:F3}s");
        try
        {
            isTransitioning = true;

            // Hide the new UI if it's being used
            if (dialogueUIController != null) {
                dialogueUIController.HideDialogue();
                Debug.Log("[DialogueControl] Called HideDialogue on new DialogueUIController");
            }

            // --- RESET/CANCEL FIRST ---
            float resetStartTime = Time.realtimeSinceStartup;
            Debug.Log($"[TIMEDBG] Calling ResetDialogue at {resetStartTime:F3}s");
            Task resetTask = llmDialogueManager.ResetDialogue();
            yield return StartCoroutine(WaitForTask(resetTask)); // Wait for the async ResetDialogue task to complete
            float resetEndTime = Time.realtimeSinceStartup;
            Debug.Log($"[TIMEDBG] ResetDialogue completed after {resetEndTime - resetStartTime:F3}s");

            // --- THEN SAVE ---
            LLMCharacter characterToSave = llmDialogueManager.CurrentCharacter; 
            Task saveTask = null; 

            if (characterToSave != null) {
                Debug.Log($"[DialogueControl.Deactivate] Character to save: {characterToSave.save}, Current chat count: {characterToSave.chat.Count}");
                if (!string.IsNullOrEmpty(characterToSave.save)) {
                    try {
                        float saveStartTime = Time.realtimeSinceStartup;
                        Debug.Log($"[TIMEDBG] Starting save for '{characterToSave.save}' at {saveStartTime:F3}s");
                        saveTask = characterToSave.Save(characterToSave.save); 
                    } catch (Exception e) {
                        Debug.LogError($"CRITICAL ERROR starting save for '{characterToSave.save}': {e.Message}\nStack trace: {e.StackTrace}");
                        saveTask = null; 
                    }
                } else { Debug.LogWarning($"Character has empty 'save' property. Cannot save conversation state."); }
            } else { Debug.LogWarning("Could not get active LLMCharacter reference to save conversation."); }

            // Wait for the save task to complete (if it was started) AFTER the try-catch block
            if (saveTask != null) {
                float saveWaitStartTime = Time.realtimeSinceStartup;
                Debug.Log($"[TIMEDBG] Waiting for save task at {saveWaitStartTime:F3}s");
                yield return StartCoroutine(WaitForTask(saveTask));
                float saveEndTime = Time.realtimeSinceStartup;
                Debug.Log($"[TIMEDBG] Save completed after {saveEndTime - saveWaitStartTime:F3}s");
                
                if (!saveTask.IsFaulted) {
                     Debug.Log($"[DialogueControl.Deactivate] Successfully completed save for conversation state: '{characterToSave?.save ?? "Unknown"}'");
                } else {
                     Debug.LogError($"[DialogueControl.Deactivate] Save task FAILED for '{characterToSave?.save ?? "Unknown"}': {saveTask.Exception}");
                }
            } else {
                 Debug.Log("[DialogueControl.Deactivate] No save task was started (character null or save name empty).");
            }
            
            // --- ANIMATE IMMEDIATELY (No delay) ---
            Debug.Log($"[TIMEDBG] Starting deactivation animation immediately at {Time.realtimeSinceStartup:F3}s");
            
            // --- ANIMATE ---
            float animStartTime = Time.realtimeSinceStartup;
            Debug.Log($"[TIMEDBG] Starting deactivation animation at {animStartTime:F3}s");
            anim.Rebind();
            anim.Update(0f);
            anim.Play("DialogueDeactivate");
            yield return null;
            
            int frameCount = 0;
            while (anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1) 
            {
                frameCount++;
                if (frameCount % 60 == 0) // Log every ~1 second
                {
                    Debug.Log($"[TIMEDBG] Animation progress: {anim.GetCurrentAnimatorStateInfo(0).normalizedTime:F2}, elapsed: {Time.realtimeSinceStartup - animStartTime:F1}s");
                }
                yield return null;
            }
            
            float animEndTime = Time.realtimeSinceStartup;
            Debug.Log($"[TIMEDBG] Animation completed after {animEndTime - animStartTime:F3}s");

            dialogueCanvas.SetActive(false);
            if (GameControl.GameController.currentState == GameState.DIALOGUE) {
                if(defaultHud) defaultHud.SetActive(true);
                GameControl.GameController.currentState = GameState.DEFAULT;
            }
        }
        finally
        {
            // Ensure flags are reset even if the coroutine is stopped or errors
            isTransitioning = false;
            deactivationCoroutine = null;
            float totalTime = Time.realtimeSinceStartup - startTime;
            Debug.Log($"[TIMEDBG] DeactivateDialogue Coroutine Finished after {totalTime:F3}s");
        }
    }

    // Helper coroutine to wait for an async Task
    private IEnumerator WaitForTask(Task task) 
    {
        while (!task.IsCompleted) {
            yield return null;
        }
        if (task.IsFaulted) {
            Debug.LogError($"Async Task failed: {task.Exception}");
        }
    }

    private void Update()
    {
        if (isTransitioning) return;
        if ((Input.GetKeyDown(KeyCode.Escape) && GameControl.GameController.currentState == GameState.DIALOGUE) ||
            (GameControl.GameController.currentState == GameState.FINAL && !shutdown))
        {
            if (GameControl.GameController.currentState == GameState.FINAL) shutdown = true;
            audioControl.PlaySFX_Exit();
            Deactivate();
        }
    }

    public void Deactivate()
    {
        float startTime = Time.realtimeSinceStartup;
        Debug.Log($"[TIMEDBG] Deactivate called at {startTime:F3}s");
        
        if (!dialogueCanvas.activeInHierarchy) {
            Debug.LogWarning("[Deactivate] Called but canvas is not active. Returning.");
            return;
        }

        // If a deactivation is already running, stop it first to prevent overlap
        if (deactivationCoroutine != null) {
            Debug.LogWarning("[Deactivate] Stopping existing deactivation coroutine.");
            StopCoroutine(deactivationCoroutine);
            isTransitioning = false; // Reset the transitioning flag
        }

        Debug.Log("[Deactivate] Starting DeactivateDialogue coroutine.");
        deactivationCoroutine = StartCoroutine(DeactivateDialogue());
    }
}
