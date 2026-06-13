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
                projectile.speed = 150;
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
