using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destructible : MonoBehaviour
{
    private Rigidbody[] rigidbodies;

    private void Start()
    {
        // Find all child rigidbodies
        rigidbodies = GetComponentsInChildren<Rigidbody>();
        
        // Disable all rigidbodies initially
        foreach (Rigidbody rb in rigidbodies)
        {
            rb.isKinematic = true;
        }
    }

    public void Break()
    {
        foreach (Rigidbody rb in rigidbodies)
        {
            rb.isKinematic = false;
        }
    }
}
