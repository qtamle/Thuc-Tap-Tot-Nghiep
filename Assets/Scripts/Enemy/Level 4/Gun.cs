using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class Gun : NetworkBehaviour
{
    [Header("Raycast Check")]
    public LayerMask wallCheck;
    public float rayDistance;
    public Transform raycastTransform;
    public bool hasChecked = false;

    [Header("Shoot")]
    public GameObject bulletPrefab;
    public Transform shootTransform;
    public float shootInterval = 3.5f;
    public float bulletSpeed;
    private float shootTimer = 0f;

    [Header("Ground Check")]
    public LayerMask groundLayer;
    public Transform groundCheck;

    private Rigidbody2D rb;
    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }
    private void Update()
    {
        if (!hasChecked)
        {
            RaycastHit2D hit = Physics2D.Raycast(raycastTransform.position, transform.right, rayDistance, wallCheck);

            if (hit.collider != null)
            {
                Flip();
            }

            hasChecked = true;
        }

        if (IsGrounded())
        {
            rb.gravityScale = 0f;
        }

        // Xử lý bắn đạn
        shootTimer += Time.deltaTime;
        if (shootTimer >= shootInterval && IsGrounded())
        {
            StartCoroutine(Shoot());
            shootTimer = 0f;
        }

    }

    private void Flip()
    {
        Vector3 localScale = transform.localScale;
        localScale.x *= -1; 
        transform.localScale = localScale;
    }

    private IEnumerator Shoot()
    {
        animator.SetTrigger("Shoot");

        yield return new WaitForSeconds(0.5f);

        GameObject bullet = Instantiate(bulletPrefab, shootTransform.position, Quaternion.identity);

        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            Vector2 shootDirection = (transform.localScale.x > 0) ? Vector2.right : Vector2.left;
            rb.linearVelocity = shootDirection * bulletSpeed;
        }

        bullet.GetComponent<NetworkObject>().Spawn();
    }

    bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.1f, groundLayer);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(raycastTransform.position, raycastTransform.position + transform.right * rayDistance);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, 0.1f);
    }
}
