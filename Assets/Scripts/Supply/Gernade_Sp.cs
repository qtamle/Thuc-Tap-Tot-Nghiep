using UnityEngine;

public class Gernade_Sp : MonoBehaviour , ISupplyActive
{
    public SupplyData supplyData;
    [SerializeField] private bool isActive = true;
    [SerializeField] private int damage;

    public void Active()
    {
        Debug.Log("Active va Throw Gernade");
        isActive = false;
    }

    public void CanActive()
    {
        isActive = true;
    }

    public void CheckCollider()
    {

    }


}
