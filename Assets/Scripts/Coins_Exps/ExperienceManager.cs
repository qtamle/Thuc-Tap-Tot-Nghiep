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
        if (levelSystem != null)
        {
            Debug.Log("Da save Level");
            levelSystem.AddExperience(experienceCount);
        }
    }
}

