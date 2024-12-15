using UnityEngine;
using TMPro;

public class CoinsUI : MonoBehaviour
{
    public TMP_Text coinType1Text;   
    public TMP_Text coinType2Text; 
    private int currentCoinType1;
    private int currentCoinType2;

    private CoinsManager coinsManager; 

    private void Start()
    {
        coinsManager = UnityEngine.Object.FindFirstObjectByType<CoinsManager>();

        // Khởi tạo giá trị ban đầu
        if (coinsManager != null)
        {
            currentCoinType1 = (int)coinsManager.coinType1Count;
            currentCoinType2 = (int)coinsManager.coinType2Count;
        }

        // Cập nhật UI lần đầu tiên
        UpdateCoinsUI();
    }

    private void Update()
    {
        if (coinsManager != null)
        {
            if (currentCoinType1 != (int)coinsManager.coinType1Count || currentCoinType2 != (int)coinsManager.coinType2Count)
            {
                currentCoinType1 = (int)coinsManager.coinType1Count;
                currentCoinType2 = (int)coinsManager.coinType2Count;

                UpdateCoinsUI();
            }
        }
    }

    private void UpdateCoinsUI()
    {
        coinType1Text.text = "Coin Type 1: " + currentCoinType1;
        coinType2Text.text = "Coin Type 2: " + currentCoinType2;
    }
}
