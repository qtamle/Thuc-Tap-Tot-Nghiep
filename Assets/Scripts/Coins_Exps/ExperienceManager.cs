using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ExperienceManager : MonoBehaviour
{
    public static ExperienceManager Instance { get; private set; }

    public TMP_Text experienceText;
    public int experienceCount = 0;
    private Experience experienceIncrease;
    private LevelSystem levelSystem;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Giữ object tồn tại giữa các Scene
        }
        else
        {
            Destroy(gameObject); // Hủy object trùng lặp nếu đã có instance
        }
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

    private void Start()
    {
        experienceIncrease = FindFirstObjectByType<Experience>();
        levelSystem = FindFirstObjectByType<LevelSystem>();

        if (experienceIncrease != null)
        {
            Debug.Log("Tìm thấy tăng kinh nghiệm");
        }

        experienceCount = 0;
        UpdateExperienceUI();
    }

    public void AddExperience(int amount)
    {
        experienceCount += amount;

        if (experienceIncrease != null)
        {
            experienceCount += experienceIncrease.increaseExperience;
        }

        UpdateExperienceUI();
    }

    private void UpdateExperienceUI()
    {
        if (experienceText != null)
        {
            experienceText.text = $"Exp in Scene: {experienceCount}";
        }
    }

    // 🟢 **Singleton - Gọi từ bất kỳ đâu**
    public async Task SubmitExperience()
    {
        if (levelSystem != null)
        {
            Debug.Log("Đã lưu Level!");
            await levelSystem.AddExperience(experienceCount);
            experienceCount = 0;
        }
    }
}
