using UnityEngine;

public class BillboardEvidence : MonoBehaviour
{
    public Transform targetCamera; // usually the main camera

    void LateUpdate()
    {
        if (targetCamera == null)
            targetCamera = Camera.main.transform;

        // Make the sprite face the camera
        transform.LookAt(transform.position + targetCamera.forward);
    }
}
