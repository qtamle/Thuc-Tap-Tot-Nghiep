﻿using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject[] enemies;
    public Transform[] spawnPoints;
    public Transform[] element1SpawnPoints;
    public float spawnInterval = 2f;
    public float spawnHeightOffset = 1f;

    private bool stopSpawning = false;

    private void Start()
    {
        StartCoroutine(SpawnEnemies());
    }

    IEnumerator SpawnEnemies()
    {
        while (!stopSpawning)
        {
            Transform spawnPoint;
            GameObject enemy;

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

            // Đặt tag hoặc layer để nhận diện là Enemy (nếu chưa có)
            if (!spawnedEnemy.CompareTag("Enemy"))
            {
                spawnedEnemy.tag = "Enemy";
            }

            AdjustSpawnInterval();

            yield return new WaitForSeconds(spawnInterval);

            // Kiểm tra nếu đạt chỉ tiêu
            if (EnemyManager.Instance != null && EnemyManager.Instance.killTarget <= EnemyManager.Instance.EnemiesKilled)
            {
                stopSpawning = true;
            }
        }
    }

    private void AdjustSpawnInterval()
    {
        if (EnemyManager.Instance != null)
        {
            int enemiesKilled = EnemyManager.Instance.EnemiesKilled;

            // Nếu đã tiêu diệt >= 30 quái, giảm spawnInterval
            if (enemiesKilled >= 30 && spawnInterval > 1f)
            {
                spawnInterval -= 0.2f; 
                Debug.Log($"Spawn interval decreased to: {spawnInterval} seconds.");
            }
        }
    }
}