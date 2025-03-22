using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Boss5Health : NetworkBehaviour, DamageInterface
{
    public static Boss5Health Instance;

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

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        currentHealth = maxHealth;
        IntializeBossHealth();
        UpdateHealthBar();
        UpdateHealthBarColor();
    }

    public void IntializeBossHealth()
    {
        GameObject sliderObj = GameObject.FindWithTag("BossSlider");
        if (sliderObj != null)
        {
            healthBarSlider = sliderObj.GetComponent<Slider>();
        }
        else
        {
            Debug.LogWarning("BossSlider tag not found!");
        }

        GameObject fillObj = GameObject.FindWithTag("BossFill");
        if (fillObj != null)
        {
            healthBarFill = fillObj.GetComponent<Image>();
        }
        else
        {
            Debug.LogWarning("BossFill tag not found!");
        }
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
