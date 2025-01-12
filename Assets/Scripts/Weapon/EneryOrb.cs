using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EneryOrb : MonoBehaviour
{
    [Header("Settings Attack")]
    public LayerMask enemyLayer;
    public LayerMask bossLayer;
    private bool isAttackBoss = false;
    public Transform playerSpawn;

    [Header("Orb")]
    public GameObject attackOrbPrefab;
    public float orbitRadius = 3f;
    public float orbitSpeed = 90f; // Tốc độ quay của quỹ đạo
    public float attackRadius = 5f;

    [Header("Coins")]
    public GameObject coinPrefab;
    public GameObject secondaryCoinPrefab;
    public LayerMask groundLayer;

    [Header("Experience Orbs")]
    public GameObject experienceOrbPrefab;
    public float orbLaunchForce = 5f;
    public float orbMoveToPlayer = 15f;
    public float orbMoveDelay = 2f;

    [Header("Health Potions")]
    public GameObject healthPotionPrefab;
    public float potionMoveToPlayer = 25f;
    public float potionMoveDelay = 0.5f;

    [Header("Settings Amount")]
    public float coinSpawnMin = 3;
    public float coinSpawnMax = 6;
    public float secondaryCoinSpawnMin = 2;
    public float secondaryCoinSpawnMax = 4;

    private CoinsManager coinsManager;
    private Transform player;
    private IEnemySpawner[] enemySpawners;

    private GameObject[] attackOrbs = new GameObject[3]; // Mảng chứa các quả cầu
    private Vector3[] orbitPositions = new Vector3[3]; // Các vị trí quỹ đạo của quả cầu

    private Gold goldIncrease;
    private Brutal brutal;

    private CoinPoolManager coinPoolManager;
    private ExperienceOrbPoolManager orbPoolManager;
    private PlayerHealth health;
    private Lucky lucky;

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
        OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        coinPoolManager = FindFirstObjectByType<CoinPoolManager>();
        orbPoolManager = FindFirstObjectByType<ExperienceOrbPoolManager>();

        if (coinPoolManager == null || orbPoolManager == null)
        {
            Debug.LogError("PoolManager components are missing!");
            return;
        }

        coinsManager = UnityEngine.Object.FindFirstObjectByType<CoinsManager>();

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject == null)
        {
            Debug.LogError("Player GameObject is not found!");
            return;
        }

        health = playerObject.GetComponent<PlayerHealth>();
        if (health == null)
        {
            Debug.LogError("PlayerHealth component is missing on Player!");
            return;
        }

        player = playerObject.transform;
        if (playerSpawn == null)
        {
            Debug.LogError("PlayerSpawn transform is not assigned in the Inspector!");
            return;
        }

        for (int i = 0; i < attackOrbs.Length; i++)
        {
            if (attackOrbPrefab == null)
            {
                Debug.LogError("Attack Orb Prefab is not assigned!");
                continue;
            }

            attackOrbs[i] = Instantiate(attackOrbPrefab, playerSpawn.position, Quaternion.identity);
            attackOrbs[i].transform.parent = playerSpawn;
        }

        enemySpawners = FindObjectsOfType<MonoBehaviour>().OfType<IEnemySpawner>().ToArray();

        goldIncrease = FindFirstObjectByType<Gold>();
        brutal = FindFirstObjectByType<Brutal>();
        lucky = FindFirstObjectByType<Lucky>();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void FixedUpdate()
    {
        for (int i = 0; i < attackOrbs.Length; i++)
        {
            float angle = (i * 120f) + (Time.time * orbitSpeed);

            Vector2 orbitPositionXY = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad) * orbitRadius, Mathf.Sin(angle * Mathf.Deg2Rad) * orbitRadius);
            Vector2 newPositionXY = new Vector2(playerSpawn.position.x, playerSpawn.position.y) + orbitPositionXY;
            Vector3 newPosition = new Vector3(newPositionXY.x, newPositionXY.y, playerSpawn.position.z);

            attackOrbs[i].transform.position = newPosition;
            attackOrbs[i].transform.SetParent(null); 
        }
    }

    private void Update()
    {
        AttackWithOrbs();
    }

    private void AttackWithOrbs()
    {
        foreach (GameObject orb in attackOrbs)
        {
            if (orb == null) continue;

            Collider2D[] enemies = Physics2D.OverlapCircleAll(orb.transform.position, attackRadius, enemyLayer);

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

                    if (brutal != null)
                    {
                        health.HealHealth(1);
                    }

                    Destroy(enemy.gameObject);
                    SpawnCoins(coinPrefab, coinSpawnMin, coinSpawnMax, enemy.transform.position);

                    if (Random.value <= 0.30f)
                    {
                        SpawnCoins(secondaryCoinPrefab, secondaryCoinSpawnMin, secondaryCoinSpawnMax, enemy.transform.position);
                    }

                    if (Random.value <= 0.15f && lucky != null)
                    {
                        SpawnHealthPotions(enemy.transform.position, 1);
                    }

                    SpawnExperienceOrbs(enemy.transform.position, 5);
                }
            }

            Collider2D[] bosses = Physics2D.OverlapCircleAll(orb.transform.position, attackRadius, bossLayer);

            foreach (Collider2D boss in bosses)
            {
                DamageInterface damageable = boss.GetComponent<DamageInterface>();

                if (damageable != null && damageable.CanBeDamaged() && !isAttackBoss)
                {
                    damageable.TakeDamage(1);
                    isAttackBoss = true;
                    damageable.SetCanBeDamaged(false);

                    SpawnCoins(coinPrefab, coinSpawnMin * 10, coinSpawnMax * 10, boss.transform.position);

                    if (Random.value <= 0.25f)
                    {
                        SpawnCoins(secondaryCoinPrefab, secondaryCoinSpawnMin * 5, secondaryCoinSpawnMax * 5, boss.transform.position);
                    }

                    if (Random.value <= 0.15f && lucky != null)
                    {
                        SpawnHealthPotions(boss.transform.position, 1);
                    }

                    SpawnExperienceOrbs(boss.transform.position, 20);
                }
            }

            isAttackBoss = false;

            Collider2D[] Snake = Physics2D.OverlapCircleAll(orb.transform.position, attackRadius, bossLayer);

            foreach (Collider2D sn in Snake)
            {
                MachineSnakeHealth partHealth = sn.GetComponent<MachineSnakeHealth>();
                SnakeHealth snakeHealth = sn.GetComponentInParent<SnakeHealth>();

                if (partHealth != null && !isAttackBoss && snakeHealth.IsStunned())
                {
                    if ((MachineSnakeHealth.attackedPartID == -1 || MachineSnakeHealth.attackedPartID == partHealth.partID) && !partHealth.isAlreadyHit)
                    {
                        partHealth.TakeDamage(1);
                        isAttackBoss = true;

                        snakeHealth.SetCanBeDamaged(false);

                        SpawnCoins(coinPrefab, coinSpawnMin * 13, coinSpawnMax * 13, partHealth.transform.position);

                        if (Random.value <= 0.25f)
                        {
                            SpawnCoins(secondaryCoinPrefab, secondaryCoinSpawnMin * 5, secondaryCoinSpawnMax * 5, partHealth.transform.position);
                        }

                        if (Random.value <= 0.15f && lucky != null)
                        {
                            SpawnHealthPotions(partHealth.transform.position, 1);
                        }

                        SpawnExperienceOrbs(partHealth.transform.position, 25);
                    }
                }

                if (snakeHealth != null && snakeHealth.bodyPartsAttacked == snakeHealth.totalBodyParts)
                {
                    HeadController headController = sn.GetComponentInChildren<HeadController>();
                    if (headController != null && !headController.isHeadAttacked && !isAttackBoss)
                    {
                        headController.TakeDamage(1);
                        isAttackBoss = true;
                        headController.isHeadAttacked = true;

                        snakeHealth.SetCanBeDamaged(false);
                        SpawnCoins(coinPrefab, coinSpawnMin * 13, coinSpawnMax * 13, headController.transform.position);

                        if (Random.value <= 0.25f)
                        {
                            SpawnCoins(secondaryCoinPrefab, secondaryCoinSpawnMin * 5, secondaryCoinSpawnMax * 5, headController.transform.position);
                        }

                        if (Random.value <= 0.15f && lucky != null)
                        {
                            SpawnHealthPotions(headController.transform.position, 1);
                        }

                        SpawnExperienceOrbs(headController.transform.position, 25);
                    }
                }
                else
                {
                    Debug.LogWarning($"SnakeHealth is null for {sn.gameObject.name}");
                }
            }
            isAttackBoss = false;
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

            GameObject coin = coinPoolManager.GetCoinFromPool(coinType);
            coin.transform.position = spawnPosition;

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

            if (hit.collider != null && hit.collider != coinRb.GetComponent<Collider2D>())
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

                    orbRb.MovePosition(orb.transform.position + direction * Time.deltaTime * orbMoveToPlayer);

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

    void SpawnHealthPotions(Vector3 position, int potionCount)
    {
        for (int i = 0; i < potionCount; i++)
        {
            float randomAngle = Random.Range(0f, 360f);

            float potionX = position.x + Mathf.Cos(randomAngle * Mathf.Deg2Rad);
            float potionY = position.y + Mathf.Sin(randomAngle * Mathf.Deg2Rad);
            Vector3 spawnPosition = new Vector3(potionX, potionY, position.z);

            GameObject potion = Instantiate(healthPotionPrefab, spawnPosition, Quaternion.identity);

            Rigidbody2D potionRb = potion.GetComponent<Rigidbody2D>();
            if (potionRb != null)
            {
                Vector2 randomForce = new Vector2(Random.Range(-2f, 2f), Random.Range(1f, 1f)) * 2.5f;
                potionRb.AddForce(randomForce, ForceMode2D.Impulse);

                potionRb.bodyType = RigidbodyType2D.Kinematic;

                StartCoroutine(MovePotionToPlayer(potion, potionMoveDelay));
            }
        }
    }

    IEnumerator MovePotionToPlayer(GameObject potion, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (potion != null && player != null)
        {
            Rigidbody2D potionRb = potion.GetComponent<Rigidbody2D>();
            if (potionRb != null)
            {
                while (potion != null && player != null)
                {
                    Vector3 direction = (player.position - potion.transform.position).normalized;
                    potionRb.MovePosition(potion.transform.position + direction * Time.deltaTime * potionMoveToPlayer);

                    if (Vector3.Distance(potion.transform.position, player.position) < 0.5f)
                    {
                        Destroy(potion);
                        health.HealHealth(3);
                        yield break;
                    }

                    yield return null;
                }
            }
        }
        else
        {
            yield break;
        }
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(playerSpawn.position, orbitRadius);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(playerSpawn.position, attackRadius);
    }
}
