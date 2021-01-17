using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class throwable_crate : MonoBehaviour
{
    private Rigidbody r;
    private BoxCollider c;

    //initialize crate
    private void Start()
    {
        r = gameObject.AddComponent<Rigidbody>();
        r.isKinematic = true;
        c = gameObject.AddComponent<BoxCollider>();
        c.size = new Vector3(0.75f, 0.65f, 0.75f);
        c.center = new Vector3(0.0f, 0.32f, 0.0f);
    }

    //crate is destroyed when hitting the floor after bieng thrown
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == 8)
        {
            Destroy(gameObject);
        }
    }
}
