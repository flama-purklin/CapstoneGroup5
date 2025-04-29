using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class MysteryNavigation : MonoBehaviour, IDragHandler
{

    RectTransform rectTransform;
    RectTransform tempRect;

    [SerializeField] public Camera mysteryCam;

    Vector3 initCamPos;

    //settings
    [SerializeField] float maxZoom = 300f;
    [SerializeField] float minZoom = 100f;
    [SerializeField] float zoomRate = 1f;
    float maxMag;

    //background bounds
    float backWidth;
    float backHeight;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        tempRect = new GameObject("TempRect", typeof(RectTransform)).GetComponent<RectTransform>();

        backWidth = rectTransform.rect.width;
        backHeight = rectTransform.rect.height;
        
    }

    void OnEnable()
    {
        initCamPos = Camera.main.transform.localPosition;
        //Time.timeScale = 0f;
        Debug.Log("Mystery Enabled");
    }

    void OnDisable()
    {
        //Camera.main.orthographic = false;
        //Camera.main.transform.localPosition = initCamPos;
        //Time.timeScale = 1f;
        Debug.Log("Mystery Disabled");
    }

    // Update is called once per frame
    void Update()
    {

        ZoomCheck();


    }

    private void ZoomCheck()
    {
        float zoomAmt = Input.GetAxis("Mouse ScrollWheel");

        Vector3 currentScale = transform.localScale;
        //Debug.Log(zoomAmt);

        //zoom in handler
        if (mysteryCam.orthographicSize < maxZoom && zoomAmt < 0)
        {
            // Debug.Log("Zooming out");
            //transform.localScale = currentScale + Vector3.one * zoomRate;

            mysteryCam.orthographicSize += zoomAmt + zoomRate;
        }
        else if (zoomAmt < 0)
        {
            mysteryCam.orthographicSize = maxZoom;
            //transform.localScale = Vector3.one * maxZoom;
        }

        //zoom out handler
        if (mysteryCam.orthographicSize > minZoom && zoomAmt > 0)
        {
            // Debug.Log("Zooming in");
            //transform.localScale = currentScale - Vector3.one * zoomRate;

            mysteryCam.orthographicSize -= zoomAmt + zoomRate;
        }
        else if (zoomAmt > 0)
        {
            mysteryCam.orthographicSize = minZoom;
            //transform.localScale = Vector3.one * minZoom;
        }
    }

    public void OnDrag(PointerEventData data)
    {
        // Change to left mouse button for unified input scheme
        // Only respond if we're not in a theory mode (which would need the left click for other interactions)
        if (Input.GetMouseButton(0) && 
            GameControl.GameController.currentState == GameState.MYSTERY && 
            GameObject.FindFirstObjectByType<NodeControl>().theoryMode == TheoryMode.None)
        {
            //pre-orthographic attempts

            /*Vector2 newPosition = rectTransform.anchoredPosition + data.delta;

            //check whether the motion is within the bounds of the rectTransform
            Rect background = RectTransformToScreenSpace(rectTransform, newPosition);
            

            tempRect.anchorMin = rectTransform.anchorMin;
            tempRect.anchorMax = rectTransform.anchorMax;
            tempRect.anchoredPosition = rectTransform.anchoredPosition;
            tempRect.sizeDelta = rectTransform.sizeDelta;


            Vector2 topLeftPos = Camera.main.ViewportToScreenPoint(new Vector2(0, 0));
            Vector2 bottomRightPos = Camera.main.ViewportToScreenPoint(new Vector2(1, 1));

            

            //if (topLeft && bottomRight)
                rectTransform.anchoredPosition = newPosition;
            //else
                //Debug.Log("one bound outside");*/

            //figure out the current delta change data
            Vector3 dragAttempt = data.delta;
            Vector3 newPos = mysteryCam.transform.position + dragAttempt;
            float height = 2f * mysteryCam.orthographicSize;
            float width = height * mysteryCam.aspect;

            //identifying the bounds of the visible screen
            Vector3 topLeft = newPos - new Vector3(width / 2f, height / 2f);
            Vector3 bottomRight = newPos + new Vector3(width / 2f, height / 2f);

            //identifying the bounds of the background
            Vector3 backLeft = rectTransform.position - new Vector3(backWidth / 3f, backHeight / 3f);
            Vector3 backRight = rectTransform.position + new Vector3(backWidth / 3f, backHeight / 3f);

            //Rect background = RectTransformToScreenSpace(rectTransform, newPos);

            //check x
            if (topLeft.x < backLeft.x && dragAttempt.x > 0)
                dragAttempt.x = 0;
            else if (bottomRight.x > backRight.x && dragAttempt.x < 0)
                dragAttempt.x = 0;

            //check y
            if (topLeft.y < backLeft.y && dragAttempt.y > 0)
                dragAttempt.y = 0;
            else if (bottomRight.y > backRight.y && dragAttempt.y < 0)
                dragAttempt.y = 0;

            //apply the effects to the camera transform 
            mysteryCam.transform.position = mysteryCam.transform.position - dragAttempt;
        }
    }

    public static Rect RectTransformToScreenSpace(RectTransform transform, Vector3 newPos)
    {
        Vector2 size = Vector2.Scale(transform.rect.size, transform.lossyScale);
        return new Rect(new Vector2(newPos.x, newPos.y) - (size * 0.5f), size);
    }
}
