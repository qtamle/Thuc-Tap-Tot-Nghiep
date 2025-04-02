using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : NetworkBehaviour, DamagePlayerInterface
{
    [Header("Health Settings")]
    public int maxHealth = 20;
    public int currentHealth;

    [Header("Shield Settings")]
    public int currentShield = 0;

    [Header("UI Health")]
    [SerializeField]
    public TMP_Text healthText;

    [SerializeField]
    public TMP_Text shieldText;

    [Header("Damage Settings")]
    public SpriteRenderer playerSprite;
    public float invincibilityDuration = 2f;
    public float flashInterval = 0.1f;

    [Header("Check Add")]
    public bool isInvincible = false;
    public bool hasRevived = false;
    private bool isImmortalActive = false;
    private bool hasCheckedSacrifice = false;
    private bool hasAddedSavedHealth = false;
    public bool hasAddShieldGloves = false;
    public bool hasAddShieldClaws = false;
    public bool hasAddShieldChainsaw = false;

    private LevelSystem levelSystem;
    private GameObject CurrentHealth;
    private GameObject player;

    [Header("Supply")]
    private AngelGuardian angel;
    private Sacrifice Sacrifice;
    private EnergyShield energyShield;
    private Immortal immortal;
    private Brutal brutal;
    private Dodge dodge;

    [Header("Weapon")]
    private Gloves gloves;
    private Claws claws;
    private Attack attack; // Chainsaw

    private WeaponPlayerInfo weaponInfo;

    [SerializeField]
    private PlayerHealthData healthData;

    public delegate void HealthChangedHandler(ulong clientId, int currentHealth);
    private bool isDead = false; // Thêm biến này

    public event HealthChangedHandler OnHealthChanged;

    private void Awake() { }

    private void Start()
    {
        InitializePlayer();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (IsServer)
        {
            GameManager.Instance.SavePlayerHealthData(OwnerClientId, currentHealth, currentShield);
            GameManager.Instance.UnregisterPlayerHealth(OwnerClientId);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        InitializePlayer();
    }

    private void InitializePlayer()
    {
        // Player is Gameobject
        player = gameObject;

        // Find Text shield and health in scene
        GameObject CurrentHealth = GameObject.Find("CurrentHealth");
        GameObject Shield = GameObject.Find("Shield");

        GameObject currentHealthObject = GameObject.FindGameObjectWithTag("Health");
        GameObject shieldObject = GameObject.FindGameObjectWithTag("Shield");

        if (currentHealthObject != null)
        {
            healthText = currentHealthObject?.GetComponent<TMP_Text>();
            healthText.text = currentHealth.ToString();
        }

        if (shieldObject != null)
        {
            shieldText = shieldObject?.GetComponent<TMP_Text>();
            shieldText.text = currentShield.ToString();
        }
        // 1. KHÔI PHỤC DỮ LIỆU TỪ GAMEMANAGER (nếu có)
        if (IsServer)
        {
            var (savedHealth, savedShield) = GameManager.Instance.GetPlayerHealthData(
                OwnerClientId
            );
            if (savedHealth > 0) // Nếu có dữ liệu đã lưu
            {
                currentHealth = savedHealth;
                currentShield = savedShield;
                Debug.Log(
                    $"Khôi phục máu/khiên: {currentHealth}/{maxHealth}, Shield: {currentShield}"
                );
            }
        }
        // Find supply in scene
        angel = FindFirstObjectByType<AngelGuardian>();
        Sacrifice = FindFirstObjectByType<Sacrifice>();
        energyShield = FindFirstObjectByType<EnergyShield>();
        immortal = FindFirstObjectByType<Immortal>();
        brutal = FindFirstObjectByType<Brutal>();
        dodge = FindFirstObjectByType<Dodge>();

        // 2. XỬ LÝ LEVEL SYSTEM (chỉ cộng vào maxHealth)
        levelSystem = FindFirstObjectByType<LevelSystem>();
        if (levelSystem != null && !hasAddedSavedHealth)
        {
            int healthBonus = levelSystem.health; // Số máu thêm mỗi cấp
            maxHealth = 20 + healthBonus; // Cập nhật maxHealth (20 + bonus)

            // Nếu KHÔNG có dữ liệu từ GameManager (lần đầu vào game)
            if (GameManager.Instance.GetPlayerHealthData(OwnerClientId).health <= 0)
            {
                currentHealth = maxHealth; // Set đầy máu
            }

            hasAddedSavedHealth = true;
            Debug.Log($"Bonus máu từ LevelSystem: +{healthBonus}. MaxHealth: {maxHealth}");
        }

        UpdateHealthUI();

        // Check supply Sacrifice, convert health to shield
        CheckSacrifice();

        if (immortal != null && !isImmortalActive)
        {
            invincibilityDuration += 1.5f;
            isImmortalActive = true;
        }

        // Find Weapon Info (level weapon) and Gloves to upgrade shield for level 3
        weaponInfo = GetComponent<WeaponPlayerInfo>();

        gloves = GetComponent<Gloves>();
        if (
            weaponInfo != null
            && gloves != null
            && !hasAddShieldGloves
            && weaponInfo.weaponLevel > 2
        )
        {
            currentShield += 10;
            UpdateShieldUI();
            hasAddShieldGloves = true;
        }
        else
        {
            Debug.Log("Khong đạt đủ điều kiện!");
        }

        claws = GetComponent<Claws>();
        if (weaponInfo != null && claws != null && !hasAddShieldClaws && weaponInfo.weaponLevel > 2)
        {
            maxHealth += 10;
            currentHealth = maxHealth;
            UpdateHealthUI();
            hasAddShieldClaws = true;
        }

        attack = GetComponent<Attack>();
        if (
            weaponInfo != null
            && attack != null
            && !hasAddShieldChainsaw
            && weaponInfo.weaponLevel > 2
        )
        {
            currentShield += 10;
            UpdateShieldUI();
            hasAddShieldChainsaw = true;
        }
    }

    //private void OnLevelUpdated(int level, int experience, int experienceToNextLevel)
    //{
    //    // Tăng maxHealth mỗi khi lên cấp
    //    maxHealth += 5;
    //    Debug.Log("Da tang 5 mau");

    //    // Đảm bảo currentHealth không vượt quá maxHealth mới
    //    if (currentHealth > maxHealth)
    //    {
    //        currentHealth = maxHealth;
    //    }

    //    UpdateHealthUI(); // Cập nhật UI để phản ánh thay đổi
    //}
    //private void OnDestroy()
    //{
    //    // Gỡ bỏ sự kiện khi PlayerHealth bị hủy để tránh lỗi
    //    if (levelSystem != null)
    //    {
    //        levelSystem.OnLevelDataUpdated -= OnLevelUpdated;
    //    }
    //}



    public void DamagePlayer(int damage)
    {
        if (isDead || isInvincible) // Kiểm tra thêm isDead
        {
            return;
        }

        if (dodge != null && Random.Range(0f, 1f) <= 0.15f)
        {
            Debug.Log("Player dodged the damage!");
            StartCoroutine(InvincibilityCoroutine(invincibilityDuration));
            return;
        }

        if (energyShield != null && energyShield.TryBlockDamage())
        {
            Debug.Log("Damage blocked by Energy Shield.");
            StartCoroutine(InvincibilityCoroutine(invincibilityDuration));
            return;
        }

        if (currentShield > 0)
        {
            currentShield -= (damage * 2);
            currentShield = Mathf.Max(currentShield, 0);
            UpdateShieldUI();

            StartCoroutine(InvincibilityCoroutine(invincibilityDuration));
        }
        else
        {
            if (brutal != null)
            {
                damage *= 5;
            }

            currentHealth -= damage;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
            UpdateHealthUI();

            if (currentHealth <= 0 && currentShield <= 0)
            {
                // if (!hasRevived && angel.IsReady())
                // {
                //     Debug.Log("Player is resurrected!");
                //     currentHealth = maxHealth;
                //     angel.CanActive();
                //     hasRevived = true;
                //     UpdateHealthUI();
                // }
                // else
                // {
                //     Die();
                // }
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

    public void UpdateHealthUI()
    {
        if (healthText != null)
        {
            healthText.text = currentHealth + " / " + maxHealth;
        }
    }

    public void UpdateShieldUI()
    {
        if (shieldText != null)
        {
            shieldText.text = currentShield + "";
        }
    }

    private void Die()
    {
        if (isDead)
            return; // Đảm bảo chỉ thực hiện 1 lần
        if (IsServer)
        {
            isDead = true;
            Debug.Log("Player has died!");
            // gameObject.SetActive(false);
            GameManager.Instance.PlayerDied(NetworkObject.OwnerClientId, gameObject);
        }
        else
        {
            DieServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void DieServerRpc()
    {
        if (isDead)
            return; // Đảm bảo chỉ thực hiện 1 lần
        isDead = true;
        Debug.Log("Player form client died!");

        GameManager.Instance.PlayerDied(NetworkObject.OwnerClientId, gameObject);
    }

    private System.Collections.IEnumerator InvincibilityCoroutine(float duration)
    {
        isInvincible = true;
        float elapsed = 0f;
        player.tag = "Untagged";

        while (elapsed < duration)
        {
            playerSprite.enabled = !playerSprite.enabled;
            yield return new WaitForSeconds(flashInterval);
            elapsed += flashInterval;
        }

        player.tag = "Player";
        playerSprite.enabled = true;
        isInvincible = false;
    }

    private void CheckSacrifice()
    {
        if (hasCheckedSacrifice)
            return;

        if (Sacrifice != null)
        {
            Debug.Log("Sacrifice found! Converting health to shield.");
            currentShield += currentHealth;
            currentShield += 30;
            currentHealth = 0;
            UpdateHealthUI();
            UpdateShieldUI();
            Sacrifice.CanActive();
            hasCheckedSacrifice = true;
        }
    }
}
