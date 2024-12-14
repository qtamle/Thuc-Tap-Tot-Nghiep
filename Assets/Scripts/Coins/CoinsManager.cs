using TMPro;
using UnityEngine;
using System;
using System.IO;

[Serializable]
public class CoinsData
{
    public int totalCoinType1Count; // Tổng coin Type 1
    public int totalCoinType2Count; // Tổng coin Type 2
}

public class CoinsManager : MonoBehaviour
{
    public int coinType1Count = 0; // Coin hiện tại trong Scene
    public int coinType2Count = 0; // Coin hiện tại trong Scene

    public int totalCoinType1Count = 0; // Tổng coin (đã cộng dồn)
    public int totalCoinType2Count = 0; // Tổng coin (đã cộng dồn)

    public TMP_Text coinType1Text;
    public TMP_Text coinType2Text;

    private void Start()
    {
        // Đọc dữ liệu JSON khi bắt đầu Scene
        LoadCoins();

        // Reset coin hiện tại về 0 khi bắt đầu Scene
        coinType1Count = 0;
        coinType2Count = 0;

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

    // Lưu dữ liệu tổng coin dạng JSON vào tệp
    public void SaveCoins()
    {
        // Cộng dồn coin hiện tại vào tổng coin
        totalCoinType1Count += coinType1Count;
        totalCoinType2Count += coinType2Count;

        CoinsData coinsData = new CoinsData
        {
            totalCoinType1Count = totalCoinType1Count,
            totalCoinType2Count = totalCoinType2Count
        };

        string json = JsonUtility.ToJson(coinsData, true);

        // Đường dẫn thư mục và tệp
        string folderPath = Application.dataPath + "/Data";
        string filePath = folderPath + "/CoinsData.json";

        // Tạo thư mục nếu chưa tồn tại
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        // Lưu dữ liệu vào tệp
        File.WriteAllText(filePath, json);
        Debug.Log("Coins data saved to: " + filePath);
    }

    // Tải dữ liệu tổng coin từ JSON
    public void LoadCoins()
    {
        // Đường dẫn thư mục và tệp
        string folderPath = Application.dataPath + "/Data";
        string filePath = folderPath + "/CoinsData.json";

        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            CoinsData coinsData = JsonUtility.FromJson<CoinsData>(json);

            // Gán tổng số coin đã lưu
            totalCoinType1Count = coinsData.totalCoinType1Count;
            totalCoinType2Count = coinsData.totalCoinType2Count;
        }
        else
        {
            // Nếu tệp không tồn tại, khởi tạo giá trị
            totalCoinType1Count = 0;
            totalCoinType2Count = 0;
        }
    }

    private void OnDisable()
    {
        // Lưu dữ liệu khi thoát Scene
        SaveCoins();
    }
}
