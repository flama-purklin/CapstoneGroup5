using System.Collections;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class CameraControl : MonoBehaviour
{
    float startingY;
    float startingZ;

    GameObject player;
    //CarDetection carDetection;
    private Filmic60sSetup filmicSetup; // Reference to the film effect script for DoF control

    float carCenter;
    float carBoundMin;
    float carBoundMax;

    [SerializeField] private float carBufferAmt = 2f;
    [SerializeField] private float transitionTime = 0.5f;

    bool transition = false;
    private bool boundsInitialized = false;  // NEW: Track if bounds are set


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Start the coroutine to find (enables spawning player and camera at runtime)
        StartCoroutine(FindPlayerAsync());

/*
        player = GameObject.FindWithTag("Player");
        //carDetection = player.GetComponent<CarDetection>();
        if (!player){Debug.LogError("Player not found!");return;}

        //store the y and z values so that they are always constant
        startingY = transform.position.y;
        startingZ = transform.position.z;

        // NEW: find initial car if player is in it
        var carDetection = player.GetComponent<CarDetection>();
        if (carDetection && carDetection.GetCurrentCar())
        {
            CarUpdate(carDetection.GetCurrentCar().gameObject, true);
        }*/
    }

    // Coroutine to find player
    IEnumerator FindPlayerAsync()
    {
        // Continue checking until player found
        while (player == null)
        {
            player = GameObject.FindWithTag("Player");

            if (player != null)
            {
                break; // Exit when found
            }

            // Wait for one frame before retrying
            yield return null;
        }

        // Proceed with the rest of the logic once the player is found
        if (player != null)
        {
            //store the y and z values so that they are always constant
            startingY = transform.position.y;
            startingZ = transform.position.z;

            // Get the Filmic60sSetup component for DoF control
            filmicSetup = GetComponent<Filmic60sSetup>();

            // NEW: find initial car if player is in it
            var carDetection = player.GetComponent<CarDetection>();
            if (carDetection && carDetection.GetCurrentCar())
            {
                CarUpdate(carDetection.GetCurrentCar().gameObject, true);
            }
        }
        else
        {
            Debug.LogError("Player not found! The coroutine finished but the player still wasn't found.");
        }
    }

    void LateUpdate() {

        if (!player || transition || !boundsInitialized) return;

        //get the player x val
        Vector3 playerPos = player.transform.position;

        //clamp to the bounds of the current traincar
        float camX = Mathf.Clamp(playerPos.x, carBoundMin, carBoundMax);

        //apply to the cam
        transform.position = new Vector3(camX, startingY, startingZ);

    }

    //called whenever a new car is entered
    public void CarUpdate(GameObject newCar, bool initial)
    {

        if (!newCar) return;

        float prevCarEdge = transform.position.x;

        //retrieve the new bounds from the new car object
        carCenter = newCar.transform.position.x;

        // NEW: For car visibility handling
        Renderer carRenderer = newCar.GetComponentInChildren<Renderer>();
        if (!carRenderer)
        {
            Debug.LogError("No renderer found in car!");
            return;
        }

        //get the bounds from a renderer in the objects children
        //Vector3 size = newCar.GetComponentInChildren<Renderer>().bounds.size;
        
        //carBoundMin = carCenter - size.x / 2;
        //carBoundMax = carCenter + size.x / 2;

        //set the actual clamps with the buffer space amount
        Bounds bounds = carRenderer.bounds;
        carBoundMin = bounds.min.x + carBufferAmt;
        carBoundMax = bounds.max.x - carBufferAmt;

        boundsInitialized = true;

        //calculate whether new car is left or right of the old
        if (initial)
        {
            // if it's the initial car, just set the position directly
            float camX = Mathf.Clamp(player.transform.position.x, carBoundMin, carBoundMax);
            transform.position = new Vector3(camX, startingY, startingZ);
        }
        else
        {
            float targetX;
            if (prevCarEdge < carBoundMin)
                targetX = carBoundMin;
            else if (prevCarEdge > carBoundMax)
                targetX = carBoundMax;
            else
                targetX = prevCarEdge;

            // transition lol
            StartCoroutine(CarTransition(prevCarEdge, targetX));
        }
    }

    private IEnumerator CarTransition(float startX, float endX)
    {
        transition = true;

        float elapsed = 0;
        Vector3 startPos = new Vector3(startX, startingY, startingZ);
        Vector3 endPos = new Vector3(endX, startingY, startingZ);

        while (elapsed < transitionTime)
        {
            elapsed += Time.deltaTime;
            // Using SmoothStep for nicer easing
            float t = Mathf.SmoothStep(0.0f, 1.0f, elapsed / transitionTime);
            Vector3 newPos = Vector3.Lerp(startPos, endPos, t);
            transform.position = newPos;
            
            yield return null;
        }

        // Ensure final position is set precisely
        transform.position = endPos;
        transition = false;
    }
}
