using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CoinsManager : MonoBehaviour
{
    public event Action<int, int> OnCoinsUpdated;

    public int coinType1Count = 0; // Coin hiện tại trong Scene
    public int coinType2Count = 0; // Coin hiện tại trong Scene

    public int totalCoinType1Count = 0; // Tổng coin lưu vào CloudSave
    public int totalCoinType2Count = 0; // Tổng coin lưu vào CloudSave

    public TMP_Text coinType1Text;
    public TMP_Text coinType2Text;

    private static CoinsManager instance; // Giữ CoinsManager tồn tại giữa các Scene
    public static CoinsManager Instance => instance;

    public CoinsData coinsData;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        // Đăng ký sự kiện khi scene thay đổi
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Kiểm tra nếu đang chuyển sang Scene Login, hủy LevelSystem
        if (scene.name == "Login")
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        // Hủy đăng ký sự kiện khi đối tượng bị hủy
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private async void Start()
    {
        if (instance == this) // Chỉ load 1 lần
        {
            await LoadCoinsFromCloud();
        }

        UpdateCoinUIMenu();
    }

    public void UpdateCoinUI()
    {
        if (coinType1Text != null)
            coinType1Text.text = "Coin Type 1: " + coinType1Count.ToString();

        if (coinType2Text != null)
            coinType2Text.text = "Coin Type 2: " + coinType2Count.ToString();
    }

    public void UpdateCoinUIMenu()
    {
        if (coinType1Text != null)
            coinType1Text.text = "Coin Type 1: " + totalCoinType1Count.ToString();

        if (coinType2Text != null)
            coinType2Text.text = "Coin Type 2: " + totalCoinType2Count.ToString();
    }

    public void AddCoins(int type1, int type2)
    {
        coinType1Count += type1;
        coinType2Count += type2;

        UpdateCoinUI();
    }

    public void AddCoinsTotal(CoinsData coinsData)
    {
        // Cộng dồn số tiền kiếm được từ Scene 2 vào tổng tiền của Scene 1
        coinsData.totalCoinType1Count += coinType1Count;
        coinsData.totalCoinType2Count += coinType2Count;

        // Reset tiền kiếm được trong Scene 2 về 0
        coinType1Count = 0;
        coinType2Count = 0;
    }

    public async Task<bool> TryPurchase(int basePrice)
    {
        // 🔹 Kiểm tra đủ tiền không
        if (totalCoinType1Count < basePrice)
        {
            Debug.LogWarning(
                $"Not enough coins to purchase. Required: {basePrice}, Available: {totalCoinType1Count}"
            );
            return false;
        }
        if (coinsData == null)
        {
            Debug.LogError("⚠ Failed to load CoinsData. Purchase aborted.");
            return false;
        }

        // 🔹 Trừ tiền trong dữ liệu CoinsData
        coinsData.totalCoinType1Count = Mathf.Max(coinsData.totalCoinType1Count - basePrice, 0);

        // 🔹 Cập nhật vào CoinsManager
        totalCoinType1Count = coinsData.totalCoinType1Count;

        // 🔹 Lưu dữ liệu mới lên CloudSave
        await SaveService.SaveCoinData(coinsData);

        // 🔹 Gọi sự kiện cập nhật UI nếu có
        OnCoinsUpdated?.Invoke(totalCoinType1Count, totalCoinType2Count);

        Debug.Log(
            $"✅ Purchase successful! Spent {basePrice} coins. Remaining: {totalCoinType1Count}"
        );

        return true;
    }

    public async Task<bool> TryUpgrade(int upgradeCost)
    {
        if (totalCoinType2Count >= upgradeCost)
        {
            coinsData.totalCoinType2Count = Mathf.Max(
                coinsData.totalCoinType2Count - upgradeCost,
                0
            );
            // 🔹 Cập nhật vào CoinsManager
            totalCoinType2Count = coinsData.totalCoinType2Count;

            OnCoinsUpdated?.Invoke(totalCoinType1Count, totalCoinType2Count);

            Debug.Log(
                $"🔹 Upgrade successful! Spent {upgradeCost} coins. Remaining: {totalCoinType2Count}"
            );

            // 🔹 Lưu số coin còn lại lên CloudSave
            await SaveService.SaveCoinData(coinsData);

            return true;
        }
        else
        {
            Debug.LogWarning(
                $"❌ Not enough coins to upgrade. Required: {upgradeCost}, Available: {totalCoinType2Count}"
            );
            return false;
        }
    }

    public async void SaveCoinsToCloud()
    {
        AddCoinsTotal(coinsData);

        // Reset coin hiện tại (Scene 2 & 3)
        coinType1Count = 0;
        coinType2Count = 0;

        // Lưu dữ liệu cập nhật lên CloudSave
        await SaveService.SaveCoinData(coinsData);

        // Cập nhật UI
        totalCoinType1Count = coinsData.totalCoinType1Count;
        totalCoinType2Count = coinsData.totalCoinType2Count;
        UpdateCoinUIMenu();

        Debug.Log(
            $"✅ Updated CloudSave: TotalCoin1 = {totalCoinType1Count}, TotalCoin2 = {totalCoinType2Count}"
        );
    }

    public async Task LoadCoinsFromCloud()
    {
        coinsData = await SaveService.LoadCoinData();

        totalCoinType1Count = coinsData.totalCoinType1Count;
        totalCoinType2Count = coinsData.totalCoinType2Count;

        UpdateCoinUIMenu();
        Debug.Log(
            $"✅ Loaded CloudSave: TotalCoin1 = {totalCoinType1Count}, TotalCoin2 = {totalCoinType2Count}"
        );
    }
}
