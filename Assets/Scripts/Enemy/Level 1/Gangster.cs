using UnityEngine;
using System.Collections;

public class Gangster : MonoBehaviour
{
    [Header("Spawn Settings")]
    public Vector2 spawnPosition = new Vector2(0f, 8f);
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;
    public GameObject rockPrefab; // Prefab đá
    public Transform playerTransform;

    [Header("Charging Settings")]
    public LayerMask wallLayer;
    public float chargeSpeed = 10f;
    public float resetX = 0f;
    public float resetY = 4.5f;
    public Transform WallCheck;
    public float wallCheckRadius;

    private Rigidbody2D rb;
    private bool isGrounded = false;
    private bool isCharging = false;
    private bool isSkillActive = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (rb == null)
        {
            Debug.LogError("Rigidbody2D is missing!");
        }

        Spawn();
    }

    private void Update()
    {
        CheckGround();

        if (Input.GetKeyDown(KeyCode.P))
        {
            UseJumpSkill();
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            UseChargeSkill();
        }
    }

    public void Spawn()
    {
        transform.position = spawnPosition;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = 4f;
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
    }

    public void UseJumpSkill()
    {
        if (isGrounded)
        {
            StartCoroutine(ExecuteJumpSkill());
        }
    }

    private IEnumerator ExecuteJumpSkill()
    {
        isSkillActive = true;

        float targetY = 15f;
        rb.linearVelocity = new Vector2(0, 40f);
        yield return new WaitForSeconds(1f);

        transform.position = new Vector2(0f, targetY);
        yield return new WaitForSeconds(0.5f);

        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 4f;
        yield return new WaitForSeconds(1f);

        //if (isGrounded)
        //{
        //    SpawnRocks();
        //}
    }

    public void UseChargeSkill()
    {
        if (!isCharging)
        {
            StartCoroutine(ExecuteChargeSkill());
        }
    }
    private IEnumerator ExecuteChargeSkill()
    {
        yield return new WaitForSeconds(1f);

        isCharging = true;

        // Xác định vị trí gần player
        Vector2 targetPosition = new Vector2(playerTransform.position.x, playerTransform.position.y);

        float additionalHeight = 0.5f;
        targetPosition.y += additionalHeight;

        //// Sử dụng raycast dưới chân để tìm vị trí mặt đất
        //RaycastHit2D groundHit = Physics2D.Raycast(new Vector2(targetPosition.x, targetPosition.y + 1f), Vector2.down, 2f, groundLayer);

        //if (groundHit.collider != null)
        //{
        //    targetPosition.y = groundHit.point.y;
        //}

        transform.position = targetPosition;
        rb.gravityScale = 4f;

        float chargeDirectionX = playerTransform.position.x > transform.position.x ? 1f : -1f;

        FlipToDirection(chargeDirectionX);

        yield return new WaitForSeconds(2f);

        while (!Physics2D.OverlapCircle(WallCheck.position, wallCheckRadius, wallLayer))
        {
            if (isGrounded)
            {
                rb.linearVelocity = new Vector2(chargeDirectionX * chargeSpeed, rb.linearVelocity.y);
                yield return null;
            }
        }

        rb.linearVelocity = Vector2.zero;
        SpawnRocks();
        yield return new WaitForSeconds(5f);

        transform.position = new Vector2(resetX, resetY);
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 1;
        isCharging = false;
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
                randomX = Random.Range(-4f, 4f);
            }
            while (Mathf.Abs(randomX - lastRockX) < 2f); 

            lastRockX = randomX;

            Vector2 rockPosition = new Vector2(randomX, 15f);
            GameObject rock = Instantiate(rockPrefab, rockPosition, Quaternion.identity);

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
    }
}
