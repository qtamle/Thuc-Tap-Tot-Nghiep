using UnityEngine;

[CreateAssetMenu(fileName = "PlayerHealthData", menuName = "Scriptable Objects/PlayerHealthData")]
public class PlayerHealthData : ScriptableObject
{
    public int currentHealth;
    public int currentShield;
}
