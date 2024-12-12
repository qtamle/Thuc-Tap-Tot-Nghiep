using UnityEngine;

public class CoinsScript : MonoBehaviour
{
    [Header("Layer")]
    public LayerMask groundLayer;
    public LayerMask wallLayer;
    private Rigidbody2D rb;

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
            Destroy(gameObject);
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
        }

        if ((wallLayer.value & (1 << collision.gameObject.layer)) > 0)
        {
            BounceOffWall(); 
        }
    }


    private void BounceOffGround()
    {
        rb.AddForce(new Vector2(Random.Range(-1f, 1f), Random.Range(2f, 3f)) * 2f, ForceMode2D.Impulse);
    }

    private void StopCoin()
    {
        rb.gravityScale = 0;
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;
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
