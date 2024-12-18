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

    public event Action<int, int, int> OnLevelDataUpdated;

    private string filePath;

    private void Awake()
    {
        if (FindFirstObjectByType<LevelSystem>() != null && FindFirstObjectByType<LevelSystem>() != this)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);

    }


    private void Start()
    {
        filePath = Path.Combine(Application.persistentDataPath, "Data/LevelData.json");
        LoadLevelData();
    }

    public void AddExperience(int amount)
    {
        experience += amount;
        CalculateLevel();
        SaveLevelData();

        OnLevelDataUpdated?.Invoke(level, experience, experienceToNextLevel);
    }

    private void CalculateLevel()
    {
        while (experience >= experienceToNextLevel)
        {
            level++;
            experience -= experienceToNextLevel;
            experienceToNextLevel += 50;
        }
    }

    public void SaveLevelData()
    {
        LevelData data = new LevelData
        {
            level = level,
            experience = experience,
            experienceToNextLevel = experienceToNextLevel
        };
        Directory.CreateDirectory(Path.GetDirectoryName(filePath));
        File.WriteAllText(filePath, JsonUtility.ToJson(data, true));
        Debug.Log("File path: " + filePath);
    }

    public void LoadLevelData()
    {
        Debug.Log("Da load file level");
        if (File.Exists(filePath))
        {
            LevelData data = JsonUtility.FromJson<LevelData>(File.ReadAllText(filePath));
            level = data.level;
            experience = data.experience;
            experienceToNextLevel = data.experienceToNextLevel;
        }
        Debug.Log("Level " + level);
        Debug.Log("experience " + experience);
        Debug.Log("experienceToNextLevel " + experienceToNextLevel);

    }
}

