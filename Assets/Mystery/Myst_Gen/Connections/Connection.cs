using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Connection : MonoBehaviour
{
    [Header("UI Attributes")]
    [SerializeField] Image visualConn;
    [SerializeField] TMP_Text connectionDesc;
    [SerializeField] RectTransform rect;

    [Header("Object Refs")]
    public GameObject startObj;
    public GameObject endObj;

    //MysteryConnection ref
    MysteryConnection mystConn;

    [Header("Data")]
    public string type;
    public bool discovered = false;

    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Debug.Log("Updating");
        if (discovered)
            StartCoroutine(ConnectionUpdate());
    }

    void FixedUpdate()
    {
        //Debug.Log("FixedUpdating");
        
    }

    //associated node objects are passed from NodeControl
    public void ConnectionSpawn(GameObject origin, GameObject result, MysteryConnection connection)
    {
        startObj = origin;
        endObj = result;
        mystConn = connection;
        type = mystConn.Type;

        //assign the length to the connection obj
        //rect = GetComponent<RectTransform>();
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

        connectionDesc.text = type;

        //Debug.Log("new connection successfully created");
        DiscoveryCheck();
    }

    IEnumerator ConnectionUpdate()
    {
        //assign the length to the connection obj
        //rect = GetComponent<RectTransform>();
        Vector2 hypotenuse = endObj.transform.localPosition - startObj.transform.localPosition;
        float dist = hypotenuse.magnitude;
        rect.sizeDelta = new Vector2(dist, 20f);

        //assign the position
        transform.localPosition = Vector2.Lerp(startObj.transform.localPosition, endObj.transform.localPosition, 0.5f);

        //calculate and apply the rotation
        float x = hypotenuse.x;
        float y = hypotenuse.y;
        float theta = 2 * Mathf.PI + Mathf.Atan2(y, -x);
        //Debug.Log(x + " " + y + " " + theta);

        Vector2 direction = new Vector2(Mathf.Sin(theta), Mathf.Cos(theta));

        transform.localRotation = Quaternion.LookRotation(Vector3.forward, direction);

        //Debug.Log("Updated connection");
        yield return null;
    }

    public void DiscoveryCheck()
    {
        if (GameControl.GameController.coreConstellation.Nodes[mystConn.Source].Discovered
            && GameControl.GameController.coreConstellation.Nodes[mystConn.Target].Discovered)
        {
            visualConn.enabled = true;
            connectionDesc.enabled = true;
            discovered = true;
        }
        else
        {
            visualConn.enabled = false;
            connectionDesc.enabled = false;
            discovered = false;
        }
    }
}
