using UnityEngine;

public class BoomPostion : MonoBehaviour
{
    public static BoomPostion Instance;
    public Transform[] bombPositions;

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
