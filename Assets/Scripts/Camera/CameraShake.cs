using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public float shakeAmt = 0.1f;
    public float shakeSpeed = 1.0f;

    private Vector3 startPos;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startPos = transform.localPosition;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //generate perlin motion
        float xMove = Mathf.PerlinNoise(Time.time * shakeSpeed, 0);
        float yMove = Mathf.PerlinNoise(0, Time.time * shakeSpeed);

        //create vector to apply
        Vector3 shakeOffset = new Vector3(xMove, yMove, 0) * shakeAmt;
        //Debug.Log(shakeOffset);
        transform.localPosition = startPos + shakeOffset;
    }
}
