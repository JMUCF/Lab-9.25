using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AvoiderDll;

public class AvoiderHelper : MonoBehaviour
{
    private OurAvoider avoider;
    public GameObject target;
    public GameObject node;
    public float speed;
    public float range;
    public bool showGizmos;

    void Start()
    {
        avoider = gameObject.AddComponent<OurAvoider>();
        //incase helper values are not given
        if (speed == 0)
            speed = 3.5f;
        if (range == 0)
            range = 3f;

        avoider.target = target;
        avoider.node = node;
        avoider.showGizmos = showGizmos;
        avoider.speed = speed;
        avoider.range = range;
    }

    void Update()
    {
        avoider.showGizmos = showGizmos; //in update so you can toggle during runtime. Cant put speed or range in here, causes issues
    }
}
