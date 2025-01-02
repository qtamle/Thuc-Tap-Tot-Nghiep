using UnityEngine;

[CreateAssetMenu(fileName = "HandleBoss", menuName = "HandleBosses/CreateBoss")]
public class HandleBoss : ScriptableObject
{
    public string bossName;
    public bool isDeaftead;
    public string nextScene;
    public string supplyScene = "SupplyScene";
}
