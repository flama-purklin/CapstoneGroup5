using System.Collections;
using UnityEngine;

public class NodeNotif : MonoBehaviour
{
    [SerializeField] Animator anim;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void NodeUnlock()
    {
        anim.SetTrigger("Play");
    }

    //lol not needed, please ignore
    IEnumerator NotifAnim()
    {
        anim.Play("nodeNotif");

        yield return new WaitForSeconds(5f);


        anim.Rebind();
        anim.Update(0f);
        anim.Play("node");
        while (anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1)
        {
            yield return null;
        }
    }
}
