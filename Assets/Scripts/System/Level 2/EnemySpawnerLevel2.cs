﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class EnemySpawnerLevel2 : NetworkBehaviour, IEnemySpawner
{
    public static EnemySpawnerLevel2 Instance;

    [Header("Enemy Data")]
    public EnemySpawnData[] enemySpawnDatas;

    [Header("Show UI")]
    public GameObject warningBoss;
    public GameObject bossLevel1;
    public GameObject UIHealthBoss;

    public GameObject BossSpawnPosition;

    [Header("Hide UI")]
    public GameObject remain;

    [Header("Boss Level 1 Script")]
    public AssassinBossSkill assassin;

    private NetworkVariable<bool> stopSpawning = new NetworkVariable<bool>(false);
    private NetworkVariable<int> currentTotalSpawnCount = new NetworkVariable<int>(0);
    public int maxTotalSpawnCount;

    [Header("Time Spawn")]
    private float timeElapsed = 0f;
    public float spawnSpeedIncreaseInterval = 70f;
    public float spawnSpeedDecreaseAmount = 0.2f;
    private float minSpawnTimeLimit = 1f;
    private float maxSpawnTimeLimit = 1f;

    private NetworkVariable<bool> isBossSpawned = new NetworkVariable<bool>(false);

    [Header("Enemy Select")]
    public int numberOfPicks;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (!IsServer)
            return; // Chỉ server mới spawn quái
        else
        {
            EnemyManager.Instance.killTarget.Value = 5;
            // KillCounterUI.Instance.CounterUI();
            BossSpawnPosition = GameObject.FindWithTag("BossSpawner");

            StartCoroutine(Initialize());
        }
    }

    public IEnumerator Initialize()
    {
        // Gọi hàm này từ nơi khác để khởi tạo EnemySpawner
        if (IsServer)
        {
            yield return new WaitForSeconds(4f); // Đợi 4 giây trước khi bắt đầu spawn
            List<EnemySpawnData> pickedEnemies = PickRandomEnemies();

            foreach (var spawnData in pickedEnemies)
            {
                StartCoroutine(SpawnEnemyIndependently(spawnData));
            }
        }
    }

    public override void OnNetworkSpawn()
    {
        if (IsClient)
        {
            HideBossHealthUI();
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
                spawnData.minSpawnTime = Mathf.Max(
                    spawnData.minSpawnTime - spawnSpeedDecreaseAmount,
                    minSpawnTimeLimit
                );
                spawnData.maxSpawnTime = Mathf.Max(
                    spawnData.maxSpawnTime - spawnSpeedDecreaseAmount,
                    maxSpawnTimeLimit
                );
            }

            timeElapsed = 0f;
        }
    }

    private List<EnemySpawnData> PickRandomEnemies()
    {
        // Đảm bảo số lượng bóc không lớn hơn danh sách enemy có sẵn
        int pickCount = Mathf.Min(numberOfPicks, enemySpawnDatas.Length);

        List<EnemySpawnData> pickedEnemies = enemySpawnDatas
            .OrderBy(x => Random.value) // Xáo trộn danh sách
            .Take(pickCount) // Chọn số lượng cần bóc
            .ToList();

        Debug.Log("Enemy picked:");
        foreach (var enemy in pickedEnemies)
        {
            Debug.Log($"- {enemy.enemyPrefab.name}");
        }

        return pickedEnemies;
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
                spawnedEnemy.GetComponent<NetworkObject>().Spawn(true);

                if (!spawnedEnemy.CompareTag("Enemy"))
                {
                    SetTagRecursive(spawnedEnemy, "Enemy");
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
                && !isBossSpawned.Value
            )
            {
                stopSpawning.Value = true;
                isBossSpawned.Value = true;
                StartCoroutine(HandleBossSpawn());
                break;
            }
        }
    }

    private void SetTagRecursive(GameObject obj, string tag)
    {
        obj.tag = tag;
        foreach (Transform child in obj.transform)
        {
            SetTagRecursive(child.gameObject, tag);
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
                BossSpawnPosition.transform.position,
                Quaternion.identity
            );
            bossSpawned.GetComponent<NetworkObject>().Spawn(true);
            AssassinHealth.Instance.IntializeBossHealthServerRpc();
            AssassinBossSkill.Instance.Active();
        }

        yield return new WaitForSeconds(0.5f);

        if (UIHealthBoss != null)
            UIHealthBoss.SetActive(true);
    }

    public void TestHandleBossSpawn()
    {
        if (!IsServer)
        {
            return;
        }
        remain.SetActive(false);

        warningBoss.SetActive(true);

        warningBoss.SetActive(false);
        ShowBossHealthUI();
        if (bossLevel1 != null)
        {
            GameObject bossSpawned = Instantiate(
                bossLevel1,
                BossSpawnPosition.transform.position,
                Quaternion.identity
            );
            bossSpawned.GetComponent<NetworkObject>().Spawn(true);

            AssassinBossSkill.Instance.Active();
        }

        if (UIHealthBoss != null)
            UIHealthBoss.SetActive(true);
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
