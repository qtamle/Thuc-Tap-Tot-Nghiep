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
        // Debug.Log("Weapon id la " + index);
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
            // Debug.LogError("❌ SaveWeaponData: Data is NULL!");
            return;
        }

        string weaponID = GetID(data.WeaponID);

        if (string.IsNullOrEmpty(weaponID))
        {
            // Debug.LogError("❌ SaveWeaponData: Weapon ID is NULL or EMPTY!");
            return;
        }

        // Debug.Log(
        //     $"✅ SaveWeaponData: Saving Weapon {data.weaponName} (ID: {weaponID}) at Level {data.currentLevel}"
        // );

        try
        {
            await Client.Save(weaponID, data);
            // Debug.Log($"✅ SaveWeaponData: Successfully saved {data.weaponName} (ID: {weaponID})");
        }
        catch (Exception ex)
        {
            // Debug.LogError(
            //     $"❌ SaveWeaponData: Failed to save {data.weaponName} (ID: {weaponID}) - Error: {ex.Message}"
            // );
        }
    }

    public static async Task DeleteWeaponData(CharacterWeaponData data)
    {
        await Client.Delete(GetID(data.WeaponID));
    }

    // --- Lưu CoinsData lên CloudSave ---
    public static async Task SaveCoinData(CoinsData data)
    {
        if (data == null)
        {
            Debug.LogError("❌ SaveCoinData: Data is NULL!");
            return;
        }

        try
        {
            await Client.Save("CoinsData", data);
            Debug.Log(
                $"✅ SaveCoinData: Successfully saved TotalCoin1 = {data.totalCoinType1Count}, TotalCoin2 = {data.totalCoinType2Count}"
            );
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ SaveCoinData: Failed to save coins - Error: {ex.Message}");
        }
    }

    // --- Tải CoinsData từ CloudSave ---
    public static async Task<CoinsData> LoadCoinData()
    {
        try
        {
            CoinsData data = await Client.Load<CoinsData>("CoinsData");
            if (data != null)
            {
                // Debug.Log(
                //     $"✅ LoadCoinData: Loaded TotalCoin1 = {data.totalCoinType1Count}, TotalCoin2 = {data.totalCoinType2Count}"
                // );
                return data;
            }
            else
            {
                // Debug.LogWarning(
                //     "⚠ LoadCoinData: No coin data found, initializing default values..."
                // );

                CoinsData defaultData = new CoinsData
                {
                    totalCoinType1Count = 5000,
                    totalCoinType2Count = 5000,
                };

                // 🟢 **Tự động lưu dữ liệu mặc định lên CloudSave**
                await SaveCoinData(defaultData);
                // Debug.Log("✅ Default coin data saved to CloudSave.");

                return defaultData;
            }
        }
        catch (Exception ex)
        {
            // Debug.LogError($"❌ LoadCoinData: Failed to load coins - Error: {ex.Message}");

            CoinsData defaultData = new CoinsData
            {
                totalCoinType1Count = 5000,
                totalCoinType2Count = 5000,
            };

            // 🟢 **Tự động lưu dữ liệu mặc định ngay cả khi gặp lỗi**
            await SaveCoinData(defaultData);
            // Debug.Log("✅ Default coin data saved due to error.");

            return defaultData;
        }
    }

    public static async Task SaveLevelData(LevelData data)
    {
        if (data == null)
        {
            Debug.LogError("❌SaveServices:  LevelData: Data is NULL!");
            return;
        }

        try
        {
            await Client.Save("LevelData", data);
            Debug.Log(
                $"✅SaveServices:  LevelData: Successfully saved Level = {data.level}, Experience = {data.experience}, ExperienceToNextLevel = {data.experienceToNextLevel}, Health = {data.health}"
            );
        }
        catch (Exception ex)
        {
            Debug.LogError(
                $"❌SaveServices:  SaveLevelnData: Failed to save Level - Error: {ex.Message}"
            );
        }
    }

    public static async Task<LevelData> LoadLevelData()
    {
        try
        {
            LevelData data = await Client.Load<LevelData>("LevelData");
            if (data != null)
            {
                Debug.Log(
                    $"✅ LoadLevelData: Loaded Level = {data.level}, Experience = {data.experience}"
                );
                return data;
            }
            else
            {
                Debug.LogWarning(
                    "⚠ LevelData: No level data found, initializing default values..."
                );

                LevelData defaultData = new LevelData
                {
                    level = 0,
                    experience = 0,
                    experienceToNextLevel = 1000,
                    health = 0,
                    lastRewardedLevel = 0
                };

                // 🟢 **Tự động lưu dữ liệu mặc định lên CloudSave**
                await SaveLevelData(defaultData);
                Debug.Log("✅ Default level data saved to CloudSave.");

                return defaultData;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ LoadLevelData: Failed to load level - Error: {ex.Message}");

            LevelData defaultData = new LevelData
            {
                level = 0,
                experience = 0,
                experienceToNextLevel = 1000,
                health = 0,
            };

            // 🟢 **Tự động lưu dữ liệu mặc định lên CloudSave**
            await SaveLevelData(defaultData);
            Debug.Log("✅ Default level data saved to CloudSave.");

            return defaultData;
        }
    }
}
