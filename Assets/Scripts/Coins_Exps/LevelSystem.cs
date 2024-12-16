using UnityEngine;
using System;
using System.IO;

[Serializable]
public class LevelData
{
    public int level; // Cấp độ hiện tại
    public int experience; // Kinh nghiệm hiện tại
    public int experienceToNextLevel; // Kinh nghiệm cần để lên cấp
}

public class LevelSystem : MonoBehaviour
{
    public int level = 0;
    public int experience = 0;
    public int experienceToNextLevel = 100;

    private string filePath;

    private void Start()
    {
        // Đặt đường dẫn lưu trữ tệp JSON trong thư mục Assets/Data
        filePath = Application.dataPath + "/Data/LevelData.json";

        // Tải dữ liệu khi bắt đầu
        LoadLevelData();
    }

    // Thêm kinh nghiệm vào hệ thống và cập nhật cấp độ
    public void AddExperience(int amount)
    {
        experience += amount;

        while (experience >= experienceToNextLevel)
        {
            level++;
            experience -= experienceToNextLevel;
        }
    }

    // Lưu dữ liệu cấp độ vào JSON
    public void SaveLevelData()
    {
        LevelData data = new LevelData
        {
            level = level,
            experience = experience,
            experienceToNextLevel = experienceToNextLevel
        };

        // Chuyển đối tượng dữ liệu thành chuỗi JSON
        string json = JsonUtility.ToJson(data, true);

        // Tạo thư mục nếu chưa tồn tại
        string folderPath = Path.GetDirectoryName(filePath);
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        // Lưu JSON vào tệp
        File.WriteAllText(filePath, json);
        Debug.Log("Level data saved to: " + filePath);
    }

    // Tải dữ liệu cấp độ từ JSON
    public void LoadLevelData()
    {
        if (File.Exists(filePath))
        {
            // Đọc dữ liệu từ tệp JSON
            string json = File.ReadAllText(filePath);
            LevelData data = JsonUtility.FromJson<LevelData>(json);

            // Gán dữ liệu vào các biến cấp độ và kinh nghiệm
            level = data.level;
            experience = data.experience;
            experienceToNextLevel = data.experienceToNextLevel;

            Debug.Log("Level data loaded: " + json);
        }
        else
        {
            // Nếu tệp không tồn tại, sử dụng giá trị mặc định
            Debug.LogWarning("LevelData.json not found. Using default values.");
        }
    }

    private void OnDisable()
    {
        // Lưu dữ liệu khi thoát Scene
        SaveLevelData();
    }
}
