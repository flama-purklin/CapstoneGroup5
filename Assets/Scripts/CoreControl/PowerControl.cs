using System.Collections;
using TMPro;
using UnityEngine;

public class PowerControl : MonoBehaviour
{
    [Header("Core Variables")]
    public int powerTotal = 5000;
    public int currentPower;

    private bool powerActive = false;

    [Header("UI Elements")]
    [SerializeField] TMP_Text powerPercent;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        currentPower = powerTotal;
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
            powerPercent.text = ((float)currentPower / powerTotal).ToString("P");
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
    }

    //call with a float when you want to remove a certain percent
    public void PowerDrain(float percent)
    {
        int equivAmount = Mathf.FloorToInt(powerTotal * percent);
        currentPower -= equivAmount;
    }

    //optional visual lerp so when power is drained beyond a certain amount
    IEnumerator DrainLerp()
    {
        yield return null;
    }


    //activate the final game state
    public void GameOver()
    {
        GameControl.GameController.currentState = GameState.FINAL;

        //disable the power if necessary??
    }
}
