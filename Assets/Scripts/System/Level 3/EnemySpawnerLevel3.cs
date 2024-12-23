﻿using System.Collections;
using UnityEngine;

public class EnemySpawnerLevel3 : MonoBehaviour, IEnemySpawner
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
    public Cyborg cyborg;

    private bool stopSpawning = false;

    [Header("Max and current enemy in level")]
    public int currentTotalSpawnCount = 0;
    public int maxTotalSpawnCount;

    [Header("Time Spawn")]
    private float timeElapsed = 0f;
    public float spawnSpeedIncreaseInterval = 70f;
    public float spawnSpeedDecreaseAmount = 0.2f;
    private float minSpawnTimeLimit = 1f;
    private float maxSpawnTimeLimit = 1f;

    private void Start()
    {
        if (bossLevel1 != null)
        {
            bossLevel1.SetActive(false);
        }

        if (UIHealthBoss != null)
        {
            UIHealthBoss.SetActive(false);
        }

        if (warningBoss != null)
        {
            warningBoss.SetActive(false);
        }

        StartCoroutine(SpawnEnemies());
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

    IEnumerator SpawnEnemies()
    {
        yield return new WaitForSeconds(2f);

        while (!stopSpawning)
        {
            if (currentTotalSpawnCount < maxTotalSpawnCount)
            {
                foreach (EnemySpawnData spawnData in enemySpawnDatas)
                {
                    yield return StartCoroutine(SpawnEnemy(spawnData));
                }
            }

            if (EnemyManager.Instance != null && EnemyManager.Instance.killTarget <= EnemyManager.Instance.enemiesKilled)
            {
                stopSpawning = true;
                StartCoroutine(HandleBossSpawn());
                break;
            }

            yield return null;
        }
    }
    private IEnumerator SpawnEnemy(EnemySpawnData spawnData)
    {
        Transform spawnPoint = spawnData.spawnPoints[Random.Range(0, spawnData.spawnPoints.Length)];
        Vector3 spawnPosition = spawnPoint.position;

        float offsetY = 1.5f;
        spawnPosition.y += offsetY;

        GameObject spawnedEnemy = Instantiate(spawnData.enemyPrefab, spawnPosition, Quaternion.identity);

        if (!spawnedEnemy.CompareTag("Enemy"))
        {
            spawnedEnemy.tag = "Enemy";
        }

        currentTotalSpawnCount++;

        yield return new WaitForSeconds(Random.Range(spawnData.minSpawnTime, spawnData.maxSpawnTime));
    }

    public void OnEnemyKilled()
    {
        currentTotalSpawnCount--;

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
            cyborg.Active();
        }

        yield return new WaitForSeconds(0.5f);

        if (UIHealthBoss != null)
        {
            UIHealthBoss.SetActive(true);
        }
    }
}
