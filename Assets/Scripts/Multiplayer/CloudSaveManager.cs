using System.Collections.Generic;
using Newtonsoft.Json;
using Unity.Services.CloudSave;
using UnityEngine;

public class CloudSaveManager : MonoBehaviour
{
    private PlayerData _playerData;

    public void OnClickSaveButton()
    {
        SavePlayerData(_playerData);
    }

    public void OnClickLoadButton()
    {
        LoadPlayerData();
    }

    public void CreatePlayer()
    {
        _playerData = new PlayerData { PlayerHealth = 25, Level = Random.Range(1, 10) };
        Debug.Log("New player created.");
    }

    public async void SavePlayerData(PlayerData playerData)
    {
        string jsonData = JsonConvert.SerializeObject(playerData);
        var data = new Dictionary<string, object> { { "PlayerData", jsonData } };
        await CloudSaveService.Instance.Data.Player.SaveAsync(data);
        Debug.Log("Player data saved!");
    }

    public async void LoadPlayerData()
    {
        var playerDataDict = await CloudSaveService.Instance.Data.Player.LoadAsync(
            new HashSet<string> { "PlayerData" }
        );

        if (playerDataDict.TryGetValue("PlayerData", out var jsonData))
        {
            // Chuyển đổi jsonData về string một cách rõ ràng
            string jsonString = jsonData.ToString();

            // Loại bỏ dấu ngoặc kép ở đầu và cuối nếu có
            jsonString = jsonString.Trim('"');

            // Thay thế escaped quotes
            jsonString = jsonString.Replace("\\\"", "\"");

            Debug.Log($"Processing JSON string: {jsonString}");

            try
            {
                PlayerData playerData = JsonConvert.DeserializeObject<PlayerData>(jsonString);
                Debug.Log($"Loaded Player: {playerData.PlayerHealth}, Level: {playerData.Level}");
                _playerData = playerData; // Cập nhật dữ liệu player hiện tại
            }
            catch (JsonReaderException ex)
            {
                Debug.LogError($"Failed to deserialize player data: {ex.Message}");
                Debug.LogError($"JSON string causing error: {jsonString}");
            }
        }
        else
        {
            Debug.Log("No player data found.");
        }
    }
}
