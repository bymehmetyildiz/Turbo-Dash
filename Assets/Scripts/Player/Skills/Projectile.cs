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
    private Vector3 travelDirection;

    // Both arguments are world units per second. Configure before the first physics tick.
    public void LaunchFromPlayer(float additionalSpeed, float forwardSpeed)
    {
        speed = Mathf.Abs(additionalSpeed) / 60f;
        inheritedForwardSpeed = Mathf.Max(0f, forwardSpeed);
        firedByPlayer = true;
        ConfigureMotion();
    }

    void Start()
    {
        Destroy(gameObject, 4f);
        ConfigureMotion();
    }

    private void ConfigureMotion()
    {

        // The direction is fixed once, at launch. It used to be read from the rotation every frame, so as soon as
        // physics nudged or spun the shot (it spawns touching the vehicle that fires it) it flew off sideways or backwards.
        travelDirection = firedByPlayer
            ? Vector3.forward
            : transform.TransformDirection(transform.forward).normalized;

        Rigidbody body = GetComponent<Rigidbody>();
        if (body != null)
        {
            // This script moves the shot itself, every frame. With interpolation on, Unity overwrites the transform
            // each frame with a pose blended between the last two physics steps, throwing away most of that movement:
            // the shot crawled behind the tank at high frame rates and jumped in steps at low ones (WebGL).
            body.interpolation = RigidbodyInterpolation.None;

            if (firedByPlayer)
            {
                // Player shots are moved only by this script; hits are found by the sweep in Update
                body.isKinematic = true;
            }
            else
            {
                body.constraints = RigidbodyConstraints.FreezeRotation;
            }
        }
    }

    void Update()
    {
        Advance(Time.deltaTime);
    }

    private void Advance(float deltaTime)
    {
        if (hasHit)
            return;

        // speed was tuned as "units per frame at 60 FPS"; scale by time so it is the same on every device
        // Player shots move along world +Z, independently of the model rotation.
        float ownSpeed = firedByPlayer ? Mathf.Abs(speed) : speed;
        Vector3 step = travelDirection * ((ownSpeed * 60f + inheritedForwardSpeed) * deltaTime);

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
