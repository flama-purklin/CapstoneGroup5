using System.Collections;
using UnityEngine;

public class EvidenceControl : MinigameCanvasControl
{

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //play the animation here and fill in the information from the EvidenceData passed on from EvidenceObj
    public void Reveal(EvidenceData newData)
    {
        assignedData = newData;
        StartCoroutine(EvidenceRevealLerp());

    }

    IEnumerator EvidenceRevealLerp()
    {
        //fill in the data first
        FillEvidence(assignedData);

        anim.Play("evidenceReveal");

        yield return new WaitForSeconds(5f);

        anim.Rebind();
        anim.Update(0f);
        anim.Play("evidenceFinish");
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
