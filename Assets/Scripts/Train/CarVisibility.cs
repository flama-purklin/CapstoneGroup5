using UnityEngine;

public class CarVisibility : MonoBehaviour
{
    public bool selected = false;
    [SerializeField] MeshRenderer carFront;
    [SerializeField] GameObject carTop;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (selected)
            CarSelected();
        else
            CarDeselected();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //disable the front mesh so you can see in
    public void CarSelected()
    {
        selected = true;
        carFront.enabled = false;
        carTop.SetActive(false);
    }

    //enable the front mesh so you can't see in
    public void CarDeselected()
    {
        selected = false;
        carFront.enabled = true;
        carTop.SetActive(true);
    }
}
