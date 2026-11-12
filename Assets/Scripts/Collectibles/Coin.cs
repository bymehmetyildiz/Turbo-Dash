using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : MonoBehaviour
{
    private void Update()
    {
        transform.Rotate(0f, 180 * Time.deltaTime, 0f);

        if(IsPlayerPast())
            Release();
    }

    private bool IsPlayerPast()
    {
        Player player = Player.instance;
        return player != null && player.transform.position.z > transform.position.z + 15;
    }

    // Coins go back to the pool instead of being destroyed
    public void Release()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.ReleaseCoin(gameObject);
        else
            Destroy(gameObject);
    }
}
