using TMPro;
using Unity.Netcode;
using UnityEngine;

public class CountdownUI : NetworkBehaviour
{
    public TextMeshProUGUI countdownText; // UI hiển thị countdown
    public GameObject countdownPanel; // Panel chứa countdown

    private float countdownTime = 3f; // Bắt đầu từ 3 giây
    private bool countdownStarted = false;

    public override void OnNetworkSpawn()
    {
        if (GameManager.Instance != null && IsServer) // Chỉ server mới được lắng nghe sự kiện
        {
            GameManager.Instance.OnCountdownStart += StartCountdownServerRpc; // Lắng nghe sự kiện
        }
    }

    private void Update()
    {
        if (!countdownStarted)
            return;

        countdownTime -= Time.deltaTime;
        countdownPanel.SetActive(true);

        if (countdownTime > 0)
        {
            countdownText.text = $"{Mathf.Ceil(countdownTime)}";
        }
        else
        {
            countdownText.text = "Trận đấu bắt đầu!";
            countdownStarted = false;
            Invoke(nameof(HidePanel), 1f); // Ẩn sau 1 giây
        }
    }

    private void HidePanel()
    {
        countdownPanel.SetActive(false);
        if (IsServer)
        {
            GameManager.Instance.LoadNextScene();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void StartCountdownServerRpc()
    {
        StartCountdownClientRpc(3f);
    }

    [ClientRpc]
    private void StartCountdownClientRpc(float startTime)
    {
        countdownTime = startTime;
        countdownStarted = true;
    }
}
