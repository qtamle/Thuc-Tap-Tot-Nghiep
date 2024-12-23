using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class GangsterHealthOnline : NetworkBehaviour, DamageInterface
{
    [Header("Health Settings")]
    public int maxHealth = 3;
    //public int currentHealth;
    // NetworkVariable để đồng bộ máu
    private NetworkVariable<int> currentHealth = new NetworkVariable<int>();

    [Header("UI Settings")]
    public Slider healthBarSlider;
    public Image healthBarFill;

    private bool canBeDamaged = false;
    private float timeWhenStunned = 0f;
    private bool isStunned = false;

    public UnityEvent startTimeline;

    private void Start()
    {
        if(!IsServer) return;
        currentHealth.Value = maxHealth;
        UpdateHealthBar();
        UpdateHealthBarColor();

        // Đăng ký sự kiện để cập nhật UI khi máu thay đổi

        currentHealth.OnValueChanged += OnHealthChanged;
    }

    public void TakeDamage(int damage)
    {
        if (canBeDamaged)
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
    [ServerRpc]
    public void TakeDamageServerRpc(int damage)
    {
        if (canBeDamaged)
        {
            currentHealth.Value -= damage; // Đồng bộ với NetworkVariable

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
            healthBarFill.color = canBeDamaged ? Color.red : Color.cyan;
        }
    }
    private void OnHealthChanged(int oldHealth, int newHealth)
    {
        // Cập nhật thanh máu khi có thay đổi
        UpdateHealthBar();
    }


    private void Die()
    {
        Debug.Log("Boss bi tieu diet");
    }

    public void StunForDuration(float stunDuration)
    {
        Debug.Log(canBeDamaged + "Before stun");

        isStunned = true;
        canBeDamaged = true;
        Debug.Log(canBeDamaged + "After stun");
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