using System.Collections;
using System.Security.Cryptography;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    float startingY;
    float startingZ;

    GameObject player;
    //CarDetection carDetection;

    float carCenter;
    float carBoundMin;
    float carBoundMax;

    public float carBufferAmt;
    public float transitionTime;

    bool transition = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.FindWithTag("Player");
        //carDetection = player.GetComponent<CarDetection>();

        //store the y and z values so that they are always constant
        startingY = transform.position.y;
        startingZ = transform.position.z;
    }

    // Update is called once per frame
    void Update()
    {
        //get the player x val
        Vector3 playerPos = player.transform.position;
        float camX = playerPos.x;

        //clamp to the bounds of the current traincar
        camX = Mathf.Clamp(camX, carBoundMin, carBoundMax);


        //apply to the cam
        if (!transition)
            transform.position = new Vector3(camX, startingY, startingZ);
    }

    //called whenever a new car is entered
    public void CarUpdate(GameObject newCar, bool initial)
    {

        float prevCarEdge = transform.position.x;

        //retrieve the new bounds from the new car object
        carCenter = newCar.transform.position.x;

        //get the bounds from a renderer in the objects children
        Vector3 size = newCar.GetComponentInChildren<Renderer>().bounds.size;
        Debug.Log(size);
        carBoundMin = carCenter - size.x / 2;
        carBoundMax = carCenter + size.x / 2;

        //set the actual clamps with the buffer space amount
        carBoundMin += carBufferAmt;
        carBoundMax -= carBufferAmt;

        //calculate whether new car is left or right of the old
        float newCarEdge;
        if (prevCarEdge < carBoundMin)
            newCarEdge = carBoundMin;
        else
            newCarEdge = carBoundMax;

        //smooth transition from one clamp to the other on car enter
        if (!initial)
            StartCoroutine(CarTransition(prevCarEdge, newCarEdge));
    }

    IEnumerator CarTransition(float prevEdge, float newEdge)
    {
        transition = true;

        float currentTime = 0f;
        while (currentTime < transitionTime)
        {
            float camX = Mathf.Lerp(prevEdge, newEdge, currentTime / transitionTime);
            transform.position = new Vector3(camX, startingY, startingZ);
            currentTime += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        transform.position = new Vector3(newEdge, startingY, startingZ);
        transition = false;
    }
}
