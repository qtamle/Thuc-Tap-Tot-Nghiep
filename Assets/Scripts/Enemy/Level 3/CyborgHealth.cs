using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CyborgHealth : NetworkBehaviour, DamageInterface
{
    public static CyborgHealth Instance;

    [Header("Health Settings")]
    public int maxHealth = 3;
    public NetworkVariable<int> currentHealth = new NetworkVariable<int>(3);
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
        if (IsServer) // Chỉ server mới gọi ServerRpc
        {
            currentHealth.Value = 3;
            IntializeBossHealthServerRpc();
        }
        UpdateHealthBar();
        UpdateHealthBarColor();
    }

    [ServerRpc(RequireOwnership = false)]
    public void IntializeBossHealthServerRpc()
    {
        GameObject sliderObj = GameObject.FindWithTag("BossSlider");
        GameObject fillObj = GameObject.FindWithTag("BossFill");

        if (sliderObj != null && fillObj != null)
        {
            healthBarSlider = sliderObj.GetComponent<Slider>();
            healthBarFill = fillObj.GetComponent<Image>();

            ShowBossHealthUIClientRpc(); // Gửi tín hiệu cho tất cả client
        }
        else
        {
            Debug.LogWarning("Boss UI elements not found!");
        }
    }

    [ClientRpc]
    public void ShowBossHealthUIClientRpc()
    {
        GameObject sliderObj = GameObject.FindWithTag("BossSlider");
        GameObject fillObj = GameObject.FindWithTag("BossFill");

        if (sliderObj != null && fillObj != null)
        {
            healthBarSlider = sliderObj.GetComponent<Slider>();
            healthBarFill = fillObj.GetComponent<Image>();

            healthBarSlider.gameObject.SetActive(true); // Hiển thị thanh máu
        }
        else
        {
            Debug.LogWarning("Boss UI elements not found on client!");
        }
    }

    public void TakeDamage(int damage)
    {
        if (!IsServer) // Nếu không phải server, gửi yêu cầu lên server
        {
            Debug.Log($"[Client] Request TakeDamage: {damage}");
            TakeDamageServerRpc(damage);
            return;
        }

        Debug.Log(
            $"[Server] TakeDamage called. CanBeDamaged: {canBeDamaged.Value}, Current Health: {currentHealth.Value}"
        );

        if (canBeDamaged.Value)
        {
            int beforeHealth = currentHealth.Value;
            currentHealth.Value -= damage;
            Debug.Log($"[Server] Health changed: {beforeHealth} -> {currentHealth.Value}");

            UpdateHealthBar();

            if (currentHealth.Value <= 0)
            {
                Debug.Log("[Server] Boss is dead!");
                startTimeline.Invoke();
                Die();
            }
        }
        else
        {
            Debug.Log("[Server] Boss is immune to damage!");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void TakeDamageServerRpc(int damage)
    {
        TakeDamage(damage); // Server xử lý logic trừ máu
    }

    public IEnumerator OnBossDeath()
    {
        yield return new WaitForSeconds(1f);
        GameManager.Instance.LoadNextScene();
        // BossManager.Instance.HandleBossDefeated(currentBoss);
    }

    private void UpdateHealthBar()
    {
        UpdateHealthBarClientRpc(currentHealth.Value, maxHealth);
    }

    [ClientRpc]
    private void UpdateHealthBarClientRpc(int currentHealth, int maxHealth)
    {
        if (healthBarSlider != null)
        {
            healthBarSlider.value = (float)currentHealth / maxHealth;
        }
    }

    private void UpdateHealthBarColor()
    {
        // if (healthBarFill != null)
        // {
        //     healthBarFill.color = canBeDamaged.Value ? Color.red : Color.cyan;
        // }
        UpdateHealthBarColorClientRpc(canBeDamaged.Value);
    }

    [ClientRpc]
    private void UpdateHealthBarColorClientRpc(bool canBeDamaged)
    {
        Debug.Log("UpdateHealthBarColorClientRpc: " + canBeDamaged);
        if (healthBarFill != null)
        {
            healthBarFill.color = canBeDamaged ? Color.red : Color.cyan;
        }
    }

    private void Die()
    {
        Debug.Log("Boss bi tieu diet");
        gameObject.GetComponent<NetworkObject>().Despawn(true);
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
        if (IsServer)
        {
            Debug.Log("SetCanBeDamaged called on server");
            canBeDamaged.Value = value;
            UpdateHealthBarColor();
        }
        else
        {
            // Gọi ServerRpc để yêu cầu server thay đổi giá trị
            SetCanBeDamagedServerRpc(value);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetCanBeDamagedServerRpc(bool value)
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
