using UnityEngine;

public class FloorCheck : MonoBehaviour
{
    public string CurrentFloor { get; private set; }

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

    private void SetCurrentFloor(string floorName)
    {
        CurrentFloor = floorName;
        //Debug.Log($"Player is on {CurrentFloor}");
    }

    private void ClearCurrentFloor(string floorName)
    {
        if (CurrentFloor == floorName)
        {
            CurrentFloor = null;
            //Debug.Log("Player left the floor.");
        }
    }
}
