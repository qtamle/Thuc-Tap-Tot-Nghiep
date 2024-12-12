using UnityEngine;
using UnityEngine.UI;

public class GangsterHealth : MonoBehaviour, DamageInterface
{
    [Header("Health Settings")]
    public int maxHealth = 3;
    public int currentHealth;

    [Header("UI Settings")]
    public Slider healthBarSlider;

    private void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthBar();
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        UpdateHealthBar();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void UpdateHealthBar()
    {
        if (healthBarSlider != null)
        {
            healthBarSlider.value = (float)currentHealth / maxHealth;
        }
    }

    private void Die()
    {
        Debug.Log("Boss bi tieu diet");
    }
}