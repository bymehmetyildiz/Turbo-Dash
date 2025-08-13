using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacles : MonoBehaviour
{
    [SerializeField] private Quaternion targetRotation;

    void Start()
    {
        transform.rotation = targetRotation;        
    }

    
    void Update()
    {
        
    }

   
}
