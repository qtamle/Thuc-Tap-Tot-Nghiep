using UnityEngine;

public class BombDamage : MonoBehaviour
{
    public float radius;
    public LayerMask bombLayer;

    public void BombExplosion()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius, bombLayer);

        foreach (Collider2D hit in hits)
        {
            DamagePlayerInterface damageable = hit.GetComponent<DamagePlayerInterface>();
            if (damageable != null)
            {
                damageable.DamagePlayer(1);
            }
        }

        Destroy(gameObject);
    }
}
