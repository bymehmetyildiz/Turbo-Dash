using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectible : MonoBehaviour
{
    public CollectibleType collectibleType;

   public void Collect(Player player)
    {
        if(collectibleType == CollectibleType.Key)
            player.keys++;

        Destroy(gameObject); // Remove the collectible from the scene
    }
}

public enum CollectibleType
{
    Key,
    Coin,
    PowerUp
}
