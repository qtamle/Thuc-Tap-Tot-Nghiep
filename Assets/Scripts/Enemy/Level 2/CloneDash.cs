using System.Collections;
using UnityEngine;

public class CloneDash : MonoBehaviour
{
    public float cloneSpeed = 5f;
    private Vector3 originalPosition;
    private string playerTag = "Player";
    private Rigidbody2D rb;

    [Header("Attack")]
    public float radius;
    public Transform attackTransform;
    public LayerMask playerLayer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        originalPosition = transform.position;
    }

    public void DashTowardsPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag(playerTag);

        if (player != null)
        {
            // Tính hướng lướt từ clone tới Player
            StartCoroutine(Dash(player.transform));
        }
        else
        {
            Debug.LogError("Player không tìm thấy!");
        }
    }

    IEnumerator Dash(Transform player)
    {
        float dashDuration = 3f;

        Vector3 directionToPlayer = player.position - transform.position;
        directionToPlayer.y = 0f; 

        Vector3 dashDirection = directionToPlayer.x > 0 ? Vector3.right : Vector3.left;

        if (dashDirection == Vector3.right)
        {
            transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        }
        else
        {
            transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        }

        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = false;
        }

        // Thực hiện lướt
        float timer = 0f;
        while (timer < dashDuration)
        {
            transform.position += dashDirection * cloneSpeed * Time.deltaTime;

            Collider2D[] hitPlayer = Physics2D.OverlapCircleAll(attackTransform.position, radius, playerLayer);

            foreach (Collider2D hitPlayer2 in hitPlayer)
            {
                if (hitPlayer2 != null)
                {
                    DamagePlayerInterface damage = hitPlayer2.GetComponent<DamagePlayerInterface>();
                    if (damage != null)
                    {
                        damage.DamagePlayer(2);
                    }
                }
            }
            
            timer += Time.deltaTime;
            yield return null;
        }

        transform.position += dashDirection * cloneSpeed * (dashDuration - timer);

        if (collider != null)
        {
            collider.enabled = true;
        }

        yield return new WaitForSeconds(0.5f);
        transform.position = originalPosition;

        if (rb != null)
        {
            rb.gravityScale = 4f;
        }
        else
        {
            yield return null;
        }
    }

    void OnDrawGizmosSelected()
    {
        if (attackTransform != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackTransform.position, radius);
        }
    }
}
