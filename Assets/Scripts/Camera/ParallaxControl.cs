using UnityEngine;

public class ParallaxControl : MonoBehaviour
{
    private float length, startPos;
    GameObject cam;
    public float parallaxEffect;
    public float speedMod;

    float timer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startPos = transform.localPosition.x;
        length = GetComponent<SpriteRenderer>().bounds.size.x;
        //Debug.Log(length);
        cam = Camera.main.gameObject;
        timer = 0f;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float temp = (timer * (1 - parallaxEffect) * speedMod);
        float dist = (timer * parallaxEffect * speedMod);

        transform.localPosition = new Vector3(startPos + dist, transform.localPosition.y, transform.localPosition.z);

        timer += Time.fixedDeltaTime;

        //Debug.Log(dist + " " + (startPos + length));

        if (dist > startPos + length)
        {
            //Debug.Log("moving forward");
            //startPos -= length;
            dist -= (startPos + length);
            timer = dist / (parallaxEffect * speedMod);
        }
         
    }
}
