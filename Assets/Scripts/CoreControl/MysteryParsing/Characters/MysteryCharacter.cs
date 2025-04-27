using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;



public class MysteryCharacter
{
    [JsonProperty("core")]
    public CharacterCore Core { get; set; }

    [JsonProperty("mind_engine")]
    public MindEngine MindEngine { get; set; }

    [JsonProperty("initial_location")] // Moved back to top level
    public string InitialLocation { get; set; }

    // KeyTestimonies removed from here
}

public class CharacterCore
{
    // InitialLocation removed from here

    [JsonProperty("involvement")]
    public Involvement Involvement { get; set; }

    [JsonProperty("whereabouts")]
    public Dictionary<string, WhereaboutData> Whereabouts { get; set; } // Changed from List<Whereabouts>

    [JsonProperty("relationships")]
    public Dictionary<string, RelationshipData> Relationships { get; set; } // Changed from List<Relationship>

    [JsonProperty("agenda")]
    public Agenda Agenda { get; set; }

    [JsonProperty("appearance")] // Added
    public Appearance Appearance { get; set; }

    [JsonProperty("voice")] // Added
    public Voice Voice { get; set; }

    [JsonProperty("revelations")] // Added for Task 2 Prep
    public Dictionary<string, Revelation> Revelations { get; set; } 
}

// New Revelation class definition (Task 2 Prep)
public class Revelation
{
    [JsonProperty("content")]
    public string Content { get; set; }

    [JsonProperty("reveals")]
    public string Reveals { get; set; }

    [JsonProperty("trigger_type")]
    public string TriggerType { get; set; }

    [JsonProperty("trigger_value")]
    public string TriggerValue { get; set; }

    [JsonProperty("accessibility")]
    public string Accessibility { get; set; }
}


public class Involvement
{
    [JsonProperty("role")]
    public string Role { get; set; }

    [JsonProperty("type")]
    public string Type { get; set; }
}

// Whereabouts wrapper class removed

public class WhereaboutData
{
    //nullable has to be turned on and off for some reason? Idk
#nullable enable
    [JsonProperty("location")]
    public string? Location { get; set; }

    [JsonProperty("circumstance")]
    public string? Circumstance { get; set; }
#nullable disable

    [JsonProperty("action")]
    public string Action { get; set; }

    [JsonProperty("events")]
    public List<string> Events { get; set; }
}

// Relationship wrapper class removed

//May need to be another layer deeper if the key isn't getting stored
public class RelationshipData
{
    [JsonProperty("attitude")]
    public string Attitude { get; set; }

    [JsonProperty("history")]
    public List<string> History { get; set; }

    [JsonProperty("known_secrets")]
    public List<string> KnownSecrets { get; set; }
}

// New Appearance class definition
public class Appearance
{
    [JsonProperty("base")]
    public string Base { get; set; }

    [JsonProperty("skin_color")]
    public string SkinColor { get; set; }

    [JsonProperty("hair")]
    public string Hair { get; set; }

    [JsonProperty("outfit")]
    public string Outfit { get; set; }

    [JsonProperty("shoes")]
    public string Shoes { get; set; }

    [JsonProperty("eyes")]
    public string Eyes { get; set; }

    [JsonProperty("nose")]
    public string Nose { get; set; }

    [JsonProperty("mouth")]
    public string Mouth { get; set; }
}

// Newer Voice class definition
public class Voice
{
    [JsonProperty("voice_ID")]
    public int VoiceID { get; set; }

    [JsonProperty("timbre")]
    public int Timbre { get; set; }

    [JsonProperty("pitch")]
    public float Pitch { get; set; }

    [JsonProperty("speed")]
    public float Speed { get; set; }

    [JsonProperty("volume")]
    public float Volume { get; set; }
}


public class Agenda
{
    [JsonProperty("primary_goal")]
    public string PrimaryGoal { get; set; }
}

public class MindEngine
{
    [JsonProperty("identity")]
    public Identity Identity { get; set; }

    [JsonProperty("state_of_mind")]
    public StateOfMind StateOfMind { get; set; }
    
    [JsonProperty("speech_patterns")]
    public SpeechPatterns SpeechPatterns { get; set; }
}

public class Identity
{
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("occupation")]
    public string Occupation { get; set; }

    [JsonProperty("personality")]
    public Personality Personality { get; set; }
}

public class Personality
{
    [JsonProperty("O")]
    public float O { get; set; }
    [JsonProperty("C")]
    public float C { get; set; }
    [JsonProperty("E")]
    public float E { get; set; }
    [JsonProperty("A")]
    public float A { get; set; }
    [JsonProperty("N")]
    public float N { get; set; }
}

public class StateOfMind
{
    [JsonProperty("worries")]
    public string Worries { get; set; }

    [JsonProperty("feelings")]
    public string Feelings { get; set; }

    [JsonProperty("reasoning_style")]
    public string ReasoningStyle { get; set; }
}

public class SpeechPatterns
{
    [JsonProperty("vocabulary_level")]
    public string VocabularyLevel { get; set; }

    [JsonProperty("sentence_style")]
    public List<string> SentenceStyle { get; set; }

    [JsonProperty("speech_quirks")]
    public List<string> SpeechQuirks { get; set; }

    [JsonProperty("common_phrases")]
    public List<string> CommonPhrases { get; set; }
}

// Testimony class removed
