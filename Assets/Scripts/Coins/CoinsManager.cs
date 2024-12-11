using TMPro;
using UnityEngine;

public class CoinsManager : MonoBehaviour
{
    public int coinType1Count = 0; 
    public int coinType2Count = 0;

    public TMP_Text coinType1Text;
    public TMP_Text coinType2Text;

    // Hàm cập nhật UI
    public void UpdateCoinUI()
    {
        if (coinType1Text != null)
        {
            Debug.Log("Updating Coin Type 1 UI: " + coinType1Count);
            coinType1Text.text = "Coin Type 1: " + coinType1Count.ToString();
        }
        else
        {
            Debug.LogWarning("coinType1Text is not assigned in the Inspector!");
        }

        if (coinType2Text != null)
        {
            Debug.Log("Updating Coin Type 2 UI: " + coinType2Count);
            coinType2Text.text = "Coin Type 2: " + coinType2Count.ToString();
        }
        else
        {
            Debug.LogWarning("coinType2Text is not assigned in the Inspector!");
        }
    }

    public void AddCoins(int type1, int type2)
    {
        coinType1Count += type1;
        coinType2Count += type2;
        UpdateCoinUI(); 
    }
}
