using System.Collections;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance;

    public int killTarget;
    public int enemiesKilled = 0;

    public int EnemiesKilled => enemiesKilled;

    private void Awake()
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

    // Gọi hàm này khi quái bị tiêu diệt
    public void OnEnemyKilled()
    {
        enemiesKilled++;

        if (enemiesKilled >= killTarget)
        {
            DestroyRemainingEnemies();
        }
    }

    // Tiêu diệt các quái còn lại khi đạt chỉ tiêu
    private void DestroyRemainingEnemies()
    {
        GameObject[] remainingEnemies = GameObject.FindGameObjectsWithTag("Enemy");

        foreach (GameObject enemy in remainingEnemies)
        {
            Destroy(enemy);
        }

        Debug.Log("All remaining enemies have been destroyed.");

    }

    public void DecreaseKillTarget()
    {
        if (killTarget > 0)
        {
            killTarget--;
        }
    }
}
