using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private ParticleSystem hitEffect;
    public float speed;

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
        if (collision.gameObject.GetComponent<Obstacles>() != null)
        {
            Obstacles obstacle = collision.gameObject.GetComponent<Obstacles>();
            if (obstacle != null)
            {
                if (obstacle.obstacleType == ObstacleType.Explosive || obstacle.obstacleType == ObstacleType.Armed)
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
        }
        else if(collision.gameObject.GetComponent<Player>() != null)
        {
            Player player = collision.gameObject.GetComponent<Player>();
            if (!player.isShielded)
            {
                player.stateMachine.ChangeState(player.fastHitState);
                player.StartCoroutine(player.DeathBounce());
                player.isStarted = false;
                player.jetPack.SetActive(false);
                AudioManager.instance.PlaySound(7);
            }
        }
        AudioManager.instance.PlaySound(9);
        Destroy(gameObject, 0.1f);
    }
}

