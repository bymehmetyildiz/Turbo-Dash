using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneController : MonoBehaviour
{
    Player player = Player.instance;

    [SerializeField] private GameObject prop;
    [SerializeField] private float propSpeed;

    [SerializeField] private GameObject bomb;
    [SerializeField] private GameObject bombPrefab;
    public bool canDropBomb = true;
    [SerializeField] private float planeShotSpeed = 1.25f; // extra speed over the plane, in the Projectile's units

    void Start()
    {
        canDropBomb = true;
        if (bomb != null)
            bomb.SetActive(true);
    }

    void OnEnable()
    {
        canDropBomb = true;
        if (bomb != null)
            bomb.SetActive(true);
    }

    void Update()
    {
        if (prop != null)
            prop.transform.Rotate(0, 0, propSpeed * Time.deltaTime);

        if (UnifiedInput.Fire && canDropBomb)
            StartCoroutine(ReleaseBomb());
    }


    private IEnumerator ReleaseBomb()
    {
        canDropBomb = false;

        // Ensure player reference
        if (player == null)
        {
            player = Player.instance;
            if (player == null)
            {
                Debug.LogWarning("PlaneController.ReleaseBomb: Player.instance is null. Aborting bomb release.");
                canDropBomb = true;
                yield break;
            }
        }

        // Ensure bombPrefab reference
        if (bombPrefab == null)
        {
            Debug.LogWarning("PlaneController.ReleaseBomb: bombPrefab is null. Aborting bomb release.");
            canDropBomb = true;
            yield break;
        }

        // Ensure bomb reference
        Vector3 spawnPosition = bomb != null ? bomb.transform.position : transform.position;

        GameObject bombObject = Instantiate(bombPrefab, spawnPosition, Quaternion.Euler(-90, 90, 90));
        if (bombObject != null)
        {
            Projectile projectile = bombObject.GetComponent<Projectile>();
            if (projectile != null)
            {
                // 150 units per frame used to throw the shot out of the world before it could be seen or hit anything.
                // It now flies ahead of the plane at a visible speed, on top of the plane's own speed.
                projectile.speed = planeShotSpeed;
                projectile.firedByPlayer = true;
                projectile.inheritedForwardSpeed = player.moveSpeed * 3f; // same multiplier PlaneState flies with
                AudioManager.instance.PlaySound(10);
            }
            else
            {
                Debug.LogWarning("PlaneController.ReleaseBomb: bombPrefab is missing Projectile component.");
            }
        }
        else
        {
            Debug.LogWarning("PlaneController.ReleaseBomb: Instantiate returned null.");
        }

        if (bomb != null)
            bomb.SetActive(false);

        yield return new WaitForSeconds(player.planeReloadDur);

        if (bomb != null)
            bomb.SetActive(true);

        canDropBomb = true;
    }
}
