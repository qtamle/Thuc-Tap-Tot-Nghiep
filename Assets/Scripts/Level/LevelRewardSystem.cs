using UnityEngine;

public class LevelRewardSystem : MonoBehaviour
{
    public LevelReward[] levelRewards;

    private LevelSystem levelSystem;
    private int lastRewardedLevel = 0; 

    private void Awake()
    {
        FindAndSubscribeLevelSystem();
    }

    private void OnEnable()
    {
        FindAndSubscribeLevelSystem();
    }

    private void OnDestroy()
    {
        if (levelSystem != null)
        {
            levelSystem.OnLevelDataUpdated -= OnLevelUp;
        }
    }

    private void FindAndSubscribeLevelSystem()
    {
        levelSystem = FindObjectOfType<LevelSystem>();

        if (levelSystem != null)
        {
            Debug.Log("LevelSystem found and subscribed.");
            levelSystem.OnLevelDataUpdated += OnLevelUp;
        }
        else
        {   
            Debug.LogError("LevelSystem not found!");
        }
    }

    private void OnLevelUp(int newLevel, int experience, int experienceToNextLevel)
    {
        Debug.Log($"OnLevelUp triggered: Current Level: {newLevel}, Experience: {experience}/{experienceToNextLevel}");
        Debug.Log($"Last rewarded level: {lastRewardedLevel}");

        if (newLevel > lastRewardedLevel) 
        {
            for (int level = lastRewardedLevel + 1; level <= newLevel; level++)
            {
                Debug.Log($"Processing Level: {level}");
                if (level - 1 < levelRewards.Length && level > 0)
                {
                    LevelReward reward = levelRewards[level - 1];
                    ApplyReward(reward);
                    Debug.Log($"Reward applied for Level {level}");
                }
                else
                {
                    Debug.LogWarning($"No reward defined for this level: {level}");
                }
            }
            lastRewardedLevel = newLevel;
        }
        else
        {
            Debug.Log($"No reward applied. Current Level: {newLevel}, Last Rewarded Level: {lastRewardedLevel}");
        }
    }

    private void ApplyReward(LevelReward reward)
    {
        Debug.Log("Reward: ");
        Debug.Log("Coin type 1: " + reward.coinType1);
        Debug.Log("Coin type 2: " + reward.coinType2);
        Debug.Log("Health: " + reward.health);
    }
}

public class LevelFormula
{
    public static int CalculateExperienceToNextLevel(int level)
    {
        int baseValue = 1000;

        if (level >= 10 && level <= 19)
        {
            baseValue += 500;
        }
        else if (level >= 20 && level <= 25)
        {
            baseValue += 1000;
        }
        else if (level >= 26 && level <= 30)
        {
            baseValue += 1500;
        }

        return (100 * level) + (50 * level) + baseValue;
    }
}
