using System.Collections;
using UnityEngine;

public class Shield_Sp : MonoBehaviour, ISupplyActive
{
    [SerializeField] private bool isActive = true;
    [SerializeField] private float cooldownTime = 15f; // Cooldown riêng cho Gernade

    public float CooldownTime => cooldownTime; // Triển khai CooldownTime từ interface

    public void Active()
    {
        Debug.Log("Active va hoi lai Shield");
        isActive = false;
    }

    public  void CanActive()
    {
        isActive = true;
    }
    public bool IsReady()
    {
        return isActive;
    }

    private IEnumerator CooldownRoutine()
    {
        Debug.Log("Shield cooldown started...");
        yield return new WaitForSeconds(cooldownTime);
        CanActive();
        Debug.Log("Shield is ready!");
    }


}
