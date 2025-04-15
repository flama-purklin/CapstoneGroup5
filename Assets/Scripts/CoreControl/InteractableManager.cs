using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InteractableManager : MonoBehaviour
{
    public static HashSet<GameObject> allInteractables = new HashSet<GameObject>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static IEnumerable<GameObject> FindAllInteractablesinRange(Vector3 point, float range)
    {
        range *= range;
        return allInteractables.Where(x => x != null && (point - x.transform.position).sqrMagnitude < range);
    }
}
