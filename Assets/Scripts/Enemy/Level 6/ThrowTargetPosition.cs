using UnityEngine;

public class ThrowTargetPosition : MonoBehaviour
{
    public static ThrowTargetPosition Instance;
    public Transform[] throwTargets;

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
