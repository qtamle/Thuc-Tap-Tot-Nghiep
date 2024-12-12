using UnityEngine;

public class DamagePlayer : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            DamagePlayerInterface damage = collision.GetComponent<DamagePlayerInterface>();

            if (damage != null)
            {
                damage.DamagePlayer(1);
            }
        }

    }
}
