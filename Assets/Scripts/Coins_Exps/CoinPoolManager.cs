using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CoinPoolManager : NetworkBehaviour
{
    public NetworkObject coinPrefab;
    public NetworkObject secondaryCoinPrefab;
    public int initialPoolSize = 10;

    private Queue<NetworkObject> coinPool = new Queue<NetworkObject>();
    private Queue<NetworkObject> secondaryCoinPool = new Queue<NetworkObject>();

    public static CoinPoolManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (IsServer)
        {
            InitializePools();
        }
    }

    private void InitializePools()
    {
        PopulatePool(initialPoolSize, coinPrefab, coinPool);
        PopulatePool(initialPoolSize, secondaryCoinPrefab, secondaryCoinPool);
    }

    private void PopulatePool(int size, NetworkObject prefab, Queue<NetworkObject> pool)
    {
        for (int i = 0; i < size; i++)
        {
            NetworkObject coin = Instantiate(prefab, transform);
            coin.Spawn();
            ReturnCoinToPool(coin);
            Debug.Log("Spawn coin tu Pool: Coin " + i);
        }
    }

    public NetworkObject GetCoinFromPool(Vector3 position, bool isSecondary = false)
    {
        if (!IsServer || Instance == null)
        {
            Debug.LogError("CoinPoolManager instance is null or not running on the server.");
            return null;
        }

        Queue<NetworkObject> targetPool = isSecondary ? secondaryCoinPool : coinPool;
        NetworkObject prefab = isSecondary ? secondaryCoinPrefab : coinPrefab;

        if (targetPool.Count > 0)
        {
            NetworkObject coin = targetPool.Dequeue();
            InitializeCoin(coin, position);
            return coin;
        }
        return CreateNewCoin(prefab, position);
    }

    private NetworkObject CreateNewCoin(NetworkObject prefab, Vector3 position)
    {
        NetworkObject newCoin = Instantiate(prefab, position, Quaternion.identity);
        newCoin.Spawn();
        InitializeCoin(newCoin, position);
        return newCoin;
    }

    private void InitializeCoin(NetworkObject coin, Vector3 position)
    {
        coin.transform.position = position;
        NetworkCoin networkCoin = coin.GetComponent<NetworkCoin>();
        if (networkCoin != null)
        {
            networkCoin.IsActive.Value = true;
        }
    }

    public void ReturnCoinToPool(NetworkObject coin)
    {
        if (!IsServer || coin == null)
        {
            Debug.LogError("Invalid attempt to return coin to pool.");
            return;
        }

        NetworkCoin networkCoin = coin.GetComponent<NetworkCoin>();
        if (networkCoin != null)
        {
            networkCoin.IsActive.Value = false;
        }

        coin.transform.SetParent(transform);
        coin.transform.localPosition = Vector3.zero;

        if (coin.CompareTag("Coin"))
        {
            coinPool.Enqueue(coin);
        }
        else if (coin.CompareTag("SecondaryCoin"))
        {
            secondaryCoinPool.Enqueue(coin);
        }
    }
}
