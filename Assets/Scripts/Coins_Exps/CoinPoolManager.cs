using Unity.Netcode;
using UnityEngine;

public class CoinPoolManager : NetworkBehaviour
{
    public static CoinPoolManager Instance;

    [Header("Coin Pool Settings")]
    public GameObject coinPrefab;
    public GameObject secondaryCoinPrefab;
    public int initialPoolSize = 10;

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
        NetworkObjectPool.Singleton.RegisterPrefabInternal(coinPrefab, initialPoolSize);
        NetworkObjectPool.Singleton.RegisterPrefabInternal(secondaryCoinPrefab, initialPoolSize);
    }

    // public override void OnNetworkSpawn()
    // {
    //     if (IsServer)
    //     {
    //         // Đăng ký các prefab với NetworkObjectPool
    //     }
    // }

    public NetworkObject GetCoinFromPool(Vector3 position, bool isSecondary)
    {
        if (!IsServer)
        {
            Debug.LogWarning("Client tried to get coin from pool");
            return null;
        }
        else
        {
            Debug.Log("Server RPC get coin from pool");
        }

        GameObject prefab = isSecondary ? secondaryCoinPrefab : coinPrefab;
        if (prefab == null)
        {
            Debug.LogError("Coin prefab is null!");
            return null;
        }
        NetworkObject networkObject = NetworkObjectPool.Singleton.GetNetworkObject(
            prefab,
            position
        );
        GameObject obj = networkObject.gameObject;
        obj.GetComponent<NetworkObject>().Spawn();
        if (networkObject == null)
        {
            Debug.LogError("Failed to get coin from pool");
            return null;
        }

        if (!networkObject.IsSpawned)
        {
            networkObject.Spawn();
        }

        // Kích hoạt các component
        CoinsScript coinScript = networkObject.GetComponent<CoinsScript>();
        if (coinScript != null)
            coinScript.enabled = true;

        CircleCollider2D collider = networkObject.GetComponent<CircleCollider2D>();
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

    public void ReturnCoinToPool(NetworkObject coin)
    {
        if (!IsServer)
            return;

        // Vô hiệu hóa các component
        CoinsScript coinScript = coin.GetComponent<CoinsScript>();
        if (coinScript != null)
            coinScript.enabled = false;

        CircleCollider2D collider = coin.GetComponent<CircleCollider2D>();
        if (collider != null)
            collider.enabled = false;

        Rigidbody2D rb = coin.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.simulated = false;
            rb.linearVelocity = Vector2.zero;
        }

        if (coin.IsSpawned)
        {
            coin.Despawn(false);
        }

        NetworkObjectPool.Singleton.ReturnNetworkObject(
            coin,
            coin.CompareTag("Coin") ? coinPrefab : secondaryCoinPrefab
        );
    }
}
