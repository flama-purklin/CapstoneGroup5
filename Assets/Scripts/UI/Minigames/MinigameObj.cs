using UnityEngine;

public class MinigameObj : MonoBehaviour
{
    [SerializeField] float interactionRadius;
    [SerializeField] GameObject indicator;

    bool inRange;

    GameObject player;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected virtual void Awake()
    {
        player = GameObject.FindWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
        inRange = Vector3.Distance(player.transform.position, transform.position) < interactionRadius;

        indicator.SetActive(inRange);

        if (GameControl.GameController.currentState == GameState.DEFAULT && inRange && Input.GetKeyDown(KeyCode.E))
        {
            Interact();
        }
    }

    public virtual void Interact()
    {
        Debug.Log("Interact Called on " + name);
    }
}
