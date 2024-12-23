using UnityEngine;

public class HeadController : MonoBehaviour
{
    [Header("Hit Effect Settings")]
    private SpriteRenderer spriteRenderer;
    public Color hitColor = new Color(1f, 0.5f, 0.5f);
    private Color originalColor;
    private bool isHit = false;

    private SnakeHealth snakeHealth;

    public bool isHeadAttacked = false;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        else
        {
            Debug.LogError("SpriteRenderer is not assigned on Head object!");
        }

        snakeHealth = GetComponentInParent<SnakeHealth>(); 
    }

    public void TakeDamage(int damage)
    {
        if (isHeadAttacked || !snakeHealth.CanBeDamaged())
        {
            return; 
        }

        snakeHealth.TakeDamage(damage);

        TriggerHitEffect();

    }

    private void TriggerHitEffect()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = hitColor;
            isHit = true;
        }
    }

    private void Die()
    {
        Debug.Log("Snake Die");
    }

    public void ResetHitStatus()
    {
        if (isHit)
        {
            spriteRenderer.color = originalColor;
            isHit = false;
        }
    }

    public void ResetHeadAttackStatus()
    {
        isHeadAttacked = false;
    }
}
