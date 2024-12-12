using System.Collections;
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
    public LayerMask groundLayer;

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

        Collider2D[] bosses = Physics2D.OverlapCircleAll(attackPoints.position, radius, bossLayer);

        foreach (Collider2D boss in bosses)
        {
            GangsterHealth gangsterHealth = boss.GetComponent<GangsterHealth>();
            if (gangsterHealth != null && gangsterHealth.CanBeDamaged() && !isAttackBoss) 
            {
                gangsterHealth.TakeDamage(1); 
                isAttackBoss = true;

                gangsterHealth.SetCanBeDamaged(false);

                SpawnCoins(coinPrefab, coinSpawnMin * 12, coinSpawnMax * 12, boss.transform.position);  

                if (Random.value <= 0.25f)
                {
                    SpawnCoins(secondaryCoinPrefab, secondaryCoinSpawnMin * 5, secondaryCoinSpawnMax * 5, boss.transform.position);  
                }
            }
        }
        isAttackBoss = false;
    }

    // Hàm tạo đồng tiền tại vị trí của kẻ địch
    void SpawnCoins(GameObject coinType, float minAmount, float maxAmount, Vector3 position)
    {
        int coinCount = Random.Range((int)minAmount, (int)maxAmount + 1);

        for (int i = 0; i < coinCount; i++)
        {
            Vector3 spawnPosition = position + Vector3.up * 0.2f;
            GameObject coin = Instantiate(coinType, spawnPosition, Quaternion.identity);
            Rigidbody2D coinRb = coin.GetComponent<Rigidbody2D>();

            if (coinRb != null)
            {
                Vector2 forceDirection = new Vector2(Random.Range(-1.5f, 1.5f), Random.Range(1f, 1f)) * 2.5f;
                coinRb.AddForce(forceDirection, ForceMode2D.Impulse);

                // Kiểm tra nếu coin bị kẹt
                StartCoroutine(CheckIfCoinIsStuck(coinRb));
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

    private IEnumerator CheckIfCoinIsStuck(Rigidbody2D coinRb)
    {
        yield return new WaitForSeconds(0.1f);

        RaycastHit2D hit = Physics2D.Raycast(coinRb.transform.position, Vector2.down, 0.5f, groundLayer);
        if (hit.collider != null)
        {
            coinRb.transform.position += Vector3.up * 0.3f;
            coinRb.AddForce(Vector2.up * 2f, ForceMode2D.Impulse);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(attackPoints.position, radius);
    }
}
