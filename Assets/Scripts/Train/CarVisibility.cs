using UnityEngine;

public class CarVisibility : MonoBehaviour
{
    public bool selected = false;
    [SerializeField] public MeshRenderer carFront;
    [SerializeField] public GameObject carTop;
    private CarCharacters carCharacters;

    void Awake()
    {
        carCharacters = GetComponent<CarCharacters>();
        if (!carCharacters)
        {
            Debug.LogError($"CarCharacters component missing on {gameObject.name}!");
        }

    }

    void Start()
    {
        if (selected)
            CarSelected();
        else
            CarDeselected();
    }

    //disable the front mesh so you can see in
    public void CarDeselected()
    {
        selected = false;
        if (carFront) carFront.enabled = true;
        if (carTop) carTop.SetActive(true);
    }

    //enable the front mesh so you can't see in
    public void CarSelected()
    {
        selected = true;
        if (carFront) carFront.enabled = false;
        if (carTop) carTop.SetActive(false);
        if (carCharacters) carCharacters.InitializeCharacters();
    }

    public Bounds GetBounds()
    {
        Renderer mainRenderer = GetComponentInChildren<Renderer>();
        return mainRenderer ? mainRenderer.bounds : new Bounds(transform.position, Vector3.one);
    }
}
