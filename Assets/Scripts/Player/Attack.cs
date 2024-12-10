using UnityEngine;

public class Attack : MonoBehaviour
{
    public float radius;
    public LayerMask enemyLayer;
    public Transform attackPoints;
    private void Update()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(attackPoints.position, radius, enemyLayer);

        foreach (Collider2D enemy in enemies)
        {
            Destroy(enemy.gameObject);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(attackPoints.position, radius);
    }
}
