using System.Collections;
using UnityEngine;

public class Sacrifice : MonoBehaviour, ISupplyActive
{
    public SupplyData supplyData;
    [SerializeField] private bool isActive = true;
    [SerializeField] private float cooldownTime = 5f;

    public float CooldownTime => cooldownTime;

    public void Active()
    {
        if (!IsReady())
        {
            return;
        }

        Debug.Log("Sacrifice!");
        isActive = false;
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
}
