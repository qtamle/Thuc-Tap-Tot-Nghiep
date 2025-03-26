using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class EnemySpawnerLevel5 : NetworkBehaviour, IEnemySpawner
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
    public Boss5 skullBoss;

    private NetworkVariable<bool> stopSpawning = new NetworkVariable<bool>(false);

    [Header("Max and current enemy in level")]
    private NetworkVariable<int> currentTotalSpawnCount = new NetworkVariable<int>(0);
    public int maxTotalSpawnCount;
    public GameObject BossSpawnPostion;

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

    public override void OnNetworkSpawn()
    {
        if (IsClient)
        {
            HideBossHealthUI();
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
            Boss5Health.Instance.IntializeBossHealthServerRpc();
            Boss5.Instance.Active();
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
                BossSpawnPostion.transform.position,
                Quaternion.identity
            );
            bossSpawned.GetComponent<NetworkObject>().Spawn(true);
            Boss5.Instance.Active();
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
