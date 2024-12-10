using UnityEngine;

[CreateAssetMenu(fileName = "NewEquipment", menuName = "Weapons/Equipment")]
public class Equipment : ScriptableObject
{
    public string equipmentName;
    public string description;
    public Sprite equipmentSprite;
    public int benefitValue; // Giá trị lợi ích (VD: tăng damage, tốc độ, máu, v.v.)
    public string benefitType; // Loại lợi ích (VD: "Damage", "Speed", v.v.)
}
