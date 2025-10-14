using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;


public class VehicleController : MonoBehaviour
{
    public float speed = 10f;
    public Transform playerPosition;
    private Player player;
    public bool isUnlocked = false;

    private Rigidbody rb;
    private bool isChangingLane = false;
    private float[] lanePositions = { -1.2f, 0f, 1.2f };
    public int currentLane = 1;

    public VehicleType vehicleType;

    public GameObject bomb;
    public Transform bombPos;
    public bool canShoot = true;

    public CarCollection cars; // assign in inspector
    public int carIndex;       // upgrade index
    public int colorIndex;     // color index

    private GameObject activeCar;

    private void OnEnable()
    {
        canShoot = true;
    }

    private void Start()
    {
        player = FindObjectOfType<Player>();
        currentLane = player.currentLane;
    }

    public void SetupCar(int _carIndex, int _colorIndex)
    {
        if (vehicleType == VehicleType.Modified)
        {
            for (int i = 0; i < cars.colors.Length; i++)
            {
                for (int j = 0; j < cars.colors[i].upgradeLevels.Length; j++)
                {
                    cars.colors[i].upgradeLevels[j].SetActive(false);
                }
            }

            activeCar = null;

            // Activate chosen car
            activeCar = cars.colors[_colorIndex].upgradeLevels[_carIndex];
            activeCar.SetActive(true);
        }
    }


    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
        CheckCoinOverlap();

        if (Input.GetKeyDown(KeyCode.D) && currentLane < lanePositions.Length - 1 && !isChangingLane)        
            StartCoroutine(ChangeLane(currentLane + 1, 15));        
        else if (Input.GetKeyDown(KeyCode.A) && currentLane > 0 && !isChangingLane)        
            StartCoroutine(ChangeLane(currentLane - 1, -15));

        if (vehicleType == VehicleType.Tank && Input.GetKeyDown(KeyCode.Mouse0) && canShoot)
            StartCoroutine(ReleaseBomb());
    }

    private IEnumerator ReleaseBomb()
    {
        canShoot = false;
        Instantiate(bomb, bombPos.position, Quaternion.Euler(-90, 90, 90));        
        yield return new WaitForSeconds(1f);
        canShoot = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        Obstacles obstacle = collision.gameObject.GetComponent<Obstacles>();
        if (obstacle != null)
        {
            if(obstacle.obstacleType == ObstacleType.Explosive)
            {
                obstacle.Explode();
                gameObject.SetActive(false);
                player.stateMachine.ChangeState(player.airState);                
            }
            else
            {
                Vector3 hitPoint = collision.contacts[0].point;
                obstacle.PushRigidBodies(hitPoint, 30f, 10f);
            }   
        }

        Projectile projectile = collision.gameObject.GetComponent<Projectile>();
        if (projectile != null)
        { 
            gameObject.SetActive(false);
            player.stateMachine.ChangeState(player.airState);
        }
    }

    private void CheckCoinOverlap()
    {
        Collider[] coins = Physics.OverlapSphere(transform.position, 1f, LayerMask.GetMask("Coin"));
        foreach (Collider coin in coins)
        {
            Coin c = coin.GetComponent<Coin>();
            if (c != null)
            {
                UIManager.instance.MoveCoinImg(coin.transform.position);

                ParticleSystem particle = Instantiate(player.collectParticle, player.transform.position, Quaternion.identity);
                particle.transform.parent = player.transform;

                Destroy(coin.gameObject);
            }
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

    [ContextMenu("Delete Wheels")]
    public void DeleteWheels()
    {
        Wheels[] wheels = GetComponentsInChildren<Wheels>();
        MeshCollider[] colliders = GetComponentsInChildren<MeshCollider>();

        foreach (Wheels wheel in wheels)
        {
            DestroyImmediate(wheel);
        }

        foreach (MeshCollider col in colliders)
        {
            DestroyImmediate(col);
        }
    }
}

public enum VehicleType
{
    Car,
    Tank,
    Modified,
}

[Serializable]
public class Car
{
    public GameObject[] upgradeLevels; // 0 = base, 1 = upgraded, etc.
}

[Serializable]
public class CarCollection
{
    public Car[] colors; // 0 = Black, 1 = Blue, 2 = Cyan, etc.
}
