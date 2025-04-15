using UnityEngine;

public class MinigameObj : MonoBehaviour, IInteractable
{
    [SerializeField] float interactionRadius;
    [SerializeField] GameObject indicator;
    [SerializeField] AudioControl audioControl;

    bool inRange;

    GameObject player;

    //set an associated node Key here
    public string nodeKey;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected virtual void Awake()
    {
        player = GameObject.FindWithTag("Player");
        InteractableManager.allInteractables.Add(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        inRange = Vector3.Distance(player.transform.position, transform.position) < interactionRadius;

        indicator.SetActive(inRange);

        if (GameControl.GameController.currentState == GameState.DEFAULT && 
            inRange && Input.GetKeyDown(KeyCode.E) &&
            player.GetComponent<PlayerMovement>().closestInteractable == gameObject)
        {
            if (audioControl != null) { audioControl.PlaySFX_Enter(); }
            Interact();
        }
    }

    public virtual void Interact()
    {
        MysteryNode unlockedNode = null;
        if (!string.IsNullOrEmpty(nodeKey))
            unlockedNode = GameControl.GameController.coreConstellation.DiscoverNode(nodeKey);

        if (unlockedNode != null)
        {
            //spawn node unlock popup here to indicate to the player a new node has opened on the mystery board
        }
        // Debug.Log("Interact Called on " + name);
    }
}
