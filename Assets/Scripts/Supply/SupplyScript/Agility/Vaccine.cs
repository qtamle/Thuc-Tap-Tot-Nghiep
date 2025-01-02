using System.Collections;
using UnityEngine;

public class Vaccine : MonoBehaviour, ISupplyActive
{
    public SupplyData supplyData;
    [SerializeField] private bool isActive;
    [SerializeField] private float cooldownTime;
    public float CooldownTime => cooldownTime;

    public bool IsEffectActive { get; private set; } = false;
    private float effectDuration = 5f; 
    private float originalSpeed; 

    private PlayerMovement playerMovement;

    private void Awake()
    {
        playerMovement = FindFirstObjectByType<PlayerMovement>();
    }

    public void Active()
    {
        if (!IsReady())
        {
            return;
        }

        isActive = false;
        StartCoroutine(CooldownRoutine());
        StartCoroutine(RandomEffectLoop()); 
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

    private IEnumerator RandomEffectLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(5f);

            yield return ApplyEffect();

        }
    }

    private IEnumerator ApplyEffect()
    {
        IsEffectActive = true;

        originalSpeed = playerMovement.moveSpeed;

        float randomEffect = Random.value;
        if (randomEffect > 0.5f)
        {
            playerMovement.moveSpeed += 2.5f; 
            Debug.Log("Tăng tốc độ!");
        }
        else
        {
            playerMovement.moveSpeed -= 1.5f;  
            Debug.Log("Giảm tốc độ!");
        }

        yield return new WaitForSeconds(effectDuration);

        playerMovement.moveSpeed = originalSpeed;
        IsEffectActive = false;

        Debug.Log("Khôi phục tốc độ ban đầu.");
    }
}
