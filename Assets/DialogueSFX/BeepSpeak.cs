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
        // If the new cumulative text is shorter than our previous target, assume this is a new response.
        if (cumulativeText.Length < targetText.Length)
        {
            currentDisplayedText = "";
            lastProcessedText = "";
            lastProcessedWord = "";
            if (dialogueText != null)
                dialogueText.text = "";
        }

        targetText = cumulativeText;
        lastTargetUpdateTime = Time.time;
        if (typingCoroutine == null) { typingCoroutine = StartCoroutine(ProcessTyping()); }
    }
    private IEnumerator ProcessTyping()
    {
        // Continue until all characters in targetText have been typed
        while (currentDisplayedText.Length < targetText.Length)
        {
            // Wait until the targetText has not updated for at least stabilityDelay seconds.
            if (Time.time - lastTargetUpdateTime < stabilityDelay)
            {
                yield return new WaitForSeconds(0.03f);
                continue;
            }

            // Get what remains to be typed.
            string remaining = targetText.Substring(currentDisplayedText.Length);

            // If there is no space or punctuation immediately, wait until a complete word is available
            int boundaryIndex = FindWordBoundary(remaining);
            // If no boundary is found, we simply wait a short period and try again
            if (boundaryIndex == -1)
            {
                yield return new WaitForSeconds(0.03f);
                continue;
            }

            // Extract the complete next word plus the boundary character
            string nextChunk = remaining.Substring(0, boundaryIndex + 1);

            // Type the word letter-by-letter.
            for (int i = 0; i < nextChunk.Length; i++)
            {
                char letter = nextChunk[i];
                currentDisplayedText += letter;
                if (dialogueText != null) dialogueText.text = currentDisplayedText;

                // Determine the delay for this character.
                float delay = npcVoice.baseSpeed + UnityEngine.Random.Range(-npcVoice.speedVariance, npcVoice.speedVariance);
                if (letter == '.' || letter == '!' || letter == '?') { delay += 0.5f; }
                else if(letter == ';' || letter == ':' || letter == ',') { delay += 0.35f; }
                else if(letter == '�') { delay += 1f; }

                // assume a word ends at the first space or punctuation
                string word = ExtractCurrentWord(currentDisplayedText);
                if (!string.IsNullOrEmpty(word))
                {
                    int letterIndexInWord = word.Length - 1; // index of the just-typed letter within the current word
                    if (IsSyllable(word, letterIndexInWord))
                    {
                        PlayVoiceForSyllable(word, letterIndexInWord);
                    }
                }
                yield return new WaitForSeconds(delay);
            }
            
        }
        // Finished typing; clear the coroutine reference
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
