using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

    // Difficulty (driven by Player.Difficulty01, which follows the speed ramp)
    private const float startSpawnInterval = 1.5f;
    private const float minSpawnInterval = 0.85f;
    [SerializeField] private float multipleChanceStart = 0.15f;   // full-width rows that force a jump / slide
    [SerializeField] private float multipleChanceEnd = 0.4f;
    [SerializeField] private float doubleBlockChanceStart = 0.4f; // rows with two of the three lanes blocked
    [SerializeField] private float doubleBlockChanceEnd = 0.8f;
    private readonly List<GameObject> singleObstacles = new List<GameObject>();
    private readonly List<GameObject> multiObstacles = new List<GameObject>();
    private bool lastRowWasMultiple;

    //Coins
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private int coinPerRow;
    [SerializeField] private float coinSpacing;

    void Start()
    {
        player = FindObjectOfType<Player>();

        foreach (GameObject obstacle in obstacles)
        {
            Obstacles o = obstacle != null ? obstacle.GetComponent<Obstacles>() : null;
            if (o == null)
                continue;

            if (o.obstacleType == ObstacleType.Multiple)
                multiObstacles.Add(obstacle);
            else
                singleObstacles.Add(obstacle);
        }
    }

    //private async void CheckAdblock()
    //{
    //    if (MainDemoScene.UseAsyncMethods)
    //    {
    //        bool adblockPresent = await CrazySDK.Ad.HasAdblockAsync();
    //        adblockText.text = "Has adblock: " + adblockPresent + " (async)";
    //    }
    //    else
    //    {
    //        CrazySDK.Ad.HasAdblock(
    //            (adblockPresent) =>
    //            {
    //                adblockText.text = "Has adblock: " + adblockPresent;
    //            }
    //        );
    //    }
    //}


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

        // Rows come faster as the run speeds up
        spawnInterval = Mathf.Lerp(startSpawnInterval, minSpawnInterval, player.Difficulty01);

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
        float difficulty = player.Difficulty01;
        float rowZ = player.transform.position.z + spawnDistance;
        obstacleRowsSpawned++;

        // Full-width rows are the only ones that need a jump or a slide, so they get more common over time
        bool spawnMultiple = multiObstacles.Count > 0
            && obstacleRowsSpawned > 3
            && !lastRowWasMultiple
            && Random.value < Mathf.Lerp(multipleChanceStart, multipleChanceEnd, difficulty);

        if (singleObstacles.Count == 0)
            spawnMultiple = multiObstacles.Count > 0;

        lastRowWasMultiple = spawnMultiple;

        if (spawnMultiple)
        {
            // Always center lane, elevated
            Vector3 spawnPosMulti = new Vector3(0, 2.2f, rowZ);
            Instantiate(multiObstacles[Random.Range(0, multiObstacles.Count)], spawnPosMulti, Quaternion.identity);
            SpawnCoinReward(rowZ + 8f, Random.Range(0, lanes.Length));
            return;
        }

        if (singleObstacles.Count == 0)
            return;

        int safeLaneIndex = Random.Range(0, lanes.Length);
        int blockedCount = 1;
        if (obstacleRowsSpawned >= 3 && Random.value < Mathf.Lerp(doubleBlockChanceStart, doubleBlockChanceEnd, difficulty))
            blockedCount = 2;

        int spawned = 0;

        for (int i = 0; i < lanes.Length; i++)
        {
            if (i == safeLaneIndex || spawned >= blockedCount)
                continue;

            Vector3 spawnPos = new Vector3(lanes[i], 0, rowZ);
            Instantiate(GetSingleObstacle(), spawnPos, Quaternion.identity);
            spawned++;
        }

        SpawnCoinReward(rowZ, safeLaneIndex);
    }

    private GameObject GetSingleObstacle()
    {
        return singleObstacles[Random.Range(0, singleObstacles.Count)];
    }

    private void SpawnCoinReward(float zPos, int safeLaneIndex)
    {
        if (coinPrefab == null || Random.value > coinTrailChance)
            return;

        if (Random.value < zigZagCoinChance)
        {
            SpawnZigZagCoins(zPos, safeLaneIndex);
            return;
        }

        float[] lanes = { -1.2f, 0f, 1.2f };
        SpawnCoin(zPos, lanes[safeLaneIndex]);
    }

    // Starts in the safe lane and stays there for the first coins, so the trail never leads into an obstacle
    private void SpawnZigZagCoins(float zPos, int startLaneIndex)
    {
        float[] lanes = { -1.2f, 0f, 1.2f };
        int last = lanes.Length - 1;
        int laneIndex = Mathf.Clamp(startLaneIndex, 0, last);
        int direction = laneIndex == 0 ? 1 : laneIndex == last ? -1 : (Random.value > 0.5f ? 1 : -1);
        int amount = Mathf.Max(coinPerRow, 6);

        for (int i = 0; i < amount; i++)
        {
            if (i >= 3 && i % 2 == 1)
            {
                laneIndex += direction;
                if (laneIndex <= 0 || laneIndex >= last)
                {
                    laneIndex = Mathf.Clamp(laneIndex, 0, last);
                    direction *= -1;
                }
            }

            Vector3 pos = new Vector3(lanes[laneIndex], 1f, zPos + i * coinSpacing);
            Instantiate(coinPrefab, pos, Quaternion.identity);
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

        // Coins sit on the line the player is actually flying on
        float airCoinHeight = player.stateMachine.currentstate == player.planeState ? 9.8f : 10f;
        Vector3 spawnPos = new Vector3( lanes[Random.Range(0, lanes.Length)], airCoinHeight, player.transform.position.z + spawnDistance);

        for (int i = 0; i < coinPerRow; i++)
        {
            Vector3 pos = spawnPos + new Vector3(0, 0, i * coinSpacing);
            Instantiate(coinPrefab, pos, Quaternion.identity);
        }
    }


}
