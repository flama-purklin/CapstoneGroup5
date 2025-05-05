using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class NPCSpriteCombiner : MonoBehaviour
{
    // To hold json data in a format that allows inspector visability
    [System.Serializable]
    public class Appearance
    {
        public string Base;
        public string SkinColor;
        public string Hair;
        public string Outfit;
        public string Shoes;
        public string Eyes;
        public string Nose;
        public string Mouth;
    }

    // Like a dictionary but visible in inspector
    [System.Serializable]
    public class NPCAppearanceDict
    { 
        // Can also store the instance, might be good idea but prob just unneccacary overhead
        public string NPCName;
        public Appearance Appearance;
    }

    // *** Dictionarys to hold Json values bs file names *** //
    private readonly Dictionary<string, string> eyesBodyMap = new Dictionary<string, string>
    {
        { "Blue", "EyesB" },
        { "Blue2", "EyesB2" },
        { "Green", "EyesG" },
        { "Green2", "EyesG2" },
        { "Brown", "EyesBr" },
        { "Brown2", "EyesBr2" }
    };
    private readonly Dictionary<string, string> eyesPortraitMap = new Dictionary<string, string>
    {
        { "Blue", "Eyes1B" },
        { "Blue2", "Eyes1B2" },
        { "Green", "Eyes1G" },
        { "Green2", "Eyes1G2" },
        { "Brown", "Eyes1Br" },
        { "Brown2", "Eyes1Br2" }
    };
    private readonly Dictionary<string, string> hairBackPortraitMap = new Dictionary<string, string>
    {
        { "Black", "Hair1Back" },
        { "Blonde", "Hair1Back 1" },
        { "Brown", "Hair1Back 2" },
        { "Red", "Hair1Back 3" }
    };
    // Using _ as a special symbol for replacement. Should be color {B, G, R, Y}
    private readonly Dictionary<string, string> hairFrontPortraitBackMap = new Dictionary<string, string>
    {
        { "Black", "Hair1Front_" },
        { "Blonde", "Hair1Front_ 1" },
        { "Brown", "Hair1Front_ 2" },
        { "Red", "Hair1Front_ 3" }
    };

    // Abstraction: colors // Append this to base file based on outfit color
    private readonly Dictionary<string, string> hairOutfitAppendMap = new Dictionary<string, string>
    {
        { "Blue", "B" },
        { "Yellow", "Y" },
        { "Green", "G" },
        { "Red", "R" }
    };

    // Abstraction: colors {black: "", blonde " 1", brown: " 2", red: " 3"} //Append thhis to base file based on hair color
    private readonly Dictionary<string, string> hairColorAppendMap = new Dictionary<string, string>
    {
        { "Black", "" },
        { "Blonde", " 1" },
        { "Brown", " 2" },
        { "Red", " 3" }
    };

    // Abstraction: colors // Append this to base file based on outfit color
    private readonly Dictionary<string, string> outfitColorAppendMap = new Dictionary<string, string>
    {
        { "Blue", "1" },
        { "Yellow", "2" },
        { "Green", "3" },
        { "Red", "4" }
    };

    // Abstraction: skin, added incase llm needs more description than skin1, skin2 // use this to decide what sub-folder to enter
    private readonly Dictionary<string, string> skinColorAppendMap = new Dictionary<string, string>
    {
        { "Skin1", "Skin1" },
        { "Skin2", "Skin2" },
        { "Skin3", "Skin3" }
    };

    // Abstraction: skin, added incase llm needs more description than skin1, skin2 // use this to decide what sub-folder to enter
    private readonly Dictionary<string, string> outfitColorFolderMap = new Dictionary<string, string>
    {
        { "Blue", "Blue" },
        { "Yellow", "Yellow" },
        { "Green", "Green" },
        { "Red", "Red" }
    };

    // THE DICTIONARY THAT STORES BASES. Using _ as a special symbol for replacement. ex: color {B, G, R, Y}. Then append based on directories.
    private readonly Dictionary<string, string> fileNameBasePortraitMap = new Dictionary<string, string>
    {
        { "HairBack", "Hair1Back" },
        { "HairFront", "Hair1Front" }, //Needs 2 appends.
        { "Shoulders", "Base" },
        { "Mouth", "Mouth" },
        { "Nose", "Nose" },
        { "Head", "HeadShape1"}
    };


    // Ignore below, the file naming is prety consistnat if not very descriptive
    //... This is gonna be dificult. Would be easier to just rename the files, expecialy since they are in their own folders
    // Actually, just taking advantage of directories would be better. have each color parted out into outfit/haircolor folders and put the textures in that.
    // Then grabbing the textures would be as simple as putting the keys in as the path.
    // ***************************************************** //


    // Global Vars
    [SerializeField] private bool debug = false;
    [SerializeField] private string assetRoot = "Sprites/PiecewiseNPCs/"; // For file paths from "Assets/Resources" *note: needs to be there for Resources.Load() to work.
    [SerializeField] private List<NPCAppearanceDict> npcAppearances = new List<NPCAppearanceDict>(); //show what npc's appearences have been parsed.


    // Called from NPCManager's spawn npc in car. Takes npc instance and parses JSON data.
    // Should call methods to grab char sprites from file system based on parsed appearence.
    // Also call methods to layer those sprits into whole sprites, and create an anim controler from it.
    public void ApplyAppearance(GameObject npcInstance, string characterName)
    {
        // Grab data and store in list for easy access. (Can convert to a dict once parsing confirmed)
        // Or also make a dict, but again prob unnecesary. Depends how many calls we need to make post Apply.
        Appearance npcAppearance = new Appearance();

        npcAppearance.Base = GameControl.GameController.coreMystery.Characters[characterName].Core.Appearance.Base.Trim();
        npcAppearance.SkinColor = GameControl.GameController.coreMystery.Characters[characterName].Core.Appearance.SkinColor.Trim();
        npcAppearance.Hair = GameControl.GameController.coreMystery.Characters[characterName].Core.Appearance.Hair.Trim();
        npcAppearance.Outfit = GameControl.GameController.coreMystery.Characters[characterName].Core.Appearance.Outfit.Trim();
        npcAppearance.Shoes = GameControl.GameController.coreMystery.Characters[characterName].Core.Appearance.Shoes.Trim();
        npcAppearance.Eyes = GameControl.GameController.coreMystery.Characters[characterName].Core.Appearance.Eyes.Trim();
        npcAppearance.Nose = GameControl.GameController.coreMystery.Characters[characterName].Core.Appearance.Nose.Trim();
        npcAppearance.Mouth = GameControl.GameController.coreMystery.Characters[characterName].Core.Appearance.Mouth.Trim();

        // Sanity check to ensure fields filled. If not, skip the rest and dont add entry to list.
        if (string.IsNullOrEmpty(npcAppearance.Base) || string.IsNullOrEmpty(npcAppearance.SkinColor) || string.IsNullOrEmpty(npcAppearance.Hair)
            || string.IsNullOrEmpty(npcAppearance.Outfit) || string.IsNullOrEmpty(npcAppearance.Shoes) || string.IsNullOrEmpty(npcAppearance.Eyes)
            || string.IsNullOrEmpty(npcAppearance.Nose) || string.IsNullOrEmpty(npcAppearance.Mouth))
        {
            Debug.LogWarning($"NPC Appearance Fields are empty for {characterName}, skipping sprite assembly...");
            return;
        }

        // Add parsed data to dictionary-like list for visual debugging and simple access.
        NPCAppearanceDict dictEntry = new NPCAppearanceDict();
        dictEntry.NPCName = characterName;
        dictEntry.Appearance = npcAppearance;
        npcAppearances.Add(dictEntry);

        // TODO: use the keys from the appearance object to find the sprites in the file paths.

        // TODO: Combine those sprites into single layer sprites.
        // Needed for Animator 4xWalkFront 4xWalkBack, 2xIdleFront, 2xIdleBack, 1xProfile

        // TODO: Create NPCAnimContainer object. Populate
        NPCAnimContainer animContainer = ScriptableObject.CreateInstance<NPCAnimContainer>();
        animContainer.name = $"Anim_{characterName}";

        Dictionary<string, Texture2D> profileLayers = GatherLayersProfile(npcAppearance);
        animContainer.profile = BuildProfileSprite(profileLayers);
        //animContainer.idleFront =
        //...





        // Assign container to animation manager. Could also sanity check that the animContainer's fields not empty, but prob not needed.
/*        NPCAnimManager animManager = npcInstance.GetComponentInChildren<NPCAnimManager>();
        if (animManager != null)
        {
            animManager.SetAnimContainer(animContainer);
        }*/
    }

    // This was a gpt method, we probably cant use this method as our layers are variable dependent.
    public Sprite[] BuildAnimationFrames(Texture2D[] layers, int numFrames)
    {
        Sprite[] animation = new Sprite[numFrames];
        // Composite each layer into a new Texture2D for each frame
        // (Assume consistent dimensions and alignment)

        // Then use Sprite.Create for each frame and return the array

        return animation;
    }


    // Build a single profile sprite from the appearance data
    public Dictionary<string, Texture2D> GatherLayersProfile(Appearance appearance)
    {
        // Folder path and append references
        string portraitFolder = "Portraits/";
        string skinFolder = skinColorAppendMap[appearance.SkinColor];
        string hairColorAppend = hairColorAppendMap[appearance.Hair];
        string colorFolder = outfitColorFolderMap[appearance.Outfit];

        // Setting File Paths
        string eyesPath = $"{assetRoot}{portraitFolder}Eyes/{eyesPortraitMap[appearance.Eyes]}";
        string mouthPath = $"{assetRoot}{portraitFolder}{skinFolder}/{fileNameBasePortraitMap["Mouth"]}{appearance.Mouth}";
        string nosePath = $"{assetRoot}{portraitFolder}{skinFolder}/{fileNameBasePortraitMap["Nose"]}{appearance.Nose}";
        string headPath = $"{assetRoot}{portraitFolder}{skinFolder}/{fileNameBasePortraitMap["Head"]}";
        string hairBackPath = $"{assetRoot}{portraitFolder}{skinFolder}/{fileNameBasePortraitMap["HairBack"]}{hairColorAppend}";
        string shouldersPath = $"{assetRoot}{portraitFolder}{skinFolder}/{outfitColorFolderMap[appearance.Outfit]}/{fileNameBasePortraitMap["Shoulders"]}{outfitColorAppendMap[appearance.Outfit]}";
        string hairFrontPath = $"{assetRoot}{portraitFolder}{skinFolder}/{outfitColorFolderMap[appearance.Outfit]}/{fileNameBasePortraitMap["HairFront"]}{hairOutfitAppendMap[appearance.Outfit]}{hairColorAppendMap[appearance.Hair]}";

        if (debug)
        {
            Debug.Log($"[NPCSpriteCombiner] Portrait: EyesPath set as {eyesPath}");
            Debug.Log($"[NPCSpriteCombiner] Portrait: MouthPath set as {mouthPath}");
            Debug.Log($"[NPCSpriteCombiner] Portrait: NosePath set as {nosePath}");
            Debug.Log($"[NPCSpriteCombiner] Portrait: HeadPath set as {headPath}");
            Debug.Log($"[NPCSpriteCombiner] Portrait: HairBackPath set as {hairBackPath}");
            Debug.Log($"[NPCSpriteCombiner] Portrait: SholdersPath set as {shouldersPath}");
            Debug.Log($"[NPCSpriteCombiner] Portrait: HairFrontPath set as {hairFrontPath}");
        }

        // Composite each layer into a new Texture2D 
        Texture2D hairBack = Resources.Load<Texture2D>(hairBackPath);
        Texture2D head = Resources.Load<Texture2D>(headPath);
        Texture2D shoulders = Resources.Load<Texture2D>(shouldersPath);
        Texture2D hairFront = Resources.Load<Texture2D>(hairFrontPath);
        Texture2D eyes = Resources.Load<Texture2D>(eyesPath);
        Texture2D nose = Resources.Load<Texture2D>(nosePath);
        Texture2D mouth = Resources.Load<Texture2D>(mouthPath);

        // Sanity check
        if (hairBack == null || head == null || shoulders == null || hairFront == null || eyes == null || nose == null || mouth == null)
        {
            Debug.LogWarning("[NPCSpriteCombiner] Missing one or more sprite layers.");
            return null;
        }
        else { if (debug) { Debug.LogWarning("[NPCSpriteCombiner] Portrait texture retrieval successful!"); } }

        // Make dict for easy lookup for extrenal programs
        Dictionary<string, Texture2D> layers = new Dictionary<string, Texture2D>
        {
            ["HairBack"] = hairBack,
            ["Shoulders"] = shoulders,
            ["Head"] = head,
            ["HairFront"] = hairFront,
            ["Eyes"] = eyes,
            ["Nose"] = nose,
            ["Mouth"] = mouth
        };

        return layers;
    }

    // Working Test method to smash profile the textures into a single sprite
    public Sprite BuildProfileSprite(Dictionary<string, Texture2D> layers)
    {
        if (layers == null || layers.Count == 0)
        {
            Debug.LogWarning("No layers provided to build composite sprite.");
            return null;
        }

        int width = 64;
        int height = 64;

        Texture2D finalTex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        Color32[] finalPixels = new Color32[width * height];

        // Start with transparent background
        for (int i = 0; i < finalPixels.Length; i++)
            finalPixels[i] = new Color32(0, 0, 0, 0);

        // Draw layers in the correct order
        string[] drawOrder = new string[] {
        "HairBack",
        "Shoulders",
        "Head",
        "HairFront",
        "Eyes",
        "Nose",
        "Mouth"
    };

        foreach (string key in drawOrder)
        {
            if (!layers.ContainsKey(key)) continue;

            Texture2D layer = layers[key];
            Color32[] layerPixels = layer.GetPixels32();

            for (int i = 0; i < finalPixels.Length; i++)
            {
                finalPixels[i] = AlphaBlend(finalPixels[i], layerPixels[i]);
            }
        }

        finalTex.SetPixels32(finalPixels);
        finalTex.Apply();

        // Create and return a sprite
        return Sprite.Create(finalTex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 64f);
    }
    // Helper for above test method
    private Color32 AlphaBlend(Color32 bg, Color32 fg)
    {
        float alpha = fg.a / 255f;
        float invAlpha = 1f - alpha;

        byte r = (byte)(fg.r * alpha + bg.r * invAlpha);
        byte g = (byte)(fg.g * alpha + bg.g * invAlpha);
        byte b = (byte)(fg.b * alpha + bg.b * invAlpha);
        byte a = (byte)(Mathf.Clamp01(alpha + bg.a / 255f) * 255);

        return new Color32(r, g, b, a);
    }

}
