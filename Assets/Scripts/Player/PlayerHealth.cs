using TMPro;
using UnityEngine;

public class PlayerHealth : MonoBehaviour, DamagePlayerInterface
{
    [Header("Health Settings")]
    public int maxHealth = 20;
    public int currentHealth;

    [Header("UI Health")]
    public TMP_Text healthText;

    [Header("Damage Settings")]
    public SpriteRenderer playerSprite;
    public float invincibilityDuration = 2f;
    public float flashInterval = 0.1f;

    private bool isInvincible = false;

    private void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();
    }

    public void DamagePlayer(int damage)
    {
        if (isInvincible)
            return;

        currentHealth -= damage;

        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthUI();

        if (currentHealth > 0)
        {
            StartCoroutine(InvincibilityCoroutine());
        }
        else
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        currentHealth += amount;

        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthUI();
    }

    private void UpdateHealthUI()
    {
        if (healthText != null)
        {
            healthText.text = currentHealth + " / " + maxHealth;
        }
    }
    
    private void Die()
    {
        Debug.Log("Player has died!");
    }

    private System.Collections.IEnumerator InvincibilityCoroutine()
    {
        isInvincible = true;
        float elapsed = 0f;

        while (elapsed < invincibilityDuration)
        {
            playerSprite.enabled = !playerSprite.enabled;
            yield return new WaitForSeconds(flashInterval);
            elapsed += flashInterval;
        }

        playerSprite.enabled = true;
        isInvincible = false;
    }
}
