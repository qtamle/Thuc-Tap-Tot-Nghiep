using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class EnemyShootOnline : NetworkBehaviour
{
    [Header("Move")]
    public float moveSpeed;
    private bool movingRight = true;
    private bool isGrounded = false;

    [Header("Check")]
    public Transform wallCheck;
    public Transform groundCheck;
    public LayerMask wallLayer;
    public LayerMask groundLayer;

    [Header("Shoot")]
    public GameObject bulletPrefab;
    public Transform shootPoint;
    public float shootCooldown;
    private float shootTimer;
    private bool isShooting = false;

    private Rigidbody2D rb;
    public Animator anim;

    private void Start()
    {

        rb = GetComponent<Rigidbody2D>();
        shootTimer = shootCooldown;
    }

    private void Update()
    {

        if (isGrounded && !isShooting)
        {
            Move();
            if (IsHittingWall())
            {
                Flip();
            }

            // Cập nhật thời gian bắn
            shootTimer -= Time.deltaTime;
            if (shootTimer <= 0f)
            {
                StartCoroutine(ShootAndPause());

            }
        }
    }


    IEnumerator ShootAndPause()
    {
        isShooting = true;
        anim.SetTrigger("Shoot");

        yield return new WaitForSeconds(1f);

        Shoot();

        yield return new WaitForSeconds(0.75f);

        isShooting = false;
        shootTimer = shootCooldown;
        anim.SetTrigger("Run");
    }

    void Shoot()
    {

        GameObject bullet = Instantiate(bulletPrefab, shootPoint.position, Quaternion.identity);
        bullet.GetComponent<NetworkObject>().Spawn(true);
        Bullet bulletScript = bullet.GetComponent<Bullet>();
        bulletScript.SetDirection(movingRight ? Vector2.right : Vector2.left);

        bullet.GetComponent<NetworkObject>().Despawn(true);
        Destroy(bullet, 5f);
    }

    bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.1f, groundLayer);
    }

    bool IsHittingWall()
    {
        return Physics2D.OverlapCircle(wallCheck.position, 0.1f, wallLayer);
    }

    void Move()
    {

        transform.Translate(Vector2.right * moveSpeed * Time.deltaTime * (movingRight ? 1 : -1));
    }

    void Flip()
    {
        movingRight = !movingRight;

        Vector3 localScale = transform.localScale;
        localScale.x *= -1;
        transform.localScale = localScale;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if ((groundLayer.value & (1 << collision.gameObject.layer)) > 0)
        {
            isGrounded = true;

            rb.gravityScale = 0;
        }
    }

    //private void OnCollisionExit2D(Collision2D collision)
    //{
    //    if ((groundLayer.value & (1 << collision.gameObject.layer)) > 0)
    //    {
    //        isGrounded = false;

    //        rb.gravityScale = 1;
    //    }
    //}

    private void OnDrawGizmosSelected()
    {
        if (wallCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(wallCheck.position, 0.1f);
        }

        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, 0.1f);
        }
    }
}
