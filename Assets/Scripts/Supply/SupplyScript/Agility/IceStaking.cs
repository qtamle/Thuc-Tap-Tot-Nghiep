﻿using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IceStaking : MonoBehaviour, ISupplyActive
{
    public SupplyData supplyData;

    [SerializeField]
    private bool isActive = false;

    [SerializeField]
    private float cooldownTime = 5f;

    [SerializeField]
    private int healAmount = 10;

    public float CooldownTime => cooldownTime;

    private PlayerHealth healthPlayer;

    private bool hasAddShield = false;

    private void Start()
    {
        healthPlayer = GetComponentInParent<PlayerHealth>();

        SceneManager.sceneLoaded += OnSceneLoaded;

    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        hasAddShield = false;

        StartCoroutine(DelayedCheckAndHeal());
    }

    private IEnumerator DelayedCheckAndHeal()
    {
        yield return new WaitForSeconds(0.5f); 

        healthPlayer = GetComponentInParent<PlayerHealth>();
        if (healthPlayer != null)
        {
            Debug.Log("Tìm thấy PlayerHealth, bắt đầu kiểm tra level...");
            CheckAndHealPlayer();
        }
        else
        {
            Debug.LogWarning("Không tìm thấy PlayerHealth sau khi load scene!");
        }
    }

    private void CheckAndHealPlayer()
    {
        string[] levelTags = { "Level1", "Level2", "Level3", "Level4", "Level5", "Level6" };

        GameObject[] allObjects = FindObjectsOfType<GameObject>();

        bool tagFound = false;

        foreach (GameObject obj in allObjects)
        {
            foreach (string tag in levelTags)
            {
                if (obj.CompareTag(tag) && hasAddShield == false)
                {
                    HealPlayer(healAmount);
                    tagFound = true;
                    hasAddShield = true;
                    return;
                }
            }
        }

        if (!tagFound)
        {
            Debug.LogWarning("No matching level tags found in the scene.");
        }
    }

    private void HealPlayer(int amount)
    {
        if (healthPlayer != null)
        {
            healthPlayer.HealShield(amount);
        }
        else
        {
            Debug.LogWarning("PlayerHealth not found! Cannot heal.");
        }
    }

    public void Active()
    {
        if (!IsReady())
        {
            return;
        }

        Debug.Log("Active and Heal Player");
        isActive = false;
        CheckAndHealPlayer();
        StartCoroutine(CooldownRoutine());
    }

    public void CanActive()
    {
        isActive = false;
    }

    public bool IsReady()
    {
        return isActive;
    }

    private IEnumerator CooldownRoutine()
    {
        yield return new WaitForSeconds(cooldownTime);
        CanActive();
    }
}
