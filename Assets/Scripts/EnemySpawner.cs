using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject[] enemies;
    public Transform[] spawnPoints;
    public Transform[] element1SpawnPoints;
    public float spawnInterval = 2f;
    public float spawnHeightOffset = 1f;

    private void Start()
    {
        // Start spawning enemies
        StartCoroutine(SpawnEnemies());
    }

    IEnumerator SpawnEnemies()
    {
        while (true)
        {
            Transform spawnPoint;
            GameObject enemy;

            if (Random.Range(0, 2) == 0)
            {
                spawnPoint = element1SpawnPoints[Random.Range(0, element1SpawnPoints.Length)];
                enemy = enemies[Random.Range(0, enemies.Length)];
            }
            else
            {
                spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
                enemy = enemies[Random.Range(0, enemies.Length)];
            }

            Vector3 spawnPosition = spawnPoint.position + new Vector3(0, spawnHeightOffset, 0);

            Instantiate(enemy, spawnPosition, Quaternion.identity);

            yield return new WaitForSeconds(spawnInterval);
        }
    }
}
