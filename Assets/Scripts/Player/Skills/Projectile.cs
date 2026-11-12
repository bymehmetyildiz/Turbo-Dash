using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private ParticleSystem hitEffect;
    public float speed;

    // Set by the tank / plane when they fire: the shot keeps the shooter's forward speed on top of its own,
    // so it always pulls away from the vehicle no matter how fast the run has become.
    public float inheritedForwardSpeed;
    public bool firedByPlayer;

    private bool hasHit;

    void Start()
    {
        Destroy(gameObject, 4f);
    }

    void Update()
    {
        if (hasHit)
            return;

        // speed was tuned as "units per frame at 60 FPS"; scale by time so it is the same on every device
        Vector3 step = transform.TransformDirection(transform.forward) * (speed * 60f * Time.deltaTime)
                     + Vector3.forward * (inheritedForwardSpeed * Time.deltaTime);

        // Fast shots can cross a thin target between two frames, so sweep the path first
        float distance = step.magnitude;
        if (firedByPlayer && distance > 0.001f
            && Physics.SphereCast(transform.position, 0.3f, step / distance, out RaycastHit hit, distance, ~0, QueryTriggerInteraction.Ignore))
        {
            GameObject target = FindTarget(hit.collider);
            if (target != null)
            {
                transform.position = hit.point;
                HandleHit(target, hit.point);
                return;
            }
        }

        transform.position += step;
    }

    private static GameObject FindTarget(Collider col)
    {
        Obstacles obstacle = col.GetComponentInParent<Obstacles>();
        if (obstacle != null)
            return obstacle.gameObject;

        Planes aircraft = col.GetComponentInParent<Planes>();
        if (aircraft != null)
            return aircraft.gameObject;

        return null;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (hasHit)
            return;

        Vector3 point = collision.contactCount > 0 ? collision.GetContact(0).point : transform.position;
        GameObject target = FindTarget(collision.collider);
        HandleHit(target != null ? target : collision.gameObject, point);
    }

    private void HandleHit(GameObject other, Vector3 hitPoint)
    {
        Player player = other.GetComponentInParent<Player>();

        // The player's own shots never hurt the player or the vehicle they were fired from
        if (firedByPlayer && (player != null || other.GetComponentInParent<VehicleController>() != null))
            return;

        hasHit = true;

        if (hitEffect != null)
            Instantiate(hitEffect, transform.position, Quaternion.identity);

        Obstacles obstacle = other.GetComponent<Obstacles>();
        Planes aircraft = other.GetComponent<Planes>();

        if (obstacle != null)
        {
            if (obstacle.obstacleType == ObstacleType.Explosive || obstacle.obstacleType == ObstacleType.Armed)
            {
                Destroy(obstacle.gameObject);
            }
            else
            {
                obstacle.PushRigidBodies(hitPoint, 30f, 10f); // explosionForce, explosionRadius
            }
        }
        else if (aircraft != null)
        {
            aircraft.Explode();
        }
        else if (player != null)
        {
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
