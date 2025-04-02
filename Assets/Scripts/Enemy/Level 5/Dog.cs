using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class Dog : NetworkBehaviour
{
    [Header("Raycast Check")]
    public LayerMask wallCheck;
    public float rayDistance;
    public Transform wallCheckTransform;
    public bool hasChecked = false;

    [Header("Ground Check")]
    public LayerMask groundLayer;
    public Transform groundCheck;

    [Header("Dash")]
    public float dashSpeed = 5f;
    public float dashDuration = 0.5f;
    public float waitBeforeDash = 2f;
    public Transform dashCheckTransform;
    public float dashCollisionRadius = 0.2f;

    [Header("Wall Collision")]
    public float destroyDelay = 2f;

    [Header("Attack")]
    public Transform attackCheckTransform;
    public LayerMask playerLayer;
    public float attackRadius;

    private bool isDashing = false;
    private bool canDash = true;
    private bool isDashOne = false;  

    private Rigidbody2D rb;
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        animator.SetBool("Idle", true);
    }

    private void FixedUpdate()
    {
        if (!hasChecked)
        {
            RaycastHit2D hit = Physics2D.Raycast(wallCheckTransform.position, wallCheckTransform.right, rayDistance, wallCheck);
            if (hit.collider != null)
            {
                Flip();
            }
            hasChecked = true;
        }

        if (canDash && !isDashing && IsGrounded())
        {
            StartCoroutine(Dash());
        }

        if (isDashing)
        {
            if (Physics2D.OverlapCircle(dashCheckTransform.position, dashCollisionRadius, wallCheck))
            {
                if (isDashOne)
                {
                    StopDash();
                    StartCoroutine(DestroyAfterDelay());
                }
                else
                {
                    StopDash();
                }
            }

            Collider2D[] hitPlayer = Physics2D.OverlapCircleAll(attackCheckTransform.position, attackRadius, playerLayer);
            foreach (Collider2D collider in hitPlayer)
            {
                if (collider.CompareTag("Player"))
                {
                    TakeDamagePlayer(collider);
                }
            }
        }
    }

    private IEnumerator Dash()
    {
        canDash = false;

        animator.SetBool("Idle", false);
        animator.SetBool("BeforeRun", true);

        yield return new WaitForSeconds(waitBeforeDash);

        isDashing = true;
        Vector2 dashDirection = transform.localScale.x > 0 ? Vector2.right : Vector2.left;
        float dashTime = 0f;

        animator.SetBool("Run", true);
        animator.SetBool("BeforeRun", false);

        while (dashTime < dashDuration)
        {
            rb.linearVelocity = dashDirection * dashSpeed;
            dashTime += Time.deltaTime;
            yield return null;
        }

        StopDash();
        Flip();
        isDashOne = true; 
        yield return new WaitForSeconds(1f);  
        canDash = true;
    }

    private void StopDash()
    {
        isDashing = false;
        rb.linearVelocity = Vector2.zero;

        animator.SetBool("Idle", true);
        animator.SetBool("BeforeRun", false);
        animator.SetBool("Run", false);
    }

    private IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(destroyDelay);
        //Destroy(gameObject);
        gameObject.GetComponent<NetworkObject>().Despawn(true);
    }

    private void Flip()
    {
        Vector3 localScale = transform.localScale;
        localScale.x *= -1;
        transform.localScale = localScale;
    }

    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.1f, groundLayer);
    }

    private void TakeDamagePlayer(Collider2D player)
    {
        DamagePlayerInterface damage = player.GetComponent<DamagePlayerInterface>();
        if (damage != null)
        {
            damage.DamagePlayer(1); 
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(wallCheckTransform.position, wallCheckTransform.position + wallCheckTransform.right * rayDistance);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(dashCheckTransform.position, dashCollisionRadius);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(attackCheckTransform.position, attackRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, 0.1f);
    }
}
