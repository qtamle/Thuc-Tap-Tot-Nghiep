using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelRewardSystem : MonoBehaviour
{
    public LevelReward[] levelRewards;

    private LevelSystem levelSystem;
    private int lastRewardedLevel = 0;

    public CoinsManager coinsManager;

    private void Awake()
    {
        coinsManager = CoinsManager.Instance; // Dùng Singleton
        levelSystem = LevelSystem.Instance; // Dùng Singleton

        if (levelSystem != null)
        {
            levelSystem.OnLevelDataUpdated -= OnLevelUp; // Đảm bảo không bị trùng
            levelSystem.OnLevelDataUpdated += OnLevelUp;
        }
    }

    private async void OnLevelUp(int newLevel, int experience, int experienceToNextLevel)
    {
        if (newLevel > lastRewardedLevel)
        {
            for (int level = lastRewardedLevel + 1; level <= newLevel; level++)
            {
                if (level - 1 < levelRewards.Length && level > 0)
                {
                    LevelReward reward = levelRewards[level - 1];
                    await ApplyReward(reward); // 🟢 Chờ từng phần thưởng hoàn tất
                }
            }
            await levelSystem.UpdateLastRewardedLevel(newLevel);
        }
    }

    private async Task ApplyReward(LevelReward reward)
    {
        Debug.Log("Reward: ");
        Debug.Log("Coin type 1: " + reward.coinType1);
        Debug.Log("Coin type 2: " + reward.coinType2);
        Debug.Log("Health: " + reward.health);

        // coinsManager = FindFirstObjectByType<CoinsManager>();
        if (coinsManager != null)
        {
            coinsManager.AddCoins(reward.coinType1, reward.coinType2);

            coinsManager.SaveCoinsToCloud();
            Debug.Log($"Coins added to file: Type1={reward.coinType1}, Type2={reward.coinType2}");
        }
        else
        {
            Debug.LogError("CoinsManager not found! Reward coins not added.");
        }

        if (levelSystem != null)
        {
            await levelSystem.AddHealth(reward.health);
        }
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
