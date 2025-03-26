using UnityEngine;

public class GunPositions : MonoBehaviour
{
    public static GunPositions Instance;
    public Transform[] gunPositions;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
