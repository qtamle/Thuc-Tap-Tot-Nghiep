using EZCameraShake;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using EZCameraShake;

public class BigBomb : NetworkBehaviour
{
    public float damageRadius = 1.3f;
    public int damageAmount = 2;
    public LayerMask playerLayer;

    private bool isShaking;

    IEnumerator WaitForExplode()
    {
        yield return new WaitForSeconds(1.5f);
    }

    private IEnumerator ResetShakeState()
    {
        yield return new WaitForSeconds(0.2f);
        isShaking = false;
    }

    public void Explode()
    {
        StartCoroutine(WaitForExplode());

        CameraShaker.Instance.ShakeOnce(5f, 5f, 0.2f, 0.2f);

        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(
            transform.position,
            damageRadius,
            playerLayer
        );
        foreach (var hitCollider in hitColliders)
        {
            DamagePlayerInterface damage = hitCollider.GetComponent<DamagePlayerInterface>();
            if (damage != null)
            {
                damage.DamagePlayer(damageAmount);
            }
        }
        gameObject.GetComponent<NetworkObject>().Despawn(true);
        // Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, damageRadius);
    }
}
