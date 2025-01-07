using System.Collections;
using UnityEngine;

public class Medkit_Supply : MonoBehaviour, ISupplyActive
{
    public SupplyData supplyData;
    [SerializeField] private bool isActive = true;
    [SerializeField] private float cooldownTime = 5f; 

    public float CooldownTime => cooldownTime;

    private PlayerHealth healthPlayer;

    public int healHealthAmount;

    private void Start()
    {
        healthPlayer = FindFirstObjectByType<PlayerHealth>();
    }

    public void Active()
    {
        if (!IsReady())
        {
            Debug.Log($"HealthKit is on cooldown!");
            return;
        }

        Debug.Log("Active and Heal Player");
        isActive = false;
        HealPlayer();
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

    private void HealPlayer()
    {
        if (healthPlayer != null)
        {
            healthPlayer.HealHealth(healHealthAmount);
        }
    }
}
