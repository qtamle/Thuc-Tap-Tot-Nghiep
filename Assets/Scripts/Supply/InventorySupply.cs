using System.Collections.Generic;
using UnityEngine;

public class InventorySupply : MonoBehaviour
{
    private SupplyManager supplyManager;
    private Dictionary<SupplyData, GameObject> activeSupplies = new Dictionary<SupplyData, GameObject>();

    private void Start()
    {
        Debug.Log("InventorySupply Start() called");

        supplyManager = SupplyManager.Instance;
        if (supplyManager == null)
        {
            Debug.LogError("SupplyManager.Instance is null!");
            return;
        }
        Debug.Log("SupplyManager instance found successfully");
        supplyManager.OnInventoryChanged += UpdateInventory;
        InitializeInventorySupplies();
    }

    private void InitializeInventorySupplies()
    {

        List<SupplyData> inventory = supplyManager.GetPlayerInventory();
        if (inventory == null)
        {
            Debug.LogError("GetPlayerInventory returned null!");
            return;
        }

        Debug.Log($"Found {inventory.Count} items in player inventory");

        foreach (SupplyData supply in inventory)
        {
            if (supply == null)
            {
                Debug.LogError("Found null SupplyData in inventory!");
                continue;
            }


            if (supply.supplyPrefab == null)
            {
                Debug.LogError($"Supply {supply.supplyName} has null prefab!");
                continue;
            }

            GameObject supplyInstance = Instantiate(supply.supplyPrefab);
            supplyInstance.transform.SetParent(transform);

            activeSupplies[supply] = supplyInstance;

            supplyInstance.SetActive(false);
        }

        Debug.Log($"Finished initialization. ActiveSupplies count: {activeSupplies.Count}");
    }

    // Phương thức xử lý khi có item mới
    private void UpdateInventory(SupplyData newSupply)
    {
        if (!activeSupplies.ContainsKey(newSupply))
        {
            GameObject supplyInstance = Instantiate(newSupply.supplyPrefab);
            supplyInstance.transform.SetParent(transform);
            SpriteRenderer supplySprite = supplyInstance.GetComponentInChildren<SpriteRenderer>();
            if (supplySprite != null)
            {
                supplySprite.enabled = false;
            }
            activeSupplies[newSupply] = supplyInstance;
            supplyInstance.SetActive(true);
            Debug.Log($"Added new supply: {newSupply.supplyName}");
        }
    }
    public void ActiveSupplyByIndex(int index)
    {
        List<SupplyData> inventory = supplyManager.GetPlayerInventory();
        if (inventory == null)
        {
            Debug.LogError("GetPlayerInventory returned null in ActiveSupplyByIndex!");
            return;
        }

        if (index < 0 || index >= inventory.Count)
        {
            return;
        }

        SupplyData supply = inventory[index];
        if (supply == null)
        {
            return;
        }
        ActiveSupply(supply);
    }

    public void ActiveSupply(SupplyData supply)
    {
        if (!activeSupplies.ContainsKey(supply))
        {
            foreach (var pair in activeSupplies)
            {
                Debug.Log($"- {pair.Key.supplyName}");
            }
            return;
        }

        GameObject supplyObj = activeSupplies[supply];
        if (supplyObj == null)
        {
            return;
        }

        ISupplyActive supplyActive = supplyObj.GetComponent<ISupplyActive>();
        if (supplyActive == null)
        {
            return;
        }

        // Kiểm tra trạng thái sẵn sàng trước khi gọi Active
        if (!supplyActive.IsReady())
        {
            Debug.Log($"{supply.supplyName} is on cooldown!");
            return;
        }

        supplyActive.Active(); // Gọi Active từ interface
    }


    public bool CanActiveSupply(SupplyData supply)
    {
        Debug.Log($"CanActiveSupply check for: {supply.supplyName}");

        if (!activeSupplies.ContainsKey(supply))
        {
            Debug.Log($"Supply {supply.supplyName} not found in activeSupplies");
            return false;
        }

        GameObject supplyObj = activeSupplies[supply];
        if (supplyObj == null)
        {
            Debug.Log($"GameObject for {supply.supplyName} is null");
            return false;
        }

        ISupplyActive supplyActive = supplyObj.GetComponent<ISupplyActive>();
        if (supplyActive == null)
        {
            Debug.Log($"{supply.supplyName} doesn't implement ISupplyActive");
            return false;
        }

        Debug.Log($"{supply.supplyName} can be activated");
        return true;
    }
}
