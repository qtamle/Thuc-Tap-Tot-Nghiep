using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class BulletDamage : NetworkBehaviour
{
    public float destroyTime = 10f; // Thời gian trước khi hủy đối tượng

    void Start()
    {
        if (IsServer) // Chỉ server mới có thể hủy đối tượng
        {
            StartCoroutine(DestroyAfterSeconds(destroyTime));
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        NetworkObject go = GetComponent<NetworkObject>();
        if (collision.gameObject.CompareTag("Player"))
        {
            DamagePlayerInterface damage = collision.GetComponent<DamagePlayerInterface>();

            if (damage != null)
            {
                damage.DamagePlayer(1);
                if (IsServer)
                {
                    go.Despawn(true);
                }
            }
        }
    }

    IEnumerator DestroyAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);

        if (TryGetComponent(out NetworkObject networkObject) && networkObject.IsSpawned)
        {
            Debug.Log("Destroying object: " + gameObject.name);
            networkObject.Despawn(true); // Đồng bộ hóa việc hủy đối tượng với các client
            Destroy(gameObject); // Hủy đối tượng trên server
        }
        else
        {
            Debug.LogWarning("Object is not spawned or already destroyed.");
        }
    }
}
