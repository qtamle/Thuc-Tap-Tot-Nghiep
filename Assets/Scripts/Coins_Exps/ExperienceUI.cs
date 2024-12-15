using TMPro;
using UnityEngine;

public class ExperienceUI : MonoBehaviour
{
    public TMP_Text experienceText;  
    private int currentExperience;

    private ExperienceManager experienceManager;

    private void Start()
    {
        experienceManager = UnityEngine.Object.FindFirstObjectByType<ExperienceManager>();

        if (experienceManager != null)
        {
            currentExperience = experienceManager.experienceCount;
        }

        UpdateExperienceUI();
    }

    private void Update()
    {
        if (experienceManager != null)
        {
            if (currentExperience != experienceManager.experienceCount)
            {
                currentExperience = experienceManager.experienceCount;
                UpdateExperienceUI();
            }
        }
    }

    private void UpdateExperienceUI()
    {
        experienceText.text = "Ex: " + currentExperience;
    }
}
