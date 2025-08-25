using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    
    void Start()
    {
        Destroy(gameObject, 4f);
    }

    
    void Update()
    {
        transform.Translate(transform.forward * 75f * Time.deltaTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Obstacles obstacle = collision.gameObject.GetComponent<Obstacles>();
        if (obstacle != null)
        {
            obstacle.PushRigidBodies(Random.Range(-5, 5), Random.Range(5, 15), Random.Range(20, 35));
            Destroy(gameObject, 0.1f);
        }
        
    }
}
