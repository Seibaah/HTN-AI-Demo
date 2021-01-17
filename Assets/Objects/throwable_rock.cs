using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class throwable_rock : MonoBehaviour
{
    private Rigidbody r;
    private BoxCollider c;

    //initialize rock
    private void Start()
    {
        r = gameObject.AddComponent<Rigidbody>();
        r.isKinematic = true;
        c = gameObject.AddComponent<BoxCollider>();
        c.size = new Vector3(0.8f, 0.8f, 0.8f);
    }
}
