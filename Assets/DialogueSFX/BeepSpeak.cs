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

    public TMP_Text dialogueText;
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

    // Called every time there is a new update from the LLM.
    public void UpdateStreamingText(string cumulativeText)
    {
        // Don't update if the text hasn't changed - prevents duplicate processing
        if (cumulativeText == targetText)
        {
            return;
        }
        
        // Update the target text
        targetText = cumulativeText;
        lastTargetUpdateTime = Time.time;
        
        // If we need to start a new typing process, ALWAYS reset the state first
        if (typingCoroutine == null) 
        { 
            // Clean slate for new typing process - prevents text duplication
            currentDisplayedText = "";
            lastProcessedText = "";
            lastProcessedWord = "";
            if (dialogueText != null)
                dialogueText.text = "";
                
            // Now start the typing coroutine with a clean state
            typingCoroutine = StartCoroutine(ProcessTyping());
        }
        
        // If this is a "final" update (likely the last chunk from LLM), 
        // make sure we have a way to force complete the text display, but
        // give the typing animation much more time to complete naturally
        if (cumulativeText.EndsWith(".") || cumulativeText.EndsWith("!") || cumulativeText.EndsWith("?"))
        {
            // Start a backup timer to ensure typingCoroutine gets cleaned up,
            // but with a much longer delay to allow the typing animation to play
            StartCoroutine(EnsureTypingCompletesWithLongDelay());
        }
    }
    
    // Modified method with a longer delay to allow typing animation to complete naturally
    private IEnumerator EnsureTypingCompletesWithLongDelay()
    {
        // Wait for a much longer time (8 seconds) after receiving final punctuation
        // This should allow most reasonable text segments to finish typing naturally
        yield return new WaitForSeconds(60.0f);
        
        // If the typing coroutine is still running after this very long time, it might be genuinely stuck
        if (typingCoroutine != null)
        {
            Debug.LogWarning("[BeepSpeak] Typing still active 60s after final update. Force completing display.");
            
            // Force complete the text display
            if (dialogueText != null)
            {
                dialogueText.text = targetText;
                currentDisplayedText = targetText;
            }
            
            // Stop the typing coroutine if it's still running
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
                typingCoroutine = null;
                Debug.Log("[BeepSpeak] ProcessTyping force completed after long timeout, typingCoroutine set to null");
            }
        }
    }
    
    // Method to force immediate completion without waiting
    public void ForceCompleteTyping()
    {
        // Skip any remaining typing animation
        if (dialogueText != null)
        {
            string previousText = dialogueText.text;
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
                // This makes the transition less jarring when forcing completion of a long text
                StartCoroutine(SmoothTextTransition(previousText, targetText));
            }
            else
            {
                // Small difference - just set the text directly
                dialogueText.text = targetText;
                currentDisplayedText = targetText;
            }
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
        // Calculate transition frames based on text length difference
        int framesToTransition = Mathf.Min(10, Mathf.CeilToInt((toText.Length - fromText.Length) / 50f));
        
        // Do a quick fade transition to avoid jarring instant text replacement
        float startTime = Time.realtimeSinceStartup;
        
        // First briefly fade out the current text (very quick, just 2-3 frames)
        float fadeOutDuration = 0.05f;
        float elapsed = 0f;
        Color originalColor = dialogueText.color;
        Color transparentColor = originalColor;
        transparentColor.a = 0;
        
        while (elapsed < fadeOutDuration)
        {
            elapsed = Time.realtimeSinceStartup - startTime;
            float t = elapsed / fadeOutDuration;
            dialogueText.color = Color.Lerp(originalColor, transparentColor, t);
            yield return null;
        }
        
        // Switch to the complete text while invisible
        dialogueText.text = toText;
        currentDisplayedText = toText;
        
        // Fade back in
        startTime = Time.realtimeSinceStartup;
        fadeOutDuration = 0.1f;
        elapsed = 0f;
        
        while (elapsed < fadeOutDuration)
        {
            elapsed = Time.realtimeSinceStartup - startTime;
            float t = elapsed / fadeOutDuration;
            dialogueText.color = Color.Lerp(transparentColor, originalColor, t);
            yield return null;
        }
        
        // Ensure we're back to normal
        dialogueText.color = originalColor;
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
        
        // Start the safety timeout to ensure typing eventually completes
        if (finalText.EndsWith(".") || finalText.EndsWith("!") || finalText.EndsWith("?"))
        {
            StartCoroutine(EnsureTypingCompletesWithLongDelay());
        }
        
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
            
            // Update the UI
            if (dialogueText != null)
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
        if (dialogueText != null)
        {
            dialogueText.text = targetText;
            Debug.Log("[BeepSpeak DEBUG] Final text set in UI: '" + targetText + "'");
        }
        currentDisplayedText = targetText;
        
        Debug.Log("[BeepSpeak DEBUG] ProcessTyping completed, typed " + targetText.Length + " characters");
        typingCoroutine = null;
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
        float randomPitch = (randomSeed % (npcVoice.pitchVariance * 200f) - (npcVoice.pitchVariance * 100f)) / 100f;
        float randomVolume = (randomSeed % (npcVoice.volumeVariance * 200f) - (npcVoice.volumeVariance * 100f)) / 100f;

        // Modify pitch based on phrase endings
        float pitchModifier = 0f;
        float volumeModifier = 0f;
        if (currentSentence.sentenceEnd == '?')
        {
            pitchModifier = 0.05f; // Slight raise for questions
            if (currentWord == currentSentence.words[^1]) pitchModifier += 0.05f; // Last word pitch increase
            if (currentSyllable == currentWord.syllables[^1]) pitchModifier += 0.1f; // Final syllable
        }
        else if (currentSentence.sentenceEnd == '!')
        {
            pitchModifier = 0.02f;
            volumeModifier = 0.1f;
        }
        else
        {
            if (currentWord == currentSentence.words[^1])
            {
                pitchModifier -= 0.1f;
                volumeModifier -= 0.05f;
            }
        }
        //Modify voice based on given emotion (ADD LATER)

        audioSource.pitch = npcVoice.basePitch + pitchModifier + randomPitch;
        audioSource.volume = npcVoice.baseVolume + volumeModifier + randomVolume;
        //Debug.Log($"Playing syllable {currentSyllable.text}");
        audioSource.PlayOneShot(clip);
    }

    //determines whether a syllable "starts" at a given letter. used for accurate-ish syllable count
    private bool IsSyllable(string text, int index)
    {
        if (index < 0 || index >= text.Length)
        {
            Debug.LogError($"Syllable check out of bounds! Index: {index}, Text Length: {text.Length}");
            return false;
        }

        char c = text[index];
        string vowels = "aeiouyAEIOUY";
        string ichar = "iI";
        string tchar = "tT";
        string nchar = "nN";
        string lchar = "lL";

        if (text.Length == 1) return true;//hardcode exceptions for 'W', '7', e.t.c.? how do i handle multi-syllable 1 letter words?
        if (!vowels.Contains(c)) return false;//vowels determine syllables
        if (index == 0) { return true; }

        //Check if any letters remain. This is because punctuation can throw it off otherwise.
        bool remainingLetters = false;
        for (int i = index + 1; i <= text.Length - 1; i++)
        {
            if (Char.IsLetter(text[i]))
            {
                remainingLetters = true;
                break;
            }
        }

        //previous character is consonant or 'i' and not "tion"
        bool isSyllable = !vowels.Contains(text[index - 1]) ||
                   (ichar.Contains(text[index - 1]) &&
                   (!remainingLetters || !(tchar.Contains(text[index - 2]) && nchar.Contains(text[index + 1]))));

        // Special case: final "e" //dont forget consonant LE like uncle        
        if ((c == 'e' || c == 'E') && !remainingLetters)
        {
            // Count only if "e" is the *only* vowel in the word
            bool hasOtherVowel = false;
            for (int i = 0; i < text.Length - 1; i++)
            {
                if (vowels.Contains(text[i]))
                {
                    hasOtherVowel = true;
                    break;
                }
            }
            if (hasOtherVowel)
                isSyllable = false; // Silent "e"
            if (index > 2 && !vowels.Contains(text[index - 2]) && lchar.Contains(text[index - 1]))
                isSyllable = true;//exception like "uncle"
        }

        return isSyllable;
    }
}
