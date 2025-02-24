using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    // Lưu trữ trạng thái Ready của từng người chơi
    private Dictionary<ulong, bool> playerReadyStatus = new Dictionary<ulong, bool>();

    // Lưu trữ vũ khí đã chọn của từng người chơi
    private Dictionary<ulong, WeaponSO> playerWeaponData = new Dictionary<ulong, WeaponSO>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetPlayerReadyStatus(ulong clientId, bool isReady, WeaponSO weaponData)
    {
        Debug.Log(
            $"Setting player {clientId} ready status to {isReady} with weapon {weaponData?.weaponName}"
        );

        // Cập nhật trạng thái Ready và vũ khí đã chọn
        playerReadyStatus[clientId] = isReady;
        if (isReady)
        {
            if (weaponData != null)
            {
                playerWeaponData[clientId] = weaponData;
            }
            else
            {
                Debug.LogError($"Weapon data is null for client {clientId}");
            }
        }
        else
        {
            playerWeaponData.Remove(clientId);
        }

        // Gọi phương thức kiểm tra trạng thái Ready của tất cả người chơi
        CheckAllPlayersReady();
    }

    // Kiểm tra xem tất cả người chơi đã Ready chưa
    private void CheckAllPlayersReady()
    {
        // Chỉ server mới được quyết định chuyển scene
        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("NetworkManager.Singleton is null");
            return;
        }

        if (!NetworkManager.Singleton.IsServer)
            return;

        // Kiểm tra xem tất cả người chơi đã Ready chưa
        if (
            NetworkManager.Singleton.ConnectedClients == null
            || NetworkManager.Singleton.ConnectedClients.Count == 0
        )
        {
            Debug.Log("No connected clients");
            return;
        }

        Debug.Log(
            $"Checking all players ready status. Total players: {NetworkManager.Singleton.ConnectedClients.Count}"
        );
        foreach (var client in NetworkManager.Singleton.ConnectedClients)
        {
            ulong clientId = client.Key;
            Debug.Log(
                $"Player {clientId} ready status: {playerReadyStatus.ContainsKey(clientId) && playerReadyStatus[clientId]}"
            );
            if (!playerReadyStatus.ContainsKey(clientId) || !playerReadyStatus[clientId])
            {
                return; // Nếu có người chơi chưa Ready, thoát khỏi hàm
            }
        }

        // Nếu tất cả người chơi đã Ready, chuyển scene
        LoadNextScene();
    }

    // Phương thức để chuyển scene
    private void LoadNextScene()
    {
        // Chỉ server mới được phép chuyển scene
        if (!NetworkManager.Singleton.IsServer)
            return;

        // Load scene mới
        NetworkManager.Singleton.SceneManager.LoadScene("Test", LoadSceneMode.Single);
    }

    // Phương thức để lấy dữ liệu vũ khí của người chơi
    public WeaponSO GetPlayerWeaponData(ulong clientId)
    {
        if (playerWeaponData.TryGetValue(clientId, out WeaponSO weaponData))
        {
            Debug.Log("Game Manager: " + weaponData.weaponName + " " + weaponData.WeaponID);
            return weaponData;
        }
        return null;
    }
}
