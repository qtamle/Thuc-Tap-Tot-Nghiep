using UnityEngine;

public class CoinsScript : MonoBehaviour
{
    public LayerMask groundLayer;
    public LayerMask wallLayer;
    private Rigidbody2D rb;

    [Header("Coin Type")]
    public bool isCoinType1;
    public bool isCoinType2;

    private CoinsManager coinsManager;

    public string playerLayerName = "Player";

    private void OnEnable()
    {
        int playerLayer = LayerMask.NameToLayer(playerLayerName);
        int coinLayer = gameObject.layer; 

        if (playerLayer >= 0 && coinLayer >= 0)
        {
            Physics2D.IgnoreLayerCollision(coinLayer, playerLayer, true);
        }
    }

    private void OnDisable()
    {
        // Khôi phục lại va chạm nếu cần
        int playerLayer = LayerMask.NameToLayer(playerLayerName);
        int coinLayer = gameObject.layer;

        if (playerLayer >= 0 && coinLayer >= 0)
        {
            Physics2D.IgnoreLayerCollision(coinLayer, playerLayer, false);
        }
    }

    private void Start()
    {
        coinsManager = UnityEngine.Object.FindFirstObjectByType<CoinsManager>();

        rb = GetComponent<Rigidbody2D>();

        if (coinsManager == null)
        {
            Debug.LogError("CoinsManager không tìm thấy!");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Kiểm tra va chạm với nền
        if ((groundLayer.value & (1 << collision.gameObject.layer)) > 0)
        {
            rb.gravityScale = 0;
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero;
        }

        // Kiểm tra nếu va chạm với Player
        if (collision.CompareTag("Player"))
        {
            CollectCoin();
        }

        if ((wallLayer.value & (1 << collision.gameObject.layer)) > 0)
        {
            BounceOffWall();
        }
    }

    private void BounceOffWall()
    {
        rb.AddForce(new Vector2(-rb.linearVelocity.x, rb.linearVelocity.y) * 2f, ForceMode2D.Impulse);
    }

    // Hàm thu thập coin khi Player va chạm với coin
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

        Destroy(gameObject);
    }

    public void SetCoinType(bool type1, bool type2)
    {
        isCoinType1 = type1;
        isCoinType2 = type2;
    }
}
