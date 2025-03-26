using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class TrapDamage : NetworkBehaviour
{
    public Vector2 boxSize = new Vector2(2f, 2f);
    public LayerMask playerLayer;

    public bool canDamage = false;
    public float despawnTime = 10f; // Thời gian trước khi hủy đối tượng

    void Start()
    {
        if (IsServer) // Chỉ server mới có thể despawn đối tượng
        {
            StartCoroutine(DespawnAfterSeconds(despawnTime));
        }
    }

    void Update()
    {
        if (canDamage)
        {
            Collider2D[] colliders = Physics2D.OverlapBoxAll(
                transform.position,
                boxSize,
                0f,
                playerLayer
            );

            foreach (Collider2D collider in colliders)
            {
                if (collider.CompareTag("Player"))
                {
                    DamagePlayerInterface damage = collider.GetComponent<DamagePlayerInterface>();

                    if (damage != null)
                    {
                        damage.DamagePlayer(1);
                        if (IsServer)
                        {
                            GetComponent<NetworkObject>().Despawn(true);
                        }
                    }
                }
            }
        }
    }

    IEnumerator DespawnAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);

        if (TryGetComponent(out NetworkObject networkObject) && networkObject.IsSpawned)
        {
            Debug.Log("Despawning object: " + gameObject.name);
            networkObject.Despawn(true); // Đồng bộ hóa việc hủy đối tượng với các client
        }
        else
        {
            Debug.LogWarning("Object is not spawned or already destroyed.");
        }
    }

    public void SetCanDamage(bool value)
    {
        canDamage = value;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, boxSize);
    }
}
