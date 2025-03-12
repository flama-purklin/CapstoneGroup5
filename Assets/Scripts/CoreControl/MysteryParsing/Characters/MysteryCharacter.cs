using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;



public class MysteryCharacter
{
    [JsonProperty("core")]
    public CharacterCore Core { get; set; }

    [JsonProperty("mind_engine")]
    public MindEngine MindEngine { get; set; }

    [JsonProperty("initial_location")]
    public string InitialLocation { get; set; }

    [JsonProperty("key_testimonies")]
    public Dictionary<string, Testimony> KeyTestimonies { get; set; }
}

public class CharacterCore
{
    [JsonProperty("involvement")]
    public Involvement Involvement { get; set; }

    [JsonProperty("whereabouts")]
    public List<Whereabouts> Whereabouts { get; set; }

    [JsonProperty("relationships")]
    public List<Relationship> Relationships { get; set; }

    [JsonProperty("agenda")]
    public Agenda Agenda { get; set; }
}

public class Involvement
{
    [JsonProperty("role")]
    public string Role { get; set; }

    [JsonProperty("type")]
    public string Type { get; set; }

    [JsonProperty("mystery_attributes")]
    public List<string> MysteryAttributes { get; set; }
}

//this should be restructured in the json, would be better to have each key be a reference to the character object
//Ex. Dictionary<string, Whereabout> where string is "second_class_car_1" rather than arbitrary index
//items should be added chronologically anyways, so the index is unnecessary
public class Whereabouts
{
    [JsonProperty("key")]
    public string Key { get; set; }

    [JsonProperty("value")]
    public WhereaboutData WhereaboutData { get; set; }
}

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

//this should be restructured in the json, would be better to have each key be a reference to the character object
//Ex. Dictionary<string, Relationship> where string is "penelope_valor" rather than the full character name
public class Relationship
{
    [JsonProperty("key")]
    public string CharName { get; set; }

    [JsonProperty("value")]
    public RelationshipData RelationshipData { get; set; }
}

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


public class Testimony
{
    [JsonProperty("content")]
    public string Content { get; set; }

    [JsonProperty("reveals")]
    public string Reveals { get; set; }

    [JsonProperty("requires")]
    public List<string> Requires {  get; set; }

    [JsonProperty("state")]
    public string State { get; set; }

    [JsonProperty("methods")]
    public List<string> Methods { get; set; }
}

