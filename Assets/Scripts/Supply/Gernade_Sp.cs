using UnityEngine;
using System.Collections;

public class Gernade_Sp : MonoBehaviour, ISupplyActive
{
    public SupplyData supplyData;
    [SerializeField] private bool isActive;
    [SerializeField] private float cooldownTime; // Cooldown riêng cho Gernade
    [SerializeField] private int damage;

    public float CooldownTime => cooldownTime; // Triển khai CooldownTime từ interface

    public void Active()
    {
        if (!IsReady())
        {
            return;
        }

        Debug.Log("Active and Throw Gernade");
        isActive = false;
        StartCoroutine(CooldownRoutine());
    }

        
    public void CanActive()
    {
        isActive = false;
    }
    public bool IsReady()
    {
        return isActive; // Kiểm tra trạng thái sẵn sàng
    }

    private IEnumerator CooldownRoutine()
    {
        Debug.Log($"{supplyData.supplyName} cooldown started...");
        yield return new WaitForSeconds(cooldownTime);
        CanActive();
        Debug.Log($"{supplyData.supplyName} is ready!");
    }
}
