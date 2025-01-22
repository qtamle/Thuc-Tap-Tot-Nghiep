using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    public int currentLevel; 
    public int maxLevel = 4;

    private int originalLevel; 

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        currentLevel = originalLevel;
        OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (weaponData != null)
        {
            originalLevel = weaponData.currentLevel; 
            weaponName = weaponData.weaponName;
            weaponSprite = weaponData.weaponSprite;
            Debug.Log($"Weapon data updated: {weaponName} - Level {currentLevel}");
        }
        else
        {
            Debug.LogWarning("Weapon data is null. Please assign a WeaponSO asset.");
        }
    }

    private void OnValidate()
    {
        if (weaponData != null)
        {
            weaponName = weaponData.weaponName;
            weaponSprite = weaponData.weaponSprite;
        }
    }

    public void UpgradeWeapon()
    {
        if (currentLevel < maxLevel)
        {
            int upgradeCost = weaponData.upgradeCosts[currentLevel - 1];
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
