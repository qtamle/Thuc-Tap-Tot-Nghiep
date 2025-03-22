using System.Collections;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DaggerCollision : MonoBehaviour
{
    [Header("Explode")]
    public Vector2 radiusDamage;
    public LayerMask damageLayer;

    [Header("Check")]
    public LayerMask checkGroundLayer;
    public LayerMask wallLayer;

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

    private Rigidbody2D rb;

    private IEnemySpawner[] enemySpawners;
    private CoinsManager coinsManager;
    private Transform player;

    private Gold goldIncrease;
    private CoinPoolManager coinPoolManager;
    private ExperienceOrbPoolManager orbPoolManager;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        rb = GetComponent<Rigidbody2D>();

        coinPoolManager = FindFirstObjectByType<CoinPoolManager>();
        orbPoolManager = FindFirstObjectByType<ExperienceOrbPoolManager>();

        coinsManager = UnityEngine.Object.FindFirstObjectByType<CoinsManager>();
        enemySpawners = FindObjectsOfType<MonoBehaviour>().OfType<IEnemySpawner>().ToArray();

        goldIncrease = FindFirstObjectByType<Gold>();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Update()
    {
        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.transform;
                Debug.Log("Player found with tag.");
            }
            else
            {
                int playerLayer = LayerMask.NameToLayer("Player");
                if (playerLayer != -1)
                {
                    GameObject[] objectsInLayer = FindObjectsOfType<GameObject>();
                    foreach (GameObject obj in objectsInLayer)
                    {
                        if (obj.layer == playerLayer)
                        {
                            player = obj.transform;
                            Debug.Log($"Player found using layer: {obj.name}");
                            break;
                        }
                    }
                }

                if (player == null)
                {
                    Debug.LogError(
                        "Player not found! Make sure the Player has the correct tag or layer."
                    );
                }
            }
        }
    }

    public void DaggerDamage()
    {
        Collider2D[] enemyDamage = Physics2D.OverlapBoxAll(
            transform.position,
            radiusDamage,
            0f,
            damageLayer
        );
        foreach (Collider2D enemy in enemyDamage)
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
                    SpawnCoins(
                        secondaryCoinPrefab,
                        secondaryCoinSpawnMin,
                        secondaryCoinSpawnMax,
                        enemy.transform.position
                    );
                }

                SpawnExperienceOrbs(enemy.transform.position, 5);
            }
        }
    }

    void SpawnCoins(GameObject coinType, float minAmount, float maxAmount, Vector3 position)
    {
        bool isGoldIncreaseActive = goldIncrease != null && goldIncrease.IsReady();

        int coinCount = Random.Range((int)minAmount, (int)maxAmount + 1);

        if (isGoldIncreaseActive)
        {
            coinCount += goldIncrease.increaseGoldChange;
        }

        for (int i = 0; i < coinCount; i++)
        {
            Vector3 spawnPosition = position + Vector3.up * 0.2f;

            NetworkObject coin = CoinPoolManager.Instance.GetCoinFromPool(
                spawnPosition,
                coinType == secondaryCoinPrefab
            );
            coin.transform.position = spawnPosition;

            Rigidbody2D coinRb = coin.GetComponent<Rigidbody2D>();

            if (coinRb != null)
            {
                Vector2 forceDirection =
                    new Vector2(Random.Range(-1.5f, 1.5f), Random.Range(1f, 1f)) * 2.5f;
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
            RaycastHit2D hit = Physics2D.Raycast(
                coinRb.transform.position,
                Vector2.down,
                0.5f,
                groundLayer
            );
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

            GameObject orb = orbPoolManager.GetOrbFromPool(spawnPosition);
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

        if (orb != null && player != null)
        {
            Rigidbody2D orbRb = orb.GetComponent<Rigidbody2D>();
            if (orbRb != null)
            {
                while (orb != null && player != null)
                {
                    Vector3 direction = (player.position - orb.transform.position).normalized;

                    orbRb.MovePosition(
                        orb.transform.position + direction * Time.deltaTime * orbMoveToPlayer
                    );

                    if (Vector3.Distance(orb.transform.position, player.position) < 0.5f)
                    {
                        orbPoolManager.ReturnOrbToPool(orb);
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

    IEnumerator AttractCoinToPlayer(GameObject coin, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (coin != null && player != null)
        {
            Rigidbody2D coinRb = coin.GetComponent<Rigidbody2D>();
            if (coinRb != null)
            {
                while (coin != null && player != null)
                {
                    Vector3 direction = (player.position - coin.transform.position).normalized;

                    coinRb.MovePosition(
                        coin.transform.position + direction * Time.deltaTime * orbMoveToPlayer
                    );

                    if (Vector3.Distance(coin.transform.position, player.position) < 0.5f)
                    {
                        Destroy(coin);
                        yield break;
                    }

                    yield return null;
                }
            }
        }
        else
        {
            // Nếu coin hoặc player bị xóa, dừng Coroutine
            yield break;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, radiusDamage);
    }
}
