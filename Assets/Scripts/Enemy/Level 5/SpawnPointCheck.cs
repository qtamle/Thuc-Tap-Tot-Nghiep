using UnityEngine;

public class SpawnPointCheck : MonoBehaviour
{
    public static SpawnPointCheck Instance;

    [Header("Skill 2 Settings")]
    [SerializeField]
    public GameObject[] SpamPointsLeft;

    [SerializeField]
    public GameObject[] SpamPointsRight;
}
