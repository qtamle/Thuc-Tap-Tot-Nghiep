using UnityEngine;

public class BoomPosition : MonoBehaviour
{
    public static BoomPosition Instance;
    public Transform[] randomBombTargets;

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
