[System.Serializable]
public class PlayerData
{
    // Thông tin cơ bản
    public int PlayerHealth { get; set; }

    public int PlayerShield { get; set; }

    public float PlayerSpeed { get; set; }
    public int Level { get; set; }

    // Tiền tệ
    public int Gold { get; set; }
    public int Gems { get; set; }

    // Tiến trình
    public int Experience { get; set; }
    public int NextLevelExperience { get; set; }
}
