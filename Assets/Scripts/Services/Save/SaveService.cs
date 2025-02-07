using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public static class SaveService
{
#if LOCAL_TEST
    private static readonly ISaveClient Client = new PlayerPrefClient();
#else
    private static readonly ISaveClient Client = new CloudSaveClient();
#endif

    private static string GetID(string id) => $"ID{id}";

    public static async Task<CharacterWeaponData> GetWeaponID(string index)
    {
        Debug.Log("Weapon id la " + index);
        return await Client.Load<CharacterWeaponData>(GetID(index));
    }

    public static async Task<List<CharacterWeaponData>> GetWeaponsID()
    {
        var weaponDataList = await Client.Load<CharacterWeaponData>(
            GetID("1"),
            GetID("2"),
            GetID("3"),
            GetID("4"),
            GetID("5"),
            GetID("6")
        );

        return weaponDataList?.ToList() ?? new List<CharacterWeaponData>();
    }

    public static async Task SaveWeaponData(CharacterWeaponData data)
    {
        if (data == null)
        {
            Debug.LogError("❌ SaveWeaponData: Data is NULL!");
            return;
        }

        string weaponID = GetID(data.WeaponID);

        if (string.IsNullOrEmpty(weaponID))
        {
            Debug.LogError("❌ SaveWeaponData: Weapon ID is NULL or EMPTY!");
            return;
        }

        Debug.Log(
            $"✅ SaveWeaponData: Saving Weapon {data.weaponName} (ID: {weaponID}) at Level {data.currentLevel}"
        );

        try
        {
            await Client.Save(weaponID, data);
            Debug.Log($"✅ SaveWeaponData: Successfully saved {data.weaponName} (ID: {weaponID})");
        }
        catch (Exception ex)
        {
            Debug.LogError(
                $"❌ SaveWeaponData: Failed to save {data.weaponName} (ID: {weaponID}) - Error: {ex.Message}"
            );
        }
    }

    public static async Task DeleteWeaponData(CharacterWeaponData data)
    {
        await Client.Delete(GetID(data.WeaponID));
    }
}
