using UnityEngine;

public class Floor : MonoBehaviour
{
    public delegate void FloorEvent(string floorName, Transform leftTransform, Transform rightTransform);
    public static event FloorEvent OnPlayerEnter;
    public static event FloorEvent OnPlayerExit;

    public Transform LeftFloor; // Gắn transform của LeftFloor
    public Transform RightFloor; // Gắn transform của RightFloor

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            OnPlayerEnter?.Invoke(gameObject.name, LeftFloor, RightFloor);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            OnPlayerExit?.Invoke(gameObject.name, LeftFloor, RightFloor);
        }
    }
}
