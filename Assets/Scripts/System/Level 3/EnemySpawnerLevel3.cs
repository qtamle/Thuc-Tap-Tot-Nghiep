﻿using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class EnemySpawnerLevel3 : NetworkBehaviour, IEnemySpawner
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
    public GameObject BossSpawnPostion;

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
        if (!IsServer)
            return; // Chỉ server mới spawn quái
        else
        {
            EnemyManager.Instance.killTarget.Value = 2;
            // KillCounterUI.Instance.CounterUI();
            BossSpawnPostion = GameObject.FindWithTag("BossSpawner");

            foreach (var spawnData in enemySpawnDatas)
            {
                StartCoroutine(SpawnEnemyIndependently(spawnData));
            }
        }
    }

    void Update()
    {
        if (!IsServer)
            return;

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
                spawnedEnemy.GetComponent<NetworkObject>().Spawn();

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
    }

    private IEnumerator HandleBossSpawn()
    {
        if (remain != null)
            remain.SetActive(false);
        if (warningBoss != null)
            warningBoss.SetActive(true);

        yield return new WaitForSeconds(3f);

        if (warningBoss != null)
            warningBoss.SetActive(false);
        ShowBossHealthUI();

        if (bossLevel1 != null)
        {
            GameObject bossSpawned = Instantiate(
                bossLevel1,
                BossSpawnPostion.transform.position,
                Quaternion.identity
            );
            bossSpawned.GetComponent<NetworkObject>().Spawn();
            CyborgHealth.Instance.IntializeBossHealthServerRpc();
            Cyborg.Instance.Active();
        }

        yield return new WaitForSeconds(0.5f);

        if (UIHealthBoss != null)
        {
            UIHealthBoss.SetActive(true);
        }
    }

    public void TestHandleBossSpawn()
    {
        if (!IsServer)
            return;

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
            GameObject bossSpawned = Instantiate(
                bossLevel1,
                BossSpawnPostion.transform.position,
                Quaternion.identity
            );
            bossSpawned.GetComponent<NetworkObject>().Spawn(true);
            Cyborg.Instance.Active();
        }

        if (UIHealthBoss != null)
        {
            UIHealthBoss.SetActive(true);
        }
    }

    public void ShowBossHealthUI()
    {
        ShowBossHealthUIServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    public void ShowBossHealthUIServerRpc()
    {
        UIHealthBoss.SetActive(true);
        warningBoss.SetActive(true);
        ShowBossHealthUIClientRpc();
    }

    [ClientRpc]
    private void ShowBossHealthUIClientRpc()
    {
        UIHealthBoss.SetActive(true);
        warningBoss.SetActive(true);
        warningBoss.SetActive(false);
    }

    public void HideBossHealthUI()
    {
        if (!IsServer)
            return; // Chỉ Server mới có quyền gọi

        UIHealthBoss.SetActive(false);
        warningBoss.SetActive(false);
        HideBossHealthUIClientRpc();
    }

    [ClientRpc]
    private void HideBossHealthUIClientRpc()
    {
        if (!IsServer) // Server đã tự bật, chỉ client cần bật
        {
            warningBoss.SetActive(false);
            UIHealthBoss.SetActive(false);
        }
    }
}
