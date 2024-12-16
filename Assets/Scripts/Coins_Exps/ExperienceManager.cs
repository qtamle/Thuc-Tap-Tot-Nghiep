using TMPro;
using UnityEngine;

public class ExperienceManager : MonoBehaviour
{
    public int experienceCount = 0; // Kinh nghiệm kiếm được trong Scene
    public TMP_Text experienceText;

    private LevelSystem levelSystem;

    private void Start()
    {
        // Tìm LevelSystem trong Scene
        levelSystem = FindObjectOfType<LevelSystem>();

        // Đặt kinh nghiệm kiếm được trong Scene về 0 khi bắt đầu
        experienceCount = 0;

        // Cập nhật UI
        UpdateExperienceUI();
    }

    public void AddExperience(int amount)
    {
        // Tăng kinh nghiệm kiếm được trong Scene
        experienceCount += amount;

        // Cập nhật UI
        UpdateExperienceUI();
    }

    private void UpdateExperienceUI()
    {
        // Cập nhật UI hiển thị kinh nghiệm hiện tại
        if (experienceText != null)
        {
            experienceText.text = $"Exp in Scene: {experienceCount}";
        }
        else
        {
            Debug.LogWarning("experienceText is not assigned in the Inspector!");
        }
    }

    private void OnDisable()
    {
        // Cộng dồn kinh nghiệm khi thoát Scene
        if (levelSystem != null)
        {
            levelSystem.AddExperience(experienceCount);
        }
    }
}
