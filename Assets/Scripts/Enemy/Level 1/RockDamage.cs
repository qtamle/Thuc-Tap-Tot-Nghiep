using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class RockDamage : NetworkBehaviour
{
    private RockPool rockPool;

    private void Start()
    {
        rockPool = FindFirstObjectByType<RockPool>();
    }

    private void OnEnable()
    {
        StartCoroutine(WaitForReturnToPool());
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            DamagePlayerInterface damage = collision.GetComponent<DamagePlayerInterface>();

            if (damage != null)
            {
                damage.DamagePlayer(2);
                StopAllCoroutines();
                // Chỉ server mới được phép trả đối tượng về pool
                if (IsServer)
                {
                    rockPool.ReturnRock(gameObject);
                }
            }
        }
    }

    IEnumerator WaitForReturnToPool()
    {
        yield return new WaitForSeconds(4f);
        rockPool.ReturnRock(gameObject);
    }
}
