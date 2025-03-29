using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class AssassinShoot : NetworkBehaviour
{
    [Header("Move Settings")]
    public float moveSpeed;
    public Transform wallCheck;
    public LayerMask wallLayer;
    private bool movingRight = true;

    [Header("Shoot Settings")]
    public GameObject bulletPrefabs;
    public Transform shootPoint;
    public Transform shootPointDown;
    public float bulletSpeed = 5f;
    private float shootCooldown;
    private float nextShootTime;

    [Header("Raycast")]
    public Transform raycastOrigin;
    public float raycastDistance;
    public LayerMask groundLayer;

    private bool hasCollidedWithWall = false;
    private bool initialRaycastUsed = false;

    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();

        animator.SetBool("isWalk", true);
        PerformInitialRaycast();
        StartCoroutine(CallHitColliderr());
        SetNextShootTime();
    }

    private void Update()
    {
        if (Time.time >= nextShootTime)
        {
            StartCoroutine(ShootWithDelay());
            SetNextShootTime();
        }
        else
        {
            Move();
        }

        if (hasCollidedWithWall && IsHittingWall())
        {
            Flip();
        }
    }

    bool IsHittingWall()
    {
        return Physics2D.OverlapCircle(wallCheck.position, 0.1f, wallLayer);
    }

    void Move()
    {
        animator.SetBool("isWalk", true);
        transform.Translate(Vector2.right * moveSpeed * Time.deltaTime * (movingRight ? 1 : -1));
    }

    void Flip()
    {
        movingRight = !movingRight;

        Vector3 localScale = transform.localScale;
        localScale.x *= -1;
        transform.localScale = localScale;
    }

    void PerformInitialRaycast()
    {
        if (!initialRaycastUsed)
        {
            RaycastHit2D hit = Physics2D.Raycast(
                transform.position,
                Vector2.right * (movingRight ? 1 : -1),
                5f,
                wallLayer
            );

            if (hit.collider != null)
            {
                Debug.Log("Initial Raycast: Wall detected, no flip.");
            }
            else
            {
                Debug.Log("Initial Raycast: No wall detected, flipping.");
                Flip();
            }

            initialRaycastUsed = true;
        }
    }

    IEnumerator ShootWithDelay()
    {
        float originalSpeed = moveSpeed;
        moveSpeed = 0;

        yield return new WaitForSeconds(1f);

        ShootBullet();

        yield return new WaitForSeconds(2f);

        ShootBullet();

        Invoke(nameof(ResumeMovement), 0.5f);

        SetNextShootTime();
    }

    void ShootBullet()
    {
        animator.SetBool("isWalk", false);
        animator.SetBool("isShoot", true);

        if (bulletPrefabs != null && shootPoint != null && shootPointDown != null)
        {
            bool hitGround = Physics2D.Raycast(
                raycastOrigin.position,
                Vector2.up,
                raycastDistance,
                groundLayer
            );
            Transform selectedShootPoint = hitGround ? shootPoint : shootPointDown;

            Quaternion originalRotation = transform.rotation;

            if (hitGround)
            {
                transform.rotation = Quaternion.Euler(0, 0, 180);
            }

            GameObject bullet = Instantiate(
                bulletPrefabs,
                selectedShootPoint.position,
                Quaternion.identity
            );
            bullet.GetComponent<NetworkObject>().Spawn();
            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();

            if (rb != null)
            {
                rb.linearVelocity = (hitGround ? Vector2.up : Vector2.down) * bulletSpeed;
            }

            StartCoroutine(ResetRotation(originalRotation));
        }
    }

    IEnumerator ResetRotation(Quaternion originalRotation)
    {
        yield return new WaitForSeconds(1f);
        transform.rotation = originalRotation;

        animator.SetBool("isWalk", true);
        animator.SetBool("isShoot", false);
    }

    void ResumeMovement()
    {
        moveSpeed = moveSpeed == 0 ? 1.5f : moveSpeed;
    }

    void SetNextShootTime()
    {
        nextShootTime = Time.time + Random.Range(4f, 6f);
    }

    private void OnDrawGizmosSelected()
    {
        if (wallCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(wallCheck.position, 0.1f);
        }

        if (shootPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(shootPoint.position, 0.1f);
        }

        if (raycastOrigin != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(
                raycastOrigin.position,
                raycastOrigin.position + Vector3.up * raycastDistance
            );
        }

        if (raycastOrigin != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(
                transform.position,
                transform.position + Vector3.right * 5f * (movingRight ? 1 : -1)
            );
        }
    }

    IEnumerator CallHitColliderr()
    {
        yield return new WaitForSeconds(7f);
        hasCollidedWithWall = true;
        yield break;
    }
}
