using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacles : MonoBehaviour
{
    private Rigidbody[] rbs;
    public ObstacleType obstacleType;
    Player player;

    //Explosive
    public Animator anim;
    [SerializeField] private ParticleSystem explosion;

    //Armed
    [SerializeField] private GameObject cannonBall;
    [SerializeField] private Transform cannonPos;
    private bool isShooting;

    public AudioSource collapse;
    private bool nearMissAwarded;
    private bool threatened;

    private void Start()
    {
        rbs = GetComponentsInChildren<Rigidbody>();

        foreach (Rigidbody rb in rbs)
        {
            rb.isKinematic = true;
        }

        player = Player.instance;
        anim = GetComponentInChildren<Animator>();
    
    }

    public void ActivateRigidbodies()
    {
        if (obstacleType == ObstacleType.Explosive)
            return;

        Collider[] collider = GetComponents<Collider>();

        foreach (Collider col in collider)
        {
            col.enabled = false;
        }

        foreach (Rigidbody rb in rbs)
        {
            rb.isKinematic = false;
            rb.AddForce(Vector3.forward * 2, ForceMode.Impulse);
        }
    }

    public void PushRigidBodies(Vector3 explosionPoint, float explosionForce, float explosionRadius)
    {
        if (obstacleType == ObstacleType.Explosive || obstacleType == ObstacleType.Armed)
            return;

        Collider[] collider = GetComponents<Collider>();

        foreach (Collider col in collider)
        {
            col.enabled = false;
        }

        foreach (Rigidbody rb in rbs)
        {
            rb.isKinematic = false;
            // Push away from the explosion point
            rb.AddExplosionForce(explosionForce, explosionPoint, explosionRadius, 1f, ForceMode.Impulse);
        }
    }


    private void Update()
    {
        if (IsPlayerPast())
            Destroy(gameObject);

        if (obstacleType == ObstacleType.Explosive)
        {
            float distance = Vector3.Distance(player.transform.position, transform.position);
            if (distance < 20f && player.isStarted)
            {
                anim.SetBool("Walk", true);
                transform.Translate(transform.forward * -10 * Time.deltaTime);
            }
        }

        if (obstacleType == ObstacleType.Armed && player.isStarted && IsPlayerDetected() && !isShooting)
            StartCoroutine(Shoot());

        CheckNearMiss();

    }

    private bool IsPlayerPast()
    {
        if(player.transform.position.z > transform.position.z + 75f)
            return true;

        return false;
    }

    public void Explode()
    {
        if (player.isShielded)
            return;

        Instantiate(explosion, transform.position, Quaternion.identity);
        Destroy(gameObject);
        AudioManager.instance.PlaySound(9);
    }

    private IEnumerator Shoot()
    {
        isShooting = true;
        AudioManager.instance.PlaySound(10);
        while (IsPlayerDetected() && player.isStarted)
        {
            Instantiate(cannonBall, cannonPos.position, Quaternion.identity);
            yield return new WaitForSeconds(3f);
        }
        isShooting = false;
    }

    private bool IsPlayerDetected()
    {
        RaycastHit hit;

        if (Physics.Raycast(cannonPos.transform.position, -transform.forward, out hit, 30f))
        {
            if (hit.collider.CompareTag("Player"))
                return true;
        }

        return false;
    }

    // A near miss is a late dodge: the player was on a collision course inside the reaction window
    // and still got past (by switching lane, jumping over or sliding under).
    private void CheckNearMiss()
    {
        if (nearMissAwarded || player == null || !player.isStarted)
            return;

        float zDelta = transform.position.z - player.transform.position.z;

        if (!threatened)
        {
            if (zDelta <= 0f || zDelta > player.moveSpeed * player.nearMissWindow || !player.IsOnFoot)
                return;

            bool sameLane = obstacleType == ObstacleType.Multiple
                || Mathf.Abs(transform.position.x - player.transform.position.x) < 0.6f;

            if (sameLane)
                threatened = true;

            return;
        }

        if (zDelta < -1.5f)
        {
            nearMissAwarded = true;
            player.RegisterNearMiss(transform.position);
        }
    }
}

public enum ObstacleType
{    
    Single,
    Multiple,
    Explosive,
    Armed,
}
