using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : MonoBehaviour
{
    private Player player;

    private void Start()
    {
        player = FindObjectOfType<Player>();
    }

    private void Update()
    {
        transform.Rotate(0f, 180 * Time.deltaTime, 0f);

        if(IsPlayerPast())
            Destroy(gameObject);
    }

    private bool IsPlayerPast()
    {
        if (player.transform.position.z > transform.position.z + 15)
            return true;

        return false;
    }

  

}


