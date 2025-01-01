using System.Collections;
using UnityEngine;

public class Medkit_Supply : MonoBehaviour, ISupplyActive
{
    [SerializeField] private bool isActive = true;
    [SerializeField] private float cooldownTime = 5f; // Cooldown riêng cho HealthKit
    [SerializeField] private int healAmount;

    public float CooldownTime => cooldownTime;

    public void Active()
    {
        if (!IsReady())
        {
            Debug.Log($"HealthKit is on cooldown!");
            return;
        }

        Debug.Log("Active and Heal Player");
        isActive = false;
        StartCoroutine(CooldownRoutine());
    }

    public void CanActive()
    {
        isActive = true;
    }

    public bool IsReady()
    {
        return isActive;
    }

    private IEnumerator CooldownRoutine()
    {
        Debug.Log("HealthKit cooldown started...");
        yield return new WaitForSeconds(cooldownTime);
        CanActive();
        Debug.Log("HealthKit is ready!");
    }

}
