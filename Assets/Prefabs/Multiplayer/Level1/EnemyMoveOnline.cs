using UnityEngine;
using Unity.Netcode;
public class EnemyMoveOnline : NetworkBehaviour
{
    public float moveSpeed;
    public Transform wallCheck;
    public Transform groundCheck;
    public LayerMask wallLayer;
    public LayerMask groundLayer;

    private bool movingRight = true;
    private bool isGrounded = false;
    private Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        isGrounded = IsGrounded();

        if (isGrounded)
        {
            Move();
            if (IsHittingWall())
            {
                Flip();
            }
        }
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
