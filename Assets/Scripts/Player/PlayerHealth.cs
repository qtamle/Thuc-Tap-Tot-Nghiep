using TMPro;
using UnityEngine;

public class PlayerHealth : MonoBehaviour, DamagePlayerInterface
{
    [Header("Health Settings")]
    public int maxHealth = 20;
    public int currentHealth;

    [Header("Shield Settings")]
    public int currentShield = 0;  

    [Header("UI Health")]
    public TMP_Text healthText;
    public TMP_Text shieldText;  

    [Header("Damage Settings")]
    public SpriteRenderer playerSprite;
    public float invincibilityDuration = 2f;
    public float flashInterval = 0.1f;

    private bool isInvincible = false;

    private void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();
        UpdateShieldUI();
    }

    public void DamagePlayer(int damage)
    {
        if (isInvincible)
            return;

        if (currentShield > 0)
        {
            currentShield -= damage;
            currentShield = Mathf.Max(currentShield, 0); 
            UpdateShieldUI();

            StartCoroutine(InvincibilityCoroutine(invincibilityDuration));
        }
        else
        {
            currentHealth -= damage;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
            UpdateHealthUI();

            if (currentHealth <= 0)
            {
                Die();
            }
            else
            {
                StartCoroutine(InvincibilityCoroutine(invincibilityDuration));
            }
        }
    }

    public void HealHealth(int amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthUI();
    }

    public void HealShield(int amount)
    {
        currentShield += amount;
        UpdateShieldUI();
    }

    private void UpdateHealthUI()
    {
        if (healthText != null)
        {
            healthText.text = currentHealth + " / " + maxHealth;
        }
    }

    private void UpdateShieldUI()
    {
        if (shieldText != null)
        {
            shieldText.text = currentShield + "";
        }
    }

    private void Die()
    {
        Debug.Log("Player has died!");
    }

    private System.Collections.IEnumerator InvincibilityCoroutine(float duration)
    {
        isInvincible = true;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            playerSprite.enabled = !playerSprite.enabled;
            yield return new WaitForSeconds(flashInterval);
            elapsed += flashInterval;
        }

        playerSprite.enabled = true;
        isInvincible = false;
    }
}
