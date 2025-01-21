using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class WeaponData : MonoBehaviour
{
    [Header("Equip Weapon Data")]
    public WeaponSO weaponData;

    [Header("Weapon Details")]
    public string weaponName;
    public int basePrice;
    public Sprite weaponSprite;

    [Header("Upgrade Levels")]
    public int[] upgradePrices = new int[4];

    [Header("Equipment Slots")]
    public int maxSlots;
    public List<Equipment> equippedLoot = new List<Equipment>();

    [Header("Upgrade Levels")]
    public int currentLevel = 1;
    public int maxLevel = 4;

    private void Update()
    {
        if (weaponData != null)
        {
            weaponSprite = weaponData.weaponSprite;
        }
    }
    public void UpgradeWeapon()
    {
        if (currentLevel < maxLevel)
        {
            int upgradeCost = weaponData.upgradeCosts[currentLevel - 1]; // Lấy giá nâng cấp từ WeaponSO
            currentLevel++;
            weaponData.currentLevel = currentLevel;
            Debug.Log($"Weapon {weaponName} upgraded to level {currentLevel} with cost {upgradeCost}.");
        }
        else
        {
            Debug.Log("Max level reached for " + weaponName);
        }
    }
}
