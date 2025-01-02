using UnityEngine;

[CreateAssetMenu(fileName = "HandleBoss", menuName = "HandleBosses/CreateBoss")]
public class HandleBoss : ScriptableObject
{
    public string bossName; // Tên của boss
    public bool isDefeated; 
    public string nextScene; // Cảnh tiếp theo sau boss
    public string supplyScene = "SupplyScene"; // Cảnh tiếp tế
}

