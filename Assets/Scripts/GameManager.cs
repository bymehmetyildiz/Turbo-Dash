using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject[] platforms;
    private Player player;

    //Vehicles
    [SerializeField] private GameObject[] vehicles;
    [SerializeField] private GameObject[] obstacles;
    [SerializeField] private float spawnInterval;
    private float minSpawnInterval = 1f;
    private float maxSpawnInterval = 3f;
    [SerializeField] private float spawnTimer;
    [SerializeField] private float spawnDistance;


    void Start()
    {
        player = FindObjectOfType<Player>();
    }

    
    void Update()
    {
        SpawnPlatform();

        if (player.isStarted)
        {
            spawnTimer += Time.deltaTime;
            if (spawnTimer >= spawnInterval)
            {
                SpawnVehicle();
                spawnTimer = 0f;
                spawnInterval = Random.Range(minSpawnInterval, maxSpawnInterval);

                if(minSpawnInterval > 0.25f)
                    minSpawnInterval -= minSpawnInterval * 0.001f; 

                if(maxSpawnInterval > 1f)
                    maxSpawnInterval -= maxSpawnInterval * 0.001f;
            }
        }

        float[] lanes = { -1.2f, 1.2f };

    }

    private void SpawnPlatform()
    {
        for (int i = 0; i < platforms.Length; i++)
        {
            if (player.transform.position.z > platforms[i].transform.position.z + 150f)
            {
                platforms[i].transform.position = new Vector3(
                    platforms[i].transform.position.x,
                    platforms[i].transform.position.y,
                    platforms[i].transform.position.z + 315f
                );
            }

        }
    }

    private void SpawnVehicle()
    {
        /*
        float[] lanes = { -1.2f, 1.2f };

        foreach (float laneX in lanes)
        {
            // 50% chance to spawn a vehicle on this lane
            if (Random.value < 0.5f)
            {
                GameObject vehicle = vehicles[Random.Range(0, vehicles.Length)];
                Vector3 spawnPos = new Vector3(
                    laneX,
                    0,
                    player.transform.position.z + spawnDistance
                );
                Instantiate(vehicle, spawnPos, Quaternion.Euler(0, 180, 0));
            }
        }
        */

        float[] lanes = {1.2f, 0, -1.2f};
        
        GameObject obstacle = obstacles[Random.Range(0, obstacles.Length)];
        float lane;
        Vector3 spawnPos;
        if (obstacle.GetComponent<Obstacles>().obstacleType == ObstacleType.Single)
        {
           lane = lanes[Random.Range(0, lanes.Length)];
           spawnPos = new Vector3(
               lane,
               0,
               player.transform.position.z + spawnDistance
       );
        }
        else
        {
            lane = lanes[1];
            spawnPos = new Vector3(
               lane,
               2.2f,
               player.transform.position.z + spawnDistance
       );
        }
        Instantiate(obstacle, spawnPos, Quaternion.Euler(0, 0, 0));
        
    }


}
