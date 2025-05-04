using System.Collections.Generic;
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
    private readonly Dictionary<string, string> bodiesHairBackMap = new Dictionary<string, string>
    {
        { "Blonde", "" },
        { "Brown", "" },
        { "Black", "" },
        { "Red", "" },
    };
    //... This is gonna be dificult. Would be easier to just rename the files, expecialy since they are in their own folders
    // Actually, just taking advantage of directories would be better. have each color parted out into outfit/haircolor folders and put the textures in that.
    // Then grabbing the textures would be as simple as putting the keys in as the path.
    // ***************************************************** //


    // Global Vars
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
        //animContainer.profile =
        //animContainer.idleFront =
        //...

        // Assign container to animation manager. Could also sanity check that the animContainer's fields not empty, but prob not needed.
        NPCAnimManager animManager = npcInstance.GetComponentInChildren<NPCAnimManager>();
        if (animManager != null)
        {
            animManager.SetAnimContainer(animContainer);
        }
    }


    // Incomplete helper method to get texture using path and key
    public Texture2D LoadPart(string category, string key)
    {
        return Resources.Load<Texture2D>($"Sprites/NPCParts/{category}/{key}");
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


    // Temp helper. Cant use this as is, but it may be useful.
    // Args may have to be modified
    public Sprite BuildProfile(Texture2D[] layers)
    {
        // Composite each layer into a new Texture2D 

        // Then use Sprite.Create
        //Sprite temp;
        //temp.Create(...);

        return null;
    }

}
