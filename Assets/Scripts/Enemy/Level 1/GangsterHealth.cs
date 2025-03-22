using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class GangsterHealth : NetworkBehaviour, DamageInterface
{
    public static GangsterHealth Instance;

    [Header("Health Settings")]
    private int maxHealth = 3;
    public NetworkVariable<int> currentHealth = new NetworkVariable<int>(3);

    [Header("UI Settings")]
    public Slider healthBarSlider;
    public Image healthBarFill;

    private NetworkVariable<bool> canBeDamaged = new NetworkVariable<bool>(false);
    private float timeWhenStunned = 0f;
    private NetworkVariable<bool> isStunned = new NetworkVariable<bool>(false);

    public UnityEvent startTimeline;

    [SerializeField]
    private HandleBoss currentBoss;


    private void Start()
    {
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

    public IEnumerator OnBossDeath()
    {
        yield return new WaitForSeconds(1f);
        // GameManager.Instance.LoadNextScene();
        // BossManager.Instance.HandleBossDefeated(currentBoss);
    }

    private void UpdateHealthBar()
    {
        // if (healthBarSlider != null)
        // {
        //     healthBarSlider.value = (float)currentHealth.Value / maxHealth;
        // }
        UpdateHealthBarClientRpc();
    }

    [ClientRpc]
    private void UpdateHealthBarClientRpc()
    {
        if (healthBarSlider != null)
        {
            healthBarSlider.value = (float)currentHealth.Value / maxHealth;
        }
    }

    private void UpdateHealthBarColor()
    {
        // if (healthBarFill != null)
        // {
        //     healthBarFill.color = canBeDamaged.Value ? Color.red : Color.cyan;
        // }
        UpdateHealthBarColorClientRpc();
    }

    [ClientRpc]
    private void UpdateHealthBarColorClientRpc()
    {
        if (healthBarFill != null)
        {
            healthBarFill.color = canBeDamaged.Value ? Color.red : Color.cyan;
        }
    }

    private void Die()
    {
        Debug.Log("Boss bi tieu diet");
        StartCoroutine(OnBossDeath());
    }

    public void StunForDuration(float stunDuration)
    {
        Debug.Log(canBeDamaged + "Before stun");
        isStunned.Value = true;
        canBeDamaged.Value = true;
        Debug.Log(canBeDamaged + "After stun");
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
        if (!IsServer)
            return;
        if (isStunned.Value && Time.time > timeWhenStunned)
        {
            isStunned.Value = false;
            canBeDamaged.Value = false;
            UpdateHealthBarColor();
        }
    }
}
