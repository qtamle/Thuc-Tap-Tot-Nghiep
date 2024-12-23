using UnityEngine;

public class MachineSnakeHealth : MonoBehaviour
{
    [Header("Boss Health Reference")]
    private SnakeHealth bossHealth;

    [Header("Sprite Settings")]
    private SpriteRenderer spriteRenderer;
    public Color hitColor = new Color(1f, 0.5f, 0.5f);

    private Color originalColor;
    private bool isHit = false;
    private bool isInvulnerable = false;
    public bool isAlreadyHit = false;

    [Header("Part ID")]
    public int partID;

    public static int attackedPartID = -1;

    private void Start()
    {
        bossHealth = GetComponentInParent<SnakeHealth>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        else
        {
            Debug.LogError("SpriteRenderer is not assigned on " + gameObject.name);
        }
    }

    public void TakeDamage(int damage)
    {
        if (isInvulnerable || bossHealth == null || !bossHealth.CanBeDamaged() || attackedPartID != -1 && attackedPartID != partID || isAlreadyHit)
        {
            return;
        }

        bossHealth.TakeDamage(damage);
        TriggerHitEffect();
        SetInvulnerable(true);

        attackedPartID = partID;
        isAlreadyHit = true;

        bossHealth.BodyPartAttacked();
    }

    private void TriggerHitEffect()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = hitColor;
            isHit = true;
        }
    }

    private void SetInvulnerable(bool status)
    {
        isInvulnerable = status;
    }

    public void ResetInvulnerability()
    {
        SetInvulnerable(false);
        ResetColor();

        attackedPartID = -1; 
    }

    private void ResetColor()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
    }

    public void ResetHitStatus()
    {
        if (!isAlreadyHit) 
        {
            isAlreadyHit = false; 
        }
        attackedPartID = -1;
    }
}
