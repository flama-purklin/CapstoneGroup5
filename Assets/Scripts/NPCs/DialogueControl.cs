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

    [SerializeField] private AudioControl audioControl;

    [SerializeField] private Animator anim;

    private bool isTransitioning = false;
    private bool shutdown = false;

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

    public void DisplayNPCDialogueStreaming(string dialogue)
    {
        // Forward the text update to BeepSpeak if available
        if (beepSpeak != null)
        {
            beepSpeak.UpdateStreamingText(dialogue);
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
        characterName.text = llmCharacter.AIName; 
        StartCoroutine(ActivateDialogueAnimation()); 
    }

    private IEnumerator ActivateDialogueAnimation() 
    {
        audioControl.PlaySFX_Enter();
        isTransitioning = true;
        dialogueCanvas.SetActive(true);
        anim.Play("DialogueActivate");
        yield return null; 
        while (anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1) yield return null; 
        llmDialogueManager.InitializeDialogue();
        isTransitioning = false;
    }

    private IEnumerator DeactivateDialogue()
    {
        Debug.Log("[DialogueControl] DeactivateDialogue Coroutine Started.");
        isTransitioning = true;

        // --- RESET/CANCEL FIRST ---
        Debug.Log("[DialogueControl.Deactivate] Calling llmDialogueManager.ResetDialogue() (which should call CancelRequests)...");
        Task resetTask = llmDialogueManager.ResetDialogue();
        yield return StartCoroutine(WaitForTask(resetTask)); // Wait for the async ResetDialogue task to complete
        Debug.Log("[DialogueControl.Deactivate] AFTER WaitForTask(resetTask). ResetDialogue() finished."); // ADDED LOG

        // --- THEN SAVE ---
        LLMCharacter characterToSave = llmDialogueManager.CurrentCharacter; 
        Task saveTask = null; 

        if (characterToSave != null) {
            Debug.Log($"[DialogueControl.Deactivate] Character to save: {characterToSave.save}, Current chat count: {characterToSave.chat.Count}");
            if (!string.IsNullOrEmpty(characterToSave.save)) {
                try {
                    Debug.Log($"ATTEMPTING to save conversation state for '{characterToSave.save}'");
                    saveTask = characterToSave.Save(characterToSave.save); 
                } catch (Exception e) {
                    Debug.LogError($"CRITICAL ERROR starting save for '{characterToSave.save}': {e.Message}\nStack trace: {e.StackTrace}");
                    saveTask = null; 
                }
            } else { Debug.LogWarning($"Character has empty 'save' property. Cannot save conversation state."); }
        } else { Debug.LogWarning("Could not get active LLMCharacter reference to save conversation."); }

        // Wait for the save task to complete (if it was started) AFTER the try-catch block
        if (saveTask != null) {
            Debug.Log($"[DialogueControl.Deactivate] Waiting for save task for '{characterToSave?.save ?? "Unknown"}'...");
            yield return StartCoroutine(WaitForTask(saveTask));
            Debug.Log($"[DialogueControl.Deactivate] AFTER WaitForTask(saveTask) for '{characterToSave?.save ?? "Unknown"}'."); // ADDED LOG
            if (!saveTask.IsFaulted) {
                 Debug.Log($"[DialogueControl.Deactivate] Successfully completed save for conversation state: '{characterToSave?.save ?? "Unknown"}'");
            } else {
                 Debug.LogError($"[DialogueControl.Deactivate] Save task FAILED for '{characterToSave?.save ?? "Unknown"}': {saveTask.Exception}");
            }
        } else {
             Debug.Log("[DialogueControl.Deactivate] No save task was started (character null or save name empty).");
        }
        
        // --- THEN ANIMATE ---
        Debug.Log("[DialogueControl.Deactivate] Starting UI deactivation animation...");
        anim.Rebind();
        anim.Update(0f);
        anim.Play("DialogueDeactivate");
        yield return null; 
        while (anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1) yield return null; 

        dialogueCanvas.SetActive(false);
        if (GameControl.GameController.currentState == GameState.DIALOGUE) {
            if(defaultHud) defaultHud.SetActive(true);
            GameControl.GameController.currentState = GameState.DEFAULT;
        }
        isTransitioning = false;
        Debug.Log("[DialogueControl] DeactivateDialogue Coroutine Finished.");
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
        if (isTransitioning || !dialogueCanvas.activeInHierarchy) return;
        StartCoroutine(DeactivateDialogue());
    }
}
