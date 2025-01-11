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

    private void Update()
    {
        if (weaponData != null)
        {
            weaponSprite = weaponData.weaponSprite;
        }
    }

    public void UpgradeWeapon(int level)
    {
        if (level < 0 || level >= upgradePrices.Length)
        {
            Debug.LogError("Level không hợp lệ!");
            return;
        }

        Debug.Log($"Upgrade to level {level} costs {upgradePrices[level]}.");
    }
}
