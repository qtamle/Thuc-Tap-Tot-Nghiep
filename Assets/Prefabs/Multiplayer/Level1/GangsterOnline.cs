using UnityEngine;
using System.Collections;
using System.Threading.Tasks;
using System;
using Unity.Netcode.Components;
using Unity.Netcode;

public class GangsterOnline : NetworkBehaviour
{
    [Header("Spawn Settings")]
    public Vector2 spawnPosition = new Vector2(0f, 8f);
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;
    public GameObject rockPrefab;
    public Transform playerTransform;

    [Header("Charging Settings")]
    public LayerMask wallLayer;
    public float chargeSpeed = 10f;
    public float resetX = 0f;
    public float resetY = 4.5f;
    public Transform WallCheck;
    public float wallCheckRadius;

    public Rigidbody2D rb;
    private bool isGrounded = false;
    private bool isCharging = false;
    private bool isSkillActive = false;

    [Header("Attack Settings")]
    public LayerMask player;
    public Transform ChargingAttackTransform;
    public float radiusCharging;

    private bool isUsingSkill = false;

    private GangsterHealthOnline gangsterHealth;
    private void Start()
    {
        Activate();
    }
    

    private void OnEnable()
    {
        rb = GetComponent<Rigidbody2D>();
        gangsterHealth = GetComponent<GangsterHealthOnline>();
        FindPlayerTrans();

        if (rb == null)
        {
            Debug.LogError("Rigidbody2D is missing!");
        }

        Spawn();
    }
    private void FindPlayerTrans()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
        else
        {
            Debug.LogError("Cannot find PlayerOnline!");
        }
    }

    
    public void Activate()
    {
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
                int randomSkill = UnityEngine.Random.Range(0, 2);

                if (randomSkill == 0)
                {
                    UseJumpSkill();
                }
                else if (randomSkill == 1)
                {
                    UseChargeSkill();
                }

                float randomDelay = UnityEngine.Random.Range(3f, 4f);
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
        Debug.Log("Da spam ra Boss");
        transform.position = spawnPosition;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = 2f;
        }
    }

    void CheckGround()
    {
        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
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
        StopAllCoroutines();
        rb.linearVelocity = Vector2.zero;
        isUsingSkill = false;
        isCharging = false; 
        isSkillActive = false;

    }

    public void UseJumpSkill()
    {
        if (isUsingSkill) return; 
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
        rb.linearVelocity = new Vector2(0, 30f);
        yield return new WaitForSeconds(1f);

        transform.position = new Vector2(0f, targetY);
        yield return new WaitForSeconds(0.5f);

        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 4f;
        yield return new WaitForSeconds(1f);

        isUsingSkill = false; 
    }
    public void UseChargeSkill()
    {
        if (isUsingSkill) return; 
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
        Debug.Log(isCharging);
        isCharging = true;
        Debug.Log(isCharging);
        if (playerTransform == null)
        {
            Debug.LogError("playerTransform is not assigned!");
            yield break; 
        }

        float offsetX = playerTransform.position.x > transform.position.x ? -3f : 3f;
        Vector2 targetPosition = new Vector2(playerTransform.position.x + offsetX, playerTransform.position.y);
        float additionalHeight = 0.5f;
        targetPosition.y += additionalHeight;

        transform.position = targetPosition;

        if (rb != null)
        {
            rb.gravityScale = 4f;
        }
        else
        {
            Debug.LogError("Rigidbody2D is missing!");
            yield break;
        }

        yield return new WaitForSeconds(0.7f);

        if (playerTransform == null) yield break;

        float chargeDirectionX = playerTransform.position.x > transform.position.x ? 1f : -1f;
        FlipToDirection(chargeDirectionX);

        if (WallCheck != null)
        {
            while (!Physics2D.OverlapCircle(WallCheck.position, wallCheckRadius, wallLayer))
            {
                FlipToDirection(chargeDirectionX);

                if (isGrounded)
                {
                    rb.linearVelocity = new Vector2(chargeDirectionX * chargeSpeed, rb.linearVelocity.y);
                }

                Collider2D playerCollider = Physics2D.OverlapCircle(ChargingAttackTransform.position, radiusCharging, player);
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

        rb.linearVelocity = Vector2.zero;

        if (gangsterHealth != null)
        {
            Debug.Log("tong vo tuong");
            gangsterHealth.StunForDuration(3f);
        }
        else
        {
            Debug.Log("gangsterHealth missing");
        }

        SpawnRocks();
        yield return new WaitForSeconds(3f);

        transform.position = new Vector2(resetX, resetY);
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 4f;
        isCharging = false;
        isUsingSkill = false;
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
        float lastRockX = Mathf.NegativeInfinity;

        for (int i = 0; i < 3; i++)
        {
            float randomX;

            do
            {
                randomX = UnityEngine.Random.Range(-4.2f, 4.2f);
            }
            while (Mathf.Abs(randomX - lastRockX) < 2f); 

            lastRockX = randomX;

            Vector2 rockPosition = new Vector2(randomX, 15f);
            GameObject rock = Instantiate(rockPrefab, rockPosition, Quaternion.identity);
            rock.GetComponent<NetworkObject>().Spawn();

            Destroy(rock, 5f);
        }
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if ((groundLayer.value & (1 << collision.gameObject.layer)) > 0)
        {
            rb.gravityScale = 0;
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
