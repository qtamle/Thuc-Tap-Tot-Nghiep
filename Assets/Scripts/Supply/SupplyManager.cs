using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class SupplyManager : MonoBehaviour
{
    public static SupplyManager Instance { get; private set; }

    [SerializeField] private List<SupplyData> supplyDataLList;
    private List<SupplyData> playerInventory = new List<SupplyData>();

    public Transform supplySlot1;
    public Transform supplySlot2;
    public Transform supplySlot3;

    private List<Transform> slots;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        InitializeSlots();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            DebugRemainingSupplies();
        }
    }

    public void DebugRemainingSupplies()
    {
        if (supplyDataLList.Count == 0)
        {
            Debug.Log("Không còn supply nào trong danh sách supplyDataLList.");
            return;
        }

        Debug.Log("Danh sách các supply còn lại trong supplyDataLList:");
        foreach (var supply in supplyDataLList)
        {
            Debug.Log($"- {supply.supplyName} ({supply.supplyType})");
        }
    }

    private void InitializeSlots()
    {
        slots = new List<Transform>()
        {
            supplySlot1,
            supplySlot2,
            supplySlot3
        };
        SpawnRandomSupply();
    }


    public void SpawnRandomSupply()
    {
        // Phân loại supply theo SupplyType
        Dictionary<SupplyType, List<SupplyData>> suppliesByType = new Dictionary<SupplyType, List<SupplyData>>();
        foreach (SupplyData supply in supplyDataLList)
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
            Debug.Log($"SupplyType: {pair.Key}, Supplies: {string.Join(", ", pair.Value.ConvertAll(s => s.supplyName))}");
        }

        // Lấy danh sách các loại SupplyType
        List<SupplyType> supplyTypes = new List<SupplyType>(suppliesByType.Keys);

        // Debug danh sách các loại SupplyType
        Debug.Log($"Danh sách các loại SupplyType trước khi shuffle: {string.Join(", ", supplyTypes)}");

        if (supplyTypes.Count < slots.Count)
        {
            Debug.LogWarning("Không đủ loại SupplyType để spawn cho tất cả các slot!");
            return;
        }

        // Shuffle danh sách các loại SupplyType để đảm bảo ngẫu nhiên
        Shuffle(supplyTypes);

        // Debug danh sách các loại SupplyType sau khi shuffle
        Debug.Log($"Danh sách các loại SupplyType sau khi shuffle: {string.Join(", ", supplyTypes)}");

        for (int i = 0; i < slots.Count; i++)
        {
            // Lấy loại SupplyType cho slot hiện tại
            SupplyType currentType = supplyTypes[i];
            Debug.Log($"Slot {i + 1} sẽ spawn loại SupplyType: {currentType}");

            if (suppliesByType[currentType].Count > 0)
            {
                // Chọn ngẫu nhiên một supply từ loại tương ứng
                List<SupplyData> availableSupplies = suppliesByType[currentType];
                Debug.Log($"Danh sách supply khả dụng cho loại {currentType}: {string.Join(", ", availableSupplies.ConvertAll(s => s.supplyName))}");

                int randomIndex = Random.Range(0, availableSupplies.Count);
                SupplyData selectedSupply = availableSupplies[randomIndex];

                Debug.Log($"Vật phẩm được chọn: {selectedSupply.supplyName}");

                // Spawn supply tại vị trí slot
                GameObject spawnedSupply = Instantiate(selectedSupply.supplyPrefab, slots[i].position, Quaternion.identity);

                // Gắn script SupplyPickup vào supply vừa spawn
                SupplyPickup pickup = spawnedSupply.GetComponent<SupplyPickup>();
                if (pickup != null)
                {
                    pickup.supplyData = selectedSupply; // Gán dữ liệu supply
                }
                else
                {
                    Debug.LogWarning($"SupplyPrefab {selectedSupply.supplyPrefab.name} không có script SupplyPickup!");
                }

                // Xóa supply đã dùng để tránh trùng lặp (nếu cần)
                availableSupplies.RemoveAt(randomIndex);

                Debug.Log($"Danh sách supply còn lại sau khi spawn cho loại {currentType}: {string.Join(", ", availableSupplies.ConvertAll(s => s.supplyName))}");
            }
            else
            {
                Debug.LogWarning($"Không tìm thấy supply nào thuộc loại {currentType} cho slot {i + 1}.");
            }
        }

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
        if (!playerInventory.Contains(supply))
        {
            playerInventory.Add(supply);
            Debug.Log($"Đã thêm {supply.supplyName} vào Inventory. Tổng số vật phẩm: {playerInventory.Count}");
        }
    }


}
