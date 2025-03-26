using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class AssassinHealth : NetworkBehaviour, DamageInterface
{
    public static AssassinHealth Instance;

    [Header("Health Settings")]
    public int maxHealth = 3;
    public NetworkVariable<int> currentHealth = new NetworkVariable<int>();
    private NetworkVariable<bool> isStunned = new NetworkVariable<bool>(false);
    private NetworkVariable<bool> canBeDamaged = new NetworkVariable<bool>(false);

    [Header("UI Settings")]
    public Slider healthBarSlider;
    public Image healthBarFill;

    private float timeWhenStunned = 0f;

    public UnityEvent startTimeline;

    [SerializeField]
    private HandleBoss currentBoss;

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
        currentHealth.Value = maxHealth;
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
        if (canBeDamaged.Value)
        {
            currentHealth.Value -= damage;
            UpdateHealthBar();

            if (currentHealth.Value <= 0)
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
            healthBarSlider.value = (float)currentHealth.Value / maxHealth;
        }
    }

    private void UpdateHealthBarColor()
    {
        if (healthBarFill != null)
        {
            healthBarFill.color = canBeDamaged.Value ? Color.red : Color.cyan;
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
        isStunned.Value = true;
        canBeDamaged.Value = true;
        timeWhenStunned = Time.time + stunDuration;
        UpdateHealthBarColor();
    }

    public bool CanBeDamaged()
    {
        return canBeDamaged.Value;
    }

    public void SetCanBeDamaged(bool value)
    {
        canBeDamaged.Value = value;
        UpdateHealthBarColor();
    }

    private void Update()
    {
        if (isStunned.Value && Time.time > timeWhenStunned)
        {
            isStunned.Value = false;
            canBeDamaged.Value = false;
            UpdateHealthBarColor();
        }
    }
}
