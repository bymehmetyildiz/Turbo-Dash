using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planes : MonoBehaviour
{
    [SerializeField] private GameObject[] prop;
    [SerializeField] private float propSpeed;
    [SerializeField] private float moveSpeed;

    [SerializeField] private ParticleSystem explosion;

    private Player player;

    void Start()
    {
        player = FindObjectOfType<Player>();
    }

    
    void Update()
    {
        foreach (var item in prop)
        {
            if(item != null)
                item.transform.Rotate(0, 0, propSpeed * Time.deltaTime);            
        }

        transform.Translate(-transform.forward * moveSpeed * Time.deltaTime);

        if(player.transform.position.z > transform.position.z + 30f)
            Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Instantiate(explosion, transform.position, Quaternion.identity);
            player.stateMachine.ChangeState(player.airState);
            AudioManager.instance.PlaySound(9);
            Destroy(gameObject);
        }

        if (collision.gameObject.GetComponent<Projectile>() != null)
        {
            Instantiate(explosion, transform.position, Quaternion.identity);
            AudioManager.instance.PlaySound(9);
            Destroy(gameObject);
        }
    }
}
