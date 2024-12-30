using System.Collections.Generic;
using UnityEngine;

public class InventorySupply : MonoBehaviour
{
    private SupplyManager supplyManager;
    // Dictionary để lưu trữ các instance của supply prefabs
    private Dictionary<SupplyData, GameObject> activeSupplies = new Dictionary<SupplyData, GameObject>();

    private void Start()
    {
        supplyManager = SupplyManager.Instance;
        // Khởi tạo các supply từ inventory
        InitializeInventorySupplies();
    }

    private void InitializeInventorySupplies()
    {
        // Lấy danh sách inventory từ SupplyManager
        List<SupplyData> inventory = supplyManager.GetPlayerInventory();

        foreach (SupplyData supply in inventory)
        {
            // Tạo instance của prefab
            GameObject supplyInstance = Instantiate(supply.supplyPrefab);
            // Có thể set parent, position tùy nhu cầu
            supplyInstance.transform.SetParent(transform);

            // Lưu vào dictionary để quản lý
            activeSupplies[supply] = supplyInstance;

            // Ẩn object đi nếu cần
            supplyInstance.SetActive(false);
        }
    }

    // Kích hoạt supply theo index trong inventory
    public void ActiveSupplyByIndex(int index)
    {
        List<SupplyData> inventory = supplyManager.GetPlayerInventory();
        if (index >= 0 && index < inventory.Count)
        {
            SupplyData supply = inventory[index];
            ActiveSupply(supply);
        }
    }

    // Kích hoạt supply cụ thể
    public void ActiveSupply(SupplyData supply)
    {
        if (activeSupplies.ContainsKey(supply))
        {
            GameObject supplyObj = activeSupplies[supply];
            ISupplyActive supplyActive = supplyObj.GetComponent<ISupplyActive>();

            if (supplyActive != null)
            {
                supplyActive.Active();
                Debug.Log($"Activated {supply.supplyName}");
            }
            else
            {
                Debug.LogWarning($"{supply.supplyName} doesn't implement ISupplyActive");
            }
        }
    }

    // Kiểm tra xem supply có thể active không
    public bool CanActiveSupply(SupplyData supply)
    {
        if (activeSupplies.ContainsKey(supply))
        {
            GameObject supplyObj = activeSupplies[supply];
            ISupplyActive supplyActive = supplyObj.GetComponent<ISupplyActive>();

            if (supplyActive != null)
            {
                return true;
            }
        }
        return false;
    }
}
