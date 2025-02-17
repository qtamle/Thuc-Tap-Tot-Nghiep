using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CyborgHealth : MonoBehaviour, DamageInterface
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

    public UnityEvent startTimeline;
    [SerializeField] private HandleBoss currentBoss;

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
                startTimeline.Invoke();
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
        StartCoroutine(OnBossDeath());
        Debug.Log("Boss bi tieu diet");
    }

    public IEnumerator OnBossDeath()
    {
        yield return new WaitForSeconds(1f);

        BossManager.Instance.HandleBossDefeated(currentBoss);
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
