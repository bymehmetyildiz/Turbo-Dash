using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject[] platforms;
    private Player player;

    //Obstacles
    [SerializeField] private GameObject[] aircrafts;
    [SerializeField] private GameObject[] obstacles;
    private float spawnInterval = 1.5f;
    [SerializeField] private float spawnTimer;
    [SerializeField] private float spawnDistance;

    //Coins
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private int coinPerRow;
    [SerializeField] private float coinSpacing;

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
        ObstacleType type = obstacle.GetComponent<Obstacles>().obstacleType;

        if (type == ObstacleType.Multiple)
        {
            // Always center lane, elevated
            Vector3 spawnPosMulti = new Vector3(0, 2.2f, player.transform.position.z + spawnDistance);
            Instantiate(obstacle, spawnPosMulti, Quaternion.identity);
            return;
        }

        // Otherwise it's a Single
        int laneIndex1 = Random.Range(0, lanes.Length);
        int laneIndex2;
        do
        {
            laneIndex2 = Random.Range(0, lanes.Length);
        } while (laneIndex1 == laneIndex2);

        float lane1 = lanes[laneIndex1];
        float lane2 = lanes[laneIndex2];

        Vector3 spawnPos1 = new Vector3(lane1, 0, player.transform.position.z + spawnDistance);
        Vector3 spawnPos2 = new Vector3(lane2, 0, player.transform.position.z + spawnDistance);

        Instantiate(obstacle, spawnPos1, Quaternion.identity);

        // Pick second obstacle (must be Single)
        GameObject obstacle2;
        do
        {
            obstacle2 = obstacles[Random.Range(0, obstacles.Length)];
        } while (obstacle2.GetComponent<Obstacles>().obstacleType == ObstacleType.Multiple);

        if (Random.value > 0.5f)
            Instantiate(obstacle2, spawnPos2, Quaternion.identity);
        else
            SpawnCoin(spawnPos2.z, lane2);
    }



    private void SpawnAircraft()
    {
        if ((player.stateMachine.currentstate == player.planeState
            || player.stateMachine.currentstate == player.jetState) && player.transform.position.y > 9)
        {
            float[] lanes = { -3.5f, 0f, 3.5f };
            float lane = lanes[Random.Range(0, lanes.Length)];
            Vector3 spawnPos = new Vector3(
                lane,
                9.5f,
                player.transform.position.z + spawnDistance);
            GameObject aircraft = aircrafts[Random.Range(0, aircrafts.Length)];
            Instantiate(aircraft, spawnPos, Quaternion.Euler(0, 180, 0));

            //Spawn Second Plane
            float lane2 = lanes[Random.Range(0, lanes.Length)];
            if (lane2 != lane)
            {
                Vector3 spawnPos2 = new Vector3(
                    lane2,
                    9.5f,
                    player.transform.position.z + spawnDistance + 30f);
                Instantiate(aircrafts[Random.Range(0, aircrafts.Length)], spawnPos2, Quaternion.Euler(0, 180, 0));
            }

        }

    }

    private void SpawnCoin(float zPos, float lane)
    {
        float[] lanes = { 1.2f, 0, -1.2f };
        Vector3 spawnPos = new Vector3(
        lane,
        0.5f, // coin height
        zPos
        );

        for (int i = 0; i < coinPerRow; i++)
        {
            Vector3 pos = spawnPos + new Vector3(0, 0, i * coinSpacing);
            Instantiate(coinPrefab, pos, Quaternion.identity);
        }

    }

}
