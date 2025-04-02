using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class EnemySpawnerLevel6 : NetworkBehaviour, IEnemySpawner
{
    [Header("Enemy Data")]
    public EnemySpawnData[] enemySpawnDatas;

    [Header("Show UI")]
    public GameObject warningBoss;
    public GameObject bossLevel1;
    public GameObject UIHealthBoss;

    [Header("Hide UI")]
    public GameObject remain;

    [Header("Boss Level 6 Script")]
    public CaptainSkill captainBoss;
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

    [Header("Enemy Select")]
    public int numberOfPicks;
    private void Start()
    {
        if (!IsServer)
            return; // Chỉ server mới spawn quái
        else
        {
            EnemyManager.Instance.killTarget.Value = 2;
            // KillCounterUI.Instance.CounterUI();
            BossSpawnPostion = GameObject.FindWithTag("BossSpawner");

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

                if (!spawnedEnemy.CompareTag("Enemy"))
                {
                    spawnedEnemy.tag = "Enemy";
                }
                spawnedEnemy.GetComponent<NetworkObject>().Spawn(true);
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
        // Chỉ gọi ServerRpc nếu là Client (tránh gọi thừa khi đang là Server)
        if (!IsServer)
        {
            OnEnemyKilledServerRpc();
        }
        // Nếu là Server, xử lý trực tiếp
        else
        {
            currentTotalSpawnCount.Value--;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void OnEnemyKilledServerRpc()
    {
        currentTotalSpawnCount.Value--;
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
        ShowBossHealthUI();
        if (bossLevel1 != null)
        {
            GameObject bossSpawned = Instantiate(
                bossLevel1,
                BossSpawnPostion.transform.position,
                Quaternion.identity
            );
            bossSpawned.GetComponent<NetworkObject>().Spawn(true);
            CaptainHealth.Instance.IntializeBossHealthServerRpc();
            CaptainSkill.Instance.Active();
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
            CaptainHealth.Instance.IntializeBossHealthServerRpc();

            CaptainSkill.Instance.Active();
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
