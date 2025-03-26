using UnityEngine;

public class FireBoomTransfrom : MonoBehaviour
{
    public static FireBoomTransfrom Instance;
    public Transform[] targetTransformBomb;
    public Transform[] newTarget;

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
