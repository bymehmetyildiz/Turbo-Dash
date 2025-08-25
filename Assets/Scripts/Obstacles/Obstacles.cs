using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacles : MonoBehaviour
{
    private Rigidbody[] rbs;
    public ObstacleType obstacleType;
    Player player;

    private void Start()
    {
        rbs = GetComponentsInChildren<Rigidbody>();

        foreach (Rigidbody rb in rbs)
        {
            rb.isKinematic = true;
        }

        player = FindObjectOfType<Player>();
    }

    public void ActivateRigidbodies()
    {
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

    public void PushRigidBodies(float x, float y, float z)
    {
        Collider[] collider = GetComponents<Collider>();

        foreach (Collider col in collider)
        {
            col.enabled = false;
        }

        foreach (Rigidbody rb in rbs)
        {
            rb.isKinematic = false;
            rb.AddForce(new Vector3(x, y, z), ForceMode.Impulse);
        }
    }


    private void Update()
    {
        if(IsPlayerPast())
            Destroy(gameObject);
    }

    private bool IsPlayerPast()
    {
        if(player.transform.position.z > transform.position.z + 30)
            return true;

        return false;
    }

}

public enum ObstacleType
{    
    Single,
    Multiple,
}
