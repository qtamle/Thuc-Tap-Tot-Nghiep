using System.Collections;
using UnityEngine;

public class BulletSnakeDamage : MonoBehaviour
{
    private BulletBoss4Pool BulletBoss4Pool;

    private void Start()
    {
        BulletBoss4Pool = FindFirstObjectByType<BulletBoss4Pool>();
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
                BulletBoss4Pool.ReturnBullet(gameObject);
            }
        }
    }

    IEnumerator WaitForReturnToPool()
    {
        yield return new WaitForSeconds(5f);
        BulletBoss4Pool.ReturnBullet(gameObject);
    }
}
