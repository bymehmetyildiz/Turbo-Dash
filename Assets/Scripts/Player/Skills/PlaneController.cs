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
        prop.transform.Rotate(propSpeed * Time.deltaTime, 0, 0);

        if ((Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.Space)) && canDropBomb)        
            StartCoroutine(ReleaseBomb());
    }

    private IEnumerator ReleaseBomb()
    {
        canDropBomb = false;
        AudioManager.instance.PlaySound(10);
        Instantiate(bombPrefab, bomb.transform.position, Quaternion.Euler(-90,90,90));
        bomb.SetActive(false);
        yield return new WaitForSeconds(player.planeReloadDur);
        bomb.SetActive(true);
        canDropBomb = true;
    }
}
