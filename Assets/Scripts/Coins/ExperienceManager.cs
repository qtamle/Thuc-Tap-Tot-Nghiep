using TMPro;
using UnityEngine;

public class ExperienceManager : MonoBehaviour
{
    public int experienceCount = 0;  

    public TMP_Text experienceText;

    public void UpdateExperienceUI()
    {
        if (experienceText != null)
        {
            experienceText.text = "Ex: " + experienceCount.ToString();
        }
        else
        {
            Debug.LogWarning("experienceText is not assigned in the Inspector!");
        }
    }

    public void AddExperience(int amount)
    {
        experienceCount += amount;
        UpdateExperienceUI();
    }
}
