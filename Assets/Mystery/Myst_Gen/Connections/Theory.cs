using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using Unity.VisualScripting;

public class Theory : Connection, IPointerDownHandler
{

    NodeControl control;

    bool placing = true;
    Connection realConn;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        control = GameObject.FindFirstObjectByType<NodeControl>();
    }

    // Update is called once per frame
    public override void Update()
    {
        if (!placing)
        {
            StartCoroutine(ConnectionUpdate());
        }
    }

    public void OnPointerDown(PointerEventData pointEventData)
    {
        if (Input.GetMouseButtonDown(0) && control.theoryMode == TheoryMode.Removal)
        {
            control.TheoryRemoved(gameObject);
        }
    }

    public void KillYourself()
    {
        //remove from any references of existing theories, then kill yourself
        Debug.Log("Theory has been eliminated");
        Destroy(gameObject);
    }

    public void NodeAssign(VisualNode node)
    {
        //if this is the first click received, set the startObj
        if (leadObj == null)
        {
            placing = true;
            leadObj = node.gameObject;

            //start the temp connection
            StartCoroutine(TempConnection());
            Debug.Log("startObj chosen");
        }
        //if this is the second click received, set the endObj and fully place
        else
        {
            answerObj = node.gameObject;
            placing = false;
            transform.SetAsFirstSibling();

            //place the 
            control.TheoryPlaced();

            //check whether the theory has a real connection associated with it
            Debug.Log("endObj chosen");
            StartCoroutine(TheoryEvaluate());
            
        }
    }

    IEnumerator TheoryEvaluate()
    {
        List<GameObject> allConnects = leadObj.GetComponent<VisualNode>().connections;
        Debug.Log("Checking " + allConnects.Count + " connections");

        for (int i = 0; i < allConnects.Count; i++)
        {
            GameObject tempStart = allConnects[i].GetComponent<Connection>().leadObj;
            GameObject tempEnd = allConnects[i].GetComponent<Connection>().answerObj;

            if ((tempStart == leadObj || tempStart == answerObj) && (tempEnd == leadObj || tempEnd == answerObj))
                realConn = allConnects[i].GetComponent<Connection>();
        }

        Debug.Log("Real Connection exists: " + (realConn != null));

        yield return null;
    }

    public void Reveal()
    {
        if (realConn != null)
        {
            //correct animation
            Debug.Log("A real connection was found, theory correct!");
            animControl.SetTrigger("confirm");

            //do the things that a correct reveal should do here

        }
        else
        {
            //incorrect animation
            Debug.Log("This theory was a bust");
            animControl.SetTrigger("bust");

            //do the things that an incorrect reveal should do here
        }
    }

    public void ConfirmFinish()
    {
        realConn.confirmed = true;
        realConn.DiscoveryCheck();
        Debug.Log("Success - Simulation Anim Complete");

        KillYourself();
    }

    public void BustFinish()
    {
        Debug.Log("Bust - Simulation Anim Complete");
        //TODO - might want to add to a list of disproven theories, so the player doesn't make the same mistake twice
        KillYourself();
    }

    //call this to make the connection stretch between starting point and mouse location
    IEnumerator TempConnection()
    {
        while (answerObj == null)
        {
            //keep an eye on input.mousepos, may not function as intended
            Vector2 hypotenuse = Input.mousePosition - leadObj.transform.position;
            float dist = hypotenuse.magnitude;
            rect.sizeDelta = new Vector2(dist, 20f);

            //assign the position
            transform.position = Vector2.Lerp(leadObj.transform.position, Input.mousePosition, 0.5f);

            //calculate and apply the rotation
            float x = hypotenuse.x;
            float y = hypotenuse.y;
            float theta = 2 * Mathf.PI + Mathf.Atan2(y, -x);
            //Debug.Log(x + " " + y + " " + theta);

            Vector2 direction = new Vector2(Mathf.Sin(theta), Mathf.Cos(theta));

            transform.localRotation = Quaternion.LookRotation(Vector3.forward, direction);

            yield return new WaitForEndOfFrame();
        }
    }
}
