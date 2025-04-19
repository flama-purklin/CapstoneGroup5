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

    private void Start()
    {
        if (!llmDialogueManager) { Debug.LogError("LLMDialogueManager reference not set!"); enabled = false; return; }
        if (!dialogueCanvas) { Debug.LogError("DialogueCanvas reference not set!"); enabled = false; return; }
        if (!anim) { Debug.LogError("Animator reference not set!"); enabled = false; return; }
        dialogueCanvas.SetActive(false);
        llmDialogueManager.RegisterDialogueControl(this);
    }

    public void DisplayNPCDialogue(string dialogue)
    {
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
        Debug.Log($"[BEEP DEBUG] DisplayNPCDialogueStreaming called with text length: {dialogue.Length}");
        Debug.Log($"[BEEP DEBUG] First 40 chars: '{dialogue.Substring(0, Math.Min(40, dialogue.Length))}'");
        
        // Only start a new BeepSpeak animation if:
        // 1. BeepSpeak is not currently typing OR
        // 2. This appears to be a completely new message (significant length difference)
        if (beepSpeak != null)
        {
            // Get current text length being displayed/processed
            int currentLength = beepSpeak.GetCurrentTargetLength();
            
            Debug.Log($"[BEEP DEBUG] Current BeepSpeak text length: {currentLength}, IsPlaying: {beepSpeak.IsPlaying}");
            
            // If BeepSpeak is currently typing and this isn't a drastically different message,
            // DON'T interrupt the animation - store the text for later display
            if (beepSpeak.IsPlaying && dialogue.Length <= currentLength + 20) 
            {
                // Store the latest text as the "final" version but don't interrupt current animation
                Debug.Log($"[BEEP DEBUG] Calling SetFinalText - NOT interrupting animation");
                beepSpeak.SetFinalText(dialogue);
                Debug.Log($"[DialogueControl] Stored final text (len={dialogue.Length}) without interrupting current animation (len={currentLength})");
            }
            else 
            {
                // Either BeepSpeak is not currently playing, or this is a much larger text update
                // In this case, it's appropriate to start a new animation
                Debug.Log($"[BEEP DEBUG] Calling UpdateStreamingText - STARTING NEW animation");
                Debug.Log($"[BEEP DEBUG] Reason: {(beepSpeak.IsPlaying ? "Large text diff" : "Not currently playing")}");
                beepSpeak.UpdateStreamingText(dialogue);
                Debug.Log($"[DialogueControl] Starting new BeepSpeak animation for text (len={dialogue.Length})");
            }
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
        if (defaultHud) defaultHud.SetActive(false);

        //set the character name
        characterName.text = llmCharacter.AIName;

        //set the character profile
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
    }

    private IEnumerator DeactivateDialogue()
    {
        float startTime = Time.realtimeSinceStartup;
        Debug.Log($"[TIMEDBG] DeactivateDialogue Coroutine Started at {startTime:F3}s");
        try
        {
            isTransitioning = true;

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
