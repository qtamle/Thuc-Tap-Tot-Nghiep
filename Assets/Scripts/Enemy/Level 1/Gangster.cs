﻿using System.Collections;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using EZCameraShake;
using System.Collections.Generic;

public class Gangster : NetworkBehaviour
{
    public static Gangster Instance;

    [Header("Spawn Settings")]
    public Vector2 spawnPosition = new Vector2(0f, 8f);
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;
    public Transform playerTransform;

    [Header("Charging Settings")]
    public LayerMask wallLayer;
    public float chargeSpeed = 10f;
    public float resetX = 0f;
    public float resetY = 4.5f;
    public Transform WallCheck;
    public float wallCheckRadius;

    private NetworkRigidbody2D rb;
    private bool isGrounded = false;
    private bool isCharging = false;
    private bool isSkillActive = false;

    [Header("Attack Settings")]
    public LayerMask player;
    public Transform ChargingAttackTransform;
    public float radiusCharging;

    [SerializeField] private Transform bossTransform;
    [SerializeField] private float rangeSize = 4f;     
    [SerializeField] private float minSpacing = 1.5f;

    private bool isUsingSkill = false;
    private int skillUsageCount = 0;

    private GangsterHealth gangsterHealth;
    private RockPool rockPool;
    private Animator animator;

    void Awake()
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
        animator = GetComponent<Animator>();
        animator.SetBool("Idle", true);

        //Debug.Log("Boss.Start()");
        rockPool = FindFirstObjectByType<RockPool>();
        rb = GetComponent<NetworkRigidbody2D>();
        gangsterHealth = GetComponent<GangsterHealth>();

        if (rb == null)
        {
            Debug.LogError("Rigidbody2D is missing!");
        }

        Spawn();
        FindPlayerTransform();
    }

    private void FindPlayerTransform()
    {
        Debug.Log("Boss.FindPlayerTransform()");
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            int playerLayer = LayerMask.NameToLayer("Player");

            GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();

            foreach (var obj in allObjects)
            {
                if (obj.layer == playerLayer)
                {
                    playerTransform = obj.transform;
                    break;
                }
            }

            if (playerTransform == null)
            {
                Debug.LogError("Không tìm thấy Player theo tag hoặc layer!");
            }
        }
    }

    public void Active()
    {
        Debug.Log("Boss.Active()");
        gameObject.SetActive(true);

        isGrounded = false;
        isCharging = false;
        isSkillActive = false;

        StartCoroutine(RandomSkill());
    }

    IEnumerator RandomSkill()
    {
        yield return new WaitForSeconds(3f);

        while (true)
        {
            if (!isUsingSkill)
            {
                if (skillUsageCount < 3)
                {
                    //animator.SetTrigger("Jump");
                    yield return new WaitForSeconds(0.4f);
                    UseJumpSkill();
                    skillUsageCount++;
                    Debug.Log("Jump: " + skillUsageCount);
                }
                else
                {
                    UseChargeSkill();
                    skillUsageCount = 0;
                }

                float randomDelay = Random.Range(3f, 4f);
                yield return new WaitForSeconds(randomDelay);
            }
            else
            {
                yield return null;
            }
        }
    }

    private void Update()
    {
        CheckGround();
    }

    public void Spawn()
    {
        Debug.Log("Boss.spawn()");
        transform.position = spawnPosition;

        if (rb != null)
        {
            rb.Rigidbody2D.linearVelocity = Vector2.zero;
            rb.Rigidbody2D.gravityScale = 2f;
        }
    }

    void CheckGround()
    {
        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(
                groundCheck.position,
                groundCheckRadius,
                groundLayer
            );
        }
        else
        {
            Debug.LogWarning("Ground Check Transform is not assigned!");
        }

        if (gangsterHealth != null && gangsterHealth.currentHealth.Value <= 0)
        {
            StopAllActions();
        }
    }

    private void StopAllActions()
    {
        animator.SetTrigger("Death");
        StopAllCoroutines();
        rb.Rigidbody2D.linearVelocity = Vector2.zero;
        isUsingSkill = false;
        isCharging = false;
        isSkillActive = false;
    }

    public void UseJumpSkill()
    {
        if (isUsingSkill)
            return;
        isUsingSkill = true;

        if (isGrounded)
        {
            StartCoroutine(ExecuteJumpSkill());
        }
        else
        {
            isUsingSkill = false;
        }
    }

    private IEnumerator ExecuteJumpSkill()
    {
        isSkillActive = true;

        float targetY = 15f;
        rb.Rigidbody2D.linearVelocity = new Vector2(0, 30f + targetY);
        yield return new WaitForSeconds(1f);

        rb.Rigidbody2D.linearVelocity = Vector2.zero;
        rb.Rigidbody2D.gravityScale = 4f;

        yield return new WaitUntil(() => isGrounded);

        CameraShaker.Instance?.ShakeOnce(5f, 5f, 0.1f, 0.1f);

        yield return new WaitForSeconds(1f);

        isUsingSkill = false;
    }

    public void UseChargeSkill()
    {
        if (isUsingSkill)
            return;
        isUsingSkill = true;

        if (!isCharging)
        {
            StartCoroutine(ExecuteChargeSkill());
        }
        else
        {
            isUsingSkill = false;
        }
    }

    private IEnumerator ExecuteChargeSkill()
    {
        yield return new WaitForSeconds(1f);

        isCharging = true;

        FindPlayerTransform();


        float offsetX = playerTransform.position.x > transform.position.x ? -3f : 3f;
        Vector2 targetPosition = new Vector2(
            playerTransform.position.x + offsetX,
            playerTransform.position.y
        );
        float additionalHeight = 0.5f;
        targetPosition.y += additionalHeight;

        transform.position = targetPosition;

        if (rb != null)
        {
            rb.Rigidbody2D.gravityScale = 4f;
        }
        else
        {
            Debug.LogError("Rigidbody2D is missing!");
            yield break;
        }

        yield return new WaitForSeconds(0.7f);

        if (playerTransform == null)
            yield break;

        float chargeDirectionX = playerTransform.position.x > transform.position.x ? 1f : -1f;
        FlipToDirection(chargeDirectionX);


        if (WallCheck != null)
        {
            while (!Physics2D.OverlapCircle(WallCheck.position, wallCheckRadius, wallLayer))
            {
                FlipToDirection(chargeDirectionX);

                animator.SetBool("Run", true);
                animator.SetBool("Idle", false);

                yield return new WaitForSeconds(0.1f);

                if (isGrounded)
                {
                    rb.Rigidbody2D.linearVelocity = new Vector2(
                        chargeDirectionX * chargeSpeed,
                        rb.Rigidbody2D.linearVelocity.y
                    );
                }

                Collider2D playerCollider = Physics2D.OverlapCircle(
                    ChargingAttackTransform.position,
                    radiusCharging,
                    player
                );
                if (playerCollider != null)
                {
                    PlayerHealth playerHealth = playerCollider.GetComponent<PlayerHealth>();
                    if (playerHealth != null)
                    {
                        playerHealth.DamagePlayer(2);
                    }
                }

                yield return null;
            }
        }
        else
        {
            Debug.LogError("WallCheck Transform is not assigned!");
            yield break;
        }

        rb.Rigidbody2D.linearVelocity = Vector2.zero;

        if (gangsterHealth != null)
        {
            CameraShaker.Instance.ShakeOnce(5f, 5f, 0.1f, 0.1f);
            animator.SetBool("Run", false);
            animator.SetBool("Stun", true);
            gangsterHealth.StunForDuration(3f);
        }

        SpawnRocks();
        yield return new WaitForSeconds(3f);

        transform.position = new Vector2(resetX, resetY);
        rb.Rigidbody2D.linearVelocity = Vector2.zero;
        rb.Rigidbody2D.gravityScale = 4f;
        isCharging = false;
        isUsingSkill = false;

        animator.SetBool("Idle", true);
        animator.SetBool("Stun", false);
    }

    private void FlipToDirection(float directionX)
    {
        if (directionX > 0 && transform.rotation != Quaternion.identity)
        {
            transform.rotation = Quaternion.identity; // Quay về hướng phải
        }
        else if (directionX < 0 && transform.rotation != Quaternion.Euler(0, 180, 0))
        {
            transform.rotation = Quaternion.Euler(0, 180, 0); // Quay về hướng trái
        }
    }

    void SpawnRocks()
    {
        if (bossTransform == null)
        {
            Debug.LogWarning("Chưa gán Boss Transform!");
            return;
        }

        float bossX = bossTransform.position.x;
        float spawnY = 15f;

        List<float> rockXs = new List<float>();

        float leftX = Random.Range(bossX - rangeSize, bossX - minSpacing);
        rockXs.Add(leftX);

        float rightX = Random.Range(bossX + minSpacing, bossX + rangeSize);
        rockXs.Add(rightX);

        float middleX;
        int safety = 10;
        do
        {
            middleX = Random.Range(bossX - rangeSize, bossX + rangeSize);
            safety--;
        } while ((Mathf.Abs(middleX - leftX) < minSpacing || Mathf.Abs(middleX - rightX) < minSpacing) && safety > 0);
        rockXs.Add(middleX);

        foreach (float x in rockXs)
        {
            Vector2 rockPosition = new Vector2(x, spawnY);
            GameObject rock = rockPool.GetRock(rockPosition);
            rock.transform.position = rockPosition;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if ((groundLayer.value & (1 << collision.gameObject.layer)) > 0)
        {
            rb.Rigidbody2D.gravityScale = 0;
            if (isSkillActive)
            {
                SpawnRocks();
                isSkillActive = false;
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(WallCheck.position, wallCheckRadius);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(ChargingAttackTransform.position, radiusCharging);
    }
}
