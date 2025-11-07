using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleMember : MonoBehaviour
{
    private AudioSource collapse;

    void Start()
    {
        collapse = GetComponentInParent<Obstacles>().collapse;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            if (collapse != null)
            {
                collapse.pitch = Random.Range(0.8f, 1.2f);
                collapse.Play();
            }

        }
    }
}
