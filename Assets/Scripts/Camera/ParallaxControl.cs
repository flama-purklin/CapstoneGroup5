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

        

        //resets the location once it gets too far from its origin
        if (dist > startPos + length)
        {
            
            //startPos -= length;

            //this offset should make the teleportation more seamless - might need to be messed with more
            dist -= (startPos + length);
            timer = dist / (parallaxEffect * speedMod);
        }
         
    }
}
