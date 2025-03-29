using Unity.Netcode;
using UnityEngine;

public class BouncingSawLauncher : NetworkBehaviour
{
    [Header("Saw Settings")]
    public GameObject bouncingSawPrefab;
    public float throwSpeed = 15f;
    public LayerMask enemyLayer;
    public float searchRadius = 15f;

    public void LaunchBouncingSaw()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(
            transform.position,
            searchRadius,
            enemyLayer
        );

        if (enemies.Length > 0)
        {
            Transform targetEnemy = enemies[0].transform;

            GameObject saw = Instantiate(
                bouncingSawPrefab,
                transform.position,
                Quaternion.identity
            );
            saw.GetComponent<NetworkObject>().Spawn();
            ChainsawLevel4 bouncingSaw = saw.GetComponent<ChainsawLevel4>();
            if (bouncingSaw != null)
            {
                Vector2 direction = (targetEnemy.position - transform.position).normalized;
                bouncingSaw.SetupSaw(direction);
                saw.GetComponent<Rigidbody2D>().linearVelocity = direction * throwSpeed;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, searchRadius);
    }
}
