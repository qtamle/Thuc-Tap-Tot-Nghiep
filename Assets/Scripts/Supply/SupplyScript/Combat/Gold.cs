using System.Collections;
using UnityEngine;

public class Gold : MonoBehaviour, ISupplyActive
{
    [Header("Gold Increase")]
    public int increaseGoldChange = 10;

    public SupplyData supplyData;
    [SerializeField] private bool isActive;
    [SerializeField] private float cooldownTime;

    public float CooldownTime => cooldownTime;

    public void Active()
    {
        if (!IsReady())
        {
            return;
        }

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
        yield return new WaitForSeconds(cooldownTime);
        CanActive();
    }
}