﻿using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class EnemySpawnerLevel4 : NetworkBehaviour, IEnemySpawner
{
    [Header("Enemy Data")]
    public EnemySpawnData[] enemySpawnDatas;

    [Header("Show UI")]
    public GameObject warningBoss;
    public GameObject bossLevel1;
    public GameObject UIHealthBoss;

    [Header("Hide UI")]
    public GameObject remain;

    [Header("Boss Level 1 Script")]
    public SnakeController snakeBoss;

    private NetworkVariable<bool> stopSpawning = new NetworkVariable<bool>(false);

    [Header("Max and current enemy in level")]
    private NetworkVariable<int> currentTotalSpawnCount = new NetworkVariable<int>(0);
    public int maxTotalSpawnCount;

    [Header("Time Spawn")]
    private float timeElapsed = 0f;
    public float spawnSpeedIncreaseInterval = 70f;
    public float spawnSpeedDecreaseAmount = 0.2f;
    private float minSpawnTimeLimit = 1f;
    private float maxSpawnTimeLimit = 1f;

    private NetworkVariable<bool> isBossSpawn = new NetworkVariable<bool>(false);

    private void Start()
    {
        if (bossLevel1 != null)
        {
            // bossLevel1.SetActive(false);
        }

        if (UIHealthBoss != null)
        {
            UIHealthBoss.SetActive(false);
        }

        if (warningBoss != null)
        {
            warningBoss.SetActive(false);
        }

        foreach (var spawnData in enemySpawnDatas)
        {
            StartCoroutine(SpawnEnemyIndependently(spawnData));
        }
    }

    void Update()
    {
        timeElapsed += Time.deltaTime;

        if (timeElapsed >= spawnSpeedIncreaseInterval)
        {
            foreach (var spawnData in enemySpawnDatas)
            {
                if (spawnData.minSpawnTime > minSpawnTimeLimit)
                {
                    spawnData.minSpawnTime -= spawnSpeedDecreaseAmount;
                }

                if (spawnData.maxSpawnTime > maxSpawnTimeLimit)
                {
                    spawnData.maxSpawnTime -= spawnSpeedDecreaseAmount;
                }

                spawnData.minSpawnTime = Mathf.Max(spawnData.minSpawnTime, minSpawnTimeLimit);
                spawnData.maxSpawnTime = Mathf.Max(spawnData.maxSpawnTime, maxSpawnTimeLimit);
            }

            timeElapsed = 0f;
        }
    }

    private IEnumerator SpawnEnemyIndependently(EnemySpawnData spawnData)
    {
        yield return new WaitForSeconds(1f);

        while (!stopSpawning.Value)
        {
            if (currentTotalSpawnCount.Value < maxTotalSpawnCount)
            {
                Transform spawnPoint = spawnData.spawnPoints[
                    Random.Range(0, spawnData.spawnPoints.Length)
                ];
                Vector3 spawnPosition = spawnPoint.position;
                spawnPosition.y += 1.5f;

                GameObject spawnedEnemy = Instantiate(
                    spawnData.enemyPrefab,
                    spawnPosition,
                    Quaternion.identity
                );

                if (!spawnedEnemy.CompareTag("Enemy"))
                {
                    spawnedEnemy.tag = "Enemy";
                }

                currentTotalSpawnCount.Value++;
            }

            yield return new WaitForSeconds(
                Random.Range(spawnData.minSpawnTime, spawnData.maxSpawnTime)
            );

            if (
                EnemyManager.Instance != null
                && EnemyManager.Instance.killTarget.Value
                    <= EnemyManager.Instance.enemiesKilled.Value
                && !isBossSpawn.Value
            )
            {
                stopSpawning.Value = true;
                isBossSpawn.Value = true;
                StartCoroutine(HandleBossSpawn());
                break;
            }

            yield return null;
        }
    }

    public void OnEnemyKilled()
    {
        currentTotalSpawnCount.Value--;

        if (stopSpawning.Value)
        {
            currentTotalSpawnCount.Value = 0;
        }
    }

    private IEnumerator HandleBossSpawn()
    {
        if (remain != null)
        {
            remain.SetActive(false);
        }

        if (warningBoss != null)
        {
            warningBoss.SetActive(true);
        }

        yield return new WaitForSeconds(3f);

        if (warningBoss != null)
        {
            warningBoss.SetActive(false);
        }

        if (bossLevel1 != null)
        {
            bossLevel1.SetActive(true);
            snakeBoss.Active();
        }

        yield return new WaitForSeconds(0.5f);

        if (UIHealthBoss != null)
        {
            UIHealthBoss.SetActive(true);
        }
    }

    public void TestHandleBossSpawn()
    {
        if (remain != null)
        {
            remain.SetActive(false);
        }

        if (warningBoss != null)
        {
            warningBoss.SetActive(true);
        }

        if (warningBoss != null)
        {
            warningBoss.SetActive(false);
        }

        if (bossLevel1 != null)
        {
            bossLevel1.SetActive(true);
            SnakeController.Instance.Active();
        }
        ;

        if (UIHealthBoss != null)
        {
            UIHealthBoss.SetActive(true);
        }
    }
}
