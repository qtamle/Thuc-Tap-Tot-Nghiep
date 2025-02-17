using System;
using System.IO;
using System.Threading.Tasks;
using Unity.Android.Gradle.Manifest;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public class LevelData
{
    public int level; // Cấp độ hiện tại
    public int experience; // Kinh nghiệm hiện tại
    public int experienceToNextLevel; // Kinh nghiệm cần để lên cấp
    public int health;

    public int lastRewardedLevel;
}

public class LevelSystem : MonoBehaviour
{
    public static LevelSystem Instance { get; private set; }

    public int level = 0;
    public int experience = 0;
    public int experienceToNextLevel = 100;
    public int health;

    public event Action<int, int, int> OnLevelDataUpdated;
    public event Action<int, int, int> OnLevelLoading;

    public LevelData data;

    // private string filePath;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Hủy object trùng lặp
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Đăng ký sự kiện khi scene thay đổi
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Kiểm tra nếu đang chuyển sang Scene Login, hủy LevelSystem
        if (scene.name == "Login")
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        // Hủy đăng ký sự kiện khi đối tượng bị hủy
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private async void Start()
    {
        // filePath = Path.Combine(Application.persistentDataPath, "Data/LevelData.json");
        await LoadLevelDataFromCloud();
    }

    public async Task AddExperience(int amount)
    {
        experience += amount;
        CalculateLevel();
        data.experience = experience;
        await SaveService.SaveLevelData(data);

        Debug.Log(
            $"Triggering OnLevelDataUpdated: Level {level}, Exp {experience}/{experienceToNextLevel}"
        );
        OnLevelDataUpdated?.Invoke(level, experience, experienceToNextLevel);
    }

    public async Task AddHealth(int amount)
    {
        health += amount;
        data.health = health;
        await SaveService.SaveLevelData(data);
        Debug.Log($"Health added. Current health: {health}");
    }

    private void CalculateLevel()
    {
        while (experience >= experienceToNextLevel)
        {
            level++;
            experience -= experienceToNextLevel;
            experienceToNextLevel = LevelFormula.CalculateExperienceToNextLevel(level);
            data.level = level;
            data.experience = experience;
            data.experienceToNextLevel = experienceToNextLevel;
        }
    }

    public async Task UpdateLastRewardedLevel(int newLevel)
    {
        data.lastRewardedLevel = newLevel;
        await SaveService.SaveLevelData(data);
        Debug.Log($"✅ LastRewardedLevel updated: {newLevel}");
    }

    public async Task LoadLevelDataFromCloud()
    {
        Debug.Log("Loading level data...");
        // if (File.Exists(filePath))
        // {
        //     LevelData data = JsonUtility.FromJson<LevelData>(File.ReadAllText(filePath));
        //     level = data.level;
        //     experience = data.experience;
        //     experienceToNextLevel = data.experienceToNextLevel;
        //     health = data.health;
        // }

        data = await SaveService.LoadLevelData();
        level = data.level;
        experience = data.experience;
        experienceToNextLevel = data.experienceToNextLevel;
        health = data.health;
        Debug.Log(
            $"Level {level}, Experience {experience}/{experienceToNextLevel}, Health {health}"
        );
        OnLevelLoading?.Invoke(level, experience, experienceToNextLevel);
    }
}
