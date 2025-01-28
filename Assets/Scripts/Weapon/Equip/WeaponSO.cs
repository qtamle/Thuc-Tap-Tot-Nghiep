using UnityEngine;

[CreateAssetMenu(fileName = "NewWeapons", menuName = "Weapons/WeaponData")]
public class WeaponSO : ScriptableObject
{
    [Header("Info Weapon")]
    public GameObject weapon;
    public Sprite weaponSprite;
    public string weaponName;
    public string description;

    [Header("Level Weapon")]
    public int currentLevel = 1;

    [Header("Owned")]
    public bool isOwned = false;

    [Header("Level Needed To Buy")]
    public int requiredLevel = 1;

    [Header("Upgrade Price")]
    public int[] upgradeCosts = new int[4];
}
