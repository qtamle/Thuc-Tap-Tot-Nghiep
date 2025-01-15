using UnityEngine;
using System;
using System.IO;

[Serializable]
public class LevelData
{
    public int level; // Cấp độ hiện tại
    public int experience; // Kinh nghiệm hiện tại
    public int experienceToNextLevel; // Kinh nghiệm cần để lên cấp
    public int health;
}

public class LevelSystem : MonoBehaviour
{
    public int level = 0;
    public int experience = 0;
    public int experienceToNextLevel = 100;
    public int health;

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

        Debug.Log($"Triggering OnLevelDataUpdated: Level {level}, Exp {experience}/{experienceToNextLevel}");
        OnLevelDataUpdated?.Invoke(level, experience, experienceToNextLevel);
    }

    public void AddHealth(int amount)
    {
        health += amount;
        SaveLevelData();
        Debug.Log($"Health added. Current health: {health}");
    }

    private void CalculateLevel()
    {
        while (experience >= experienceToNextLevel)
        {
            level++;
            experience -= experienceToNextLevel;
            experienceToNextLevel = LevelFormula.CalculateExperienceToNextLevel(level);
        }
    }
    public void SaveLevelData()
    {
        LevelData data = new LevelData
        {
            level = level,
            experience = experience,
            experienceToNextLevel = experienceToNextLevel,
            health = health
        };
        Directory.CreateDirectory(Path.GetDirectoryName(filePath));
        File.WriteAllText(filePath, JsonUtility.ToJson(data, true));
        Debug.Log("File path: " + filePath);
    }

    public void LoadLevelData()
    {
        Debug.Log("Loading level data...");
        if (File.Exists(filePath))
        {
            LevelData data = JsonUtility.FromJson<LevelData>(File.ReadAllText(filePath));
            level = data.level;
            experience = data.experience;
            experienceToNextLevel = data.experienceToNextLevel;
            health = data.health;
        }
        Debug.Log($"Level {level}, Experience {experience}/{experienceToNextLevel}, Health {health}");
    }
}

