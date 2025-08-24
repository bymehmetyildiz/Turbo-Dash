using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;


public class CarController : MonoBehaviour
{
    public float speed = 10f;
    public Transform playerPosition;

    private Rigidbody rb;
    private bool isChangingLane = false;
    private float[] lanePositions = { -1.2f, 0f, 1.2f };
    public int currentLane = 1;

    void Start()
    {
             
    }

    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);

        if (Input.GetKeyDown(KeyCode.D) && currentLane < lanePositions.Length - 1 && !isChangingLane)        
            StartCoroutine(ChangeLane(currentLane + 1, 15));        
        else if (Input.GetKeyDown(KeyCode.A) && currentLane > 0 && !isChangingLane)        
            StartCoroutine(ChangeLane(currentLane - 1, -15));

    }

    private void OnCollisionEnter(Collision collision)
    {
        Obstacles obstacle = collision.gameObject.GetComponent<Obstacles>();

        if (obstacle != null)
        {      
            obstacle.PushRigidBodies();
        }
    }

    public IEnumerator ChangeLane(int targetLane, int angle)
    {
        isChangingLane = true;

        Vector3 startPosition = transform.position;
        Vector3 endPosition = new Vector3(lanePositions[targetLane], transform.position.y, transform.position.z);

        Quaternion startRotation = transform.rotation;
        Quaternion tiltRotation = Quaternion.Euler(0, angle, 0);

        float duration = 0.25f; // time to complete lane change
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = Mathf.Clamp01(elapsed / duration);

            // Horizontal movement
            float xPos = Mathf.Lerp(startPosition.x, endPosition.x, t);
            transform.position = new Vector3(xPos, transform.position.y, transform.position.z);

            // Rotation tilt forward, then back
            if (t < 0.5f)
            {
                // First half: tilt to target angle
                transform.rotation = Quaternion.Slerp(startRotation, tiltRotation, t * 2f);
            }
            else
            {
                // Second half: tilt back to upright
                transform.rotation = Quaternion.Slerp(tiltRotation, Quaternion.identity, (t - 0.5f) * 2f);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Snap to final lane position & upright rotation
        transform.position = new Vector3(endPosition.x, transform.position.y, transform.position.z);
        transform.rotation = Quaternion.identity;

        currentLane = targetLane;
        isChangingLane = false;
    }

}

