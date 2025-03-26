using UnityEngine;

public class ShootingPointPosition : MonoBehaviour
{
    public static ShootingPointPosition Instance;
    public Transform[] shootingPoints;

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
