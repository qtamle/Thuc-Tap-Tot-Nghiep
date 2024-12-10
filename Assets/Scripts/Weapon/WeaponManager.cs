using System;
using UnityEngine;
using UnityEngine.UI;

public class WeaponManager : MonoBehaviour
{
    public WeaponData weaponData;

    [Header("UI Elements")]
    public Text weaponNameText;
    public Image weaponImage;
    public Text weaponPriceText;
    public Transform equipmentSlotsContainer; // Container chứa các ô trang bị
    public GameObject equipmentSlotPrefab; // Prefab đại diện cho một ô trang bị

    private int currentUpgradeLevel = 0;

    private void Start()
    {
        DisplayWeaponInfo();
    }

    public void UpgradeWeapon()
    {
        if(currentUpgradeLevel < 4)
        {
            int upgradePrice = weaponData.upgradePrices[currentUpgradeLevel];

            currentUpgradeLevel++;
            weaponData.maxSlots = currentUpgradeLevel;
            DisplayWeaponInfo();
            
        }
        else
        {
            Debug.Log("Weapon is fully upgraded!");
        }
    }
    private void DisplayWeaponInfo()
    {
        weaponNameText.text = weaponData.weaponName;
        weaponImage.sprite = weaponData.weaponSprite;
        weaponPriceText.text = currentUpgradeLevel < 4
            ? $"Upgrade Price: {weaponData.upgradePrices[currentUpgradeLevel]}"
            : "Fully Upgraded!";
    }
    private void InitializeEquipmentSlots()
    {
        for(int i = 0; i < 4; i++)
        {
            GameObject slot = Instantiate(equipmentSlotPrefab, equipmentSlotsContainer);
            slot.GetComponent<Button>().interactable = false;
        }
        
    }
    private void UpdateEquipmentSlots()
    {
        for (int i = 0; i < equipmentSlotsContainer.childCount; i++)
        {
            var slot = equipmentSlotsContainer.GetChild(i).GetComponent<Button>();
            slot.interactable = i < weaponData.maxSlots; // Mở khóa theo cấp nâng cấp
        }
    }

    public void EquipLoot(Equipment equipment, int slotIndex)
    {
        if (slotIndex < weaponData.maxSlots)
        {
            weaponData.equippedLoot[slotIndex] = equipment;
            Debug.Log($"Equipped {equipment.equipmentName} in slot {slotIndex}");
        }
        else
        {
            Debug.Log("Slot is locked!");
        }
    }
}
