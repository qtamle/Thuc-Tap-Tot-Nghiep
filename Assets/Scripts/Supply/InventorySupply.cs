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
        Debug.Log("Starting InitializeInventorySupplies()");

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

            Debug.Log($"Processing supply: {supply.supplyName}");

            if (supply.supplyPrefab == null)
            {
                Debug.LogError($"Supply {supply.supplyName} has null prefab!");
                continue;
            }

            GameObject supplyInstance = Instantiate(supply.supplyPrefab);
            Debug.Log($"Instantiated prefab for {supply.supplyName}");

            supplyInstance.transform.SetParent(transform);
            Debug.Log($"Set parent for {supply.supplyName}");

            activeSupplies[supply] = supplyInstance;
            Debug.Log($"Added {supply.supplyName} to activeSupplies dictionary");

            supplyInstance.SetActive(false);
            Debug.Log($"Disabled {supply.supplyName} GameObject");
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
            activeSupplies[newSupply] = supplyInstance;
            supplyInstance.SetActive(false);
            Debug.Log($"Added new supply: {newSupply.supplyName}");
        }
    }
    public void ActiveSupplyByIndex(int index)
    {
        Debug.Log($"ActiveSupplyByIndex called with index: {index}");

        List<SupplyData> inventory = supplyManager.GetPlayerInventory();
        if (inventory == null)
        {
            Debug.LogError("GetPlayerInventory returned null in ActiveSupplyByIndex!");
            return;
        }

        Debug.Log($"Current inventory size: {inventory.Count}");

        if (index < 0 || index >= inventory.Count)
        {
            Debug.LogError($"Invalid index: {index}. Inventory size: {inventory.Count}");
            return;
        }

        SupplyData supply = inventory[index];
        if (supply == null)
        {
            Debug.LogError($"Supply at index {index} is null!");
            return;
        }

        Debug.Log($"Attempting to activate {supply.supplyName}");
        ActiveSupply(supply);
    }

    public void ActiveSupply(SupplyData supply)
    {
        Debug.Log($"ActiveSupply called for: {supply.supplyName}");

        if (!activeSupplies.ContainsKey(supply))
        {
            Debug.LogError($"Supply {supply.supplyName} not found in activeSupplies dictionary!");
            Debug.Log("Current activeSupplies contents:");
            foreach (var pair in activeSupplies)
            {
                Debug.Log($"- {pair.Key.supplyName}");
            }
            return;
        }

        GameObject supplyObj = activeSupplies[supply];
        if (supplyObj == null)
        {
            Debug.LogError($"GameObject for {supply.supplyName} is null in dictionary!");
            return;
        }

        ISupplyActive supplyActive = supplyObj.GetComponent<ISupplyActive>();
        if (supplyActive == null)
        {
            Debug.LogWarning($"{supply.supplyName} doesn't implement ISupplyActive");
            return;
        }

        supplyActive.Active();
        Debug.Log($"Successfully activated {supply.supplyName}");
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
