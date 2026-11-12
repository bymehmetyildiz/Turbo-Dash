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

    // Half extents of the box used to detect a crash with the flying player
    private const float hitHalfWidth = 1.2f;
    private const float hitHalfHeight = 1.6f;
    private const float hitHalfLength = 2.5f;

    private float lastZDelta = float.MaxValue;
    private bool destroyed;

    void Start()
    {
        player = Player.instance;
    }

    void Update()
    {
        foreach (var item in prop)
        {
            if(item != null)
                item.transform.Rotate(0, 0, propSpeed * Time.deltaTime);
        }

        transform.Translate(-transform.forward * moveSpeed * Time.deltaTime);

        CheckPlayerCrash();

        if(player.transform.position.z > transform.position.z + 30f)
            Destroy(gameObject);
    }

    // The player is a CharacterController, which does not raise reliable collision events against this rigidbody,
    // and the closing speed is high enough to skip past the collider in one frame. So the crash is checked by hand.
    private void CheckPlayerCrash()
    {
        if (destroyed || player == null)
            return;

        Vector3 delta = transform.position - player.transform.position;
        float previousZDelta = lastZDelta;
        lastZDelta = delta.z;

        bool flying = player.stateMachine.currentstate == player.planeState
                   || player.stateMachine.currentstate == player.jetState;
        if (!flying || player.isShielded)
            return;

        if (Mathf.Abs(delta.x) > hitHalfWidth || Mathf.Abs(delta.y) > hitHalfHeight)
            return;

        // Inside the box now, or passed through it since the last frame
        if (delta.z <= hitHalfLength && previousZDelta >= -hitHalfLength)
            CrashIntoPlayer();
    }

    private void CrashIntoPlayer()
    {
        player.stateMachine.ChangeState(player.airState);
        Explode();
    }

    // Also called by the player's projectiles
    public void Explode()
    {
        if (destroyed)
            return;

        destroyed = true;

        if (explosion != null)
            Instantiate(explosion, transform.position, Quaternion.identity);

        AudioManager.instance.PlaySound(9);
        Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (destroyed)
            return;

        if (collision.gameObject.CompareTag("Player"))
        {
            bool flying = player.stateMachine.currentstate == player.planeState
                       || player.stateMachine.currentstate == player.jetState;
            if (flying && !player.isShielded)
                CrashIntoPlayer();
        }
    }
}
