using UnityEngine;

public class DashTarget : MonoBehaviour
{
    public static DashTarget Instance;
    public Transform[] dashTargets;

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
