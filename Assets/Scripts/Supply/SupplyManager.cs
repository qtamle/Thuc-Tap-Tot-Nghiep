﻿using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class SupplyManager : MonoBehaviour
{
    public static SupplyManager Instance { get; private set; }

    [SerializeField] private List<SupplyData> supplyDataLList;
    [SerializeField] private List<SupplyData> playerInventory = new List<SupplyData>();

    [SerializeField] private InventorySupply inventorySupply;
    
    [SerializeField] private Transform supplySlot1;
    [SerializeField] private Transform supplySlot2;
    [SerializeField] private Transform supplySlot3;

    private List<Transform> slots;

    // Khai báo delegate cho event
    public delegate void InventoryChangeHandler(SupplyData supply);
    // Khai báo event
    public event InventoryChangeHandler OnInventoryChanged;

    [SerializeField] private bool isUsingSupply;
    [SerializeField] private float interval = 10f; // Khoảng thời gian giữa các lần gọi (tính bằng giây)
    private Coroutine useSupplyCoroutine;

    public bool hasInitialized = false;

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

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("Scene loaded: " + scene.name);
        if (!hasInitialized)
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

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            DebugRemainingSupplies();
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            InitializeSlots();
        }
        if (Input.GetKeyDown(KeyCode.I))
        {
            DebugInventory();
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            isUsingSupply= true;
            useSupplyCoroutine = StartCoroutine(CallUseSupplyRepeatedly());
        }
        if(!isUsingSupply)
        {
            // Dừng Coroutine khi đối tượng bị vô hiệu hóa
            if (useSupplyCoroutine != null)
            {
                StopCoroutine(useSupplyCoroutine);
            }
        }
    }

    private IEnumerator CallUseSupplyRepeatedly()
    {
        while (isUsingSupply) // Lặp vô hạn, nếu muốn dừng thì cần dừng Coroutine
        {
            UseSupply(); // Gọi hàm UseSupply
            yield return new WaitForSeconds(interval); // Đợi trong khoảng thời gian nhất định
        }
    }

    public void UseSupply()
    {
        // Thêm kiểm tra null và range
        if (playerInventory == null)
        {
            Debug.LogError("playerInventory is null!");
            return;
        }

        // Duyệt qua tất cả các phần tử trong playerInventory
        for (int index = 0; index < playerInventory.Count; index++)
        {
            SupplyData supply = playerInventory[index];

            // Kiểm tra xem supply có null không
            if (supply == null)
            {
                Debug.LogError($"Supply at index {index} is null!");
                continue;  // Nếu null thì bỏ qua phần tử này và tiếp tục với phần tử tiếp theo
            }

            // Kiểm tra SupplyPrefab có null không
            if (supply.supplyPrefab == null)
            {
                Debug.LogError($"SupplyPrefab for {supply.supplyName} is null!");
                continue;  // Nếu null thì bỏ qua phần tử này và tiếp tục với phần tử tiếp theo
            }

            // Gọi phương thức kích hoạt supply nếu hợp lệ
            inventorySupply.ActiveSupplyByIndex(index);
        }
    }

    public void DebugRemainingSupplies()
    {
        if (supplyDataLList.Count == 0)
        {
            Debug.Log("Không còn supply nào trong danh sách supplyDataLList.");
            return;
        }

        string remainingSupplies = string.Join(", ", supplyDataLList.ConvertAll(s => $"{s.supplyName} ({s.supplyType})"));
        Debug.Log($"Danh sách các supply còn lại trong supplyDataLList: {remainingSupplies}");
    }

    public void DebugInventory()
    {
        if (playerInventory.Count == 0)
        {
            Debug.Log("Inventory đang trống.");
            return;
        }

        string inventoryItems = string.Join(", ", playerInventory.ConvertAll(s => $"{s.supplyName} ({s.supplyType})"));
        Debug.Log($"Danh sách các vật phẩm trong Inventory: {inventoryItems}");
    }


    private void InitializeSlots()
    {
        supplySlot1 = GameObject.FindGameObjectWithTag("Supply").transform;
        supplySlot2 = GameObject.FindGameObjectWithTag("Supply1").transform;
        supplySlot3 = GameObject.FindGameObjectWithTag("Supply2").transform;
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


                Debug.Log($"Danh sách supply còn lại sau khi spawn cho loại {currentType}: {string.Join(", ", availableSupplies.ConvertAll(s => s.supplyName))}");
            }
            else
            {
                Debug.LogWarning($"Không tìm thấy supply nào thuộc loại {currentType} cho slot {i + 1}.");
            }
        }

    }
    public void RemoveSupply(SupplyData supply)
    {
        if (supplyDataLList.Contains(supply))
        {
            supplyDataLList.Remove(supply);
            Debug.Log($"Supply {supply.supplyName} đã bị loại khỏi danh sách.");
        }
        else
        {
            Debug.LogWarning($"Supply {supply.supplyName} không tồn tại trong danh sách supplyDataLList.");
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

            OnInventoryChanged?.Invoke(supply);
            Debug.Log($"Đã thêm {supply.supplyName} vào Inventory. Tổng số vật phẩm: {playerInventory.Count}");
        }
    }

    public List<SupplyData> GetPlayerInventory()
    {
        return playerInventory;
    }
}
