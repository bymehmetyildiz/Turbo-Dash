using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject[] platforms;
    private Player player;

    //Vehicles
    [SerializeField] private GameObject[] aircrafts;
    [SerializeField] private GameObject[] obstacles;
    private float spawnInterval;
    private float minSpawnInterval = 0.75f;
    private float maxSpawnInterval = 2f;
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
                SpawnObstacle();
                SpawnAircraft();
                spawnTimer = 0f;
                spawnInterval = Random.Range(minSpawnInterval, maxSpawnInterval);
            }
        }

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

    private void SpawnObstacle()
    {
        float[] lanes = { 1.2f, 0, -1.2f };

        GameObject obstacle = obstacles[Random.Range(0, obstacles.Length)];
        float lane, lane2;
        Vector3 spawnPos, spawnPos2 = Vector3.zero;
        bool spawnSecond = false;

        if (obstacle.GetComponent<Obstacles>().obstacleType != ObstacleType.Multiple)
        {
            lane = lanes[Random.Range(0, lanes.Length)];
            lane2 = lanes[Random.Range(0, lanes.Length)];

            spawnPos = new Vector3(
                lane,
                0,
                player.transform.position.z + spawnDistance);

            if (lane != lane2)
            {
                spawnPos2 = new Vector3(
                    lane2,
                    0,
                    player.transform.position.z + spawnDistance);
                spawnSecond = true;
            }
        }
        else
        {
            lane = lanes[1];
            spawnPos = new Vector3(
                lane,
                2.2f,
                player.transform.position.z + spawnDistance);
        }

        Instantiate(obstacle, spawnPos, Quaternion.identity);

        if (spawnSecond)
        {
            GameObject obstacle2 = obstacles[Random.Range(0, obstacles.Length)];

            if (obstacle2.GetComponent<Obstacles>().obstacleType != ObstacleType.Multiple)
            {
                Instantiate(obstacle2, spawnPos2, Quaternion.identity);
                spawnSecond = false;
            }
        }

    }

    private void SpawnAircraft()
    {
        if ((player.stateMachine.currentstate == player.planeState
            || player.stateMachine.currentstate == player.jetState) && player.transform.position.y > 9)
        {
            float[] lanes = { 1.2f, 0, -1.2f };
            float lane = lanes[Random.Range(0, lanes.Length)];
            Vector3 spawnPos = new Vector3(
                lane,
                9.5f,
                player.transform.position.z + spawnDistance);
            GameObject aircraft = aircrafts[Random.Range(0, aircrafts.Length)];
            Instantiate(aircraft, spawnPos, Quaternion.Euler(0, 180, 0));

            /* Spawn Second Plane
            float lane2 = lanes[Random.Range(0, lanes.Length)];
            if(lane2 != lane)
            {
                Vector3 spawnPos2 = new Vector3(
                    lane2,
                    9.5f,
                    player.transform.position.z + spawnDistance + 30f);
                Instantiate(aircrafts[Random.Range(0, aircrafts.Length)], spawnPos2, Quaternion.Euler(0, 180, 0));
            }
            */
        }
       
    }

}
