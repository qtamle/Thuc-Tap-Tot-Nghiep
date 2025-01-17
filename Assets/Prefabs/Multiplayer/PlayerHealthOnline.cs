﻿using TMPro;
using UnityEngine;

public class PlayerHealthOnline : MonoBehaviour, DamagePlayerInterface
{
    [Header("Health Settings")]
    public int maxHealth = 20;
    public int currentHealth;

    [Header("Shield Settings")]
    public int currentShield = 0;  

    [Header("UI Health")]
    [SerializeField]public TMP_Text healthText;
    [SerializeField]public TMP_Text shieldText;  

    [Header("Damage Settings")]
    public SpriteRenderer playerSprite;
    public float invincibilityDuration = 2f;
    public float flashInterval = 0.1f;
    
    private bool isInvincible = false;
    private LevelSystem levelSystem;
    private GameObject CurrentHealth;
    private void Start()
    {
        GameObject CurrentHealth = GameObject.Find("CurrentHealth");
        GameObject Shield = GameObject.Find("Shield");
        healthText = CurrentHealth.GetComponent<TMP_Text>();
        shieldText = Shield.GetComponent<TMP_Text>();
        //levelSystem = FindFirstObjectByType<LevelSystem>();
        if (levelSystem != null)
        {
            Debug.Log("Da dang ky LevelSystem");
            // Đăng ký sự kiện
            //levelSystem.OnLevelDataUpdated += OnLevelUpdated;
        }
        else
        {
            Debug.Log("Không tìm thấy LevelSystem trong scene.");
        }
        currentHealth = maxHealth;
        UpdateHealthUI();
        UpdateShieldUI();
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
