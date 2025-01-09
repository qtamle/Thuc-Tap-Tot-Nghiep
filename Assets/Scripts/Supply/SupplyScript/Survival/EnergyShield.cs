using System.Collections;
using UnityEngine;

public class EnergyShield : MonoBehaviour, ISupplyActive
{
    public SupplyData supplyData;
    [SerializeField] private bool isActive;
    [SerializeField] private float cooldownTime;

    [SerializeField] private int shieldCharges = 3;
    [SerializeField] private int remainingCharges;

    public float CooldownTime => cooldownTime;

    private void Start()
    {
        remainingCharges = shieldCharges;
    }

    public void Active()
    {
        if (!IsReady())
        {
            return;
        }

        isActive = false;
        remainingCharges = shieldCharges;
        StartCoroutine(CooldownRoutine());
    }

    public void CanActive()
    {
        isActive = false;
    }

    public bool IsReady()
    {
        return isActive;
    }

    private IEnumerator CooldownRoutine()
    {
        yield return new WaitForSeconds(cooldownTime);
        CanActive();
    }

    public bool TryBlockDamage()
    {
        if (remainingCharges > 0)
        {
            remainingCharges--; 
            return true;
        }
        return false; 
    }
}
