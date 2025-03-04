using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class MysteryNavigation : MonoBehaviour, IDragHandler
{

    RectTransform rectTransform;

    //settings
    [SerializeField] float maxZoom = 10f;
    [SerializeField] float minZoom = 1f;
    [SerializeField] float zoomRate = 0.01f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();

        
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
        if (currentScale.x > minZoom && zoomAmt < 0)
        {
            transform.localScale = currentScale + Vector3.one * zoomRate;
        }
        else if (zoomAmt < 0)
        {
            transform.localScale = Vector3.one * minZoom;
        }

        //zoom out handler
        if (currentScale.x < maxZoom && zoomAmt > 0)
        {
            transform.localScale = currentScale - Vector3.one * zoomRate;
        }
        else if (zoomAmt > 0)
        {
            transform.localScale = Vector3.one * maxZoom;
        }
    }

    public void OnDrag(PointerEventData data)
    {
        if (Input.GetMouseButton(1))
        {
            Vector2 newPosition = rectTransform.anchoredPosition + data.delta;

            rectTransform.anchoredPosition = newPosition;
        }
    }
}
