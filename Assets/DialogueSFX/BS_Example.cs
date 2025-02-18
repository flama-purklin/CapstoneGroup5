using System.Collections.Generic;
using UnityEngine;

public class BS_Example : MonoBehaviour
{
    public BeepSpeak npc1;
    public BeepSpeak npc2;
    public BeepSpeak npc3;
    public BeepSpeak npc4;
    public BeepSpeak npc5;
    public BeepSpeak npc6;

    void Start()
    {
        DontDestroyOnLoad(gameObject);

        List<BeepSpeak.DialogueEntry> dialogueEntries = new List<BeepSpeak.DialogueEntry>
        {
            new BeepSpeak.DialogueEntry { text = "Test.", speaker = npc1 },
            new BeepSpeak.DialogueEntry { text = "Alright let's start off with an example sentence.", speaker = npc1 },
            new BeepSpeak.DialogueEntry { text = "Then a question? And an exclamation!", speaker = npc2 },
            new BeepSpeak.DialogueEntry { text = "Yoshi you gotta stop commiting tax fraud.", speaker = npc3 },
            new BeepSpeak.DialogueEntry { text = "These audio files are very noisy. Do you agree?", speaker = npc4 },
            new BeepSpeak.DialogueEntry { text = "I took this audio file from FamiTracker.", speaker = npc6 },
            new BeepSpeak.DialogueEntry { text = "Do you think this is the best sounding voice so far? I do.", speaker = npc2 },
            new BeepSpeak.DialogueEntry { text = "Ha. Ha. Ha.", speaker = npc1 },
            new BeepSpeak.DialogueEntry { text = "To show off predictable randomness, please carefully listen to the following sentence.", speaker = npc2 },
            new BeepSpeak.DialogueEntry { text = "This word, the word this, is repeated so you can hear the similarities between this and this.", speaker = npc5 },
            new BeepSpeak.DialogueEntry { text = "This presentation has been brought to you by the number five.", speaker = npc2 },

            new BeepSpeak.DialogueEntry { text = "Alright folks it's time for the top ten numbers from one through ten.", speaker = npc1 },
            new BeepSpeak.DialogueEntry { text = "Starting us off at number 10, it's number seven!", speaker = npc2 },
            new BeepSpeak.DialogueEntry { text = "At number nine, it's number eight.", speaker = npc3 },
            new BeepSpeak.DialogueEntry { text = "And at number seven it's number nine.", speaker = npc4 },
            new BeepSpeak.DialogueEntry { text = "But look out seven, at number six it's number four.", speaker = npc5 },
            new BeepSpeak.DialogueEntry { text = "Quick recap, because at number nine was number eight, which you'll need to know the context for this next number because it's number five for number four.", speaker = npc6 },
            new BeepSpeak.DialogueEntry { text = "And at number four it's number two!", speaker = npc1 },
            new BeepSpeak.DialogueEntry { text = "And number three is also number five!", speaker = npc2 },
            new BeepSpeak.DialogueEntry { text = "Two number fives? That's right foks at number six and number three.", speaker = npc1 },
            new BeepSpeak.DialogueEntry { text = "Where's number three? Didn't even make the top ten.", speaker = npc2 },
            new BeepSpeak.DialogueEntry { text = "And at number two it's number one.", speaker = npc6 },
            new BeepSpeak.DialogueEntry { text = "So close One, but there can only be one number one and that one number at number one is number FIVE!", speaker = npc1 }
        };

        npc1.StartDialogue(dialogueEntries);
    }
}