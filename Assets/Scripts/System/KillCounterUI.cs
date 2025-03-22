using TMPro;
using Unity.Netcode;
using UnityEngine;

public class KillCounterUI : NetworkBehaviour
{
    public static KillCounterUI Instance;
    public TMP_Text killCounterText;
    private NetworkVariable<int> currentKillTarget = new NetworkVariable<int>();
    private NetworkVariable<int> currentEnemiesKilled = new NetworkVariable<int>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    private void Start()
    {
        if (EnemyManager.Instance != null)
        {
            // Đọc giá trị ban đầu
            // Lắng nghe thay đổi từ NetworkVariable
            EnemyManager.Instance.killTarget.OnValueChanged += (oldValue, newValue) =>
                UpdateKillCounterUI(EnemyManager.Instance.enemiesKilled.Value, newValue);
            EnemyManager.Instance.enemiesKilled.OnValueChanged += (oldValue, newValue) =>
                UpdateKillCounterUI(newValue, EnemyManager.Instance.killTarget.Value);
        }

        UpdateKillCounterUIClientRpc();
    }

    public void UpdateKillCounterUI(int currentKills, int targetKills)
    {
        killCounterText.text = $"{targetKills - currentKills}";
    }

    public void CounterUI()
    {
        currentKillTarget.Value = EnemyManager.Instance.killTarget.Value;
        currentEnemiesKilled.Value = EnemyManager.Instance.enemiesKilled.Value;
        UpdateKillCounterUIClientRpc();
    }

    // private void Update()
    // {
    //     if (!IsServer)
    //         return;
    //     if (EnemyManager.Instance != null)
    //     {
    //         if (currentEnemiesKilled.Value != EnemyManager.Instance.enemiesKilled.Value)
    //         {
    //             currentEnemiesKilled.Value = EnemyManager.Instance.enemiesKilled.Value;
    //             UpdateKillCounterUIClientRpc();
    //         }
    //     }
    // }

    private void UpdateKillCounterUI()
    {
        killCounterText.text = $"{currentKillTarget.Value - currentEnemiesKilled.Value}";
    }

    [ClientRpc]
    private void UpdateKillCounterUIClientRpc()
    {
        killCounterText.text = $"{currentKillTarget.Value - currentEnemiesKilled.Value}";
    }
}
