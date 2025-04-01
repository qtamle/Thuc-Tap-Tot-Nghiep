using Unity.Netcode;
using UnityEngine;

public class EnemyManager : NetworkBehaviour
{
    public static EnemyManager Instance;

    public NetworkVariable<int> killTarget = new NetworkVariable<int>(10);
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

        // Khi giá trị thay đổi, gọi ClientRpc để cập nhật UI trên tất cả client
        enemiesKilled.OnValueChanged += (oldValue, newValue) =>
            UpdateKillCounterUIClientRpc(newValue, killTarget.Value);
        killTarget.OnValueChanged += (oldValue, newValue) =>
            UpdateKillCounterUIClientRpc(enemiesKilled.Value, newValue);
    }

    [ClientRpc]
    private void UpdateKillCounterUIClientRpc(int currentKills, int targetKills)
    {
        KillCounterUI.Instance?.UpdateKillCounterUI(currentKills, targetKills);
    }

    [ServerRpc(RequireOwnership = false)]
    private void onEnemyKilledServerRpc()
    {
        enemiesKilled.Value++;
    }

    // Gọi hàm này khi quái bị tiêu diệt
    public void OnEnemyKilled()
    {
        if (!IsServer)
            return; // Chỉ Server mới cập nhật biến
        onEnemyKilledServerRpc();
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
            if (enemy == null) // Kiểm tra enemy có tồn tại không
            {
                continue; // Bỏ qua nếu enemy không tồn tại
            }

            NetworkObject netObj = enemy.GetComponent<NetworkObject>();
            if (netObj == null)
            {
                netObj = enemy.GetComponentInParent<NetworkObject>();
            }
            if (netObj != null && netObj.IsSpawned)
            {
                netObj.Despawn(true); // Hủy trên tất cả client
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
