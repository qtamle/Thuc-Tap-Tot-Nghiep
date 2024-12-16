using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections;

public class LevelWindow : MonoBehaviour
{
    public TMP_Text levelText; // Hiển thị cấp độ
    public TMP_Text experienceText; // Hiển thị kinh nghiệm
    public Image experienceBarImage; // Thanh kinh nghiệm

    private int level = 0;
    private int experience = 0;
    private int experienceToNextLevel = 100;

    private void Start()
    {
        LoadLevelData();
        UpdateLevelUI();
    }

    public void LoadLevelData()
    {
        // Đường dẫn đến tệp JSON
        string folderPath = Application.dataPath + "/Data";
        string filePath = folderPath + "/LevelData.json";

        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            LevelData data = JsonUtility.FromJson<LevelData>(json);

            level = data.level;
            experience = data.experience;
            experienceToNextLevel = data.experienceToNextLevel;

            Debug.Log("Level data loaded successfully.");
        }
        else
        {
            Debug.LogWarning("LevelData.json not found. Using default values.");
        }
    }

    public void UpdateLevelUI()
    {
        if (levelText != null)
        {
            levelText.text = "Level: " + (level + 1); // Cấp độ +1 để hiển thị theo dạng thân thiện
        }

        if (experienceText != null)
        {
            experienceText.text = "Exp: " + experience + "/" + experienceToNextLevel;
        }

        if (experienceBarImage != null)
        {
            // Chạy hiệu ứng điền thanh kinh nghiệm mượt mà
            StartCoroutine(UpdateExperienceBar());
        }
    }

    private IEnumerator UpdateExperienceBar()
    {
        float targetFillAmount = (float)experience / experienceToNextLevel;
        float currentFillAmount = experienceBarImage.fillAmount;
        float fillSpeed = 0.5f; // Tốc độ điền thanh (có thể điều chỉnh)

        // Tạo hiệu ứng điền thanh từ giá trị hiện tại đến giá trị mục tiêu
        float elapsedTime = 0f;
        while (elapsedTime < fillSpeed)
        {
            experienceBarImage.fillAmount = Mathf.Lerp(currentFillAmount, targetFillAmount, (elapsedTime / fillSpeed));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Đảm bảo thanh kinh nghiệm chính xác khi hoàn thành
        experienceBarImage.fillAmount = targetFillAmount;
    }
}
