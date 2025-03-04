using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Unity.Collections;
using Unity.Netcode;
using Unity.Services.Matchmaker.Models;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class SupplyManager : NetworkBehaviour
{
    public static SupplyManager Instance;

    [SerializeField]
    private List<SupplyData> supplyDataList;

    public NetworkList<SupplyDataNetwork> networkSupplyList; // Danh sách đồng bộ

    [SerializeField]
    private Transform supplySlot1;

    [SerializeField]
    private Transform supplySlot2;

    [SerializeField]
    private Transform supplySlot3;

    private List<Transform> slots;

    private List<GameObject> spawnedSupplies = new List<GameObject>();

    // Khai báo delegate cho event
    public delegate void InventoryChangeHandler(SupplyData supply);

    // Khai báo event
    public event InventoryChangeHandler OnInventoryChanged;

    [SerializeField]
    private bool isUsingSupply;

    [SerializeField]
    private float interval = 10f; // Khoảng thời gian giữa các lần gọi (tính bằng giây)
    private Coroutine useSupplyCoroutine;

    public bool hasInitialized = false;

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
        // Khởi tạo NetworkList
        networkSupplyList = new NetworkList<SupplyDataNetwork>();
    }

    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("Scene loaded: " + scene.name);
        if (!hasInitialized && IsServer && scene.name == "SupplyScene")
        {
            InitializeSlots();
            hasInitialized = true;
            Debug.Log("Slots initialized.");
        }
        else
        {
            ResetInitialization();
        }
    }

    public void ResetInitialization()
    {
        hasInitialized = false;
        
    }

    // void Start()
    // {
    //     DontDestroyOnLoad(gameObject);
    // }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            

            InitializeSlots();
            Debug.Log("Server của Supply Manager được khởi động.");
            InitializeNetworkSupplyList();
        }
    }

    public override void OnNetworkDespawn()
    {
    }

    private void InitializeNetworkSupplyList()
    {
        // Duyệt qua supplyDataList và thêm từng phần tử vào networkSupplyList
        foreach (var supplyData in supplyDataList)
        {
            networkSupplyList.Add(new SupplyDataNetwork(supplyData));
        }

        Debug.Log("Đã thêm " + supplyDataList.Count + " vật phẩm vào networkSupplyList.");
    }

    public List<SupplyData> GetAvailableSupplies()
    {
        // Lấy danh sách các SupplyData từ networkSupplyList
        List<SupplyData> availableSupplies = new List<SupplyData>();
        foreach (var supplyNetwork in networkSupplyList)
        {
            var supplyData = supplyDataList.Find(supply =>
                supply.supplyID == supplyNetwork.supplyID
            );
            if (supplyData != null)
            {
                availableSupplies.Add(supplyData);
            }
        }
        return availableSupplies;
    }

    // private void Update()
    // {
    //     if (Input.GetKeyDown(KeyCode.D))
    //     {
    //         DebugRemainingSupplies();
    //     }
    //     if (Input.GetKeyDown(KeyCode.T))
    //     {
    //         InitializeSlots();
    //     }
    //     if (Input.GetKeyDown(KeyCode.I))
    //     {
    //         DebugInventory();
    //     }
    //     if (Input.GetKeyDown(KeyCode.E))
    //     {
    //         isUsingSupply = true;
    //         useSupplyCoroutine = StartCoroutine(CallUseSupplyRepeatedly());
    //     }
    //     if (!isUsingSupply)
    //     {
    //         // Dừng Coroutine khi đối tượng bị vô hiệu hóa
    //         if (useSupplyCoroutine != null)
    //         {
    //             StopCoroutine(useSupplyCoroutine);
    //         }
    //     }
    // }

    // private IEnumerator CallUseSupplyRepeatedly()
    // {
    //     while (isUsingSupply) // Lặp vô hạn, nếu muốn dừng thì cần dừng Coroutine
    //     {
    //         UseSupply(); // Gọi hàm UseSupply
    //         yield return new WaitForSeconds(interval); // Đợi trong khoảng thời gian nhất định
    //     }
    // }

    // public void UseSupply()
    // {
    //     // Duyệt qua tất cả các phần tử trong playerInventory
    //     for (int index = 0; index < PlayerInventorySupply.Instance.playerInventorys.Count; index++)
    //     {
    //         SupplyData supply = PlayerInventorySupply.Instance.playerInventorys[index];

    //         // Kiểm tra xem supply có null không
    //         if (supply == null)
    //         {
    //             Debug.LogError($"Supply at index {index} is null!");
    //             continue; // Nếu null thì bỏ qua phần tử này và tiếp tục với phần tử tiếp theo
    //         }

    //         // Kiểm tra SupplyPrefab có null không
    //         if (supply.supplyPrefab == null)
    //         {
    //             Debug.LogError($"SupplyPrefab for {supply.supplyName} is null!");
    //             continue; // Nếu null thì bỏ qua phần tử này và tiếp tục với phần tử tiếp theo
    //         }

    //         // Gọi phương thức kích hoạt supply nếu hợp lệ
    //         inventorySupply.ActiveSupplyByIndex(index);
    //     }
    // }

    // public void DebugRemainingSupplies()
    // {
    //     if (supplyDataLList.Count == 0)
    //     {
    //         Debug.Log("Không còn supply nào trong danh sách supplyDataLList.");
    //         return;
    //     }

    //     string remainingSupplies = string.Join(
    //         ", ",
    //         supplyDataLList.ConvertAll(s => $"{s.supplyName} ({s.supplyType})")
    //     );
    //     Debug.Log($"Danh sách các supply còn lại trong supplyDataLList: {remainingSupplies}");
    // }

    // public void DebugInventory()
    // {
    //     if (playerInventory.Count == 0)
    //     {
    //         Debug.Log("Inventory đang trống.");
    //         return;
    //     }

    //     string inventoryItems = string.Join(
    //         ", ",
    //         playerInventory.ConvertAll(s => $"{s.supplyName} ({s.supplyType})")
    //     );
    //     Debug.Log($"Danh sách các vật phẩm trong Inventory: {inventoryItems}");
    // }

    public SupplyData GetSupplyByID(string id)
    {
        return supplyDataList.FirstOrDefault(s => s.supplyID == id);
    }

    private void InitializeSlots()
    {
        supplySlot1 = GameObject.FindGameObjectWithTag("Supply").transform;
        supplySlot2 = GameObject.FindGameObjectWithTag("Supply1").transform;
        supplySlot3 = GameObject.FindGameObjectWithTag("Supply2").transform;
        slots = new List<Transform>() { supplySlot1, supplySlot2, supplySlot3 };
        SpawnRandomSupply();
    }

    public void SpawnRandomSupply()
    {
        DestroySpawnedSupplies();
        // Phân loại supply theo SupplyType
        Dictionary<SupplyType, List<SupplyData>> suppliesByType =
            new Dictionary<SupplyType, List<SupplyData>>();
        foreach (SupplyData supply in supplyDataList)
        {
            if (!suppliesByType.ContainsKey(supply.supplyType))
            {
                suppliesByType[supply.supplyType] = new List<SupplyData>();
            }
            suppliesByType[supply.supplyType].Add(supply);
        }

        // Debug danh sách phân loại
        Debug.Log("Danh sách phân loại supplies theo SupplyType:");
        foreach (var pair in suppliesByType)
        {
            Debug.Log(
                $"SupplyType: {pair.Key}, Supplies: {string.Join(", ", pair.Value.ConvertAll(s => s.supplyName))}"
            );
        }

        // Lấy danh sách các loại SupplyType
        List<SupplyType> supplyTypes = new List<SupplyType>(suppliesByType.Keys);

        // Debug danh sách các loại SupplyType
        Debug.Log(
            $"Danh sách các loại SupplyType trước khi shuffle: {string.Join(", ", supplyTypes)}"
        );

        if (supplyTypes.Count < slots.Count)
        {
            Debug.LogWarning("Không đủ loại SupplyType để spawn cho tất cả các slot!");
            return;
        }

        // Shuffle danh sách các loại SupplyType để đảm bảo ngẫu nhiên
        Shuffle(supplyTypes);

        // Debug danh sách các loại SupplyType sau khi shuffle


        for (int i = 0; i < slots.Count; i++)
        {
            // Lấy loại SupplyType cho slot hiện tại
            SupplyType currentType = supplyTypes[i];

            if (suppliesByType[currentType].Count > 0)
            {
                // Chọn ngẫu nhiên một supply từ loại tương ứng
                List<SupplyData> availableSupplies = suppliesByType[currentType];

                int randomIndex = Random.Range(0, availableSupplies.Count);
                SupplyData selectedSupply = availableSupplies[randomIndex];

                Debug.Log($"Vật phẩm được chọn: {selectedSupply.supplyName}");

                // Spawn supply tại vị trí slot
                GameObject spawnedSupply = Instantiate(
                    selectedSupply.supplyPrefab,
                    slots[i].position,
                    Quaternion.identity
                );
                spawnedSupplies.Add(spawnedSupply); // Lưu vào danh sách

                if (IsServer)
                {
                    spawnedSupply.GetComponent<NetworkObject>().Spawn();
                }

                // Gắn script SupplyPickup vào supply vừa spawn
                SupplyPickup pickup = spawnedSupply.GetComponent<SupplyPickup>();
                if (pickup != null)
                {
                    pickup.supplyData = selectedSupply; // Gán dữ liệu supply
                }
                else
                {
                    Debug.LogWarning(
                        $"SupplyPrefab {selectedSupply.supplyPrefab.name} không có script SupplyPickup!"
                    );
                }

                Debug.Log(
                    $"Danh sách supply còn lại sau khi spawn cho loại {currentType}: {string.Join(", ", availableSupplies.ConvertAll(s => s.supplyName))}"
                );
            }
            else
            {
                Debug.LogWarning(
                    $"Không tìm thấy supply nào thuộc loại {currentType} cho slot {i + 1}."
                );
            }
        }
    }

    public void DestroySpawnedSupplies()
    {
        foreach (GameObject supply in spawnedSupplies)
        {
            if (supply != null)
            {
                if (IsServer)
                {
                    supply.GetComponent<NetworkObject>().Despawn(); // Hủy đối tượng trên server (nếu có Netcode)
                }
                Destroy(supply); // Xóa GameObject khỏi scene
            }
        }
        spawnedSupplies.Clear(); // Xóa danh sách sau khi hủy
        Debug.Log("Tất cả các supply đã được hủy.");
    }

    public void RemoveSupply(FixedString32Bytes supplyId)
    {
        if (IsServer)
        {
            // Xóa từ networkSupplyList
            var supplyToRemove = FindSupplyById(supplyId);
            if (!supplyToRemove.Equals(default(SupplyDataNetwork)))
            {
                networkSupplyList.Remove(supplyToRemove);
                Debug.Log($"Đã xóa vật phẩm với ID {supplyId} khỏi networkSupplyList.");
            }

            // Xóa từ supplyDataList
            RemoveSupplyFromDataList(supplyId);
        }
    }

    public void RemoveSupplyFromDataList(FixedString32Bytes supplyId)
    {
        // Tìm phần tử cần xóa trong supplyDataList
        var supplyToRemove = supplyDataList.Find(supply =>
            new FixedString32Bytes(supply.supplyID) == supplyId
        );

        if (supplyToRemove != null)
        {
            supplyDataList.Remove(supplyToRemove);
            Debug.Log($"Đã xóa vật phẩm với ID {supplyId} khỏi supplyDataList.");
        }
    }

    public SupplyDataNetwork FindSupplyById(FixedString32Bytes supplyId)
    {
        foreach (var supply in networkSupplyList)
        {
            if (supply.supplyID == supplyId)
            {
                return supply;
            }
        }
        return default; // Trả về giá trị mặc định nếu không tìm thấy
    }

    // Hàm shuffle danh sách
    private void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int randomIndex = Random.Range(i, list.Count);
            T temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    public void AddToInventory(SupplyData supply)
    {
        PlayerInventorySupply.Instance.playerInventorys.Add(supply);

        OnInventoryChanged?.Invoke(supply);
        Debug.Log(
            $"Đã thêm {supply.supplyName} vào Inventory. Tổng số vật phẩm: {PlayerInventorySupply.Instance.playerInventorys.Count}"
        );
    }

    public List<SupplyData> GetPlayerInventory()
    {
        return PlayerInventorySupply.Instance.playerInventorys;
    }
}
