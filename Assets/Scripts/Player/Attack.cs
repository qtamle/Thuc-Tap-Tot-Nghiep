using UnityEngine;

public class Attack : MonoBehaviour
{
    [Header("Settings Attack")]
    public float radius;
    public LayerMask enemyLayer;
    public LayerMask bossLayer;
    public Transform attackPoints;
    private bool isAttackBoss = false;

    [Header("Coins")]
    public GameObject coinPrefab;
    public GameObject secondaryCoinPrefab;

    [Header("Settings Amount")]
    public float coinSpawnMin = 3;
    public float coinSpawnMax = 6;
    public float secondaryCoinSpawnMin = 2;
    public float secondaryCoinSpawnMax = 4;

    private CoinsManager coinsManager;

    private void Start()
    {
        coinsManager = UnityEngine.Object.FindFirstObjectByType<CoinsManager>();
    }

    private void Update()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(attackPoints.position, radius, enemyLayer);

        foreach (Collider2D enemy in enemies)
        {
            Destroy(enemy.gameObject); 

            SpawnCoins(coinPrefab, coinSpawnMin, coinSpawnMax, enemy.transform.position);

            // 30% xác suất rơi đồng tiền thứ hai
            if (Random.value <= 0.30f)
            {
                SpawnCoins(secondaryCoinPrefab, secondaryCoinSpawnMin, secondaryCoinSpawnMax, enemy.transform.position);
            }

            // Thông báo số kẻ địch đã bị tiêu diệt
            if (EnemyManager.Instance != null)
            {
                EnemyManager.Instance.OnEnemyKilled();
            }
        }

        Collider2D[] boss = Physics2D.OverlapCircleAll(attackPoints.position, radius, bossLayer);

        foreach (Collider2D b in boss)
        {
            DamageInterface damageInterface = b.GetComponent<DamageInterface>();
            if (damageInterface != null && !isAttackBoss)
            {
                damageInterface.TakeDamage(1);
                isAttackBoss = true;
            }
        }

    }

    // Hàm tạo đồng tiền tại vị trí của kẻ địch
    void SpawnCoins(GameObject coinType, float minAmount, float maxAmount, Vector3 position)
    {
        int coinCount = Random.Range((int)minAmount, (int)maxAmount + 1);

        for (int i = 0; i < coinCount; i++)
        {
            GameObject coin = Instantiate(coinType, position, Quaternion.identity);
            Rigidbody2D coinRb = coin.GetComponent<Rigidbody2D>();
            if (coinRb != null)
            {
                coinRb.AddForce(new Vector2(Random.Range(-1.5f, 1.5f), Random.Range(1f, 3f)) * 2.2f, ForceMode2D.Impulse);
            }

            // Thiết lập loại đồng tiền
            CoinsScript coinScript = coin.GetComponent<CoinsScript>();
            if (coinScript != null)
            {
                if (coinType == coinPrefab)
                    coinScript.SetCoinType(true, false); 
                else
                    coinScript.SetCoinType(false, true); 
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(attackPoints.position, radius);
    }
}
