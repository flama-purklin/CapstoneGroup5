using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Text.RegularExpressions;
using System;

public class BeepSpeak : MonoBehaviour
{
    private float sfxCooldown = 0.08f; // minimum seconds between SFX
    private float lastSfxTime = 0f;

    [System.Serializable]
    public class VoiceSettings
    {
        public int voiceID = 100000;
        public AudioClip[] timbre;
        public float basePitch = 1.0f;
        public float pitchVariance = 0.1f;
        public float baseVolume = 1.0f;
        public float volumeVariance = 0.1f;
        public float baseSpeed = 0.05f;
        public float speedVariance = 0.01f;
        public string emotion = "neutral";
    }
    class PrecomputedDialogue
    {
        public List<PrecomputedSentence> sentences;
    }
    class PrecomputedSentence
    {
        public List<PrecomputedWord> words;
        public char sentenceEnd; // ., ?, !, ...
    }
    class PrecomputedWord
    {
        public string text;
        public List<PrecomputedSyllable> syllables; // Positions where syllables start
    }
    class PrecomputedSyllable
    {
        public string text;
        public int index;
    }


    [System.Serializable]
    public class DialogueEntry
    {
        public string text;
        public BeepSpeak speaker;
    }

    public TMP_Text dialogueText; // Legacy reference
    [Tooltip("New UI's text component - will be used if assigned")]
    public TMP_Text newUIResponseText; // New UI reference
    public AudioSource audioSource;
    public VoiceSettings npcVoice;

    private Queue<DialogueEntry> dialogueQueue = new Queue<DialogueEntry>();
    private Coroutine typingCoroutine;
    // private bool isTyping = false; // Replaced by checking typingCoroutine != null

    // Public property to check if BeepSpeak is currently typing/playing audio
    public bool IsPlaying => typingCoroutine != null;
    
    // Debug method to check if typing coroutine is active
    public bool GetTypingCoroutineActive() => typingCoroutine != null;
    
    // Public method to forcefully stop any active typing
    public void StopTyping()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
            Debug.Log("[BeepSpeak] StopTyping called and stopped active typing coroutine");
        }
        else
        {
            Debug.Log("[BeepSpeak] StopTyping called but no active typing coroutine to stop");
        }
    }

    private PrecomputedSentence currentSentence;
    private PrecomputedWord currentWord;
    private PrecomputedSyllable currentSyllable;
    private int maxDialogueEntry = 100;
    private int currentDialogueEntry = -1;

    private string lastProcessedText = "";
    private string lastProcessedWord = "";
    private string targetText = "";
    private string currentDisplayedText = "";
    private float lastTargetUpdateTime = 0f;
    private float stabilityDelay = 0.05f;

    void Update()
    {

    }

    public void UpdateVoice(int vid = 100000, int vtimbre = 1, float pitch = 1.0f, float volume = 1.0f)
    {
        npcVoice.voiceID = vid;
        npcVoice.basePitch = pitch;
        npcVoice.baseVolume = volume;

        string[] filenames;
        if (vtimbre == 0)
        {
            filenames = new string[] { "DialogueSFX/v0_flat" };
        }
        else if (vtimbre == 1)
        {
            filenames = new string[] { "DialogueSFX/v1_flat" };
        }
        else
        {
            filenames = new string[] { "DialogueSFX/v0_flat" };
            Debug.LogError("Yo what voice you tryna load?");
        }

        AudioClip[] loadedClips = new AudioClip[filenames.Length];
        for (int i = 0; i < filenames.Length; i++)
        {
            loadedClips[i] = Resources.Load<AudioClip>(filenames[i]);
            if (loadedClips[i] == null)
            {
                Debug.LogWarning($"Failed to load {filenames[i]}. Make sure it's in a Resources folder.");
            }
        }

        npcVoice.timbre = loadedClips;
    }

    public void StartDialogue(List<DialogueEntry> dialogueEntries)
    {
        dialogueQueue.Clear();
        foreach (var entry in dialogueEntries)
        {
            dialogueQueue.Enqueue(entry);
        }
        maxDialogueEntry = dialogueQueue.Count;
        DisplayNextDialogue();
    }

    // REVISED: The core method for handling text streaming from the LLM
    public void UpdateStreamingText(string cumulativeText)
    {
        // CRITICAL: Prevent function call text from showing to players
        int actionMarkerIndex = cumulativeText.IndexOf("[/ACTION]:");
        if (actionMarkerIndex == -1)
            actionMarkerIndex = cumulativeText.IndexOf("\nACTION:");
        
        if (actionMarkerIndex != -1)
        {
            // Only display text before the function call marker
            cumulativeText = cumulativeText.Substring(0, actionMarkerIndex).TrimEnd();
            Debug.Log($"[BeepSpeak] Filtered out function call markers from displayed text");
        }
        
        // Don't update if the text hasn't changed - prevents duplicate processing
        if (cumulativeText == targetText)
        {
            return;
        }
        
        // Update the target text
        string previousText = targetText;
        targetText = cumulativeText;
        lastTargetUpdateTime = Time.time;
        
        // If we're not already typing, start a new typing process
        if (typingCoroutine == null) 
        { 
            // Clean slate for new typing process - prevents text duplication
            currentDisplayedText = "";
            lastProcessedText = "";
            lastProcessedWord = "";
            
            // Clear both text components
            if (dialogueText != null)
                dialogueText.text = "";
            if (newUIResponseText != null)
                newUIResponseText.text = "";
                
            // Now start the typing coroutine with a clean state
            typingCoroutine = StartCoroutine(ProcessTyping());
            
            Debug.Log($"[BeepSpeak] Starting new typing process for text: '{targetText}'");
        }
        else
        {
            // We're already typing, just let the coroutine continue with the updated target text
            // The ProcessTyping coroutine will read the latest targetText value
            Debug.Log($"[BeepSpeak] Updated target text while typing continues: '{targetText}'");
        }
        
        // No longer needed - timeout is now handled by BaseDialogueManager
    }
    
    // Removed redundant timeout coroutine - timeout handling now centralized in BaseDialogueManager
    
    // Method to force immediate completion without waiting
    public void ForceCompleteTyping()
    {
        // Check and update both text components
        string previousText = "";
        
        if (newUIResponseText != null)
            previousText = newUIResponseText.text;
        else if (dialogueText != null)
            previousText = dialogueText.text;
        
        int previousLength = previousText.Length;
        
        // Check if there's a significant difference between displayed and target text
        bool significantDifference = (previousLength > 0 && targetText.Length > 0 && 
            (previousLength < targetText.Length * 0.5f || 
             !targetText.StartsWith(previousText.Substring(0, Math.Min(5, previousLength)))));
        
        // Debug logs to understand what's happening
        Debug.Log($"[BeepSpeak DEBUG - COMPARISON] Force completion executed");
        Debug.Log($"[BeepSpeak DEBUG - COMPARISON] Previous displayed text: '{previousText}'");
        Debug.Log($"[BeepSpeak DEBUG - COMPARISON] Target text that should display: '{targetText}'");
        Debug.Log($"[BeepSpeak DEBUG - COMPARISON] Text lengths - Previous: {previousLength}, Target: {targetText.Length}");
        
        if (significantDifference)
        {
            Debug.LogWarning($"[BeepSpeak DEBUG - COMPARISON] Significant difference detected between displayed and target text!");
            
            // For smoother transition with large text differences, add a brief transition effect
            StartCoroutine(SmoothTextTransition(previousText, targetText));
        }
        else
        {
            // Small difference - just set the text directly
            if (newUIResponseText != null)
                newUIResponseText.text = targetText;
            else if (dialogueText != null)
                dialogueText.text = targetText;
            
            currentDisplayedText = targetText;
        }
        
        // Clean up the coroutine
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
            Debug.Log("[BeepSpeak DEBUG - COMPARISON] Typing coroutine stopped");
        }
    }
    
    // Smooth transition between texts when there's a big jump
    private IEnumerator SmoothTextTransition(string fromText, string toText)
    {
        // First determine which text component to use
        TMP_Text targetTextComponent = newUIResponseText != null ? newUIResponseText : dialogueText;
        if (targetTextComponent == null) yield break;
        
        // Calculate transition frames based on text length difference
        int framesToTransition = Mathf.Min(10, Mathf.CeilToInt((toText.Length - fromText.Length) / 50f));
        
        // Do a quick fade transition to avoid jarring instant text replacement
        float startTime = Time.realtimeSinceStartup;
        
        // First briefly fade out the current text (very quick, just 2-3 frames)
        float fadeOutDuration = 0.05f;
        float elapsed = 0f;
        Color originalColor = targetTextComponent.color;
        Color transparentColor = originalColor;
        transparentColor.a = 0;
        
        while (elapsed < fadeOutDuration)
        {
            elapsed = Time.realtimeSinceStartup - startTime;
            float t = elapsed / fadeOutDuration;
            targetTextComponent.color = Color.Lerp(originalColor, transparentColor, t);
            yield return null;
        }
        
        // Switch to the complete text while invisible
        targetTextComponent.text = toText;
        currentDisplayedText = toText;
        
        // Fade back in
        startTime = Time.realtimeSinceStartup;
        fadeOutDuration = 0.1f;
        elapsed = 0f;
        
        while (elapsed < fadeOutDuration)
        {
            elapsed = Time.realtimeSinceStartup - startTime;
            float t = elapsed / fadeOutDuration;
            targetTextComponent.color = Color.Lerp(transparentColor, originalColor, t);
            yield return null;
        }
        
        // Ensure we're back to normal
        targetTextComponent.color = originalColor;
    }

    // Added method to get the current target text length
    public int GetCurrentTargetLength()
    {
        return targetText?.Length ?? 0;
    }

    // Store final text to use once current animation completes
    // This prevents interrupting ongoing typing animations
    public void SetFinalText(string finalText)
    {
        // Only update the target text without restarting the animation
        if (string.IsNullOrEmpty(finalText) || finalText == targetText)
        {
            return;
        }
        
        // Store the final text
        targetText = finalText;
        lastTargetUpdateTime = Time.time;
        
        // No timeout needed - BaseDialogueManager now handles timing with dynamic calculation
        
        Debug.Log($"[BeepSpeak] Stored final text (len={finalText.Length}) for when current animation completes");
    }

    // Simple implementation that maintains the original character-by-character behavior
    private IEnumerator ProcessTyping()
    {
        Debug.Log("[BeepSpeak DEBUG] Starting ProcessTyping for text of length: " + targetText.Length);
        Debug.Log("[BeepSpeak DEBUG] Full target text: '" + targetText + "'");
        
        // Initialize state for tracking
        int charactersProcessed = 0;
        string lastDisplayed = "";
        
        // Process the text character by character
        while (charactersProcessed < targetText.Length)
        {
            // Check if target text has changed during typing
            if (lastDisplayed != currentDisplayedText)
            {
                Debug.Log("[BeepSpeak DEBUG] Current displayed text changed unexpectedly!");
                Debug.Log("[BeepSpeak DEBUG] Expected: '" + lastDisplayed + "'");
                Debug.Log("[BeepSpeak DEBUG] Actual: '" + currentDisplayedText + "'");
            }
            
            // Make sure we don't try to access beyond the string length
            // (in case targetText was shortened somehow)
            if (charactersProcessed >= targetText.Length)
            {
                Debug.Log("[BeepSpeak DEBUG] Breaking early, charactersProcessed >= targetText.Length");
                break;
            }
                
            // Get the next character to display
            char nextChar = targetText[charactersProcessed];
            
            // Add it to our displayed text
            currentDisplayedText += nextChar;
            lastDisplayed = currentDisplayedText;
            
            // Log every 5 characters to avoid flooding the console
            if (charactersProcessed % 5 == 0 || nextChar == '.' || nextChar == '!' || nextChar == '?')
            {
                Debug.Log($"[BeepSpeak DEBUG] Typed {charactersProcessed+1}/{targetText.Length}: Current text = '{currentDisplayedText}'");
            }
            
            // Update the UI - check BOTH text components
            if (newUIResponseText != null)
                newUIResponseText.text = currentDisplayedText;
            else if (dialogueText != null)
                dialogueText.text = currentDisplayedText;
                
            // Play appropriate sound for this character
            string currentWord = ExtractCurrentWord(currentDisplayedText);
            if (!string.IsNullOrEmpty(currentWord))
            {
                int letterIndexInWord = currentWord.Length - 1;
                if (IsSyllable(currentWord, letterIndexInWord))
                {
                    PlayVoiceForSyllable(currentWord, letterIndexInWord);
                }
            }
            
            // Calculate delay based on character type
            float delay = npcVoice.baseSpeed + UnityEngine.Random.Range(-npcVoice.speedVariance, npcVoice.speedVariance);
            
            // Add extra pause for punctuation
            if (nextChar == '.' || nextChar == '!' || nextChar == '?')
                delay += 0.3f;
            else if (nextChar == ',' || nextChar == ';' || nextChar == ':')
                delay += 0.2f;
                
            // Wait before processing the next character
            yield return new WaitForSeconds(delay);
            
            // Move to the next character
            charactersProcessed++;
        }
        
        // Ensure the complete text is displayed
        if (newUIResponseText != null)
        {
            newUIResponseText.text = targetText;
            Debug.Log("[BeepSpeak DEBUG] Final text set in new UI: '" + targetText + "'");
        }
        else if (dialogueText != null)
        {
            dialogueText.text = targetText;
            Debug.Log("[BeepSpeak DEBUG] Final text set in legacy UI: '" + targetText + "'");
        }
        
        currentDisplayedText = targetText;
        
        Debug.Log("[BeepSpeak DEBUG] ProcessTyping completed, typed " + targetText.Length + " characters");
        typingCoroutine = null;
    }

    // Helper method to determine if a character indicates a syllable
    private bool IsSyllable(string word, int index)
    {
        if (string.IsNullOrEmpty(word) || index < 0 || index >= word.Length)
            return false;
        
        // Start of word is always a syllable
        if (index == 0)
            return true;
            
        char c = word[index];
        char prev = word[index - 1];
        
        // Vowels often mark syllables when preceded by consonants
        bool isVowel = "aeiouAEIOU".IndexOf(c) >= 0;
        bool prevIsConsonant = "aeiouAEIOU".IndexOf(prev) < 0;
        
        // Complex version would look at vowel patterns, consonant clusters, etc.
        // This simplified version just checks for vowels after consonants
        return isVowel && prevIsConsonant;
    }

    private int FindWordBoundary(string text)
    {
        int space = text.IndexOf(' ');
        int period = text.IndexOf('.');
        int comma = text.IndexOf(',');
        int exclaim = text.IndexOf('!');
        int question = text.IndexOf('?');

        // Find the smallest non-negative index.
        int[] indices = new int[] { space, period, comma, exclaim, question };
        int boundary = -1;
        foreach (int idx in indices)
        {
            if (idx >= 0)
            {
                if (boundary == -1 || idx < boundary)
                    boundary = idx;
            }
        }
        return boundary;
    }
    private string ExtractCurrentWord(string text)
    {
        int lastSpace = text.LastIndexOf(' ');
        string word = lastSpace == -1 ? text : text.Substring(lastSpace + 1);
        return word;
        /*
        int lastSpace = text.LastIndexOf(' ');
        if (lastSpace == -1)
            return text.Trim();
        else
            return text.Substring(lastSpace + 1).Trim();
        */
    }

    private bool EndsWithWordBoundary(string text)
    {
        return text.EndsWith(" ") || text.EndsWith(".") || text.EndsWith(",") || text.EndsWith("!") || text.EndsWith("?");
    }

    private string GetLastWord(string text)
    {
        string[] words = text.Split(' ');
        if (words.Length == 0)
            return "";

        string last = words[words.Length - 1].Trim();
        if (string.IsNullOrEmpty(last) && words.Length > 1)
            last = words[words.Length - 2].Trim();
        return last;
    }

    private void ProcessWordForBeep(string word)
    {
        bool playedSfx = false;
        for (int i = 0; i < word.Length; i++)
        {
            if (IsSyllable(word, i))
            {
                PlayVoiceForSyllable(word, i);
                playedSfx = true;
            }
        }
        if (!playedSfx)
        {
            PlayVoiceForSyllable(word, 0);
        }
    }

    private void PlayVoiceForSyllable(string word, int index)
    {
        if (npcVoice.timbre.Length == 0) return;

        int randomSeed = npcVoice.voiceID;
        for (int i = index; i < word.Length; i++)
        {
            randomSeed += char.ToUpper(word[i]);
        }

        float punctuationPitchModifier = 0f;
        float punctuationVolumeModifier = 0f;
        if (word.EndsWith("."))
        {
            punctuationPitchModifier = -0.1f;
            punctuationVolumeModifier = -0.05f;
        }
        else if (word.EndsWith("?"))
        {
            punctuationPitchModifier = 0.05f;
            punctuationVolumeModifier = 0f;
        }
        else if (word.EndsWith("!"))
        {
            punctuationPitchModifier = 0.02f;
            punctuationVolumeModifier = 0.1f;
        }

        AudioClip clip = npcVoice.timbre[randomSeed % npcVoice.timbre.Length];

        // Apply modifiers
        float randomPitch = (randomSeed % (int)(npcVoice.pitchVariance * 200f) - (npcVoice.pitchVariance * 100f)) / 100f;
        audioSource.pitch = npcVoice.basePitch + punctuationPitchModifier + randomPitch;
        audioSource.volume = npcVoice.baseVolume + punctuationVolumeModifier;
        audioSource.PlayOneShot(clip);
    }

    private PrecomputedDialogue PrecomputeText(string text)
    {
        PrecomputedDialogue precomputed = new PrecomputedDialogue { sentences = new List<PrecomputedSentence>() };
        string[] sentenceParts = Regex.Split(text, @"(?<=[.?!�])\s+"); // Split by punctuation + space

        foreach (string originalText in sentenceParts)
        {
            if (string.IsNullOrWhiteSpace(originalText)) continue;

            PrecomputedSentence sentence = new PrecomputedSentence { words = new List<PrecomputedWord>() };

            // Make a modifiable copy
            string sentenceText = originalText.Trim();

            // Identify the last punctuation mark
            char lastChar = sentenceText[sentenceText.Length - 1];
            if (".?!�".Contains(lastChar))
            {
                sentence.sentenceEnd = lastChar;
                sentenceText = sentenceText.Substring(0, sentenceText.Length);
            }
            else
            {
                sentence.sentenceEnd = '.'; // Default if no punctuation
            }

            string[] words = sentenceText.Split(' ', System.StringSplitOptions.RemoveEmptyEntries);
            foreach (string word in words)
            {
                PrecomputedWord precomputedWord = new PrecomputedWord { text = word, syllables = new List<PrecomputedSyllable>() };

                // Mark syllables
                int LastSyllable = -1;//the starting character of the last syllable
                for (int j = 0; j < word.Length; j++)
                {
                    if (IsSyllable(word, j))
                    {
                        PrecomputedSyllable syllable = new PrecomputedSyllable();
                        syllable.index = LastSyllable + 1;
                        syllable.text = word.Substring(LastSyllable + 1, j - LastSyllable);
                        precomputedWord.syllables.Add(syllable);
                        LastSyllable = j;
                    }
                }

                sentence.words.Add(precomputedWord);
            }

            precomputed.sentences.Add(sentence);
        }
        return precomputed;
    }

    private void DisplayNextDialogue()
    {
        if (dialogueQueue.Count > 0)
        {
            DialogueEntry entry = dialogueQueue.Dequeue();
            entry.speaker.StartTyping(entry.text);
            currentDialogueEntry++;
        }
        else
        {
            //if (dialogueText != null)
            //dialogueText.text = "";
        }
    }

    private IEnumerator DelayedClear()
    {
        yield return new WaitForSeconds(0.1f); // Give enough time to see the first letter
        dialogueText.text = "";
    }

    public void StartTyping(string text)
    {
        dialogueText.text = "";
        currentDisplayedText = "";
        targetText = "";
        lastProcessedText = "";
        lastProcessedWord = "";

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        PrecomputedDialogue precomputedText = PrecomputeText(text); // Precompute sentences, words, syllables
        typingCoroutine = StartCoroutine(TypeText(precomputedText)); // Pass precomputed text
    }

    private IEnumerator TypeText(PrecomputedDialogue dialogue)
    {
        // isTyping = true; // Removed, state is tracked by typingCoroutine != null
        //Debug.Log($"Sentences: {dialogue.sentences.Count}");
        foreach (var sentence in dialogue.sentences)
        {
            currentSentence = sentence; // Track sentence context

            foreach (var word in sentence.words)
            {
                currentWord = word; // Track word context

                for (int i = 0; i < word.text.Length; i++)
                {
                    //Debug.Log($"Before Typing: {dialogueText.text}");
                    dialogueText.text += word.text[i];
                    //Debug.Log($"Typing: {word.text[i]}, Full Text: {dialogueText.text}");
                    foreach (var syl in word.syllables)
                    {
                        if (syl.index == i)
                        {
                            currentSyllable = syl; // Track syllable index
                            PlayVoice();
                            break;
                        }
                    }

                    yield return new WaitForSeconds(npcVoice.baseSpeed + UnityEngine.Random.Range(-npcVoice.speedVariance, npcVoice.speedVariance));
                }

                dialogueText.text += " "; // Space between words
            }

            // Pause for punctuation
            float pauseTime = sentence.sentenceEnd == '.' || sentence.sentenceEnd == '?' || sentence.sentenceEnd == '!' ? 0.5f :
                              sentence.sentenceEnd == '�' ? 1f : 0.35f; // Short pause for commas
            yield return new WaitForSeconds(pauseTime);
        }

        // isTyping = false; // Removed, state is tracked by typingCoroutine != null
        typingCoroutine = null; // Ensure coroutine reference is cleared here as well
    }

    private void PlayVoice()
    {
        //Only play if enough time has passed since the last sound.
        if (Time.time - lastSfxTime < sfxCooldown)
            return;
        lastSfxTime = Time.time;

        if (npcVoice.timbre.Length == 0) return; //no audio file to play

        //Predictable Random
        int randomSeed = npcVoice.voiceID;
        foreach (var c in currentSyllable.text.ToUpper())
            randomSeed += c;

        AudioClip clip = npcVoice.timbre[randomSeed % npcVoice.timbre.Length];
        float randomPitch = (randomSeed % (int)(npcVoice.pitchVariance * 200f) - (npcVoice.pitchVariance * 100f)) / 100f;
        float randomVolume = (randomSeed % (int)(npcVoice.volumeVariance * 200f) - (npcVoice.volumeVariance * 100f)) / 100f;

        // Modify pitch based on phrase endings
        float pitchModifier = 0f;
        float volumeModifier = 0f;
        
        // Check if we're at the end of a sentence
        if (currentSentence != null && currentWord != null && 
            currentSentence.words.IndexOf(currentWord) == currentSentence.words.Count - 1)
        {
            // Last word in sentence
            if (currentSentence.sentenceEnd == '.')
            {
                pitchModifier = -0.2f;
                volumeModifier = -0.1f;
            }
            else if (currentSentence.sentenceEnd == '?')
            {
                pitchModifier = 0.2f;
                volumeModifier = 0.05f;
            }
            else if (currentSentence.sentenceEnd == '!')
            {
                pitchModifier = 0.1f;
                volumeModifier = 0.15f;
            }
        }

        // Apply all modifiers
        audioSource.pitch = npcVoice.basePitch + pitchModifier + randomPitch;
        audioSource.volume = npcVoice.baseVolume + volumeModifier + randomVolume;
        
        // Play the sound
        audioSource.PlayOneShot(clip);
    }
}
