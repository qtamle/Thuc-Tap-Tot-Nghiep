using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class WeaponSelectDisplay : NetworkBehaviour
{
    public static WeaponSelectDisplay Instance { get; private set; }

    //Network
    private NetworkList<WeaponSelectState> players;

    [SerializeField]
    private PlayerSlot[] playerSlots;

    // UI
    public ScrollRect scrollRect;
    public RectTransform contentWeaponPanel;
    public RectTransform sampleListItem;
    public HorizontalLayoutGroup HLG;

    bool isSnapped;
    public float snapForce;
    float snapSpeed;

    // Action
    public event Action<WeaponData> OnSnapChanged;
    public event Action OnWeaponSelected;

    public string[] ItemNames;

    // Button
    public Button nextButton;
    public Button previousButton;
    public Button SelectButton;

    public Button readyButton;
    public Button unreadyButton;

    private int currentItem = 0;
    private float snapDuration = 0.5f;

    // Current Weapon
    public WeaponData currentSnapWeapon;

    public GameObject PlayerSlotsDisplay;

    [SerializeField]
    public GameObject weaponInfoPrefab; // Prefab chứa WeaponPlayerInfo

    private void Awake()
    {
        players = new NetworkList<WeaponSelectState>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsClient)
        {
            players.OnListChanged += HandlePlayerStateChanged;
        }
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnected;

            foreach (NetworkClient client in NetworkManager.Singleton.ConnectedClientsList)
            {
                HandleClientConnected(client.ClientId);
            }
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsClient)
        {
            players.OnListChanged -= HandlePlayerStateChanged;
        }
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnected;
        }
    }

    private void HandleClientConnected(ulong clientId)
    {
        players.Add(new WeaponSelectState(clientId, new FixedString64Bytes("1"), false));
    }

    private void HandleClientDisconnected(ulong clientId)
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].ClientId == clientId)
            {
                players.RemoveAt(i);
                break;
            }
        }
    }

    private void HandlePlayerStateChanged(NetworkListEvent<WeaponSelectState> changeEvent)
    {
        for (int i = 0; i < playerSlots.Length; i++)
        {
            if (players.Count > i)
            {
                Debug.Log("HandlePlayerStateChanged: player " + players[i].ClientId);
                Debug.Log("HandlePlayerStateChanged: WeaponID " + players[i].WeaponID);
                Debug.Log("HandlePlayerStateChanged: Ready " + players[i].IsReady);

                playerSlots[i].UpdatePlayerSlotDisplay(players[i]);

                // Cập nhật GameManager
                GameManager.Instance.SetPlayerReadyStatus(
                    players[i].ClientId,
                    players[i].IsReady,
                    players[i].WeaponID.ToString()
                );
            }
            else
            {
                playerSlots[i].DisableDisplay();
            }
        }
    }

    void Start()
    {
        isSnapped = false;

        InitializeWeaponNames();

        // Đăng ký sự kiện cho các nút
        nextButton.onClick.AddListener(NextWeapon);
        previousButton.onClick.AddListener(PreviousWeapon);
        SelectButton.onClick.AddListener(OnSelectButtonClicked);
        readyButton.onClick.AddListener(OnReadyButtonClicked);
        unreadyButton.onClick.AddListener(OnUnreadyButtonClicked);
    }

    public void OnReadyButtonClicked()
    {
        ulong clientId = NetworkManager.Singleton.LocalClientId;
        // Gọi hàm kiểm tra và *lưu trữ* kết quả trả về
        bool hasWeapon = CheckHasOwnWeapon(currentSnapWeapon); // Ready(clientId, currentSnapWeapon);

        if (hasWeapon)
        {
            Ready(clientId, currentSnapWeapon);
        }
        else
        {
            CountdownUI countDown = GameObject.FindFirstObjectByType<CountdownUI>();
            if (countDown != null)
            {
                countDown.ShowNoWeaponWarning("You do not own this weapon. Please select another weapon.");
                Debug.Log("Player does not have own weapon: " + currentSnapWeapon.weaponName);
            }
            else
            {
                Debug.LogError("Loi");
            }
        }
    }


    public void OnUnreadyButtonClicked()
    {
        ulong clientId = NetworkManager.Singleton.LocalClientId;
        Unready(clientId, currentSnapWeapon);
    }

    void Update()
    {
        // Tính toán chỉ số item hiện tại
        currentItem = Mathf.RoundToInt(
            (0 - contentWeaponPanel.localPosition.x) / (sampleListItem.rect.width + HLG.spacing)
        );

        // Đảm bảo chỉ số item hợp lệ
        currentItem = Mathf.Clamp(currentItem, 0, ItemNames.Length - 1);

        // Debug tên vũ khí hiện tại
        // Debug.Log($"Snapped to Weapon: {ItemNames[currentItem]}");

        // Nếu tốc độ kéo nhỏ và chưa snap
        if (scrollRect.velocity.magnitude < 200 && !isSnapped)
        {
            float targetPositionX = 0 - (currentItem * (sampleListItem.rect.width + HLG.spacing));
            snapSpeed += snapForce * Time.deltaTime;

            // Di chuyển dần về vị trí mục tiêu
            contentWeaponPanel.localPosition = new Vector3(
                Mathf.MoveTowards(contentWeaponPanel.localPosition.x, targetPositionX, snapSpeed),
                contentWeaponPanel.localPosition.y,
                contentWeaponPanel.localPosition.z
            );

            // Kiểm tra vị trí đã đạt mục tiêu (dùng epsilon thay vì so sánh trực tiếp)
            if (Mathf.Abs(contentWeaponPanel.localPosition.x - targetPositionX) < 0.1f)
            {
                isSnapped = true;
                snapSpeed = 0;

                // Gọi sự kiện thông báo vũ khí đã snap
                WeaponData snappedWeaponData = GetWeaponData(currentItem);
                NotifySnapChanged(snappedWeaponData);
            }
        }

        if (scrollRect.velocity.magnitude > 200)
        {
            isSnapped = false;
            snapSpeed = 0;
        }
    }

    private void InitializeWeaponNames()
    {
        List<string> weaponNames = new List<string>();
        foreach (Transform child in contentWeaponPanel)
        {
            WeaponData weaponData = child.GetComponent<WeaponData>();
            if (weaponData != null)
            {
                weaponNames.Add(weaponData.weaponName);
            }
        }
        ItemNames = weaponNames.ToArray();
    }

    // Hàm để di chuyển đến vũ khí kế tiếp
    public void NextWeapon()
    {
        if (currentItem < ItemNames.Length - 1 && !isSnapped)
        {
            currentItem++;
            SnapToCurrentItem();
        }
    }

    // Hàm để di chuyển đến vũ khí trước đó
    public void PreviousWeapon()
    {
        if (currentItem > 0 && !isSnapped)
        {
            currentItem--;
            SnapToCurrentItem();
        }
    }

    public bool CheckHasOwnWeapon(WeaponData weaponData)
    {
        if (weaponData.isOwned == true)
        {
            Debug.Log("Player has own weapon: " + weaponData.weaponName);
            return true;
        }
        else
        {
            Debug.Log("Player does not have own weapon: " + weaponData.weaponName);
            return false;
        }
    }

    private void Ready(ulong clientId, WeaponData weaponData)
    {
        if (weaponData != null && weaponData.weaponData != null)
        {
            SetPlayerReadyServerRpc(clientId, true, weaponData.weaponData.WeaponID);
            // GameManager.Instance.SetPlayerReadyStatus(clientId, true, weaponData.weaponData);
            CreatePlayersWeaponInfo(clientId, weaponData);
        }
        else
        {
            Debug.LogError("WeaponData is null or weaponData.weaponData is null");
        }
    }

    public void CreatePlayersWeaponInfo(ulong clientId, WeaponData weaponData)
    {
        // Chuyển đổi sang WeaponDataStorage trước khi gửi
        var storage = WeaponDataStorage.FromWeaponData(weaponData);
        CreateWeaponInfoForPlayerServerRpc(clientId, storage);
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

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerReadyServerRpc(
        ulong clientId,
        bool isReady,
        FixedString64Bytes weaponId,
        ServerRpcParams serverRpcParams = default
    )
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].ClientId == clientId)
            {
                var updatedPlayerState = new WeaponSelectState(
                    players[i].ClientId,
                    weaponId,
                    isReady
                );
                players[i] = updatedPlayerState;
                Debug.Log($"Player {clientId} ready status updated to {isReady}");
                break; // Thêm break để thoát khỏi vòng lặp sau khi cập nhật
            }
        }
    }

    private void Unready(ulong clientId, WeaponData weaponData)
    {
        SetPlayerReadyServerRpc(clientId, false, new FixedString64Bytes("0"));
        // GameManager.Instance.SetPlayerReadyStatus(clientId, false, null);
    }

    // Hàm thực hiện việc "snap" tới vị trí của vũ khí hiện tại
    private void SnapToCurrentItem()
    {
        isSnapped = true;
        float targetPositionX = 0 - (currentItem * (sampleListItem.rect.width + HLG.spacing));

        // Sử dụng LeanTween để di chuyển mượt mà
        LeanTween
            .moveLocalX(contentWeaponPanel.gameObject, targetPositionX, snapDuration)
            .setEase(LeanTweenType.easeInOutQuad);

        WeaponData snappedWeaponData = GetWeaponData(currentItem);
        NotifySnapChanged(snappedWeaponData);
    }

    private void NotifySnapChanged(WeaponData weaponData)
    {
        if (weaponData == null)
        {
            Debug.LogError("❌ NotifySnapChanged: weaponData is NULL!");
            return;
        }

        currentSnapWeapon = weaponData;
        ;
        if (currentSnapWeapon.weaponData == null)
        {
            Debug.LogError("⚠️ weaponData của vũ khí hiện tại bị NULL sau khi snap!");
        }

        // Debug.Log($"✅ NotifySnapChanged: Current weapon set to {weaponData.weaponName}");
        OnSnapChanged?.Invoke(weaponData);
        isSnapped = false;
    }

    private WeaponData GetWeaponData(int index)
    {
        // Tìm WeaponData từ danh sách các GameObject con
        Transform weaponTransform = contentWeaponPanel.GetChild(index);
        return weaponTransform.GetComponent<WeaponData>();
    }

    private void OnSelectButtonClicked()
    {
        ulong clientId = NetworkManager.Singleton.LocalClientId;
        SelectWeapon(currentSnapWeapon, clientId);
    }

    public void SelectWeapon(WeaponData weapon, ulong clientId)
    {
        currentSnapWeapon = weapon;
        OnWeaponSnapped(weapon);
        OnWeaponSelected?.Invoke();
        FixedString64Bytes fixedString = weapon.weaponData.WeaponID;
        SelectWeaponServerRpc(clientId, fixedString);
        Debug.Log($"Weapon selected: {weapon.weaponName}");
    }

    [ServerRpc(RequireOwnership = false)]
    private void SelectWeaponServerRpc(
        ulong clientId,
        FixedString64Bytes weaponId,
        ServerRpcParams serverRpcParams = default
    )
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].ClientId == clientId)
            {
                players[i] = new WeaponSelectState(
                    players[i].ClientId,
                    weaponId,
                    players[i].IsReady
                );
            }
        }
    }

    private void OnWeaponSnapped(WeaponData snappedWeapon)
    {
        if (snappedWeapon == null)
        {
            Debug.LogError("❌ OnWeaponSnapped: snappedWeapon is NULL!");
            return;
        }

        string weaponID = snappedWeapon.weaponData.WeaponID;
    }
}

public struct WeaponDataStorage : INetworkSerializable
{
    public string weaponName;
    public int currentLevel;

    // Add other fields you need

    public void NetworkSerialize<T>(BufferSerializer<T> serializer)
        where T : IReaderWriter
    {
        serializer.SerializeValue(ref weaponName);
        serializer.SerializeValue(ref currentLevel);
        // Serialize other fields
    }

    // Chuyển từ WeaponData thông thường sang WeaponDataStorage
    public static WeaponDataStorage FromWeaponData(WeaponData data)
    {
        return new WeaponDataStorage
        {
            weaponName = data.weaponName,
            currentLevel = data.currentLevel,
        };
    }
}
