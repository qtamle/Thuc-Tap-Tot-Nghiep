using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class EnemySpawner : NetworkBehaviour, IEnemySpawner
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
    public Gangster gangster;

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

        foreach (var spawnData in enemySpawnDatas)
        {
            StartCoroutine(SpawnEnemyIndependently(spawnData));
        }
    }

    void Update()
    {
        if (!IsServer)
            return; // Chỉ server mới điều chỉnh tốc độ spawn
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

        while (stopSpawning.Value == false)
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
            gangster.Active();
        }

        yield return new WaitForSeconds(0.5f);

        if (UIHealthBoss != null)
        {
            UIHealthBoss.SetActive(true);
        }
    }
}

[System.Serializable]
public class EnemySpawnData
{
    public GameObject enemyPrefab;
    public Transform[] spawnPoints;
    public float minSpawnTime = 1f;
    public float maxSpawnTime = 3f;
}
