using System.Collections;
using UnityEngine;

public class EnemySpawnerLevel2 : MonoBehaviour
{
    public GameObject[] enemies;
    public Transform[] spawnPoints;
    public Transform[] element1SpawnPoints;
    public Transform[] element2SpawnPoints; 
    public float spawnInterval = 2f;
    public float spawnHeightOffset = 1f;

    private bool stopSpawning = false;

    [Header("Boss Spawn")]
    public GameObject warningBoss;
    public GameObject bossLevel1;
    public GameObject UIHealthBoss;

    [Header("Hide")]
    public GameObject remain;

    public AssassinBossSkill Assassin;

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

    IEnumerator SpawnEnemies()
    {
        yield return new WaitForSeconds(2f);

        while (!stopSpawning)
        {
            Transform spawnPoint;
            GameObject enemy;

            // Spawn quái theo tỷ lệ phần trăm
            float randomValue = Random.Range(0f, 100f);

            if (randomValue < 40f)  // 40% 
            {
                spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
                enemy = enemies[0];
            }
            else if (randomValue < 80f)  // 40%
            {
                spawnPoint = element1SpawnPoints[Random.Range(0, element1SpawnPoints.Length)];
                enemy = enemies[1];
            }
            else  // 20%
            {
                spawnPoint = element2SpawnPoints[Random.Range(0, element2SpawnPoints.Length)];
                enemy = enemies[2];
            }

            Vector3 spawnPosition = spawnPoint.position + new Vector3(0, spawnHeightOffset, 0);

            GameObject spawnedEnemy = Instantiate(enemy, spawnPosition, Quaternion.identity);

            // Đặt tag hoặc layer để nhận diện là Enemy (nếu chưa có)
            if (!spawnedEnemy.CompareTag("Enemy"))
            {
                spawnedEnemy.tag = "Enemy";
            }

            AdjustSpawnInterval();

            yield return new WaitForSeconds(spawnInterval);

            // Kiểm tra nếu đạt chỉ tiêu
            if (EnemyManager.Instance != null && EnemyManager.Instance.killTarget <= EnemyManager.Instance.enemiesKilled)
            {
                stopSpawning = true;
                StartCoroutine(HandleBossSpawn());
            }
        }
    }

    private void AdjustSpawnInterval()
    {
        if (EnemyManager.Instance != null)
        {
            int enemiesKilled = EnemyManager.Instance.enemiesKilled;

            if (enemiesKilled >= 30 && spawnInterval > 1f)
            {
                spawnInterval -= 0.3f;
                Debug.Log($"Spawn interval decreased to: {spawnInterval} seconds.");
            }
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
            Assassin.Active();
        }

        yield return new WaitForSeconds(0.5f);

        if (UIHealthBoss != null)
        {
            UIHealthBoss.SetActive(true);
        }
    }
}
