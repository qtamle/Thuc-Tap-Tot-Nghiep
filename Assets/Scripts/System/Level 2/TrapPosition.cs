using UnityEngine;

public class TrapPosition : MonoBehaviour
{
    public static TrapPosition Instance;
    public Transform[] targetPositions;
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
