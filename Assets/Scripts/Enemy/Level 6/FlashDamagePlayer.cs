using UnityEngine;

public class FlashDamagePlayer : MonoBehaviour
{
    public Transform attackTransform;
    public Vector2 boxSize = new Vector2(2f, 2f);
    public LayerMask playerLayer;

    public bool canDamage = false;

    void Update()
    {
        if (canDamage)
        {
            Collider2D[] colliders = Physics2D.OverlapBoxAll(attackTransform.position, boxSize, 0f, playerLayer);

            foreach (Collider2D collider in colliders)
            {
                if (collider.CompareTag("Player"))
                {
                    DamagePlayerInterface damage = collider.GetComponent<DamagePlayerInterface>();

                    if (damage != null)
                    {
                        damage.DamagePlayer(3);
                    }
                }
            }
        }
    }

    public void SetCanDamage(bool value)
    {
        canDamage = value;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(attackTransform.position, boxSize);
    }
}
