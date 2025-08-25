using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    
    void Start()
    {
        
    }

    
    void Update()
    {
        transform.Translate(transform.forward * 75f * Time.deltaTime);
    }
}
