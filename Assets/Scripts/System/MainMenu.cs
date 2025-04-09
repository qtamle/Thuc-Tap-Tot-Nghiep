using System;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField]
    private string SceneName = "Lobby";

    public Animator animator;
    public SnapToWeapon snapToWeapon;
    public GameObject gameManagerPrefab;
    public GameObject weaponInfoPrefab; // Prefab chứa WeaponPlayerInfo
    public TMP_InputField joinCodeInputField; // Input field cho join code

    public Animator SettingAnim;
    public string JoinCode;

    public void ShowSetting()
    {
        SettingAnim.SetTrigger("ShowSetting");
    }

    public void CloseSetting()
    {
        SettingAnim.SetTrigger("CloseSetting");
    }

    public void Signout()
    {
        // Đăng xuất người dùng khỏi dịch vụ xác thực
        AuthenticationService.Instance.SignOut();

        Debug.Log("Đã đăng xuất người dùng.");
        SceneManager.LoadScene("Login", LoadSceneMode.Single);
    }

    public void ToggleMusicOn()
    {
        // Gọi hàm ToggleMusic trong MusicHandler
        MusicHandler.instance.ToggleMusic(true);
    }

    public void ToggleMusicOff()
    {
        // Gọi hàm ToggleMusic trong MusicHandler
        MusicHandler.instance.ToggleMusic(false);
    }

    public void ToggleSfxOn()
    {
        // Gọi hàm ToggleMusic trong MusicHandler
        SFXHandler.instance.ToggleSFX(true);
    }

    public void ToggleSfxOff()
    {
        // Gọi hàm ToggleMusic trong MusicHandler
        SFXHandler.instance.ToggleSFX(false);
    }

    public void Play()
    {
        animator.SetTrigger("Select");
    }

    public void Close()
    {
        animator.SetTrigger("Close");
    }

    public void Online()
    {
        animator.SetTrigger("Online");
    }

    public void Back()
    {
        animator.SetTrigger("Back");
    }

    // Wrapper cho nút bắt đầu Single Player
    public void OnClick_StartSinglePlayer()
    {
        Debug.Log("Single Player Button Clicked");
        // Chỉ đơn giản là gọi phương thức async
        // Không cần await ở đây vì wrapper là void
        StartHost(true);
    }

    // Wrapper cho nút bắt đầu Multiplayer
    public async void OnClick_StartMultiplayer()
    {
        Debug.Log("Multiplayer Button Clicked");
        // StartHost(false);
        await CreateRelay();
    }

    // --- Phương thức async StartHost (giữ nguyên) ---
    public async void StartHost(bool isSingle)
    {
        // Tạm thời tắt nút để tránh click nhiều lần (Tùy chọn)

        if (FadingScript.Instance == null)
        {
            Debug.LogError("FadingScript.Instance is null.");
        }

        // --- Phần còn lại của logic StartHost ---
        if (!isSingle)
        {
            Debug.Log("Starting Host (Multiplayer)...");
            NetworkManager.Singleton.StartHost();

            NetworkManager.Singleton.SceneManager.LoadScene("Lobby", LoadSceneMode.Single);
            // KHÔNG bật lại nút ở đây vì đã chuyển scene
        }
        else
        {
            Debug.Log("Starting Host (Single Player)...");
            WeaponData weaponData = snapToWeapon.currentSnapWeapon;
            NetworkManager.Singleton.StartHost();
            var instance = Instantiate(gameManagerPrefab);
            var instanceNetworkObject = instance.GetComponent<NetworkObject>();
            if (instanceNetworkObject != null)
            {
                instanceNetworkObject.Spawn();
            }
            else
            {
                Debug.LogError("GameManager prefab does not have a NetworkObject component.");
            }
            GetWeaponID();
            CreatePlayersWeaponInfo(NetworkManager.Singleton.LocalClientId, weaponData);
            await GameManagerLoadSceneAsync();
            // KHÔNG bật lại nút ở đây vì đã chuyển scene
        }

        // Lưu ý: Nếu việc chuyển scene thất bại hoặc không xảy ra,
        // bạn cần có cơ chế bật lại nút (ví dụ trong khối catch nếu có xử lý lỗi)
    }

    // Đổi tên và kiểu trả về thành async Task
    public async Task GameManagerLoadSceneAsync()
    {
        if (GameManager.Instance != null)
        {
            Debug.Log("GameManager loading next scene...");
            await GameManager.Instance.LoadNextScene(); // Giả sử LoadNextScene trả về Task hoặc là async
            Debug.Log("GameManager finished loading next scene.");
        }
        else
        {
            Debug.LogError("GameManager.Instance is null.");
            // Có thể ném ngoại lệ hoặc trả về một Task hoàn thành với lỗi
            await Task.CompletedTask;
        }
    }

    public void GetWeaponID()
    {
        if (
            snapToWeapon != null
            && snapToWeapon.currentSnapWeapon != null
            && snapToWeapon.currentSnapWeapon.weaponData != null
        )
        {
            WeaponSO weaponData = snapToWeapon.currentSnapWeapon.weaponData;
            if (!weaponData.isOwned)
            {
                Debug.LogError($"Weapon {weaponData.weaponName} chưa được mua!");
                return;
            } // Cập nhật GameManager
            ulong clientId = NetworkManager.Singleton.LocalClientId;
            bool Isready = true;
            string WeaponID = weaponData.WeaponID;

            GameManager.Instance.SetPlayerReadyStatus(clientId, Isready, WeaponID);
        }
    }

    public void CreatePlayersWeaponInfo(ulong clientId, WeaponData weaponData)
    {
        // Chuyển đổi sang WeaponDataStorage trước khi gửi
        var storage = WeaponDataStorage.FromWeaponData(weaponData);
        CreateWeaponInfoForPlayerServerRpc(clientId, storage);
    }

    public void StartClient()
    {
        NetworkManager.Singleton.StartClient();
    }

    public void StartSever()
    {
        NetworkManager.Singleton.StartServer();
        NetworkManager.Singleton.SceneManager.LoadScene("Lobby", LoadSceneMode.Single);
    }

    [ServerRpc(RequireOwnership = false)]
    private void CreateWeaponInfoForPlayerServerRpc(
        ulong clientId,
        WeaponDataStorage weaponDataStorage
    )
    {
        string weaponName = weaponDataStorage.weaponName;
        int weaponLevel = weaponDataStorage.currentLevel;

        GameObject weaponInfoObj = Instantiate(weaponInfoPrefab);
        NetworkObject networkObject = weaponInfoObj.GetComponent<NetworkObject>();

        networkObject.SpawnWithOwnership(clientId, false);
        weaponInfoObj.GetComponent<WeaponPlayerInfo>().SetWeaponInfo(weaponName, weaponLevel);
    }

    // Wrapper cho nút tham gia Multiplayer (Client)
    public async void OnClick_JoinMultiplayer()
    {
        Debug.Log("Joining Multiplayer...");
        string joinCodeInput = joinCodeInputField.text; // Lấy join code từ input field
        await JoinRelay(joinCodeInput);
    }

    private async Task CreateRelay()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(2);

            // Hiển thị joinCode cho người chơi chủ phòng để chia sẻ
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            RelayManager.Instance.SetJoinCode(joinCode);

            Debug.Log("Relay created with join code: " + joinCode);

            // Bạn có thể hiển thị nó trên UI hoặc log nó ra console

            NetworkManager
                .Singleton.GetComponent<UnityTransport>()
                .SetHostRelayData(
                    allocation.RelayServer.IpV4,
                    (ushort)allocation.RelayServer.Port,
                    allocation.AllocationIdBytes,
                    allocation.Key,
                    allocation.ConnectionData
                );

            StartHost(false);
        }
        catch (RelayServiceException e)
        {
            Debug.LogError("Relay creation failed: " + e.Message);
        }
    }

    // Tham gia Relay
    private async Task JoinRelay(string joinCode)
    {
        try
        {
            Debug.Log("Attempting to join Relay with code: " + joinCode);
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(
                joinCode
            );

            NetworkManager
                .Singleton.GetComponent<UnityTransport>()
                .SetClientRelayData(
                    joinAllocation.RelayServer.IpV4,
                    (ushort)joinAllocation.RelayServer.Port,
                    joinAllocation.AllocationIdBytes,
                    joinAllocation.Key,
                    joinAllocation.ConnectionData,
                    joinAllocation.HostConnectionData
                );

            StartClient();
        }
        catch (RelayServiceException e)
        {
            Debug.LogError("Failed to join Relay: " + e.Message);
        }
    }
}
