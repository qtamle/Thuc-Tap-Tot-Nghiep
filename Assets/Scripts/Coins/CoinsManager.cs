using TMPro;
using UnityEngine;
using System;
using System.IO;

[Serializable]
public class CoinsData
{
    public int coinType1Count;
    public int coinType2Count;
}

public class CoinsManager : MonoBehaviour
{
    public int coinType1Count = 0;
    public int coinType2Count = 0;

    public TMP_Text coinType1Text;
    public TMP_Text coinType2Text;

    private void Start()
    {
        // Đọc dữ liệu JSON khi bắt đầu Scene
        LoadCoins();
        UpdateCoinUI();
    }

    public void UpdateCoinUI()
    {
        if (coinType1Text != null)
        {
            coinType1Text.text = "Coin Type 1: " + coinType1Count.ToString();
        }
        else
        {
            Debug.LogWarning("coinType1Text is not assigned in the Inspector!");
        }

        if (coinType2Text != null)
        {
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

    // Lưu dữ liệu dạng JSON vào tệp
    public void SaveCoins()
    {
        CoinsData coinsData = new CoinsData
        {
            coinType1Count = coinType1Count,
            coinType2Count = coinType2Count
        };

        string json = JsonUtility.ToJson(coinsData, true);

        // Đường dẫn thư mục và tệp
        string folderPath = Application.dataPath + "/Data";
        string filePath = folderPath + "/CoinsData.json";

        // Lưu dữ liệu vào tệp
        File.WriteAllText(filePath, json);
        Debug.Log("Coins data saved to: " + filePath);
    }


    // Tải dữ liệu JSON từ tệp
    public void LoadCoins()
    {
        // Đường dẫn thư mục và tệp
        string folderPath = Application.dataPath + "/Data";
        string filePath = folderPath + "/CoinsData.json";

        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            CoinsData coinsData = JsonUtility.FromJson<CoinsData>(json);

            coinType1Count = coinsData.coinType1Count;
            coinType2Count = coinsData.coinType2Count;
        }
        else
        {
            // Nếu tệp không tồn tại, khởi tạo số coin bằng 0
            coinType1Count = 0;
            coinType2Count = 0;
        }
    }

    private void OnDisable()
    {
        // Lưu dữ liệu khi thoát Scene
        SaveCoins();
    }
}
