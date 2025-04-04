using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Shield_Sp : MonoBehaviour, ISupplyActive
{
    public SupplyData supplyData;
    [SerializeField] private bool isActive = true;
    [SerializeField] private float cooldownTime = 15f; 

    public float CooldownTime => cooldownTime;

    private PlayerHealth healPlayer;

    private void Start()
    {
        //healPlayer = FindFirstObjectByType<PlayerHealth>();
        //if (healPlayer != null)
        //{
        //    Debug.Log("Tim thay player shield");
        //}
    }

    public void Active()
    {
        if (!IsReady())
        {
            Debug.Log($"HealthKit is on cooldown!");
            return;
        }

        Debug.Log("Active and Heal Player");
        //HealShieldPlayer();
        isActive = false;
        StartCoroutine(CooldownRoutine());
    }

    private void HealShieldPlayer()
    {
        healPlayer.HealShield(30);
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
