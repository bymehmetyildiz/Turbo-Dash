using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CrazyGames;
using TMPro;
using UnityEngine.SceneManagement;

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
    [SerializeField] private float coinTrailChance = 0.8f;
    [SerializeField] private float zigZagCoinChance = 0.25f;
    private int obstacleRowsSpawned;

    //Coins
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private int coinPerRow;
    [SerializeField] private float coinSpacing;

    public TMP_Text adblockText;


    void Start()
    {
        if (CrazySDK.IsAvailable)
        {
            CrazySDK.Init(() =>
            {
                Debug.Log("CrazySDK initialized");
            });
            CheckAdblock();
        }


        player = FindObjectOfType<Player>();
        
    }

    private async void CheckAdblock()
    {
        if (MainDemoScene.UseAsyncMethods)
        {
            bool adblockPresent = await CrazySDK.Ad.HasAdblockAsync();
            adblockText.text = "Has adblock: " + adblockPresent + " (async)";
        }
        else
        {
            CrazySDK.Ad.HasAdblock(
                (adblockPresent) =>
                {
                    adblockText.text = "Has adblock: " + adblockPresent;
                }
            );
        }
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

                if(player.stateMachine.currentstate == player.planeState 
                    || player.stateMachine.currentstate == player.jetState)
                {
                    SpawnCoinInAir();
                }
            }
        }

        if(spawnInterval > 0.8f && player.isStarted)
        {
            spawnInterval -= Time.deltaTime * 0.001f; // Gradually decrease spawn interval to increase difficulty
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
        float[] lanes = { -1.2f, 0f, 1.2f };

        GameObject obstacle = obstacles[Random.Range(0, obstacles.Length)];
        ObstacleType type = obstacle.GetComponent<Obstacles>().obstacleType;
        float rowZ = player.transform.position.z + spawnDistance;
        obstacleRowsSpawned++;

        if (type == ObstacleType.Multiple)
        {
            // Always center lane, elevated
            Vector3 spawnPosMulti = new Vector3(0, 2.2f, rowZ);
            Instantiate(obstacle, spawnPosMulti, Quaternion.identity);
            SpawnCoinReward(rowZ + 8f, lanes[Random.Range(0, lanes.Length)]);
            return;
        }

        int safeLaneIndex = Random.Range(0, lanes.Length);
        int blockedCount = obstacleRowsSpawned < 3 ? 1 : Random.Range(1, 3);
        int spawned = 0;

        for (int i = 0; i < lanes.Length; i++)
        {
            if (i == safeLaneIndex || spawned >= blockedCount)
                continue;

            GameObject rowObstacle = spawned == 0 ? obstacle : GetSingleObstacle();
            Vector3 spawnPos = new Vector3(lanes[i], 0, rowZ);
            Instantiate(rowObstacle, spawnPos, Quaternion.identity);
            spawned++;
        }

        SpawnCoinReward(rowZ, lanes[safeLaneIndex]);
    }

    private GameObject GetSingleObstacle()
    {
        GameObject obstacle;
        do
        {
            obstacle = obstacles[Random.Range(0, obstacles.Length)];
        } while (obstacle.GetComponent<Obstacles>().obstacleType == ObstacleType.Multiple);

        return obstacle;
    }

    private void SpawnCoinReward(float zPos, float safeLane)
    {
        if (coinPrefab == null || Random.value > coinTrailChance)
            return;

        if (Random.value < zigZagCoinChance)
        {
            SpawnZigZagCoins(zPos);
            return;
        }

        SpawnCoin(zPos, safeLane);
    }

    private void SpawnZigZagCoins(float zPos)
    {
        float[] lanes = { -1.2f, 0f, 1.2f };
        int laneIndex = Random.Range(0, lanes.Length);
        int direction = Random.value > 0.5f ? 1 : -1;
        int amount = Mathf.Max(coinPerRow, 6);

        for (int i = 0; i < amount; i++)
        {
            laneIndex = Mathf.Clamp(laneIndex + (i % 2 == 0 ? direction : 0), 0, lanes.Length - 1);
            Vector3 pos = new Vector3(lanes[laneIndex], 1f, zPos + i * coinSpacing);
            Instantiate(coinPrefab, pos, Quaternion.identity);

            if (laneIndex == 0 || laneIndex == lanes.Length - 1)
                direction *= -1;
        }
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
        1f, // coin height
        zPos
        );

        for (int i = 0; i < coinPerRow; i++)
        {
            Vector3 pos = spawnPos + new Vector3(0, 0, i * coinSpacing);
            Instantiate(coinPrefab, pos, Quaternion.identity);
        }

    }

    private void SpawnCoinInAir()
    {
        float[] lanes = new float[] { -3.5f, 0f, 3.5f };

        Vector3 spawnPos = new Vector3( lanes[Random.Range(0, lanes.Length)], 10f, player.transform.position.z + spawnDistance);

        for (int i = 0; i < coinPerRow; i++)
        {
            Vector3 pos = spawnPos + new Vector3(0, 0, i * coinSpacing);
            Instantiate(coinPrefab, pos, Quaternion.identity);
        }
    }


}
