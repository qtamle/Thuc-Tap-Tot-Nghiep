using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class ExperienceManager : MonoBehaviour
{
    public TMP_Text experienceText;
    [SerializeField] public LevelSystem levelSystem;

    public int experienceCount = 0;

    private Experience experienceIncrease;
    private void Start()
    {
        experienceIncrease = FindFirstObjectByType<Experience>();
        if (experienceIncrease != null)
        {
            Debug.Log("Tim thay tang kinh nghiem");
        }

        levelSystem = FindAnyObjectByType<LevelSystem>();
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

