using UnityEngine; // Keep UnityEngine in case future node types need it

/// <summary>
/// Base class for nodes in the mystery constellation.
/// </summary>
public class Node
{
    public int id;
    public bool reached;        // Has this node been reached?
    public Node[] requirement;  // Which nodes must be reached first
    public string name;         // The name of the NPC that gives the info, or the evidence belongs to
}

/// <summary>
/// Represents an information node obtained from an NPC.
/// </summary>
public class InfoNode : Node
{
    // Info given by an NPC
    public string info;     // The info they give
    public string prompt;   // The trigger word(s) required to say to get them to give the info

    public InfoNode() { reached = false; }

    public InfoNode(string i, string p)
    {
        reached = false;
        info = i;
        prompt = p;
    }
}

/// <summary>
/// Represents a physical evidence node found in the game world.
/// </summary>
public class EvidenceNode : Node
{
    // Physical Evidence
    public int objectID;    // Identifier for the evidence object prefab/data
    public int location;    // TrainCarID where the evidence is found

    public EvidenceNode() { reached = false; }

    public EvidenceNode(int o)
    {
        reached = false;
        objectID = o;
    }
}
