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

    private void Start()
    {
        rbs = GetComponentsInChildren<Rigidbody>();

        foreach (Rigidbody rb in rbs)
        {
            rb.isKinematic = true;
        }

        player = FindObjectOfType<Player>();
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

        if (obstacleType == ObstacleType.Armed && IsPlayerDetected() && !isShooting)        
            StartCoroutine(Shoot());
        
    }

    private bool IsPlayerPast()
    {
        if(player.transform.position.z > transform.position.z + 30)
            return true;

        return false;
    }

    public void Explode()
    {
        if (player.isShielded)
            return;

        Instantiate(explosion, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

    private IEnumerator Shoot()
    {
        if (player.isStarted)
        {
            isShooting = true;

            while (IsPlayerDetected()) // keep firing only while player is detected
            {
                Instantiate(cannonBall, cannonPos.position, Quaternion.identity);
                yield return new WaitForSeconds(2f);
            }

            isShooting = false;
        }
    }

    private bool IsPlayerDetected()
    {
        RaycastHit hit;

        // Try both forward and backward depending on prefab orientation
        if (Physics.Raycast(cannonPos.transform.position, -transform.forward, out hit, 50f)) // limit distance
        {
            if (hit.collider.CompareTag("Player"))
                return true;
        }

        return false;
    }


}

public enum ObstacleType
{    
    Single,
    Multiple,
    Explosive,
    Armed,
}
