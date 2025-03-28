using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : NetworkBehaviour
{
    public delegate void CountdownStartHandler();
    public event CountdownStartHandler OnCountdownStart; // Sự kiện gọi đến CountdownUI
    public static GameManager Instance;
    private Dictionary<ulong, bool> playerReadyStatus = new Dictionary<ulong, bool>();

    private Dictionary<ulong, string> playerWeaponIDs = new Dictionary<ulong, string>();
    private Dictionary<ulong, int> playersClientId = new Dictionary<ulong, int>();

    public NetworkVariable<int> currentBoss;

    public NetworkVariable<bool> isSupplyScene = new NetworkVariable<bool>(false);

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            currentBoss = new NetworkVariable<int>(0);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    public override void OnNetworkSpawn() { }

    public void SetPlayerReadyStatus(ulong clientId, bool isReady, string weaponID)
    {
        Debug.Log($"Setting player {clientId} ready status to {isReady} with weaponID {weaponID}");

        playerReadyStatus[clientId] = isReady;
        if (isReady)
        {
            playerWeaponIDs[clientId] = weaponID; // Chỉ lưu WeaponID
        }
        else
        {
            playerWeaponIDs.Remove(clientId);
        }

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
        // Chỉ gọi Invoke khi tất cả người chơi đều đã Ready
        Debug.Log("All players ready! Starting countdown...");
        OnCountdownStart?.Invoke();
    }

    // Phương thức để chuyển scene
    public void LoadNextScene()
    {
        if (!NetworkManager.Singleton.IsServer)
            return;

        // Xóa tất cả player cũ trước khi chuyển scene
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            NetworkObject playerObject = client.PlayerObject;
            if (playerObject != null)
            {
                playerObject.Despawn(false);
            }
        }

        if (isSupplyScene.Value == false) // Nếu đang ở màn Supply thì chuyển sang Boss tiếp theo
        {
            isSupplyScene.Value = true;

            currentBoss.Value++;
            if (SupplyManager.Instance != null)
            {
                SupplyManager.Instance.DestroySpawnedSupplies();
            }
            if (currentBoss.Value == 5)
            {
                isSupplyScene.Value = false;
            }
            if (currentBoss.Value <= 5)
            {
                string bossScene = "Level " + currentBoss.Value;
                Debug.Log($"Loading {bossScene}");
                NetworkManager.Singleton.SceneManager.LoadScene(bossScene, LoadSceneMode.Single);
            }
            if (currentBoss.Value > 5)
            {
                Debug.Log("Loading SummaryScene");
                NetworkManager.Singleton.SceneManager.LoadScene("Summary", LoadSceneMode.Single);
                ResetGame();
            }
        }
        else
        {
            isSupplyScene.Value = false;
            Debug.Log("Loading SupplyScene");

            NetworkManager.Singleton.SceneManager.OnLoadComplete += OnSupplySceneLoaded;
            NetworkManager.Singleton.SceneManager.LoadScene("SupplyScene", LoadSceneMode.Single);
        }
        // string bossScene = "Level 4";
        // NetworkManager.Singleton.SceneManager.LoadScene(bossScene, LoadSceneMode.Single);
    }

    private void OnSupplySceneLoaded(ulong clientId, string sceneName, LoadSceneMode mode)
    {
        if (sceneName == "SupplyScene")
        {
            Debug.Log("SupplyScene Loaded! Initializing slots...");
            SupplyManager.Instance.InitializeSlots();

            // Hủy đăng ký sự kiện để tránh bị gọi nhiều lần
            NetworkManager.Singleton.SceneManager.OnLoadComplete -= OnSupplySceneLoaded;
        }
    }

    // Phương thức để lấy dữ liệu vũ khí của người chơi
    public string GetPlayerWeaponID(ulong clientId)
    {
        if (playerWeaponIDs.TryGetValue(clientId, out string weaponID))
        {
            return weaponID;
        }
        return null;
    }

    public Dictionary<ulong, int> getPlayersID()
    {
        return new Dictionary<ulong, int>(playersClientId);
    }

    public Dictionary<ulong, string> GetAllPlayerWeaponIDs()
    {
        return new Dictionary<ulong, string>(playerWeaponIDs);
    }

    public void ResetGame()
    {
        if (!NetworkManager.Singleton.IsServer)
            return;

        currentBoss.Value = 0;
        isSupplyScene.Value = false;
        Debug.Log("Resetting game, loading Boss1");
        NetworkManager.Singleton.Shutdown();
    }
}
