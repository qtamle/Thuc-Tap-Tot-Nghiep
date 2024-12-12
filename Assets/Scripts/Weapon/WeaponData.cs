using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class WeaponData : MonoBehaviour
{
    [Header("Weapon Details")]
    public string weaponName;
    public int basePrice;
    public Image weaponSprite;

    [Header("Upgrade Levels")]
    public int[] upgradePrices = new int[4];

    [Header("Equipment Slots")]
    public int maxSlots;
    public List<Equipment> equippedLoot = new List<Equipment>(); // Chiến lợi phẩm hiện được gắn

    
    public void UpgradeWeapon(int level)
    {
        if (level < 0 || level >= upgradePrices.Length)
        {
            Debug.LogError("Level không hợp lệ!");
            return;
        }

        Debug.Log($"Upgrade to level {level} costs {upgradePrices[level]}.");
        // Thực hiện logic nâng cấp vũ khí tại đây
    }
}
