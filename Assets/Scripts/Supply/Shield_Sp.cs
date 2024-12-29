using UnityEngine;

public class Shield_Sp : MonoBehaviour, ISupplyActive
{
    [SerializeField] private bool oneTimeActive = true;
    [SerializeField] private int shield;

    public  void Active()
    {
        Debug.Log("Active va hoi lai Shield");
        oneTimeActive = false;
    }

    public  void CanActive()
    {
        oneTimeActive = true;
    }

    public void CheckCollider()
    {

    }


}
