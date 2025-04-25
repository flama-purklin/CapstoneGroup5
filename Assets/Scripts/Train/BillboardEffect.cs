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
    void Update()
    {
        //transform.LookAt(camTransform);
        //360f addition is necessary so they don't face the opposite direction
        //transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y % 360f, 0);
        Quaternion temp = Quaternion.LookRotation(transform.position - camTransform.position);

        transform.rotation = Quaternion.Euler(0, temp.eulerAngles.y, 0);
    }
}
