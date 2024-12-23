using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class EnemySpawnerOnline : NetworkBehaviour
{
    public GameObject[] enemies;
    public Transform[] spawnPoints;
    public Transform[] element1SpawnPoints;
    public float spawnInterval = 2f;
    public float spawnHeightOffset = 1f;

    private bool stopSpawning = false;

    public Button SpamBTN;

    [Header("Boss Spawn")]
    public Transform spawnPointsBoss;

    public GameObject warningBoss;
    public GameObject bossLevel1;
    public GameObject UIHealthBoss;

    [Header("Hide")]
    public GameObject remain;

    public GangsterOnline gangster;
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

    }

    public void StartSpawning()
    {
        if (IsServer && IsHost)
        {
            StartCoroutine(SpawnEnemies());
        }
    }
    IEnumerator SpawnEnemies()
    {
        yield return new WaitForSeconds(2f);

        while (!stopSpawning)
        {
            Transform spawnPoint;
            GameObject enemy;

            // Kiểm tra nếu đạt chỉ tiêu
            if (EnemyManager.Instance != null && EnemyManager.Instance.killTarget <= EnemyManager.Instance.enemiesKilled)
            {
                Debug.Log($"KillTarget: {EnemyManager.Instance.killTarget}, EnemiesKilled: {EnemyManager.Instance.enemiesKilled}");

                stopSpawning = true;
                StartCoroutine(HandleBossSpawn());
            }

            // Chọn spawn point và quái ngẫu nhiên
            if (Random.Range(0, 2) == 0)
            {
                spawnPoint = element1SpawnPoints[Random.Range(0, element1SpawnPoints.Length)];
            }
            else
            {
                spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            }

            enemy = enemies[Random.Range(0, enemies.Length)];
            Vector3 spawnPosition = spawnPoint.position + new Vector3(0, spawnHeightOffset, 0);

            // Spawn quái
            GameObject spawnedEnemy = Instantiate(enemy, spawnPosition, Quaternion.identity);

            spawnedEnemy.GetComponent<NetworkObject>().Spawn();
            // Đặt tag hoặc layer để nhận diện là Enemy (nếu chưa có)
            if (!spawnedEnemy.CompareTag("Enemy"))
            {
                spawnedEnemy.tag = "Enemy";
            }

            AdjustSpawnInterval();

            yield return new WaitForSeconds(spawnInterval);

            
        }
    }

    [ServerRpc]
    public void RequestStartSpawningServerRpc()
    {
        StartCoroutine(SpawnEnemies()); // Chỉ server bắt đầu spawn
        NotifyClientsToStartClientRpc(); // Thông báo cho tất cả client
    }

    [ClientRpc]
    private void NotifyClientsToStartClientRpc()
    {
        // Thực hiện các hiệu ứng hoặc cập nhật UI nếu cần trên client
        Debug.Log("Spawning started!");
    }

    private void AdjustSpawnInterval()
    {
        if (EnemyManager.Instance != null)
        {
            int enemiesKilled = EnemyManager.Instance.enemiesKilled;

            // Nếu đã tiêu diệt >= 30 quái, giảm spawnInterval
            if (enemiesKilled >= 30 && spawnInterval > 1f)
            {
                spawnInterval -= 0.2f;
                Debug.Log($"Spawn interval decreased to: {spawnInterval} seconds.");
            }
        }
    }

    private IEnumerator HandleBossSpawn()
    {
        Debug.Log("Handle Boss Spawn");
        // Đồng bộ ẩn UI "remain" và hiển thị cảnh báo boss
        NotifyClientsBossSpawnClientRpc();

        yield return new WaitForSeconds(3f);

        // Đồng bộ kích hoạt boss và UI của boss
        NotifyClientsBossActiveClientRpc();

        // Kích hoạt logic của boss trên server
        //if (IsServer && gangster != null)
        //{
        //    Debug.Log("Active boss từ Spam");
        //    gangster.Activate();
        //}
        // Kiểm tra biến gangster và spawnPointsBoss để tránh lỗi null
        if (gangster != null && spawnPointsBoss != null)
        {
            // Tính toán vị trí spawn từ spawnPointsBoss
            Vector3 spawnPosition = spawnPointsBoss.position;

            // Tạo một GangsterOnline tại vị trí spawn
            GameObject spawnedBoss = Instantiate(gangster.gameObject, spawnPosition, Quaternion.identity);

            // Nếu dùng Netcode, spawn boss qua mạng
            if (spawnedBoss.TryGetComponent<NetworkObject>(out NetworkObject networkObject))
            {
                networkObject.Spawn();
                
            }

            Debug.Log("Boss đã được spawn tại vị trí: " + spawnPosition);
            //ActiveBoss
            
        }
        else
        {
            Debug.LogWarning("gangster hoặc spawnPointsBoss chưa được gán trong Inspector.");
        }
    }
    [ClientRpc]
    private void NotifyClientsBossSpawnClientRpc()
    {
        if (remain != null)
        {
            remain.SetActive(false);
        }

        if (warningBoss != null)
        {
            warningBoss.SetActive(true);
        }
    }

    [ClientRpc]
    private void NotifyClientsBossActiveClientRpc()
    {
        if (warningBoss != null)
        {
            warningBoss.SetActive(false);
        }

        if (bossLevel1 != null)
        {
            bossLevel1.SetActive(true);
        }

        if (UIHealthBoss != null)
        {
            UIHealthBoss.SetActive(true);
        }
    }

}
