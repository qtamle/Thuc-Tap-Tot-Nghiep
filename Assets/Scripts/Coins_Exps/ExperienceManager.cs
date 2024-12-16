using TMPro;
using UnityEngine;

public class ExperienceManager : MonoBehaviour
{
    public TMP_Text experienceText;
    [SerializeField] public LevelSystem levelSystem;

    public int experienceCount = 0;

    private void Start()
    {
        levelSystem = FindAnyObjectByType<LevelSystem>();
        experienceCount = 0;
        UpdateExperienceUI();
    }

    public void AddExperience(int amount)
    {
        experienceCount += amount;
        UpdateExperienceUI();
    }

    private void UpdateExperienceUI()
    {
        experienceText.text = $"Exp in Scene: {experienceCount}";
    }

    private void OnDisable()
    {
        Debug.Log("Da save Level");
            levelSystem.AddExperience(experienceCount);
            levelSystem.SaveLevelData();
        
    }
}

