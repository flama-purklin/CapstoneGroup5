using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class BillboardEffect : MonoBehaviour
{
    Transform camTransform;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        camTransform = Camera.main.transform;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        // Refresh reference if missing or disabled:
        if (camTransform == null || !camTransform.gameObject.activeInHierarchy)
        {
            var main = Camera.main;
            if (main != null) camTransform = main.transform;
            else return; // still no camera—do nothing
        }

        Vector3 dir = transform.position - camTransform.position;
        dir.y = 0f;
        transform.rotation = Quaternion.LookRotation(dir);
    }
}
