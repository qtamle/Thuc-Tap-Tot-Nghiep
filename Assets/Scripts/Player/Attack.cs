using System.Collections;
using System.Linq;
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

    [Header("Experience Orbs")]
    public GameObject experienceOrbPrefab;
    public float orbLaunchForce = 5f;
    public float orbMoveToPlayer = 15f;
    public float orbMoveDelay = 2f;

    [Header("Settings Amount")]
    public float coinSpawnMin = 3;
    public float coinSpawnMax = 6;
    public float secondaryCoinSpawnMin = 2;
    public float secondaryCoinSpawnMax = 4;

    private CoinsManager coinsManager;
    private Transform player;
    private IEnemySpawner[] enemySpawners;

    private void Start()
    {
        coinsManager = UnityEngine.Object.FindFirstObjectByType<CoinsManager>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        enemySpawners = FindObjectsOfType<MonoBehaviour>().OfType<IEnemySpawner>().ToArray(); 
    }

    private void Update()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(attackPoints.position, radius, enemyLayer);

        foreach (Collider2D enemy in enemies)
        {
            if (enemy != null && enemy.gameObject != null && enemy.gameObject.activeInHierarchy)
            {
                if (EnemyManager.Instance != null)
                {
                    EnemyManager.Instance.OnEnemyKilled();
                }

                foreach (IEnemySpawner spawner in enemySpawners)
                {
                    spawner.OnEnemyKilled();
                }

                Destroy(enemy.gameObject);
                SpawnCoins(coinPrefab, coinSpawnMin, coinSpawnMax, enemy.transform.position);

                if (Random.value <= 0.30f)
                {
                    SpawnCoins(secondaryCoinPrefab, secondaryCoinSpawnMin, secondaryCoinSpawnMax, enemy.transform.position);
                }

                SpawnExperienceOrbs(enemy.transform.position, 5);
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

                SpawnExperienceOrbs(boss.transform.position, 20);
            }
        }
        isAttackBoss = false;


        Collider2D[] assassin = Physics2D.OverlapCircleAll(attackPoints.position, radius, bossLayer);

        foreach (Collider2D asssass in assassin)
        {
            AssassinHealth Assassin = asssass.GetComponent<AssassinHealth>();
            if (Assassin != null && Assassin.CanBeDamaged() && !isAttackBoss)
            {
                Assassin.TakeDamage(1);
                isAttackBoss = true;

                Assassin.SetCanBeDamaged(false);

                SpawnCoins(coinPrefab, coinSpawnMin * 13, coinSpawnMax * 13, asssass.transform.position);

                if (Random.value <= 0.25f)
                {
                    SpawnCoins(secondaryCoinPrefab, secondaryCoinSpawnMin * 5, secondaryCoinSpawnMax * 5, asssass.transform.position);
                }

                SpawnExperienceOrbs(asssass.transform.position, 25);
            }
        }
        isAttackBoss = false;

        Collider2D[] cyborg = Physics2D.OverlapCircleAll(attackPoints.position, radius, bossLayer);

        foreach (Collider2D cy in cyborg)
        {
            CyborgHealth cyborgs = cy.GetComponent<CyborgHealth>();
            if (cyborgs != null && cyborgs.CanBeDamaged() && !isAttackBoss)
            {
                cyborgs.TakeDamage(1);
                isAttackBoss = true;

                cyborgs.SetCanBeDamaged(false);

                SpawnCoins(coinPrefab, coinSpawnMin * 13, coinSpawnMax * 13, cyborgs.transform.position);

                if (Random.value <= 0.25f)
                {
                    SpawnCoins(secondaryCoinPrefab, secondaryCoinSpawnMin * 5, secondaryCoinSpawnMax * 5, cyborgs.transform.position);
                }

                SpawnExperienceOrbs(cyborgs.transform.position, 25);
            }
        }
        isAttackBoss = false;
    }

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

                StartCoroutine(CheckIfCoinIsStuck(coinRb));
            }

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

        if (coinRb != null) 
        {
            RaycastHit2D hit = Physics2D.Raycast(coinRb.transform.position, Vector2.down, 0.5f, groundLayer);
            if (hit.collider != null)
            {
                coinRb.transform.position += Vector3.up * 0.3f;
                coinRb.AddForce(Vector2.up * 2f, ForceMode2D.Impulse);
            }
        }
        else
        {
            yield break;
        }
    }

    void SpawnExperienceOrbs(Vector3 position, int orbCount)
    {
        for (int i = 0; i < orbCount; i++)
        {
            float randomAngle = Random.Range(0f, 360f);

            float orbX = position.x + Mathf.Cos(randomAngle * Mathf.Deg2Rad);
            float orbY = position.y + Mathf.Sin(randomAngle * Mathf.Deg2Rad);
            Vector3 spawnPosition = new Vector3(orbX, orbY, position.z);

            GameObject orb = Instantiate(experienceOrbPrefab, spawnPosition, Quaternion.identity);
            Rigidbody2D orbRb = orb.GetComponent<Rigidbody2D>();

            if (orbRb != null)
            {
                Vector2 forceDirection = (spawnPosition - position).normalized * orbLaunchForce;
                orbRb.AddForce(forceDirection, ForceMode2D.Impulse);

                orbRb.bodyType = RigidbodyType2D.Kinematic;

                StartCoroutine(MoveOrbToPlayer(orb, orbMoveDelay));
            }
        }
    }

    IEnumerator MoveOrbToPlayer(GameObject orb, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (orb != null && player != null) // Kiểm tra nếu đối tượng orb vẫn tồn tại
        {
            Rigidbody2D orbRb = orb.GetComponent<Rigidbody2D>();
            if (orbRb != null)
            {
                while (orb != null && player != null)
                {
                    Vector3 direction = (player.position - orb.transform.position).normalized;

                    orbRb.MovePosition(orb.transform.position + direction * Time.deltaTime * orbMoveToPlayer);

                    if (Vector3.Distance(orb.transform.position, player.position) < 0.5f)
                    {
                        Destroy(orb);
                        yield break;
                    }

                    yield return null;
                }
            }
        }
        else
        {
            // Nếu orb hoặc player bị xóa, dừng Coroutine
            yield break;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(attackPoints.position, radius);
    }
}
