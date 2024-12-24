using UnityEngine;

public class Floor : MonoBehaviour
{
    public delegate void FloorEvent(string floorName);
    public static event FloorEvent OnPlayerEnter;
    public static event FloorEvent OnPlayerExit;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            OnPlayerEnter?.Invoke(gameObject.name);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            OnPlayerExit?.Invoke(gameObject.name);
        }
    }
}
