using UnityEngine;

[CreateAssetMenu(fileName = "NewWeapons", menuName = "Weapons/WeaponData")]
public class WeaponSO : ScriptableObject
{
    public GameObject weapon;
    public Sprite weaponSprite;
    public string weaponName;
    public string description;
    public int currentLevel = 1;

    public int[] upgradeCosts = new int[4];
}
