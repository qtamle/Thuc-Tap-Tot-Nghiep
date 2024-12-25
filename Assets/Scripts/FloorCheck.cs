using UnityEngine;

public class FloorCheck : MonoBehaviour
{
    public string CurrentFloor { get; private set; }
    public Transform CurrentLeftFloor { get; private set; }
    public Transform CurrentRightFloor { get; private set; }

    private void OnEnable()
    {
        Floor.OnPlayerEnter += SetCurrentFloor;
        Floor.OnPlayerExit += ClearCurrentFloor;
    }

    private void OnDisable()
    {
        Floor.OnPlayerEnter -= SetCurrentFloor;
        Floor.OnPlayerExit -= ClearCurrentFloor;
    }

    private void SetCurrentFloor(string floorName, Transform leftTransform, Transform rightTransform)
    {
        CurrentFloor = floorName;
        CurrentLeftFloor = leftTransform;
        CurrentRightFloor = rightTransform;

        Debug.Log($"Player is on {CurrentFloor} with Left: {CurrentLeftFloor?.name}, Right: {CurrentRightFloor?.name}");
    }

    private void ClearCurrentFloor(string floorName, Transform leftTransform, Transform rightTransform)
    {
        if (CurrentFloor == floorName)
        {
            CurrentFloor = null;
            CurrentLeftFloor = null;
            CurrentRightFloor = null;

            Debug.Log("Player left the floor.");
        }
    }
}
