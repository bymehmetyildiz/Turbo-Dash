using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
    public float speed;
    public Transform playerPosition;
    Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        transform.position += Vector3.forward * speed * Time.deltaTime;
    }

    private void OnCollisionEnter(Collision collision)
    {
        Obstacles obstacle = collision.gameObject.GetComponent<Obstacles>();

        if (obstacle != null)
        {
            obstacle.PushRigidBodies();
        }
    }
}
