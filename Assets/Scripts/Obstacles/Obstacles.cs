using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacles : MonoBehaviour
{
    public ObstacleType obstacleType;
    public GameObject Door;

    void Start()
    {
               
    }

    
    void Update()
    {
        
    }   
}

public enum ObstacleType
{
    Door,
    Single,
    Multiple,
}
