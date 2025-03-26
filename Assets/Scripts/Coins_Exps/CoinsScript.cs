using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CoinsScript : NetworkBehaviour
{
    [Header("Layer")]
    public LayerMask groundLayer;
    public LayerMask wallLayer;
    private NetworkRigidbody2D rb;

    [Header("Coin Type")]
    public bool isCoinType1;
    public bool isCoinType2;

    private CoinsManager coinsManager;

    public string playerLayerName = "Player";
    private bool hasBounced = false;

    [Header("Coin Life")]
    public float life = 10f;
    public float timeRemaining;
    private SpriteRenderer sprite;

    private CoinPoolManager coinPoolManager;

    [Header("Network Settings")]
    private NetworkVariable<bool> isCollected = new NetworkVariable<bool>(false);

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;

        int playerLayer = LayerMask.NameToLayer(playerLayerName);
        int coinLayer = gameObject.layer;

        if (playerLayer >= 0 && coinLayer >= 0)
        {
            Physics2D.IgnoreLayerCollision(coinLayer, playerLayer, true);
        }
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        int playerLayer = LayerMask.NameToLayer(playerLayerName);
        int coinLayer = gameObject.layer;

        if (playerLayer >= 0 && coinLayer >= 0)
        {
            Physics2D.IgnoreLayerCollision(coinLayer, playerLayer, false);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UpdateCoinInScene();
    }

    private void Start()
    {
        UpdateCoinInScene();
    }

    private void UpdateCoinInScene()
    {
        coinPoolManager = FindFirstObjectByType<CoinPoolManager>();

        coinsManager = UnityEngine.Object.FindFirstObjectByType<CoinsManager>();

        rb = GetComponent<NetworkRigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();

        if (coinsManager == null)
        {
            Debug.LogError("CoinsManager không tìm thấy!");
        }

        timeRemaining = life;
    }

    private void Update()
    {
        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;

            if (timeRemaining <= 5)
            {
                BlinkSprite();
            }
        }
        else
        {
            coinPoolManager.ReturnCoinToPool(gameObject.GetComponent<NetworkObject>());
        }
    }

    private void BlinkSprite()
    {
        if (sprite != null)
        {
            sprite.enabled = !sprite.enabled;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsServer)
            return;
        Debug.Log("Ontrigger 2D cua CoinScript");
        if (collision.CompareTag("FinalFloor"))
        {
            StopCoin();
            return;
        }

        if ((groundLayer.value & (1 << collision.gameObject.layer)) > 0)
        {
            if (!hasBounced)
            {
                BounceOffGround();
                hasBounced = true;
            }
            else
            {
                StopCoin();
            }
        }

        if (collision.CompareTag("Player"))
        {
            CollectCoin();
            // RequestCollectCoinServerRpc();
        }

        if ((wallLayer.value & (1 << collision.gameObject.layer)) > 0)
        {
            BounceOffWall();
        }
    }

    // [ServerRpc(RequireOwnership = false)]
    // private void RequestCollectCoinServerRpc()
    // {
    //     if (isCollected.Value)
    //         return;

    //     // Đánh dấu đã thu thập
    //     isCollected.Value = true;
    //     CollectCoinClientRpc();
    // }

    // [ClientRpc]
    // private void CollectCoinClientRpc()
    // {
    //     // Client nhận thông báo từ server
    //     if (IsServer)
    //         return;

    //     Debug.Log("Client received collection confirmation");
    //     HandleClientSideCollection();
    // }

    // private void HandleClientSideCollection()
    // {
    //     // Ẩn coin ngay lập tức trên mọi client
    //     gameObject.SetActive(false);

    //     // Cập nhật UI/effect cục bộ
    //     if (IsOwner)
    //     {
    //         coinsManager.AddCoins(isCoinType1 ? 1 : 0, isCoinType2 ? 1 : 0);
    //     }
    // }

    private void BounceOffGround()
    {
        rb.Rigidbody2D.AddForce(
            new Vector2(Random.Range(-1f, 1f), Random.Range(2f, 3f)) * 2f,
            ForceMode2D.Impulse
        );
    }

    private void StopCoin()
    {
        rb.Rigidbody2D.gravityScale = 0;
        rb.Rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
        rb.Rigidbody2D.linearVelocity = Vector2.zero;
        // Trước mắt test để Stop coin vì lỗi Rơi tiền mà ko trigger với tag player để collection được
        // coinPoolManager.ReturnCoinToPool(GetComponent<NetworkObject>());
    }

    private void BounceOffWall()
    {
        rb.Rigidbody2D.AddForce(
            new Vector2(-rb.Rigidbody2D.linearVelocity.x, rb.Rigidbody2D.linearVelocity.y) * 2f,
            ForceMode2D.Impulse
        );
    }

    public void CollectCoin()
    {
        if (coinsManager != null)
        {
            if (isCoinType1)
            {
                coinsManager.AddCoins(1, 0);
            }
            else if (isCoinType2)
            {
                coinsManager.AddCoins(0, 1);
            }
        }
        else
        {
            Debug.LogError("CoinsManager không tồn tại.");
        }

        coinPoolManager.ReturnCoinToPool(GetComponent<NetworkObject>());
    }

    public void SetCoinType(bool type1, bool type2)
    {
        isCoinType1 = type1;
        isCoinType2 = type2;
    }
}
