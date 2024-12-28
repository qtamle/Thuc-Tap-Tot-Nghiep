using System.Collections;
using UnityEngine;

public class BigBomb : MonoBehaviour
{
    public float damageRadius = 1.3f;
    public int damageAmount = 2;
    public LayerMask playerLayer;

    IEnumerator WaitForExplode()
    {
        yield return new WaitForSeconds(1.5f);
    }

    public void Explode()
    {
        StartCoroutine(WaitForExplode());

        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, damageRadius, playerLayer);
        foreach (var hitCollider in hitColliders)
        {
            DamagePlayerInterface damage = hitCollider.GetComponent<DamagePlayerInterface>();
            if (damage != null)
            {   
                damage.DamagePlayer(damageAmount);
            }
        }
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, damageRadius);
    }

}
