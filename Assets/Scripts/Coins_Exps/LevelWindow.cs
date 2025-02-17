using System.Collections;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelWindow : MonoBehaviour
{
    public TMP_Text levelText;
    public TMP_Text experienceText;
    public Image experienceBarImage;

    [SerializeField]
    private LevelSystem levelSystem;

    private void Awake()
    {
        levelSystem = LevelSystem.Instance; // Dùng Singleton

        if (levelSystem != null)
        {
            levelSystem.OnLevelDataUpdated -= UpdateLevelUI; // Đảm bảo không bị trùng
            levelSystem.OnLevelDataUpdated += UpdateLevelUI;
        }
    }

    private void Start()
    {
        levelSystem = FindAnyObjectByType<LevelSystem>();
        if (levelSystem != null)
        {
            Debug.Log("Da tim thay level system");
            UpdateLevelUI(
                levelSystem.level,
                levelSystem.experience,
                levelSystem.experienceToNextLevel
            );
        }
    }

    public void UpdateLevelUI(int level, int experience, int experienceToNextLevel)
    {
        levelText.text = $"Level: {level}";
        experienceText.text = $"Exp: {experience}/{experienceToNextLevel}";
        StartCoroutine(UpdateExperienceBar((float)experience / experienceToNextLevel));
    }

    private IEnumerator UpdateExperienceBar(float targetFillAmount)
    {
        float currentFillAmount = experienceBarImage.fillAmount;
        float fillSpeed = 0.5f;
        float elapsedTime = 0f;

        while (elapsedTime < fillSpeed)
        {
            experienceBarImage.fillAmount = Mathf.Lerp(
                currentFillAmount,
                targetFillAmount,
                elapsedTime / fillSpeed
            );
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        experienceBarImage.fillAmount = targetFillAmount;
    }
}
