using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class ResourcesService
{
    public static List<WeaponSO> Weapons { get; }

    static ResourcesService()
    {
        // Load all weapons from the database
        Weapons = Resources.LoadAll<WeaponSO>("WeaponData").ToList();
    }

    public static WeaponSO GetWeaponById(string ID)
    {
        return Weapons.FirstOrDefault(w => w.WeaponID == ID);
    }

    public static void ResetWeapons()
    {
        Weapons.Clear();
        Weapons.AddRange(Resources.LoadAll<WeaponSO>("WeaponData"));
        Debug.Log("🔄 Reset toàn bộ Weapons khi đổi tài khoản.");
    }
}
