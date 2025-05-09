using NUnit.Framework;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class LuggageControl : MinigameCanvasControl
{

    string currentCombo;
    bool solved = false;

    bool shutdown = false;

    [Header("Number Controls")]
    [SerializeField] List<NumberManager> nums = new List<NumberManager>();
    /*[SerializeField] NumberManager firstNum;
    [SerializeField] NumberManager secondNum;
    [SerializeField] NumberManager thirdNum;
    [SerializeField] NumberManager fourthNum;*/


    // Update is called once per frame
    void Update()
    {
        if ((GameControl.GameController.currentState == GameState.FINAL && !shutdown) || 
            (Input.GetKeyDown(KeyCode.Escape) && GameControl.GameController.currentState == GameState.MINIGAME))
        {
            shutdown = true;
            ExitLuggage();
        }
    }

    public void SetCombo(string newCombo)
    {
        //for debugging
        // Debug.Log("New Combination: " + newCombo);
        foreach (var number in nums)
        {
            number.NumberInit();
        }

        solved = false;
        currentCombo = newCombo;
        GameControl.GameController.currentState = GameState.MINIGAME;
        anim.Play("luggageInit");
    }

    public void ExitLuggage()
    {
        StartCoroutine(CloseLuggage());
    }

    IEnumerator CloseLuggage()
    {
        anim.Rebind();
        anim.Update(0f);
        anim.Play("luggageCancel");


        while (anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1)
        {
            yield return null;
        }

        //reset game state
        if (GameControl.GameController.currentState != GameState.FINAL)
        {
            GameControl.GameController.currentState = GameState.DEFAULT;
        }

        gameObject.SetActive(false);
    }

    public void CheckCombo()
    {
        string input = "";
        foreach (var num in nums)
        {
            input += num.currentIndex.ToString();
        }
     
        // Debug.Log("Testing combination: " + input);
        if (!solved && input.Equals(currentCombo))
        {
            solved = true;
            StartCoroutine(Unlock());
        }
    }

    IEnumerator Unlock()
    {
        // Debug.Log("Correct Combination!");

        //fill in the canvas from the associated EvidenceData
        FillEvidence(assignedData);

        anim.Play("luggageSuccess");

        //unlock corresponding evidence node here
        GameControl.GameController.coreConstellation.DiscoverNode(assignedData.nodeKey);

        yield return new WaitForSeconds(5f);

        anim.Rebind();
        anim.Update(0f);
        anim.Play("successExit");
        while (anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1)
        {
            yield return null;
        }

        if (GameControl.GameController.currentState != GameState.FINAL)
        {
            GameControl.GameController.currentState = GameState.DEFAULT;
        }

        gameObject.SetActive(false);


    }
}
