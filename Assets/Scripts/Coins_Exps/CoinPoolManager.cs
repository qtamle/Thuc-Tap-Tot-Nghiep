using System.Collections.Generic;
using UnityEngine;

public class CoinPoolManager : MonoBehaviour
{
    [Header("Coin Pool Settings")]
    public GameObject coinPrefab;
    public GameObject secondaryCoinPrefab;
    public int initialPoolSize = 10;
    private Queue<GameObject> coinPool = new Queue<GameObject>();
    private Queue<GameObject> secondaryCoinPool = new Queue<GameObject>();

    private void Start()
    {
        PopulateCoinPool(initialPoolSize, coinPrefab, coinPool);
        PopulateCoinPool(initialPoolSize, secondaryCoinPrefab, secondaryCoinPool);
    }

    void PopulateCoinPool(int size, GameObject coinPrefab, Queue<GameObject> pool)
    {
        for (int i = 0; i < size; i++)
        {
            GameObject coin = Instantiate(coinPrefab, transform);
            coin.SetActive(false); 
            pool.Enqueue(coin);
        }
    }

    public GameObject GetCoinFromPool(GameObject coinPrefab)
    {
        if (coinPrefab == this.coinPrefab)
        {
            if (coinPool.Count > 0)
            {
                GameObject coin = coinPool.Dequeue();
                coin.SetActive(true);

                CoinsScript coinScript = coin.GetComponent<CoinsScript>();
                if (coinScript != null)
                { 
                    coinScript.enabled = true;  
                }

                CircleCollider2D collider = coin.GetComponent<CircleCollider2D>();
                if (collider != null)
                {
                    collider.enabled = true;  
                }

                Rigidbody2D rb = coin.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.bodyType = RigidbodyType2D.Dynamic;  
                    rb.simulated = true;

                    rb.linearVelocity = Vector2.zero;  
                    rb.gravityScale = 1;
                }

                return coin;
            }
            else
            {
                GameObject newCoin = Instantiate(coinPrefab, transform);

                CoinsScript coinScript = newCoin.GetComponent<CoinsScript>();
                if (coinScript != null)
                {
                    coinScript.enabled = true;
                }

                CircleCollider2D collider = newCoin.GetComponent<CircleCollider2D>();
                if (collider != null)
                {
                    collider.enabled = true; 
                }

                Rigidbody2D rb = newCoin.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.bodyType = RigidbodyType2D.Dynamic;
                    rb.simulated = true;

                    rb.linearVelocity = Vector2.zero;
                    rb.gravityScale = 1;
                }

                return newCoin;
            }
        }
        else if (coinPrefab == secondaryCoinPrefab)
        {
            if (secondaryCoinPool.Count > 0)
            {
                GameObject coin = secondaryCoinPool.Dequeue();
                coin.SetActive(true);

                CoinsScript coinScript = coin.GetComponent<CoinsScript>();
                if (coinScript != null)
                {
                    coinScript.enabled = true;
                }

                CircleCollider2D collider = coin.GetComponent<CircleCollider2D>();
                if (collider != null)
                {
                    collider.enabled = true;  
                }

                Rigidbody2D rb = coin.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.bodyType = RigidbodyType2D.Dynamic;  
                    rb.simulated = true;

                    rb.linearVelocity = Vector2.zero;
                    rb.gravityScale = 1;
                }

                return coin;
            }
            else
            {
                GameObject newCoin = Instantiate(secondaryCoinPrefab, transform);

                CoinsScript coinScript = newCoin.GetComponent<CoinsScript>();
                if (coinScript != null)
                {
                    coinScript.enabled = true;  
                }

                CircleCollider2D collider = newCoin.GetComponent<CircleCollider2D>();
                if (collider != null)
                {
                    collider.enabled = true; 
                }

                Rigidbody2D rb = newCoin.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.bodyType = RigidbodyType2D.Dynamic;  
                    rb.simulated = true;

                    rb.linearVelocity = Vector2.zero;
                    rb.gravityScale = 1;
                }

                return newCoin;
            }
        }

        return null;
    }

    public void ReturnCoinToPool(GameObject coin)
    {
        coin.SetActive(false);

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
