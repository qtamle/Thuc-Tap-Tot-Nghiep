using UnityEngine;

[CreateAssetMenu(fileName = "NewEquipment", menuName = "Weapons/Equipment")]
public class Equipment : ScriptableObject
{
    public string equipmentName;
    public string description;
    public Sprite equipmentSprite;
    public int benefitValue;
    public string benefitType; 
}
