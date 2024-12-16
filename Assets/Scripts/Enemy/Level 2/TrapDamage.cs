using UnityEngine;

public class TrapDamage : MonoBehaviour
{
    public Vector2 boxSize = new Vector2(2f, 2f);
    public LayerMask playerLayer;

    void Update()
    {
        Collider2D[] colliders = Physics2D.OverlapBoxAll(transform.position, boxSize, 0f, playerLayer);

        foreach (Collider2D collider in colliders)
        {
            if (collider.CompareTag("Player"))
            {
                DamagePlayerInterface damage = collider.GetComponent<DamagePlayerInterface>();

                if (damage != null)
                {
                    damage.DamagePlayer(1);
                    Destroy(gameObject);
                }
            }
        }

        Destroy(gameObject, 10f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, boxSize);
    }
}
