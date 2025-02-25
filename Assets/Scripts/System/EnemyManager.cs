using Unity.Netcode;
using UnityEngine;

public class EnemyManager : NetworkBehaviour
{
    public static EnemyManager Instance;

    public NetworkVariable<int> killTarget = new NetworkVariable<int>();
    public NetworkVariable<int> enemiesKilled = new NetworkVariable<int>();

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

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
            return; // Chỉ Server quản lý việc tiêu diệt quái

        enemiesKilled.Value = 0;
    }

    // Gọi hàm này khi quái bị tiêu diệt
    public void OnEnemyKilled()
    {
        if (!IsServer)
            return; // Chỉ Server mới cập nhật biến

        enemiesKilled.Value++;

        if (enemiesKilled.Value >= killTarget.Value)
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
            NetworkObject netObj = enemy.GetComponent<NetworkObject>();
            if (netObj != null)
            {
                netObj.Despawn(); // Hủy trên tất cả client
            }
            else
            {
                Destroy(enemy);
            }
        }

        Debug.Log("All remaining enemies have been destroyed.");
    }

    public void DecreaseKillTarget()
    {
        if (!IsServer)
            return; // Chỉ Server có quyền thay đổi killTarget

        if (killTarget.Value > 0)
        {
            killTarget.Value--;
        }
    }
}
