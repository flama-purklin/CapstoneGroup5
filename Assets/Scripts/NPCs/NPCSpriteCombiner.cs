using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

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

    private readonly Dictionary<string, string> hairFrontPortraitBackMap = new Dictionary<string, string>
    {
        { "Black", "Hair1Front_" },
        { "Blonde", "Hair1Front_ 1" },
        { "Brown", "Hair1Front_ 2" },
        { "Red", "Hair1Front_ 3" }
    };

    private readonly Dictionary<string, string> hairOutfitAppendMap = new Dictionary<string, string>
    {
        { "Blue", "B" },
        { "Yellow", "Y" },
        { "Green", "G" },
        { "Red", "R" }
    };

    private readonly Dictionary<string, string> hairColorAppendMap = new Dictionary<string, string>
    {
        { "Black", "" },
        { "Blonde", " 1" },
        { "Brown", " 2" },
        { "Red", " 3" }
    };

    private readonly Dictionary<string, string> backHairColorAppendMap = new Dictionary<string, string>
    {
        { "Black", "" },
        { "Blonde", " 1" },
        { "Brown", " 3" },
        { "Red", " 2" }
    };

    private readonly Dictionary<string, string> idleAnimationAppendMap = new Dictionary<string, string>
    {
        { "Black", " 4" },
        { "Blonde", " 1" },
        { "Brown", " 1" },
        { "Red", " 1" }
    };

    private readonly Dictionary<string, string> outfitColorAppendMap = new Dictionary<string, string>
    {
        { "Blue", "1" },
        { "Yellow", "2" },
        { "Green", "3" },
        { "Red", "4" }
    };

    private readonly Dictionary<string, string> shoesColorNameMap = new Dictionary<string, string>
    {
        { "Blue", "Blue" },
        { "Brown", "Brown" },
        { "Green", "Green" },
        { "White", "White" }
    };

    private readonly Dictionary<string, string> shoesColorAppendMap = new Dictionary<string, string>
    {
        { "Blue", "Bl" },
        { "Brown", "Br" },
        { "Green", "G" },
        { "White", "W" }
    };

    private readonly Dictionary<string, string> skinColorAppendMap = new Dictionary<string, string>
    {
        { "Skin1", "Skin1" },
        { "Skin2", "Skin2" },
        { "Skin3", "Skin3" }
    };

    private readonly Dictionary<string, string> outfitColorFolderMap = new Dictionary<string, string>
    {
        { "Blue", "Blue" },
        { "Yellow", "Yellow" },
        { "Green", "Green" },
        { "Red", "Red" }
    };

    private readonly Dictionary<string, string> fileNameBasePortraitMap = new Dictionary<string, string>
    {
        { "HairBack", "Hair1Back" },
        { "HairFront", "Hair1Front" }, //Needs 2 appends.
        { "Shoulders", "Base" },
        { "Mouth", "Mouth" },
        { "Nose", "Nose" },
        { "Head", "HeadShape1"}
    };

    private readonly Dictionary<string, string> fileNameBaseBodyMap = new Dictionary<string, string>
    {
        { "HairBack", "Hair1Back" }, //Needs 2 appends.
        { "HairFront", "Hair1Front" }, //Needs 2 appends.
        { "Outfit", "Outfit" },
        { "Shoes", "Shoe1" }
    };
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

        // Use the keys from the appearance object to find the sprites in the file paths.
        // Combine those sprites into single layer sprites.
        // Needed for Animator 4xWalkFront 4xWalkBack, 2xIdleFront, 2xIdleBack, 1xProfile

        // Create NPCAnimContainer object. Populate
        NPCAnimContainer animContainer = ScriptableObject.CreateInstance<NPCAnimContainer>();
        animContainer.name = $"Anim_{characterName}";

        Dictionary<string, Texture2D> profileLayers = GatherLayersProfile(npcAppearance);
        animContainer.profile = BuildProfileSprite(profileLayers);

        Dictionary<string, Texture2D> walkFrontLayers = GatherWalkFrontLayers(npcAppearance);
        Dictionary<string, Texture2D> walkBackLayers = GatherWalkBackLayers(npcAppearance);
        Dictionary<string, Texture2D> idleFrontLayers = GatherIdleFrontLayers(npcAppearance);
        Dictionary<string, Texture2D> idleBackLayers = GatherIdleBackLayers(npcAppearance);

        animContainer.walkFront = CreateAnimation(walkFrontLayers, 4);
        animContainer.idleFront = CreateAnimation(idleFrontLayers, 2);
        animContainer.walkBack = CreateAnimation(walkBackLayers, 4);
        animContainer.idleBack = CreateAnimation(idleBackLayers, 2);

        // Sanity check ***May not work idk?
        if (animContainer.profile == null || animContainer.walkFront.Length == 0 || animContainer.idleFront.Length == 0 || animContainer.walkBack.Length == 0 || animContainer.idleBack.Length == 0)
        {
            Debug.LogError($"[NPCSpriteCombiner] Animation controler for '{characterName}' contains 1 or more null/empty fields. Assignment skipped...");
            return;
        }

        // Assign container to animation manager. Could also sanity check that the animContainer's fields not empty, but prob not needed.
        NPCAnimManager animManager = npcInstance.GetComponentInChildren<NPCAnimManager>();
        if (animManager != null)
        {
            if (debug) { Debug.Log($"[NPCSpriteCombiner] Animation Container Created for '{characterName}' successfully!"); }
            animManager.SetAnimContainer(animContainer);
        }
    }

    // 
    public Sprite[] CreateAnimation(Dictionary<string, Texture2D> layers, int numFrames)
    {
        if (layers == null || layers.Count == 0)
        {
            Debug.LogWarning("[NPCSpriteCombiner] CreateAnimation called with null or empty layers.");
            return null;
        }

        int width = 64;
        int height = 64;
        float scaleFactor = 5f;
        float pixelsPerUnit = width / (scaleFactor / 1.65f);
        Sprite[] animation = new Sprite[numFrames];

        // Determine draw order based on front or back
        string[] drawOrder;
        bool isFront = layers.ContainsKey("Eyes");

        if (isFront)
        {
            drawOrder = new string[] { "Outfit", "Shoes", "HairFront", "Eyes" };
        }
        else
        {
            drawOrder = new string[] { "Outfit", "Shoes", "HairBack" };
        }

        for (int frame = 0; frame < numFrames; frame++)
        {
            Color32[] finalPixels = new Color32[width * height];

            // Start with transparent background
            for (int i = 0; i < finalPixels.Length; i++)
                finalPixels[i] = new Color32(0, 0, 0, 0);

            foreach (string key in drawOrder)
            {
                if (!layers.ContainsKey(key)) continue;

                Texture2D tex = layers[key];
                if (tex == null) continue;

                // Get the pixels for the frame from the sprite sheet
                Color[] pixels = tex.GetPixels(frame * width, 0, width, height);
                Color32[] framePixels = new Color32[pixels.Length];
                for (int i = 0; i < pixels.Length; i++)
                    framePixels[i] = pixels[i];

                for (int i = 0; i < finalPixels.Length; i++)
                {
                    finalPixels[i] = AlphaBlend(finalPixels[i], framePixels[i]);
                }
            }

            // Create a new texture and apply the blended pixels
            Texture2D finalTex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            finalTex.SetPixels32(finalPixels);

            // Prevent blurring and unwanted tiling
            finalTex.filterMode = FilterMode.Point;
            finalTex.wrapMode = TextureWrapMode.Clamp;
            finalTex.Apply();

            // Create a sprite from the frame texture (Vector2 .5 0 is bottom center)
            animation[frame] = Sprite.Create(finalTex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.0f), pixelsPerUnit);
        }

        return animation;
    }

    // Build a single profile sprite from the appearance data
    public Dictionary<string, Texture2D> GatherWalkFrontLayers(Appearance appearance)
    {
        // Folder path and append references
        string bodyFolder = "Bodies/";
        string skinFolder = skinColorAppendMap[appearance.SkinColor];
        string hairColorAppend = hairColorAppendMap[appearance.Hair];
        string colorFolder = outfitColorFolderMap[appearance.Outfit];

        // Setting File Paths
        string eyesPath = $"{assetRoot}{bodyFolder}Eyes/Motion/{eyesBodyMap[appearance.Eyes]}";
        string shoePath = $"{assetRoot}{bodyFolder}Shoes/Motion/{shoesColorNameMap[appearance.Shoes]}/{fileNameBaseBodyMap["Shoes"]}{shoesColorAppendMap[appearance.Shoes]} 1";
        string hairFrontPath = $"{assetRoot}{bodyFolder}{skinFolder}/{outfitColorFolderMap[appearance.Outfit]}/{fileNameBaseBodyMap["HairFront"]}{hairOutfitAppendMap[appearance.Outfit]}{hairColorAppendMap[appearance.Hair]}";
        string outfitPath = $"{assetRoot}{bodyFolder}{skinFolder}/{outfitColorFolderMap[appearance.Outfit]}/{fileNameBaseBodyMap["Outfit"]}{outfitColorAppendMap[appearance.Outfit]}Front 1";

        if (debug)
        {
            Debug.Log($"[NPCSpriteCombiner] Bodies: WalkFront EyesPath set as {eyesPath}");
            Debug.Log($"[NPCSpriteCombiner] Bodies: WalkFront ShoePath set as {shoePath}");
            Debug.Log($"[NPCSpriteCombiner] Bodies: WalkFront OutfitPath set as {outfitPath}");
            Debug.Log($"[NPCSpriteCombiner] Bodies: WalkFront HairFrontPath set as {hairFrontPath}");
        }

        // Composite each layer into a new Texture2D 
        Texture2D outfit = Resources.Load<Texture2D>(outfitPath);
        Texture2D shoes = Resources.Load<Texture2D>(shoePath);
        Texture2D eyes = Resources.Load<Texture2D>(eyesPath);
        Texture2D hairFront = Resources.Load<Texture2D>(hairFrontPath);

        // Sanity check
        if (outfit == null || shoes == null || eyes == null || hairFront == null)
        {
            Debug.LogWarning("[NPCSpriteCombiner] WalkFront: Missing one or more sprite layers.");
            return null;
        }
        else { if (debug) { Debug.Log("[NPCSpriteCombiner] WalkFront texture retrieval successful!"); } }

        // Make dict for easy lookup for extrenal programs
        Dictionary<string, Texture2D> layers = new Dictionary<string, Texture2D>
        {
            ["Outfit"] = outfit,
            ["Shoes"] = shoes,
            ["Eyes"] = eyes,
            ["HairFront"] = hairFront
        };

        return layers;
    }
    public Dictionary<string, Texture2D> GatherWalkBackLayers(Appearance appearance)
    {
        // Folder path and append references
        string bodyFolder = "Bodies/";
        string skinFolder = skinColorAppendMap[appearance.SkinColor];
        string hairColorAppend = hairColorAppendMap[appearance.Hair];
        string colorFolder = outfitColorFolderMap[appearance.Outfit];

        //string eyesPath = $"{assetRoot}{bodyFolder}Eyes/Motion/{eyesBodyMap[appearance.Eyes]}";
        string shoePath = $"{assetRoot}{bodyFolder}Shoes/Motion/{shoesColorNameMap[appearance.Shoes]}/{fileNameBaseBodyMap["Shoes"]}{shoesColorAppendMap[appearance.Shoes]}";
        string hairBackPath = $"{assetRoot}{bodyFolder}HairBack/{outfitColorFolderMap[appearance.Outfit]}/{fileNameBaseBodyMap["HairBack"]}{hairOutfitAppendMap[appearance.Outfit]}{backHairColorAppendMap[appearance.Hair]}{idleAnimationAppendMap[appearance.Hair]}";
        string outfitPath = $"{assetRoot}{bodyFolder}{skinFolder}/{outfitColorFolderMap[appearance.Outfit]}/{fileNameBaseBodyMap["Outfit"]}{outfitColorAppendMap[appearance.Outfit]}Back 1";

        if (debug)
        {
            Debug.Log($"[NPCSpriteCombiner] Bodies: WalkBack ShoePath set as {shoePath}");
            Debug.Log($"[NPCSpriteCombiner] Bodies: WalkBack OutfitPath set as {outfitPath}");
            Debug.Log($"[NPCSpriteCombiner] Bodies: WalkBack HairFrontPath set as {hairBackPath}");
        }

        // Composite each layer into a new Texture2D 
        Texture2D outfit = Resources.Load<Texture2D>(outfitPath);
        Texture2D shoes = Resources.Load<Texture2D>(shoePath);
        Texture2D hairBack = Resources.Load<Texture2D>(hairBackPath);

        // Sanity check
        if (outfit == null || shoes == null || hairBack == null)
        {
            Debug.LogWarning("[NPCSpriteCombiner] WalkBack: Missing one or more sprite layers.");
            return null;
        }
        else { if (debug) { Debug.Log("[NPCSpriteCombiner] WalkBack texture retrieval successful!"); } }

        // Make dict for easy lookup for extrenal programs
        Dictionary<string, Texture2D> layers = new Dictionary<string, Texture2D>
        {
            ["Outfit"] = outfit,
            ["Shoes"] = shoes,
            ["HairBack"] = hairBack
        };

        return layers;
    }

    public Dictionary<string, Texture2D> GatherIdleFrontLayers(Appearance appearance)
    {
        // Folder path and append references
        string bodyFolder = "Bodies/";
        string skinFolder = skinColorAppendMap[appearance.SkinColor];
        string hairColorAppend = hairColorAppendMap[appearance.Hair];
        string colorFolder = outfitColorFolderMap[appearance.Outfit];

        string eyesPath = $"{assetRoot}{bodyFolder}Eyes/Idle/{eyesBodyMap[appearance.Eyes]}";
        string shoePath = $"{assetRoot}{bodyFolder}Shoes/Idle/{shoesColorNameMap[appearance.Shoes]}/{fileNameBaseBodyMap["Shoes"]}{shoesColorAppendMap[appearance.Shoes]} 1";
        string hairFrontPath = $"{assetRoot}{bodyFolder}{skinFolder}/{outfitColorFolderMap[appearance.Outfit]}/{fileNameBaseBodyMap["HairFront"]}{hairOutfitAppendMap[appearance.Outfit]}{hairColorAppendMap[appearance.Hair]}{idleAnimationAppendMap[appearance.Hair]}";
        string outfitPath = $"{assetRoot}{bodyFolder}{skinFolder}/{outfitColorFolderMap[appearance.Outfit]}/{fileNameBaseBodyMap["Outfit"]}{outfitColorAppendMap[appearance.Outfit]}Front";

        if (debug)
        {
            Debug.Log($"[NPCSpriteCombiner] Bodies: IdleFront EyesPath set as {eyesPath}");
            Debug.Log($"[NPCSpriteCombiner] Bodies: IdleFront ShoePath set as {shoePath}");
            Debug.Log($"[NPCSpriteCombiner] Bodies: IdleFront OutfitPath set as {outfitPath}");
            Debug.Log($"[NPCSpriteCombiner] Bodies: IdleFront HairFrontPath set as {hairFrontPath}");
        }

        // Composite each layer into a new Texture2D 
        Texture2D outfit = Resources.Load<Texture2D>(outfitPath);
        Texture2D shoes = Resources.Load<Texture2D>(shoePath);
        Texture2D eyes = Resources.Load<Texture2D>(eyesPath);
        Texture2D hairFront = Resources.Load<Texture2D>(hairFrontPath);

        // Sanity check
        if (outfit == null || shoes == null || eyes == null || hairFront == null)
        {
            Debug.LogWarning("[NPCSpriteCombiner] IdleFront: Missing one or more sprite layers.");
            return null;
        }
        else { if (debug) { Debug.Log("[NPCSpriteCombiner] IdleFront texture retrieval successful!"); } }

        // Make dict for easy lookup for extrenal programs
        Dictionary<string, Texture2D> layers = new Dictionary<string, Texture2D>
        {
            ["Outfit"] = outfit,
            ["Shoes"] = shoes,
            ["Eyes"] = eyes,
            ["HairFront"] = hairFront
        };

        return layers;
    }

    public Dictionary<string, Texture2D> GatherIdleBackLayers(Appearance appearance)
    {
        // Folder path and append references
        string bodyFolder = "Bodies/";
        string skinFolder = skinColorAppendMap[appearance.SkinColor];
        string hairColorAppend = hairColorAppendMap[appearance.Hair];
        string colorFolder = outfitColorFolderMap[appearance.Outfit];

        //string eyesPath = $"{assetRoot}{bodyFolder}Eyes/Idle/{eyesBodyMap[appearance.Eyes]}";
        string shoePath = $"{assetRoot}{bodyFolder}Shoes/Idle/{shoesColorNameMap[appearance.Shoes]}/{fileNameBaseBodyMap["Shoes"]}{shoesColorAppendMap[appearance.Shoes]}";
        string hairBackPath = $"{assetRoot}{bodyFolder}HairBack/{outfitColorFolderMap[appearance.Outfit]}/{fileNameBaseBodyMap["HairBack"]}{hairOutfitAppendMap[appearance.Outfit]}{backHairColorAppendMap[appearance.Hair]}"; // Inverted from front
        string outfitPath = $"{assetRoot}{bodyFolder}{skinFolder}/{outfitColorFolderMap[appearance.Outfit]}/{fileNameBaseBodyMap["Outfit"]}{outfitColorAppendMap[appearance.Outfit]}Back";


        if (debug)
        {
            Debug.Log($"[NPCSpriteCombiner] Bodies: IdleBack ShoePath set as {shoePath}");
            Debug.Log($"[NPCSpriteCombiner] Bodies: IdleBack OutfitPath set as {outfitPath}");
            Debug.Log($"[NPCSpriteCombiner] Bodies: IdleBack HairFrontPath set as {hairBackPath}");
        }

        // Composite each layer into a new Texture2D 
        Texture2D outfit = Resources.Load<Texture2D>(outfitPath);
        Texture2D shoes = Resources.Load<Texture2D>(shoePath);
        Texture2D hairBack = Resources.Load<Texture2D>(hairBackPath);

        // Sanity check
        if (outfit == null || shoes == null || hairBack == null)
        {
            Debug.LogWarning("[NPCSpriteCombiner] IdleBack: Missing one or more sprite layers.");
            return null;
        }
        else { if (debug) { Debug.Log("[NPCSpriteCombiner] IdleBack texture retrieval successful!"); } }

        // Make dict for easy lookup for extrenal programs
        Dictionary<string, Texture2D> layers = new Dictionary<string, Texture2D>
        {
            ["Outfit"] = outfit,
            ["Shoes"] = shoes,
            ["HairBack"] = hairBack
        };

        return layers;
    }

    // Get layers to build a single profile sprite from the appearance data
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
        Texture2D eyes = Resources.Load<Texture2D>(eyesPath);
        Texture2D nose = Resources.Load<Texture2D>(nosePath);
        Texture2D mouth = Resources.Load<Texture2D>(mouthPath);
        Texture2D hairFront = Resources.Load<Texture2D>(hairFrontPath);

        // Sanity check
        if (hairBack == null || head == null || shoulders == null || hairFront == null || eyes == null || nose == null || mouth == null)
        {
            Debug.LogWarning("[NPCSpriteCombiner] Portrait: Missing one or more sprite layers.");
            return null;
        }
        else { if (debug) { Debug.Log("[NPCSpriteCombiner] Portrait texture retrieval successful!"); } }

        // Make dict for easy lookup for extrenal programs
        Dictionary<string, Texture2D> layers = new Dictionary<string, Texture2D>
        {
            ["HairBack"] = hairBack,
            ["Shoulders"] = shoulders,
            ["Head"] = head,
            ["Nose"] = nose,
            ["Mouth"] = mouth,
            ["HairFront"] = hairFront,
            ["Eyes"] = eyes
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
        "Nose",
        "Mouth",
        "HairFront",
        "Eyes"
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
