using UnityEngine;

public class Medkit_Supply : MonoBehaviour, ISupplyActive
{
    [SerializeField] bool oneTimeActive = true;
    [SerializeField] int healing;

   

    public  void Active()
    {
        Debug.Log("Sau khi Active se Heal cho nguoi choi");
        oneTimeActive = false;
    }

    public  void CanActive()
    {
        oneTimeActive = true;
    }

}
