using Unity.Burst.CompilerServices;
using UnityEngine;

public class CarDetection : MonoBehaviour
{
    [SerializeField] Rigidbody rb;
    [SerializeField] LayerMask trainLayer;
    private CarVisibility currentCar;
    private CarVisibility previousCar;
    private PlayerMovement playerMovement;

    private CameraControl cameraControl; // NEW
    [SerializeField] private float raycastDistance = 2f; // NEW
    [SerializeField] private Vector3 raycastOffset = Vector3.up * 0.1f; // NEW

    private void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        if (!playerMovement)
        {
            Debug.LogError("PlayerMovement component not found on Player object!");
        }

        cameraControl = Camera.main?.GetComponent<CameraControl>();
        if (!cameraControl)
        {
            Debug.LogError("CameraControl component not found on Main Camera!");
        }

        // DetectCar();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Car"))
        {
            CarVisibility newCar = other.GetComponent<CarVisibility>();
            if (newCar != null && newCar != currentCar)
            {
                // --- DIAGNOSTIC LOGGING START ---
                
                // --- DIAGNOSTIC LOGGING END ---
                UpdateCurrentCar(newCar);
            }
            else if (newCar != null && newCar == currentCar)
            {
                 // --- DIAGNOSTIC LOGGING START ---
                 
                 // --- DIAGNOSTIC LOGGING END ---
            }
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        DetectCar();
    }

    //shoots a raycast straight down to figure out what car the player is in
    private void DetectCar()
    {
        RaycastHit hit;
        Debug.DrawRay(transform.position + raycastOffset, -Vector3.up * raycastDistance, Color.red);

        if (Physics.Raycast(transform.position + raycastOffset, -Vector3.up,
            out hit, raycastDistance, trainLayer))
        {
            GameObject rootCar = hit.transform.parent.gameObject;
            CarVisibility newCar = rootCar.GetComponent<CarVisibility>();

            if (newCar && newCar != currentCar)
            {
                // --- DIAGNOSTIC LOGGING START ---
                // Debug.Log($"[CarDetection DetectCar @ {Time.frameCount}] Raycast detected NEW car below player: {newCar.gameObject.name}");
                // --- DIAGNOSTIC LOGGING END ---
                SwitchToCar(newCar);
            }
            // Optional: Log if raycast hits but it's the same car (can be noisy)
            // else if (newCar && newCar == currentCar)
            // {
            //     Debug.Log($"[CarDetection DetectCar @ {Time.frameCount}] Raycast detected SAME car below player: {newCar.gameObject.name}");
            // }
        }
        // Optional: Log if raycast misses (can be noisy)
        // else
        // {
        //     Debug.Log($"[CarDetection DetectCar @ {Time.frameCount}] Raycast missed train layer.");
        // }
    }

    private void SwitchToCar(CarVisibility newCar)
    {
        if (currentCar)
        {
            currentCar.CarDeselected();
        }

        currentCar = newCar;
        currentCar.CarSelected();

        if (cameraControl)
        {
            cameraControl.CarUpdate(currentCar.gameObject, currentCar == null);
        }
    }

    //update the car visibility of the old and new currentcar
    private void UpdateCurrentCar(CarVisibility newCarVisibility)
    {
        if (currentCar != null)
        {
            currentCar.CarDeselected();
        }

        currentCar = newCarVisibility;
        currentCar.CarSelected();

        if (cameraControl)
        {
            cameraControl.CarUpdate(currentCar.gameObject, false);
        }
    }

    public CarVisibility GetCurrentCar()
    {
        return currentCar;
    }


}
