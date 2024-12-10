using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewWeapon", menuName = "Weapons/Weapon")]
public class WeaponData : ScriptableObject
{
    public string weaponName;
    public int basePrice;
    public Sprite weaponSprite;

    [Header("Upgrade Levels")]
    public int[] upgradePrices = new int[4];

    [Header("Equipment Slots")]
    public int maxSlots;
    public List<Equipment> equippedLoot = new List<Equipment>(); // Chiến lợi phẩm hiện được gắn

}
