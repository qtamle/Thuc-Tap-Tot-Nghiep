using UnityEngine;

public class MoveBoomSkill : MonoBehaviour
{
    public static MoveBoomSkill Instance;
    public Transform[] wayPoints;

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
