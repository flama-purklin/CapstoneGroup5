using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Text.RegularExpressions;
using System;

public class BeepSpeak : MonoBehaviour
{
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
    private bool isTyping = false;

    private PrecomputedSentence currentSentence;
    private PrecomputedWord currentWord;
    private PrecomputedSyllable currentSyllable;
    private int maxDialogueEntry = 100;
    private int currentDialogueEntry = -1;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            if (isTyping)
            {
                // Instantly finish displaying text
                StopCoroutine(typingCoroutine);
                dialogueText.text = dialogueQueue.Peek().text;
                isTyping = false;
            }
            else
            {
                // Check if there is more dialogue in the queue
                if (dialogueQueue.Count > 0)
                {
                    // Move to the next dialogue entry
                    DisplayNextDialogue();
                }
                else if (!isTyping && currentDialogueEntry == maxDialogueEntry - 1)
                {
                    // No more dialogue, clear the text
                    dialogueText.text = "";
                }
            }
        }
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

    private PrecomputedDialogue PrecomputeText(string text)
    {
        PrecomputedDialogue precomputed = new PrecomputedDialogue { sentences = new List<PrecomputedSentence>() };
        string[] sentenceParts = Regex.Split(text, @"(?<=[.?!…])\s+"); // Split by punctuation + space

        foreach (string originalText in sentenceParts)
        {
            if (string.IsNullOrWhiteSpace(originalText)) continue;

            PrecomputedSentence sentence = new PrecomputedSentence { words = new List<PrecomputedWord>() };

            // Make a modifiable copy
            string sentenceText = originalText.Trim();

            // Identify the last punctuation mark
            char lastChar = sentenceText[sentenceText.Length - 1];
            if (".?!…".Contains(lastChar))
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
        isTyping = true;
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
                              sentence.sentenceEnd == '…' ? 1f : 0.35f; // Short pause for commas
            yield return new WaitForSeconds(pauseTime);
        }

        isTyping = false;
    }

    private void PlayVoice()
    {
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
        //Debug.Log($"Random Pitch {randomPitch}");
        //Not all audio is playing???
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