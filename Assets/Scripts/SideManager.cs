using UnityEngine;

public class SideManager : MonoBehaviour
{
    private Transform player;

    public bool IsOnLeft => player.position.x < transform.position.x;
    public bool IsOnRight => player.position.x >= transform.position.x;

    private void Start()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");

        player = playerObj.transform;
    }
    void Update()
    {
        //if (IsOnLeft)
        //{
        //    Debug.Log("Player is on the Left side.");
        //}
        //else if (IsOnRight)
        //{
        //    Debug.Log("Player is on the Right side.");
        //}
    }
}
