using TMPro;
using UnityEngine;
using System.IO;

public class MenuCoinDisplay : MonoBehaviour
{
    public TMP_Text coinType1Text;
    public TMP_Text coinType2Text;

    private void Start()
    {
        LoadAndDisplayCoins();
    }

    private void LoadAndDisplayCoins()
    {
        // Đường dẫn tệp JSON
        string folderPath = Application.dataPath + "/Data";
        string filePath = folderPath + "/CoinsData.json";


        if (File.Exists(filePath))
        {
            // Đọc dữ liệu từ tệp JSON
            string json = File.ReadAllText(filePath);
            CoinsData coinsData = JsonUtility.FromJson<CoinsData>(json);

            // Hiển thị dữ liệu trên UI
            if (coinType1Text != null)
                coinType1Text.text = "" + coinsData.coinType1Count;

            if (coinType2Text != null)
                coinType2Text.text = "" + coinsData.coinType2Count;

            Debug.Log("Coins data loaded from JSON: " + json);
        }
        else
        {
            // Nếu không tìm thấy tệp JSON, hiển thị mặc định
            if (coinType1Text != null)
                coinType1Text.text = "0";

            if (coinType2Text != null)
                coinType2Text.text = "0";

            Debug.LogWarning("CoinsData.json not found. Displaying default values.");
        }
    }
}
