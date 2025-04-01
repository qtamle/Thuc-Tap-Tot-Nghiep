using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChainsawLevel4 : NetworkBehaviour
{
    [Header("Saw Settings")]
    public float moveSpeed = 10f;
    public int maxBounces = 3;
    public float searchRadius = 15f;
    public LayerMask enemyLayer;

    [Header("Attack Settings")]
    public Vector2 boxSize = new Vector2(2f, 2f);

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

    private IEnemySpawner[] enemySpawners;
    private CoinsManager coinsManager;
    private Transform player;

    private CoinPoolManager coinPoolManager;
    private ExperienceOrbPoolManager orbPoolManager;

    private Transform currentTarget;
    private int bounceCount = 0;
    private HashSet<Transform> hitEnemies = new HashSet<Transform>();

    private void Start()
    {
        coinPoolManager = FindFirstObjectByType<CoinPoolManager>();
        orbPoolManager = FindFirstObjectByType<ExperienceOrbPoolManager>();

        coinsManager = UnityEngine.Object.FindFirstObjectByType<CoinsManager>();

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject == null)
        {
            int playerLayer = LayerMask.NameToLayer("Player");
            if (playerLayer >= 0)
            {
                playerObject = FindObjectsOfType<GameObject>()
                    .FirstOrDefault(obj => obj.layer == playerLayer);
            }
            Debug.Log(
                playerObject != null
                    ? "Player object found by layer."
                    : "Player object not found by layer!"
            );
        }

        if (playerObject != null)
        {
            player = playerObject.transform;
        }
        else
        {
            Debug.LogError("Player object not found in the scene!");
        }

        enemySpawners = FindObjectsOfType<MonoBehaviour>().OfType<IEnemySpawner>().ToArray();

        FindNextTarget();
    }

    private void FixedUpdate()
    {
        if (currentTarget != null)
        {
            MoveToTarget();
        }
        else
        {
            gameObject.GetComponent<NetworkObject>().Despawn(true);

            // Destroy(gameObject);
        }
    }

    private void MoveToTarget()
    {
        transform.position = Vector2.MoveTowards(
            transform.position,
            currentTarget.position,
            moveSpeed * Time.deltaTime
        );

        if (Vector2.Distance(transform.position, currentTarget.position) < 0.1f)
        {
            OnHitTarget();
        }
    }

    private void OnHitTarget()
    {
        hitEnemies.Add(currentTarget);

        CheckForEnemies();

        // Tăng số lần nảy
        bounceCount++;
        if (bounceCount < maxBounces)
        {
            FindNextTarget();
        }
        else
        {
            gameObject.GetComponent<NetworkObject>().Despawn(true);
            // Destroy(gameObject);
        }
    }

    private void FindNextTarget()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(
            transform.position,
            searchRadius,
            enemyLayer
        );

        float shortestDistance = Mathf.Infinity;
        Transform closestEnemy = null;

        foreach (Collider2D enemy in enemies)
        {
            if (!hitEnemies.Contains(enemy.transform)) // Chỉ chọn enemy chưa bị hit
            {
                float distance = Vector2.Distance(transform.position, enemy.transform.position);
                if (distance < shortestDistance)
                {
                    shortestDistance = distance;
                    closestEnemy = enemy.transform;
                }
            }
        }

        currentTarget = closestEnemy;
    }

    public void SetupSaw(Vector2 direction)
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = direction * moveSpeed;
        }
    }

    void CheckForEnemies()
    {
        Collider2D[] enemies = Physics2D.OverlapBoxAll(transform.position, boxSize, 0f, enemyLayer);

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
                NetworkObject networkObject =
                    enemy.gameObject.GetComponentInParent<NetworkObject>();

                AttackEnemyServerRpc(networkObject.NetworkObjectId);
                SpawnCoinsServerRpc(
                    coinPrefab,
                    coinSpawnMin,
                    coinSpawnMax,
                    enemy.transform.position
                );
                if (Random.value <= 0.30f)
                {
                    SpawnCoinsServerRpc(
                        secondaryCoinPrefab,
                        secondaryCoinSpawnMin,
                        secondaryCoinSpawnMax,
                        enemy.transform.position
                    );
                }
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void AttackEnemyServerRpc(ulong enemyNetworkId)
    {
        NetworkObject enemyObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[
            enemyNetworkId
        ];
        if (enemyObject != null)
        {
            // Hủy đối tượng trên server
            enemyObject.Despawn(true);
        }
        else
        {
            gameObject.GetComponent<NetworkObject>().Despawn(true);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnCoinsServerRpc(
        bool isSecondary,
        float minAmount,
        float maxAmount,
        Vector3 position
    )
    {
        Debug.Log($"ServerRpc called - isSecondary: {isSecondary}, position: {position}");
        float initialCoinCount = Random.Range(minAmount, maxAmount + 1);
        float coinCount = initialCoinCount;

        for (int i = 0; i < coinCount; i++)
        {
            Vector3 spawnPosition = position + Vector3.up * 0.2f;
            NetworkObject coin = CoinPoolManager.Instance.GetCoinFromPool(
                spawnPosition,
                isSecondary
            );

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
                coinScript.SetCoinType(!isSecondary, isSecondary);
                // StartCoroutine(HookCoinsContinuously());
            }
        }
    }

    void SpawnCoins(GameObject coinType, float minAmount, float maxAmount, Vector3 position)
    {
        int coinCount = Random.Range((int)minAmount, (int)maxAmount + 1);

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

    [ServerRpc(RequireOwnership = false)]
    private void SpawnOrbsServerRpc(Vector3 position, int orbCount)
    {
        for (int i = 0; i < orbCount; i++)
        {
            float randomAngle = Random.Range(0f, 360f);
            float orbX = position.x + Mathf.Cos(randomAngle * Mathf.Deg2Rad);
            float orbY = position.y + Mathf.Sin(randomAngle * Mathf.Deg2Rad);
            Vector3 spawnPosition = new Vector3(orbX, orbY, position.z);

            NetworkObject orb = orbPoolManager.GetOrbFromPool(spawnPosition);
            if (orb == null)
                continue;

            Rigidbody2D orbRb = orb.GetComponent<Rigidbody2D>();
            if (orbRb != null)
            {
                Vector2 forceDirection = (spawnPosition - position).normalized * orbLaunchForce;
                orbRb.AddForce(forceDirection, ForceMode2D.Impulse);
                orbRb.bodyType = RigidbodyType2D.Kinematic;

                // Gửi RPC đến tất cả clients để bắt đầu coroutine
                StartCoroutine(MoveOrbToPlayer(orb.gameObject, orbMoveDelay));
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
                        orbPoolManager.ReturnOrbToPool(orb.GetComponent<NetworkObject>());
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
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, searchRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, boxSize);
    }
}
