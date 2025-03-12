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
    public List<Whereabout> Whereabouts { get; set; }

    [JsonProperty("relationships")]
    public Dictionary<string, Relationship> Relationships { get; set; }

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

public class Whereabout
{
    [JsonProperty("location")]
    public string Location { get; set; }

    [JsonProperty("action")]
    public string Action { get; set; }

    [JsonProperty("events")]
    public List<string> Events { get; set; }
}

public class Relationship
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
}

public class StateOfMind
{
    [JsonProperty("worries")]
    public string Worries { get; set; }

    [JsonProperty("feelings")]
    public string Feelings { get; set; }
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

