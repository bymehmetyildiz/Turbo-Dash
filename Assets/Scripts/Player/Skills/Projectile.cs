using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private ParticleSystem hitEffect;
    [SerializeField] private float speed;

    void Start()
    {
        Destroy(gameObject, 4f);
    }

    
    void Update()
    {
        transform.Translate(transform.forward * speed * Time.deltaTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Instantiate(hitEffect, transform.position, Quaternion.identity);
        Obstacles obstacle = collision.gameObject.GetComponent<Obstacles>();
        if (obstacle != null)
        {
            if(obstacle.obstacleType == ObstacleType.Explosive || obstacle.obstacleType == ObstacleType.Armed)
            {
                Destroy(obstacle.gameObject);
            }
            else
            {
                // Use the first contact point of the collision
                Vector3 hitPoint = collision.contacts[0].point;
                // Call explosion push instead of fixed x,y,z
                obstacle.PushRigidBodies(hitPoint, 30f, 10f); // explosionForce, explosionRadius
            }
            
        }
        Destroy(gameObject, 0.1f);
    }
}
