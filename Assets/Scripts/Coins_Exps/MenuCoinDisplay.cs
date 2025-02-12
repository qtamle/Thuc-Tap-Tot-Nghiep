using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class MenuCoinDisplay : MonoBehaviour
{
    public TMP_Text coinType1Text;
    public TMP_Text coinType2Text;

    private CoinsManager coinsManager;

    private async void Start()
    {
        coinsManager = FindFirstObjectByType<CoinsManager>();

        if (coinsManager != null)
        {
            coinsManager.OnCoinsUpdated += UpdateCoinUI;
        }

        await LoadAndDisplayCoins();
    }

    private async Task LoadAndDisplayCoins()
    {
        CoinsData coinsData = await SaveService.LoadCoinData();

        if (coinsData != null)
        {
            if (coinType1Text != null)
                coinType1Text.text = coinsData.totalCoinType1Count.ToString();

            if (coinType2Text != null)
                coinType2Text.text = coinsData.totalCoinType2Count.ToString();

            Debug.Log(
                $"✅ Loaded CloudSave: Coin1 = {coinsData.totalCoinType1Count}, Coin2 = {coinsData.totalCoinType2Count}"
            );
        }
        else
        {
            // Nếu không có dữ liệu, hiển thị mặc định là 0
            if (coinType1Text != null)
                coinType1Text.text = "0";

            if (coinType2Text != null)
                coinType2Text.text = "0";

            Debug.LogWarning("⚠ No coin data found in CloudSave. Displaying default values.");
        }
    }

    private void UpdateCoinUI(int totalCoinType1, int totalCoinType2)
    {
        if (coinType1Text != null)
            coinType1Text.text = totalCoinType1.ToString();

        if (coinType2Text != null)
            coinType2Text.text = totalCoinType2.ToString();
    }

    private void OnDestroy()
    {
        if (coinsManager != null)
        {
            coinsManager.OnCoinsUpdated -= UpdateCoinUI;
        }
    }
}
