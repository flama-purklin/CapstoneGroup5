using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UI;

public class PowerControl : MonoBehaviour
{
    [Header("Core Variables")]
    public int powerTotal = 5000;
    public int currentPower;
    public float lerpTime = 1f;

    private bool powerActive = false;

    private float maxMask;
    private float currentMask;

    [Header("UI Elements")]
    [SerializeField] TMP_Text powerPercent;
    [SerializeField] Image powerBack;
    [SerializeField] Image powerFront;
    [SerializeField] Image powerIcon;
    [SerializeField] RectMask2D powerMask;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        currentPower = powerTotal;
        maxMask = powerBack.rectTransform.rect.width;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (powerActive)
        {
            if (powerTotal <= 0)
            {
                StopAllCoroutines();
                GameOver();
            }
            powerPercent.text = Mathf.CeilToInt((float)currentPower / powerTotal * 100f) + "%";
        }
        else
            powerPercent.text = string.Empty;
    }

    public void PowerInit()
    {
        powerActive = true;
        StartCoroutine(PowerPerSecond());
    }

    IEnumerator PowerPerSecond()
    {
        while (powerActive)
        {
            yield return new WaitForSeconds(1f);
            PowerDrain(1);
        }
    }

    //call with an int when you know the exact amount to remove
    public void PowerDrain(int amount)
    {
        currentPower -= amount;
        StartCoroutine(DrainLerp());
    }

    //call with a float when you want to remove a certain percent
    public void PowerDrain(float percent)
    {
        int equivAmount = Mathf.FloorToInt(powerTotal * percent);
        currentPower -= equivAmount;
        StartCoroutine(DrainLerp());
    }

    //optional visual lerp so when power is drained beyond a certain amount
    IEnumerator DrainLerp()
    {
        //store the current rectMask amt
        float prevMask = powerMask.padding.z;

        //calculate the new rectMask amt
        float currentPercent = (float)currentPower / powerTotal;
        currentMask = maxMask - (maxMask * currentPercent);

        //execute the lerp only if mask diff is bigger than 5
        if (currentMask - prevMask > 5)
        {
            Vector4 newPadding = new Vector4(0, 0, currentMask, 0);
            float lerpMask = prevMask;
            float currentTime = 0f;
            while (currentTime < lerpTime)
            {
                //calculate current lerp
                lerpMask = Mathf.Lerp(prevMask, currentMask, currentTime / lerpTime);
                newPadding.z = lerpMask;

                //update the actual mask
                powerMask.padding = newPadding;

                currentTime += Time.fixedDeltaTime;
                yield return new WaitForFixedUpdate();
            }
        }
        else
        {
            Vector4 newPadding = new Vector4(0, 0, currentMask, 0);
            powerMask.padding = newPadding;
        }

        yield return null;
    }


    //activate the final game state
    public void GameOver()
    {
        GameControl.GameController.currentState = GameState.FINAL;

        //disable the power if necessary??
    }
}
