using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vehicle : MonoBehaviour
{
    public float speed;
    private float destroyDur = 5f;

    void Start()
    {
        Destroy(gameObject, destroyDur);
    }

    
    void Update()
    {
        transform.Translate(transform.forward * -speed * Time.deltaTime);
        
        if(destroyDur > 2f)
            destroyDur -= destroyDur * 0.01f;
    }
}
