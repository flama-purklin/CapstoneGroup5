using UnityEngine;

public class Connection : MonoBehaviour
{
    public GameObject startObj;
    public GameObject endObj;

    RectTransform rect;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        ConnectionSpawn(startObj, endObj);
    }

    //associated node objects are passed from NodeControl
    public void ConnectionSpawn(GameObject origin, GameObject result)
    {
        startObj = origin;
        endObj = result;

        //assign the length to the connection obj
        rect = GetComponent<RectTransform>();
        Vector2 hypotenuse = endObj.transform.localPosition - startObj.transform.localPosition;
        float dist = hypotenuse.magnitude;
        rect.sizeDelta = new Vector2(dist, 20f);

        //assign the position
        transform.localPosition = Vector2.Lerp(origin.transform.localPosition, result.transform.localPosition, 0.5f);

        //calculate and apply the rotation
        float x = hypotenuse.x;
        float y = hypotenuse.y;
        float theta = 2 * Mathf.PI + Mathf.Atan2(y, -x);
        //Debug.Log(x + " " + y + " " + theta);

        Vector2 direction = new Vector2(Mathf.Sin(theta), Mathf.Cos(theta));

        transform.localRotation = Quaternion.LookRotation(Vector3.forward, direction);

        //send to back
        transform.SetSiblingIndex(0);

        //Debug.Log("new connection successfully created");
    }
}
