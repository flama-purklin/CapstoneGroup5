using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class NumberManager : MonoBehaviour
{
    public int currentIndex;

    [Header("Number Sprites")]
    [SerializeField] List<Sprite> numbers;

    [Header("Number Objects")]
    [SerializeField] GameObject[] numSlots;

    [Header("Rotating Pointers")]
    [SerializeField] GameObject upperSlot;
    [SerializeField] GameObject currentSlot;
    [SerializeField] GameObject lowerSlot;

    float maskHeight;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        maskHeight = GetComponent<RectTransform>().rect.height;
        ResetLocations();
    }

    private void ResetLocations()
    {
        //initialize the location to be accurate
        currentSlot.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;

        //upper to the proper offset
        upperSlot.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, maskHeight);

        //lower to the proper offset
        lowerSlot.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -maskHeight);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //called when the up button is pressed
    public void NumberUp()
    {
        StartCoroutine(UpwardLerp());
    }

    //called when the down button is pressed
    public void NumberDown()
    {
        StartCoroutine(DownwardLerp());
    }

    IEnumerator UpwardLerp()
    {
        //get initial locations
        Vector3 upperStart = upperSlot.GetComponent<RectTransform>().anchoredPosition;
        Vector3 upperFinal = upperStart + Vector3.up * maskHeight;

        Vector3 currentStart = currentSlot.GetComponent<RectTransform>().anchoredPosition;
        Vector3 currentFinal = currentStart + Vector3.up * maskHeight;

        Vector3 lowerStart = lowerSlot.GetComponent<RectTransform>().anchoredPosition;
        Vector3 lowerFinal = lowerStart + Vector3.up * maskHeight;


        //execute lerp
        float currentTime = 0f;

        while (currentTime < 0.25f)
        {
            upperSlot.GetComponent<RectTransform>().anchoredPosition = Vector3.Lerp(upperStart, upperFinal, currentTime / 0.25f);
            currentSlot.GetComponent<RectTransform>().anchoredPosition = Vector3.Lerp(currentStart, currentFinal, currentTime / 0.25f);
            lowerSlot.GetComponent<RectTransform>().anchoredPosition = Vector3.Lerp(lowerStart, lowerFinal, currentTime / 0.25f);
            currentTime += Time.deltaTime;
            yield return new WaitForFixedUpdate();
        }

        //solidify final locations
        upperSlot.GetComponent<RectTransform>().anchoredPosition = upperFinal;
        currentSlot.GetComponent<RectTransform>().anchoredPosition = currentFinal;
        lowerSlot.GetComponent<RectTransform>().anchoredPosition = lowerFinal;

        //switch values
        TopToBottom();


        yield return null;
    }

    IEnumerator DownwardLerp()
    {
        //get initial locations
        Vector3 upperStart = upperSlot.GetComponent<RectTransform>().anchoredPosition;
        Vector3 upperFinal = upperStart - Vector3.up * maskHeight;

        Vector3 currentStart = currentSlot.GetComponent<RectTransform>().anchoredPosition;
        Vector3 currentFinal = currentStart - Vector3.up * maskHeight;

        Vector3 lowerStart = lowerSlot.GetComponent<RectTransform>().anchoredPosition;
        Vector3 lowerFinal = lowerStart - Vector3.up * maskHeight;


        //execute lerp
        float currentTime = 0f;

        while (currentTime < 0.25f)
        {
            upperSlot.GetComponent<RectTransform>().anchoredPosition = Vector3.Lerp(upperStart, upperFinal, currentTime / 0.25f);
            currentSlot.GetComponent<RectTransform>().anchoredPosition = Vector3.Lerp(currentStart, currentFinal, currentTime / 0.25f);
            lowerSlot.GetComponent<RectTransform>().anchoredPosition = Vector3.Lerp(lowerStart, lowerFinal, currentTime / 0.25f);

            currentTime += Time.deltaTime;
            yield return new WaitForFixedUpdate();
        }

        //solidify final locations
        upperSlot.GetComponent<RectTransform>().anchoredPosition = upperFinal;
        currentSlot.GetComponent<RectTransform>().anchoredPosition = currentFinal;
        lowerSlot.GetComponent<RectTransform>().anchoredPosition = lowerFinal;
        
        //switch values
        BottomToTop();

        yield return null;
    }

    //use to shift the top number to the bottom
    private void TopToBottom()
    {
        //store old values
        GameObject oldCurrent = currentSlot;
        GameObject oldLower = lowerSlot;
        GameObject oldUpper = upperSlot;

        //assign new values
        upperSlot = oldCurrent;
        currentSlot = oldLower;
        lowerSlot = oldUpper;

        //update the index value
        UpdateIndex();

        //new sprite for the new bottom number
        UpdateSprite(lowerSlot, currentIndex+1);
       
        ResetLocations();

        //checkCombination
        GameObject.FindFirstObjectByType<LuggageControl>().CheckCombo();
    }

    //use to shift the bottom number object to the top
    private void BottomToTop()
    {
        //store old values
        GameObject oldCurrent = currentSlot;
        GameObject oldLower = lowerSlot;
        GameObject oldUpper = upperSlot;

        //assign new values
        upperSlot = oldLower;
        currentSlot = oldUpper;
        lowerSlot = oldCurrent;

        //update the index value
        UpdateIndex();

        //new sprite for the new bottom number
        UpdateSprite(upperSlot, currentIndex - 1);

        ResetLocations();

        //check combination
        GameObject.FindFirstObjectByType<LuggageControl>().CheckCombo();
    }

    private void UpdateIndex()
    {
        Sprite currentSprite = currentSlot.GetComponent<Image>().sprite;
        currentIndex = numbers.IndexOf(currentSprite);
    }

    private void UpdateSprite(GameObject obj, int proposedIndex)
    {
        int actualIndex = (proposedIndex + 10) % 10;
        obj.GetComponent<Image>().sprite = numbers[actualIndex];
    }
}
