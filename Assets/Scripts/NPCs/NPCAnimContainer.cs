using UnityEngine;

[CreateAssetMenu(fileName = "npcAnims", menuName = "ScriptableObjects/NPCAnimContainer", order = 1)]
public class NPCAnimContainer : ScriptableObject
{
    public Sprite profile;
    public Sprite[] walkFront;
    public Sprite[] walkBack;
    public Sprite[] idleFront;
    public Sprite[] idleBack;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
