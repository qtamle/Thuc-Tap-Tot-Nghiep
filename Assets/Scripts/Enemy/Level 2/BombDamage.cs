using Unity.Netcode;
using UnityEngine;

public class BombDamage : NetworkBehaviour
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
                if (IsServer)
                {
                    NetworkObject go = GetComponent<NetworkObject>();
                    // go.Despawn(true);
                }
            }
        }

  
    }
}
