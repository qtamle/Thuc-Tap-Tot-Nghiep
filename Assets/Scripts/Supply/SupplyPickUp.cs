using System.Collections;
using UnityEngine;

public class SupplyPickup : MonoBehaviour
{
    public SupplyData supplyData;
    private bool isTransitioning = false;
    private bool canTrigger = true;
    private SupplyInfoDisplay infoDisplay;

    private void Start()
    {
        infoDisplay = FindFirstObjectByType<SupplyInfoDisplay>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && canTrigger)
        {
            if (infoDisplay != null)
            {         
                infoDisplay.DisplaySupplyInfo(this);
            }
        }
    }

    public void PickupSupply()
    {
        Debug.Log($"Player đã nhặt {supplyData.supplyName}.");
        SupplyManager.Instance.RemoveSupply(supplyData);
        SupplyManager.Instance.AddToInventory(supplyData);
        ApplyEffect();

        if (BossManager.Instance != null)
        {
            ProceedToNextBossScene();
        }

        Destroy(gameObject);
    }

    private void ProceedToNextBossScene()
    {
        if (isTransitioning) return;

        isTransitioning = true;
        StartCoroutine(WaitForBossDefeatAndProceed());
    }

    private IEnumerator WaitForBossDefeatAndProceed()
    {
        while (!BossManager.Instance.CurrentBoss.isDefeated)
        {
            yield return null;
        }

        Debug.Log("Đã chuyển sang Boss tiếp theo: " + BossManager.Instance.CurrentBoss.bossName);
        BossManager.Instance.NextBossScene(BossManager.Instance.CurrentBoss);
        BossManager.Instance.SetNextBossAfterSceneLoad();
    }

    private void ApplyEffect()
    {
        Debug.Log($"Đã áp dụng hiệu ứng của {supplyData.supplyName}.");
    }

    public void StartDisableTriggerTimer()
    {
        StartCoroutine(DisableTriggerTemporarily());
    }

    private IEnumerator DisableTriggerTemporarily()
    {
        canTrigger = false;
        yield return new WaitForSeconds(2f);
        canTrigger = true;
    }
}
