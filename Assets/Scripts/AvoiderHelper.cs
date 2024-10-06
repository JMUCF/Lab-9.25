using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AvoiderDll; // Use the namespace from your DLL

public class AvoiderHelper : MonoBehaviour
{
    private OurAvoider avoider;
    public GameObject target;
    public GameObject node;

    void Start()
    {
        avoider = gameObject.AddComponent<OurAvoider>();  // Attaching the Avoider script from the DLL
        avoider.target = target;
        avoider.node = node;

        // Optionally set other properties or call methods
        avoider.showGizmos = true;
        avoider.speed = 3.5f;
    }

    void Update()
    {
        // Interact with avoider as needed
    }
}
