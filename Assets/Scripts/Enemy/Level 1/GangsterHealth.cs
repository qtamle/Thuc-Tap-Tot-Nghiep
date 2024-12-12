using UnityEngine;
using UnityEngine.UI;

public class GangsterHealth : MonoBehaviour, DamageInterface
{
    [Header("Health Settings")]
    public int maxHealth = 3;
    public int currentHealth;

    [Header("UI Settings")]
    public Slider healthBarSlider;
    public Image healthBarFill;

    private bool canBeDamaged = false;
    private float timeWhenStunned = 0f;
    private bool isStunned = false;

    private void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthBar();
        UpdateHealthBarColor();
    }

    public void TakeDamage(int damage)
    {
        if (canBeDamaged)
        {
            currentHealth -= damage;
            UpdateHealthBar();

            if (currentHealth <= 0)
            {
                Die();
            }
        }
    }

    private void UpdateHealthBar()
    {
        if (healthBarSlider != null)
        {
            healthBarSlider.value = (float)currentHealth / maxHealth;
        }
    }

    private void UpdateHealthBarColor()
    {
        if (healthBarFill != null)
        {
            healthBarFill.color = canBeDamaged ? Color.red : Color.cyan;
        }
    }

    private void Die()
    {
        Debug.Log("Boss bi tieu diet");
    }

    public void StunForDuration(float stunDuration)
    {
        isStunned = true;
        canBeDamaged = true; 
        timeWhenStunned = Time.time + stunDuration;
        UpdateHealthBarColor();
    }

    public bool CanBeDamaged()
    {
        return canBeDamaged;
    }

    public void SetCanBeDamaged(bool value)
    {
        canBeDamaged = value;
        UpdateHealthBarColor();
    }

    private void Update()
    {
        if (isStunned && Time.time > timeWhenStunned)
        {
            isStunned = false;
            canBeDamaged = false;
            UpdateHealthBarColor();
        }
    }

}