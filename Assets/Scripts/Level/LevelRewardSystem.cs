using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelRewardSystem : MonoBehaviour
{
    public LevelReward[] levelRewards;

    private LevelSystem levelSystem;
    private int lastRewardedLevel = 0;

    public CoinsManager coinsManager;

    public GameObject rewardUI;
    private static GameObject rewardUIInstance;

    public TextMeshProUGUI levelText;
    public TextMeshProUGUI coinType1Text;
    public TextMeshProUGUI coinType2Text;
    public TextMeshProUGUI healthText;
    public Button collectButton;

    private Queue<LevelReward> pendingRewards = new Queue<LevelReward>();
    private bool isRewardUIActive = false;


    private void Awake()
    {
        coinsManager = CoinsManager.Instance; // Dùng Singleton
        levelSystem = LevelSystem.Instance; // Dùng Singleton

        if (levelSystem != null)
        {
            levelSystem.OnLevelDataUpdated -= OnLevelUp; // Đảm bảo không bị trùng
            levelSystem.OnLevelDataUpdated += OnLevelUp;
        }

        //DontDestroyOnLoad(rewardUI);

        collectButton.onClick.AddListener(OnCollectButtonPressed);

        SetupRewardUI();
    }

    private void Start()
    {
        rewardUI.SetActive(false);
    }

    private void SetupRewardUI()
    {
        if (rewardUIInstance == null)
        {
            rewardUIInstance = Instantiate(rewardUI);
            DontDestroyOnLoad(rewardUIInstance);
        }

        rewardUIInstance.SetActive(false);

        levelText = GameObject.FindGameObjectWithTag("LevelText").GetComponent<TextMeshProUGUI>();
        coinType1Text = GameObject.FindGameObjectWithTag("CoinType1Text").GetComponent<TextMeshProUGUI>();
        coinType2Text = GameObject.FindGameObjectWithTag("CoinType2Text").GetComponent<TextMeshProUGUI>();
        healthText = GameObject.FindGameObjectWithTag("HealthText").GetComponent<TextMeshProUGUI>();
        collectButton = GameObject.FindGameObjectWithTag("CollectButton").GetComponent<Button>();

        collectButton.onClick.AddListener(OnCollectButtonPressed);
        rewardUIInstance.SetActive(false);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Kiểm tra nếu đang chuyển sang Scene Login, hủy LevelSystem
        if (scene.name == "Login") { }
    }

    private async void OnLevelUp(int newLevel, int experience, int experienceToNextLevel)
    {
        lastRewardedLevel = LevelSystem.Instance.data.lastRewardedLevel;
        bool hasReward = false;

        if (newLevel > lastRewardedLevel) // Đảm bảo chỉ cấp phát khi có level mới
        {
            for (int level = lastRewardedLevel + 1; level <= newLevel; level++)
            {
                Debug.Log("Lsrw" + level);
                // Kiểm tra nếu cấp độ này chưa được nhận phần thưởng
                if (level <= levelRewards.Length && level > 0) // Thay đổi điều kiện check
                {
                    Debug.Log("Lsrw" + level);
                    // Chỉ cấp phát phần thưởng cho cấp độ hiện tại
                    LevelReward reward = levelRewards[level - 1];
                    pendingRewards.Enqueue(reward);
                    hasReward = true;
                    //await ApplyReward(reward);
                }
            }
            // Cập nhật cấp độ thưởng cuối cùng sau khi đã cấp phát xong tất cả các phần thưởng
            await levelSystem.UpdateLastRewardedLevel(newLevel);
            if (hasReward) ShowNextReward();
        }
    }

    private async void ShowNextReward()
    {
        if (pendingRewards.Count == 0)
        {
            rewardUI.SetActive(false);
            isRewardUIActive = false;
            return;
        }

        if (pendingRewards.Count > 0 && !isRewardUIActive)
        {
            LevelReward reward = pendingRewards.Dequeue();

            int nextLevel = lastRewardedLevel + 1;

            levelText.text = $"Level up! You are now level {nextLevel}";

            coinType1Text.text = $"Coins 1 + {reward.coinType1}";
            coinType2Text.text = $"Coins 2 + {reward.coinType2}";
            healthText.text = $"Health + {reward.health}";

            rewardUI.SetActive(true);
            isRewardUIActive = true;

            lastRewardedLevel = nextLevel;

            await ApplyReward(reward);
        }
    }


    public void OnCollectButtonPressed()
    {
        rewardUI.SetActive(false);
        isRewardUIActive = false;

        if (pendingRewards.Count > 0)
        {
            ShowNextReward();
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
        int baseValue = 100;

        if (level >= 10 && level <= 19)
        {
            baseValue += 50;
        }
        else if (level >= 20 && level <= 25)
        {
            baseValue += 100;
        }
        else if (level >= 26 && level <= 30)
        {
            baseValue += 150;
        }

        return (100 * level) + (50 * level) + baseValue;
    }
}
