using System.Collections;
using UnityEngine;

public class RockDamage : MonoBehaviour
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
                rockPool.ReturnRock(gameObject);
            }
        }
    }

    IEnumerator WaitForReturnToPool()
    {
        yield return new WaitForSeconds(4f);
        rockPool.ReturnRock(gameObject);
    }
}
