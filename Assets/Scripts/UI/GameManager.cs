using System.Collections;
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

    private Dictionary<ulong, int> playerHealthData = new Dictionary<ulong, int>();
    private Dictionary<ulong, int> playerShieldData = new Dictionary<ulong, int>();

    //Su kien cho gameOver
    public delegate void GameOverHandler(bool isGameOver);
    public event GameOverHandler OnGameOver; // Sự kiện thông báo game over

    private Dictionary<ulong, PlayerHealth> playerHealthComponents =
        new Dictionary<ulong, PlayerHealth>();
    private bool isGameOver = false;

    private int totalPlayers;
    private int deadPlayersCount = 0;

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

    public void PlayerDied(ulong clientId, GameObject gameObject)
    {
        if (isGameOver)
            return; // Nếu game đã kết thúc thì không xử lý tiếp
        gameObject.transform.position = new Vector3(0, 1000, 0);
        Debug.Log("GameManager player die " + clientId);
        deadPlayersCount++;

        // Kiểm tra nếu tất cả người chơi đã chết
        if (deadPlayersCount >= totalPlayers)
        {
            GameOver();
        }
    }

    private void GameOver()
    {
        isGameOver = true;

        // Kích hoạt sự kiện game over
        OnGameOver?.Invoke(true);

        // Chuyển scene sau một khoảng thời gian
        StartCoroutine(LoadSummarySceneAfterDelay(3f)); // 3 giây trước khi chuyển scene
    }

    private IEnumerator LoadSummarySceneAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Chuyển scene
        if (IsServer)
        {
            NetworkManager.Singleton.SceneManager.LoadScene("Summary", LoadSceneMode.Single);
        }
    }

    public void RegisterPlayerHealth(ulong clientId, PlayerHealth playerHealth)
    {
        if (!playerHealthComponents.ContainsKey(clientId))
        {
            playerHealthComponents[clientId] = playerHealth;
            playerHealth.OnHealthChanged += CheckPlayersHealth;
            Debug.Log($"Registered health for player {clientId}");
        }
    }

    public void UnregisterPlayerHealth(ulong clientId)
    {
        if (playerHealthComponents.TryGetValue(clientId, out PlayerHealth playerHealth))
        {
            playerHealth.OnHealthChanged -= CheckPlayersHealth;
            playerHealthComponents.Remove(clientId);
            Debug.Log($"Unregistered health for player {clientId}");
        }
    }

    private void CheckPlayersHealth(ulong clientId, int currentHealth)
    {
        if (!IsServer || isGameOver)
            return;

        bool allPlayersDead = true;
        foreach (var player in playerHealthComponents)
        {
            if (player.Value.currentHealth > 0)
            {
                allPlayersDead = false;
                break;
            }
        }

        if (allPlayersDead)
        {
            isGameOver = true;
            Debug.Log("All players are dead! Game over!");
            OnGameOver?.Invoke(true);

            // Load game over scene after 3 seconds
            StartCoroutine(LoadGameOverSceneAfterDelay(3f));
        }
    }

    private IEnumerator LoadGameOverSceneAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        NetworkManager.Singleton.SceneManager.LoadScene("GameOverScene", LoadSceneMode.Single);
    }

    // Phương thức để chuyển scene
    public void LoadNextScene()
    {
        // Lấy tổng số người chơi khi bắt đầu game
        totalPlayers = NetworkManager.Singleton.ConnectedClients.Count;
        deadPlayersCount = 0;
        if (!NetworkManager.Singleton.IsServer)
            return;

        // Xóa tất cả player cũ trước khi chuyển scene
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            NetworkObject playerObject = client.PlayerObject;
            if (playerObject != null)
            {
                PlayerHealth playerHealth = playerObject.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealthData[client.ClientId] = playerHealth.currentHealth;
                    playerShieldData[client.ClientId] = playerHealth.currentShield;
                }
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
                string bossScene = "Level " + currentBoss.Value + " - Remake";
                // string bossScene = "Level 5 - Remake";

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
        // string bossScene = "Level 1 - Remake";
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

    // Thêm vào GameManager
    public void SavePlayerHealthData(ulong clientId, int health, int shield)
    {
        playerHealthData[clientId] = health;
        playerShieldData[clientId] = shield;
        Debug.Log($"Saved health data for player {clientId}: Health={health}, Shield={shield}");
    }

    public (int health, int shield) GetPlayerHealthData(ulong clientId)
    {
        if (
            playerHealthData.TryGetValue(clientId, out int health)
            && playerShieldData.TryGetValue(clientId, out int shield)
        )
        {
            return (health, shield);
        }
        return (0, 0); // Trả về giá trị mặc định nếu không tìm thấy
    }

    public void ClearHealthData()
    {
        playerHealthData.Clear();
        playerShieldData.Clear();
    }

    // Trong GameManager
    public void ResetGameOver()
    {
        isGameOver = false;
        OnGameOver?.Invoke(false);
    }

    public void ResetGame()
    {
        if (!NetworkManager.Singleton.IsServer)
            return;

        currentBoss.Value = 0;
        isSupplyScene.Value = false;
        ClearHealthData();
        ResetGameOver();
        Debug.Log("Resetting game, loading Boss1");
        NetworkManager.Singleton.Shutdown();
    }
}
