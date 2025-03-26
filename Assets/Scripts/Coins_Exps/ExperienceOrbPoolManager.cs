using Unity.Netcode;
using UnityEngine;

public class ExperienceOrbPoolManager : NetworkBehaviour
{
    public static ExperienceOrbPoolManager Instance;

    [Header("Experience Orb Pool Settings")]
    public GameObject experienceOrbPrefab;
    public int initialPoolSize = 20;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        NetworkObjectPool.Singleton.RegisterPrefabInternal(experienceOrbPrefab, initialPoolSize);
    }

    public NetworkObject GetOrbFromPool(Vector3 position)
    {
        if (!IsServer)
        {
            Debug.LogWarning("Client tried to get orb from pool");
            return null;
        }

        if (experienceOrbPrefab == null)
        {
            Debug.LogError("Experience orb prefab is null!");
            return null;
        }

        NetworkObject networkObject = NetworkObjectPool.Singleton.GetNetworkObject(
            experienceOrbPrefab,
            position
        );

        if (networkObject == null)
        {
            Debug.LogError("Failed to get orb from pool");
            return null;
        }

        // Kích hoạt object
        // GameObject obj = networkObject.gameObject;
        // obj.GetComponent<NetworkObject>().Spawn();

        if (!networkObject.IsSpawned)
        {
            networkObject.Spawn();
        }

        // Kích hoạt các component
        ExperienceScript experienceScript = networkObject.GetComponent<ExperienceScript>();
        if (experienceScript != null)
            experienceScript.enabled = true;

        PolygonCollider2D collider = networkObject.GetComponent<PolygonCollider2D>();
        if (collider != null)
            collider.enabled = true;

        Rigidbody2D rb = networkObject.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.simulated = true;
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = 1;
        }

        return networkObject;
    }

    public void ReturnOrbToPool(NetworkObject orb)
    {
        if (!IsServer)
            return;

        // Vô hiệu hóa các component
        ExperienceScript experienceScript = orb.GetComponent<ExperienceScript>();
        if (experienceScript != null)
            experienceScript.enabled = false;

        PolygonCollider2D collider = orb.GetComponent<PolygonCollider2D>();
        if (collider != null)
            collider.enabled = false;

        Rigidbody2D rb = orb.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.simulated = false;
            rb.linearVelocity = Vector2.zero;
        }

        if (orb.IsSpawned)
        {
            orb.Despawn(false);
        }

        NetworkObjectPool.Singleton.ReturnNetworkObject(orb, experienceOrbPrefab);
    }
}
