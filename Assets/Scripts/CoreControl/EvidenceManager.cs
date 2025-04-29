using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public class EvidenceManager : MonoBehaviour
{
    [System.Serializable]
    public class ParsedEvidenceNode
    {
        public string Key;
        public MysteryNode Node;
    }

    // Managers to get and interact w/ gamestate
    [SerializeField] NPCManager npcManager; // Use bool "npcManager.SpawningComplete" to see if we should spawn evidence
    [SerializeField] TrainLayoutManager trainLayoutManager; // For hasBuiltTrain bool.
    [SerializeField] TrainManager trainManager; // To spanw the prefabbed objects
    [SerializeField] AudioControl audioControl;

    // For populating evidence object fields
    [SerializeField] GameObject evidencePrefab;
    [SerializeField] GameObject luggagePrefab;
    [SerializeField] GameObject deathPrefab;
    // Node key gets set by data

    // For overrides
    [SerializeField] float interactionRadius;
    [SerializeField] GameObject indicator; // Prefab should spawn its own.
    [SerializeField] Sprite evidenceArt;

    [SerializeField] float heightOffset = 0.25f; // Spawning offset for prefabs, could change to a field in the json
    [SerializeField] public bool enableDebug = false;
    [SerializeField] bool initialized = false;

    [SerializeField] List<TrainManager.TrainCar> trainCars;
    [SerializeField] List<ParsedEvidenceNode> parsedNodes;
    [SerializeField] List<EvidenceData> evidenceData;
    [SerializeField] List<GameObject> spawnedEvidence;
    [SerializeField] List<string> nodesToDeactivate;

    // TODO: Add object to hold scripted events here


    // Just call the coroutine to initialize
    void Start()
    {
        //parsedNodes = GameControl.GameController.coreConstellation.Nodes.Values.ToList();
        // Each node id will start with fact or evidence. Each type will be EVIDENCE, subtypes will be physical (spawn on start), barrier (luggage minigame), death (body prefab with reference to character. need to ensure npc gone b4 spawning)
        StartCoroutine(WaitThenLoadEvidence());
    }

    // Once NPCs spawn (mystery parsed and npcs availible) Get Evidence Nodes.
    private IEnumerator WaitThenLoadEvidence()
    {
        // Wait until NPCs are done spawning
        yield return new WaitUntil(() => npcManager.SpawningComplete);

        // Load and filter evidence nodes (uses dictionary like scruct), set runtime references and flags
        parsedNodes = GameControl.GameController.coreConstellation.Nodes
            .Where(kv => kv.Value.Type == "EVIDENCE")
            .Select(kv => new ParsedEvidenceNode
            {
                Key = kv.Key,
                Node = kv.Value
            }).ToList();

        // TODO: Populate list with scripted events here.

        initialized = true;

        // Print what has been collected for debugging purposes
        if (enableDebug) 
        {
            Debug.Log($"[Evidence Manager] Loaded {parsedNodes.Count} EVIDENCE nodes from GameControl.");

            foreach (var node in parsedNodes)
            {
                Debug.Log($"[Evidence Manager] Node: {node.Node.Title}, Type: {node.Node.Type}, Subtype: {node.Node.Subtype}, Discovered: {node.Node.Discovered}");
            }
        }

        // Spawn the evidence once train is built
        StartCoroutine(InitializeEvidence());
    }

    // Create the evidence objects, put them in the train, and update references in managers
    private IEnumerator InitializeEvidence()
    {
        // Wait until the train layout is fully built
        yield return new WaitUntil(() => trainLayoutManager.hasBuiltTrain);

        if (enableDebug)
        {
            Debug.Log($"[EvidenceManager] In InitializeEvidence'.");
        }

        trainCars = trainManager.trainCarList;

        // Loop through list of collected evidence nodes
        foreach (var node in parsedNodes)
        {
            // Get car index, if cant find the car in the list of train cars with the carNumber from the json, skip
            int carIndex = trainCars.FindIndex(car => car.carNumber == node.Node.CarNumber.Value);
            if (carIndex == -1)
            {
                Debug.LogWarning($"[EvidenceManager] No train car found for carNumber {node.Node.CarNumber.Value}. Skipping node {node.Key}.");
                continue;
            }


            if (enableDebug)
            {
                Debug.Log($"[EvidenceManager] Passed car exists check. Car with carNumber == {node.Node.CarNumber.Value} found in trainCars. Car index = {carIndex}");
            }

            // Try to find anchor by proviced coordinates, else pick randomly from empty anchors (prefering walls)
            string anchorName = $"Anchor ({node.Node.Coords?[0]}, {node.Node.Coords?[1]})";
            GameObject anchorObject = trainCars[carIndex].emptyAnchors.FirstOrDefault(a => a.name == anchorName);
            Transform anchor = null;

            // Realy nieche fallback that tries to spawn evidence at walls if anchor not specified/is occupied
            if (anchorObject != null)
            {
                anchor = anchorObject.transform;
            }
            else
            {
                var anchors = trainCars[carIndex].emptyAnchors;
                // Determine grid size for identifying wall anchors (should use reference to train scripts... but some charles removed it from sceene. Oh well)
                int maxX = 5;
                int maxY = 17;

                var wallAnchors = new List<GameObject>();
                var otherAnchors = new List<GameObject>();

                foreach (var a in anchors)
                {
                    var match = Regex.Match(a.name, @"\((\d+),\s*(\d+)\)");
                    if (!match.Success) continue;

                    int x = int.Parse(match.Groups[1].Value);
                    int y = int.Parse(match.Groups[2].Value);

                    // Prioritize wall anchors not blocking door
                    if (x == 0 || x == maxX || (y == 0 && x != 2 && x != 3) || (y == maxY && x != 2 && x != 3))
                    {
                        wallAnchors.Add(a);
                    }
                    else
                    {
                        otherAnchors.Add(a);
                    }
                }

                // Prioritize wall anchors, fallback to others if none
                if (wallAnchors.Count > 0)
                {
                    anchor = wallAnchors[Random.Range(0, wallAnchors.Count)].transform;
                }
                else if (otherAnchors.Count > 0)
                {
                    anchor = otherAnchors[Random.Range(0, otherAnchors.Count)].transform;
                }
                else
                {
                    Debug.LogWarning("[EvidenceManager] No anchors available to fall back to!");
                }
            }

            if (enableDebug)
            {
                Debug.Log($"[EvidenceManager] Anchor selected. {anchor.gameObject.name} in car ");
            }

            GameObject objToSpawn = null;

            // Spawn correct object
            switch (node.Node.Subtype)
            {
                case "physical":
                    if (evidencePrefab != null)
                    {
                        objToSpawn = evidencePrefab;
                    }
                    break;

                case "barrier":
                    if (luggagePrefab != null)
                    {
                        objToSpawn = luggagePrefab;
                    }
                    break;

                case "death":
                    if (deathPrefab != null)
                    {
                        objToSpawn = deathPrefab;
                    }
                    break;

                default:
                    Debug.LogWarning($"Unknown evidence subtype: {node.Node.Subtype}. Assuming physical...");
                    if (evidencePrefab != null)
                    {
                        objToSpawn = evidencePrefab;
                    }
                    continue;
            }

            if (objToSpawn != null)
            {
                GameObject instance = Instantiate(objToSpawn, anchor.position + Vector3.up * heightOffset, anchor.rotation, anchor);
                instance.name = node.Key + " Evidence";
                trainCars[carIndex].emptyAnchors.Remove(anchor.gameObject);
                trainCars[carIndex].evidenceInCar.Add(instance);
                spawnedEvidence.Add(instance);

                // TODO: Check instance name against keys of scripted events. if there add to list of objects to deactivate

                if (enableDebug)
                {
                    Debug.Log($"[EvidenceManager] Object spawned '{node.Node.Subtype}' evidence '{node.Node.Title}' at car #{node.Node.CarNumber}, anchor '{anchor.name}'.");
                }

                if (node.Node.Subtype == "death")
                {
                    // TODO: logic to check if npc is in car. Probably should be in clean up step after spawn loop.
                }

                // Set up EvidenceObjcet. Assign EvidenceData to the prefab if applicable (like in 'physical' subtype)
                EvidenceObj evidenceObj = instance.GetComponent<EvidenceObj>();
                if (evidenceObj != null)
                {
                    EvidenceData data = ScriptableObject.CreateInstance<EvidenceData>();
                    data.name = node.Key;
                    data.nodeKey = node.Key;
                    data.evidenceTitle = node.Node.Title;
                    data.evidenceDescription = node.Node.Description;
                    data.evidenceArt = evidenceArt;
                    evidenceObj.data = data;
                    evidenceObj.nodeKey = node.Key;
                    evidenceData.Add(data);
                }

                // Set up LuggageObject. Assign key, solution, and add contents to list to deactivate on cleanup.
                LuggageObj luggageObj = instance.GetComponent<LuggageObj>();
                if (luggageObj != null)
                { 
                    luggageObj.nodeKey = node.Key;
                    if (node.Node.Solution != null)
                    {
                        luggageObj.combination = node.Node.Solution;
                    }
                    if (node.Node.Contains != null)
                    {
                        foreach (string evidenceName in node.Node.Contains)
                        {
                            if (!nodesToDeactivate.Contains(evidenceName + " Evidence"))
                            {
                                nodesToDeactivate.Add(evidenceName + " Evidence");
                            }
                        }
                    }
                }

                if (enableDebug)
                {
                    Debug.Log($"[EvidenceManager] Evidence populated for '{node.Node.Subtype}' evidence '{node.Node.Title}' at car #{node.Node.CarNumber}, anchor '{anchor.name}'.");
                }
            }
        }

        // *** Hard Code! Disable Maxwell body node untill scripted events work
        nodesToDeactivate.Add("fact-maxwell-body Evidence");

        // TODO: Cleanup logic. Deactivate objects that are in nodesToDeactivate. Set up the scripted event checkers.
        foreach (string evidenceName in nodesToDeactivate)
        {
            var match = spawnedEvidence.FirstOrDefault(obj => obj != null && obj.name == evidenceName);
            if (match != null)
            {
                match.SetActive(false);
            }
        }

        initialized = true;
    }
}
