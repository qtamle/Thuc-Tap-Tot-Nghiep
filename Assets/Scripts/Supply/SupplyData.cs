using UnityEngine;

public enum SupplyType
{
    Survival,
    Combat,
    Agility
}

[CreateAssetMenu(fileName = "SupplyData", menuName = "Supply/SupplyData")]
public class SupplyData : ScriptableObject
{
    public string supplyName;
    public GameObject supplyPrefab;
    public Sprite supplySprite;
    public SupplyType supplyType;
    public float spawnWeight; // Tỷ lệ spawn

    [TextArea]
    public string description;
}
