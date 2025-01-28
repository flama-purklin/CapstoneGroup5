using Unity.Burst.CompilerServices;
using UnityEngine;

public class CarDetection : MonoBehaviour
{
    [SerializeField] Rigidbody rb;
    [SerializeField] LayerMask trainLayer;

    public GameObject currentCar;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Camera.main.GetComponent<CameraControl>().CarUpdate(currentCar);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        DetectCar();
    }

    //shoots a raycast straight down to figure out what car the player is in
    public void DetectCar()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up * .1f, -Vector3.up,  out hit, 2f, trainLayer))
        {
            Debug.Log(hit.transform.root.gameObject);
            if (currentCar != hit.transform.root.gameObject)
            {
                NewCar(hit.transform.root.gameObject);
            }
        }
        else
        {
            Debug.Log("Nothing Hit");
        }
    }

    //update the car visibility of the old and new currentcar
    private void NewCar(GameObject newCar)
    {
        currentCar.GetComponent<CarVisibility>().CarDeselected();
        currentCar = newCar;
        currentCar.GetComponent<CarVisibility>().CarSelected();

        //update the camera controls
        Camera.main.GetComponent<CameraControl>().CarUpdate(currentCar);
    }
}
