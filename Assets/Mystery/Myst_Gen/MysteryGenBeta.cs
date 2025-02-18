using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public int id;
    public bool reached;        //Has this node been reached?
    public Node[] requirement;  //Which nodes must be reached first
    public string name;         //The name of the NPC that gives the info, or the evidence belongs to
}
public class InfoNode : Node
{
    //Info given by an NPC
    public string info;     //The info they give
    public string prompt;   //The trigger word(s) required to say to get them to give the info
    public InfoNode() { reached = false; }
    public InfoNode(string i, string p)
    {
        reached = false;
        info = i;
        prompt = p;
    }
}
public class EvidenceNode : Node
{
    //Physical Evidence
    public int objectID;
    public int location;    //TrainCarID
    public EvidenceNode() { reached = false; }
    public EvidenceNode(int o)
    {
        reached = false;
        objectID = o;
    }
}

public class MysteryGenBeta
{
    // ===================================
    // Data classes and definitions
    // ===================================
    public class MysteryTemplate
    {
        public string crime; // e.g. "murder", "theft"
        public string group; // what character setup (default, all gang members, farmers, etc)
        // Additional parameters (victim/culprit setup, solution style, etc.) can be added

        public MysteryTemplate(string c)
        {
            crime = c;
            group = "default";
        }
    }
    public class Character
    {
        public string archetype;    // gangmember, grandma, cop, e.t.c.
        public float[] friends;     // happiness or friendliness value for each other person
        public float[] temperament; // morality, sanity, violence, greed, desperation, pride
        public string emotion;
        public string name;
        public int role;            // 0 = extra, 1 = culprit, 2 = victim, 3 = witness/other, CHANGE LATER
        public bool alive;
        public int trainCarID;      // Location on the train
        //Knowledge?

        public Character()
        {
            emotion = "nuetral";           
            alive = true;
            role = 0;
        }
    }
    
    public class Crime
    {
        public string tool;         //What the crime was commited with
        public string victimName;   //If it's not a victimless crime
        public int time;            //When was the crime committed
        public int trainCarID;      //Where it happened
    }

    // ===================================
    // Hardcoded lists and configuration
    // ===================================

    // Mystery Templates
    public List<MysteryTemplate> mysteryTemplates = new List<MysteryTemplate>();

    // Character Archetypes
    public List<string> archetypes = new List<string> { "Grandma", "Cop", "Gang Member" };

    // Info Nodes (the info and prompt)
    public List<InfoNode> infoNodes = new List<InfoNode>();

    // Evidence Nodes (valid objects)
    public List<EvidenceNode> evidenceNodes = new List<EvidenceNode>();

    // Valid objects for evidence (instead of int it could be game objects, maybe as a header so can drag-in in unity)
    public List<int> evidenceObjects = new List<int> { 0, 1, 2 };

    // ===================================
    // Generation Functions
    // ===================================

    /// <summary>
    /// Creates a list of hardcoded mystery templates.
    /// </summary>
    public List<MysteryTemplate> GenerateTemplates()
    {
        // Hardcoded list of templates
        List<MysteryTemplate> templates = new List<MysteryTemplate>();
        MysteryTemplate murderTemplate = new MysteryTemplate("murder");
        templates.Add(murderTemplate);
        MysteryTemplate theftTemplate = new MysteryTemplate("theft");
        templates.Add(theftTemplate);
        // Add more templates here
        return templates;
    }

    /// <summary>
    /// Generates an evidence node for each valid object id.
    /// </summary>
    public List<EvidenceNode> GenerateEvidenceNodes()
    {
        List<EvidenceNode> en = new List<EvidenceNode>();
        for(int i = 0; i < evidenceObjects.Count; i++)
        {
            en.Add(new EvidenceNode(evidenceObjects[i]));
        }
        return en;
    }

    public List<InfoNode> GenerateInfoNodes()
    {
        List<InfoNode> nodes = new List<InfoNode>();
        nodes.Add(new InfoNode("I saw Character at the scene of the crime.", ""));
        return nodes;
    }

    /// <summary>
    /// Generates a list of characters with random names, archetypes, and temperament values.
    /// </summary>
    /// <param name="count">Number of characters to generate.</param>
    public List<Character> GenerateCharacters(int count)
    {
        List<Character> characters = new List<Character>();
        // Temp name list
        string[] possibleNames = { "Alice", "Bob", "Charlie", "Diana", "Edward", "Fiona", "George", "Hannah" };

        for (int i = 0; i < count; i++)
        {
            Character c = new Character();
            // Pick a random name and append an index to avoid duplicates
            c.name = possibleNames[Random.Range(0, possibleNames.Length)] + "_" + i;
            // Randomly assign an archetype from the list
            c.archetype = archetypes[Random.Range(0, archetypes.Count)];
            // Generate random temperament values (each between 0 and 10)
            c.temperament = new float[6];
            for (int j = 0; j < c.temperament.Length; j++)
            {
                c.temperament[j] = Random.Range(-1f, 1f);
            }
            // Randomly assign a train car for the character (assuming 10 train cars, IDs 0-9)
            c.trainCarID = Random.Range(0, 10);
            c.friends = new float[count];
            for(int j = 0; j < c.friends.Length; j++)
            {
                c.friends[j] = 0f;
            }
            characters.Add(c);
        }
        return characters;
    }

    /// <summary>
    /// Selects a culprit from the list of characters based on the mystery template and their temperament.
    /// </summary>
    /// <param name="template">The mystery template, which determines which temperament trait is most relevant.</param>
    /// <param name="characters">The list of generated characters.</param>
    /// <returns>The index of the chosen culprit.</returns>
    public int PickCulprit(MysteryTemplate template, List<Character> characters)
    {
        int culpritIndex = 0;
        float maxScore = -1;
        for (int i = 0; i < characters.Count; i++)
        {
            float score = 0;
            // For a murder, use the "violence" trait (index 2)
            if (template.crime == "murder")
            {
                score = characters[i].temperament[2];
            }
            // For a theft, use the "greed" trait (index 3)
            else if (template.crime == "theft")
            {
                score = characters[i].temperament[3];
            }
            // Additional crime types could use different traits

            if (score > maxScore)
            {
                maxScore = score;
                culpritIndex = i;
            }
        }
        // Mark the chosen character as the culprit (role = 1)
        characters[culpritIndex].role = 1;
        return culpritIndex;
    }

    /// <summary>
    /// Generates the details of the crime (tool, time, location, and victim) based on the template.
    /// </summary>
    /// <param name="template">The mystery template.</param>
    /// <param name="characters">The list of generated characters.</param>
    /// <param name="culprit">Index of the culprit in the characters list.</param>
    public Crime GenerateCrime(MysteryTemplate template, List<Character> characters, int culprit)
    {
        Crime crime = new Crime();
        // Decide on a tool based on crime type
        if (template.crime == "murder")
        {
            crime.tool = "knife"; // Could be randomized among several options
        }
        else if (template.crime == "theft")
        {
            crime.tool = "hands";
        }
        // Set a random time (e.g., hour of day between 0 and 23)
        crime.time = Random.Range(0, 24);
        // Choose a random train car where the crime occurred (assume 10 train cars, IDs 0-9)
        crime.trainCarID = Random.Range(0, 10);

        // Pick a victim from those who are not the culprit
        List<Character> potentialVictims = new List<Character>();
        for (int i = 0; i < characters.Count; i++)
        {
            if (i != culprit)
                potentialVictims.Add(characters[i]);
        }
        if (potentialVictims.Count > 0)
        {
            int victimIndex = Random.Range(0, potentialVictims.Count);
            Character victim = potentialVictims[victimIndex];
            victim.role = 2; // 2 represents the victim role
            crime.victimName = victim.name;
        }
        else
        {
            crime.victimName = "None";
        }
        return crime;
    }

    /// <summary>
    /// Generates a simple node graph (the mystery "constellation") of clues.
    /// Currently hardcoded for testing purposes, and due to lack of hardcoded nodes
    /// </summary>
    /// <param name="template">The mystery template.</param>
    /// <param name="characters">The list of generated characters (which might be used to assign NPC names to info nodes).</param>
    public List<Node> GenerateConstellation(MysteryTemplate template, List<Character> characters)
    {
        List<Node> nodes = new List<Node>();
        // Decide on a random number of nodes for the main chain (for example, between 3 and 5)
        int numNodes = Random.Range(3, 6);

        for (int i = 0; i < numNodes; i++)
        {
            // Randomly choose to create an InfoNode or an EvidenceNode
            bool useInfo = (Random.value < 0.5f);
            Node newNode = null;

            if (useInfo)
            {
                // Pick a random info node template and create a copy
                int index = Random.Range(0, infoNodes.Count);
                InfoNode selected = infoNodes[index];
                InfoNode copy = new InfoNode(selected.info, selected.prompt);
                newNode = copy;
            }
            else
            {
                // Pick a random evidence node template and create a copy
                int index = Random.Range(0, evidenceNodes.Count);
                EvidenceNode selected = evidenceNodes[index];
                EvidenceNode copy = new EvidenceNode(selected.objectID);
                // Assign a random location for the evidence
                copy.location = Random.Range(0, 10);
                newNode = copy;
            }

            // Set common fields
            newNode.id = i;

            // Set the node's "name" based on type:
            if (newNode is InfoNode)
            {
                // Pick an NPC name from the extras (role 0)
                List<Character> extras = characters.FindAll(c => c.role == 0);
                if (extras.Count > 0)
                {
                    Character npc = extras[Random.Range(0, extras.Count)];
                    newNode.name = npc.name;
                    npc.role = 3; // Mark as witness/informant
                }
                else
                {
                    newNode.name = "Mysterious Stranger";
                }
            }
            else if (newNode is EvidenceNode)
            {
                newNode.name = "Evidence";
            }

            // Chain the nodes: first node has no prerequisite, others require the previous node
            newNode.requirement = (i == 0) ? null : new Node[] { nodes[i - 1] };

            nodes.Add(newNode);
        }

        // Optionally, add a red herring node that depends on the first node.
        InfoNode redHerring = new InfoNode("I heard a rumor about a hidden stash, but it seems like nothing more than gossip.", "What about the stash?");
        redHerring.id = numNodes;
        redHerring.reached = false;
        redHerring.requirement = new Node[] { nodes[0] };
        redHerring.name = "Gossiping Passenger";
        nodes.Add(redHerring);

        return nodes;
    }

    /// <summary>
    /// The master function that ties together all of the steps to generate a full mystery.
    /// </summary>
    public void GenerateMystery()
    {
        // Step 0: Populate lists
        mysteryTemplates = GenerateTemplates();
        infoNodes = GenerateInfoNodes();
        evidenceNodes = GenerateEvidenceNodes();

        // Step 1: Select a mystery template at random.
        MysteryTemplate chosenTemplate = mysteryTemplates[Random.Range(0, mysteryTemplates.Count)];
        Debug.Log("Mystery Template Chosen: " + chosenTemplate.crime);

        // Step 2: Generate a cast of characters (10?).
        List<Character> characters = GenerateCharacters(10);
        Debug.Log("Generated " + characters.Count + " characters.");

        // Step 3: Pick a culprit based on the characters' temperament.
        int culpritIndex = PickCulprit(chosenTemplate, characters);
        Debug.Log("Culprit is: " + characters[culpritIndex].name + ", the (" + characters[culpritIndex].archetype + ")");

        // Step 4: Generate the crime details.
        Crime crime = GenerateCrime(chosenTemplate, characters, culpritIndex);
        Debug.Log("Crime Details: " + chosenTemplate.crime + " committed with a " + crime.tool +
                  " in train car " + crime.trainCarID + " at hour " + crime.time +
                  ". Victim: " + crime.victimName);

        // Step 5: Generate the mystery constellation (node graph of clues).
        List<Node> constellation = GenerateConstellation(chosenTemplate, characters);
        Debug.Log("Mystery Constellation generated with " + constellation.Count + " nodes.");

        //Step 5.5: Actually generate the visual representation of the constellation
        GameObject.FindWithTag("MysteryCanvas").GetComponent<NodeControl>().CreateConstellation(constellation);

        // Step 6: (Additional detail-filling) For example, output where each character is located.
        // This is where character alibis & location should be determined, as well as who reported the crime.
        foreach (Character c in characters)
        {
            Debug.Log("Character " + c.name + " (" + c.archetype + ") is in train car " + c.trainCarID);
        }

        foreach (Node node in constellation)
        {
            if (node is InfoNode)
            {
                InfoNode inNode = node as InfoNode;
                Debug.Log("InfoNode " + inNode.id + " by " + inNode.name + ": " + inNode.info);
            }
            else if (node is EvidenceNode)
            {
                EvidenceNode evNode = node as EvidenceNode;
                Debug.Log("EvidenceNode " + evNode.id + " found in train car " + evNode.location);
            }
        }
    }
}