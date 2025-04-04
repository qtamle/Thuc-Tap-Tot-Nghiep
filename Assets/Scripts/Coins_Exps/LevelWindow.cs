using System.Collections;
using System.IO;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
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
            levelSystem.OnLevelLoading -= UpdateLevelUI;
            levelSystem.OnLevelLoading += UpdateLevelUI;
        }
    }

    private void Start()
    {
        levelSystem = LevelSystem.Instance ?? FindAnyObjectByType<LevelSystem>();
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

    private void OnDestroy()
    {
        if (levelSystem != null)
        {
            levelSystem.OnLevelDataUpdated -= UpdateLevelUI;
            levelSystem.OnLevelLoading -= UpdateLevelUI;
        }
    }

    public void UpdateLevelUI(int level, int experience, int experienceToNextLevel)
    {
        levelText.text = (level >= 30) ? "Max Level" : $"Level: {level}";
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
