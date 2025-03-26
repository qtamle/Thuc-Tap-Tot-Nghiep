using System.Collections;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Dagger : NetworkBehaviour
{
    [Header("Settings Attack")]
    public Vector2 boxSize;
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

    [Header("Health Potions")]
    public GameObject healthPotionPrefab;
    public float potionMoveToPlayer = 25f;
    public float potionMoveDelay = 0.5f;

    [Header("Settings Amount")]
    public float coinSpawnMin = 3;
    public float coinSpawnMax = 6;
    public float secondaryCoinSpawnMin = 2;
    public float secondaryCoinSpawnMax = 4;

    [Header("Attack Cooldown")]
    public float attackCooldown = 1f;
    private float lastAttackTime = -Mathf.Infinity;

    [Header("Swipe Android")]
    private Vector3 lastMousePosition;
    public float swipeThreshold = 50f;

    [Header("VFX Setting")]
    public VFXPooling vfxPool;

    [Header("Upgrade Level 2")]
    public int increaseCoin;

    [Header("Upgrade Level 3")]
    public int increaseExperience;

    [Header("Upgrade Level 4")]
    public float increaseRatePotion;

    private CoinsManager coinsManager;
    private Transform player;
    private IEnemySpawner[] enemySpawners;

    private Gold goldIncrease;
    private Brutal brutal;

    private CoinPoolManager coinPoolManager;
    private ExperienceOrbPoolManager orbPoolManager;
    private PlayerHealth health;
    private Lucky lucky;

    private WeaponInfo weaponInfo;

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
        // coin and pool manager
        coinPoolManager = FindFirstObjectByType<CoinPoolManager>();
        orbPoolManager = FindFirstObjectByType<ExperienceOrbPoolManager>();
        coinsManager = UnityEngine.Object.FindFirstObjectByType<CoinsManager>();

        // find player
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        health = GetComponent<PlayerHealth>();

        if (playerObject == null)
        {
            int playerLayer = LayerMask.NameToLayer("Player");
            if (playerLayer >= 0)
            {
                playerObject = FindObjectsOfType<GameObject>()
                    .FirstOrDefault(obj => obj.layer == playerLayer);
            }
        }

        player = playerObject.transform;

        // enemy spawn manager
        enemySpawners = FindObjectsOfType<MonoBehaviour>().OfType<IEnemySpawner>().ToArray();

        // supply manager
        goldIncrease = FindFirstObjectByType<Gold>();
        brutal = FindFirstObjectByType<Brutal>();
        lucky = FindFirstObjectByType<Lucky>();

        // weapon and upgrade manager
        weaponInfo = FindFirstObjectByType<WeaponInfo>();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Update()
    {
        if (!IsOwner)
            return;
        if (IsInputDetected() && CanAttack())
        {
            PerformAttack();
            lastAttackTime = Time.time;
        }
    }

    private bool IsInputDetected()
    {
        if (Application.isEditor)
        {
            // Kiểm tra nhấn chuột
            if (Input.GetMouseButtonDown(0))
            {
                return true;
            }
        }
        else
        {
            // Kiểm tra chạm trên màn hình cảm ứng
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);

                if (touch.phase == TouchPhase.Began)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private bool CanAttack()
    {
        return Time.time >= lastAttackTime + attackCooldown;
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
    }

    [ServerRpc(RequireOwnership = false)]
    private void NotifyEnemyKilledServerRpc()
    {
        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.OnEnemyKilled();
        }
    }

    private void PerformAttack()
    {
        ShowAttackVFX();

        Collider2D[] enemies = Physics2D.OverlapBoxAll(
            attackPoints.position,
            boxSize,
            0f,
            enemyLayer
        );

        foreach (Collider2D enemy in enemies)
        {
            if (enemy != null && enemy.gameObject != null && enemy.gameObject.activeInHierarchy)
            {
                GameObject enemyObject = enemy.transform.root.gameObject;
                NotifyEnemyKilledServerRpc();

                // foreach (IEnemySpawner spawner in enemySpawners)
                // {
                //     spawner.OnEnemyKilled();
                // }

                if (brutal != null)
                {
                    health.HealHealth(1);
                }
                // Kiểm tra và hủy enemy
                NetworkObject networkObject = enemyObject.GetComponent<NetworkObject>();
                if (networkObject != null)
                {
                    // Gọi ServerRpc để hủy đối tượng
                    AttackEnemyServerRpc(networkObject.NetworkObjectId);
                }
                else
                {
                    Debug.LogWarning("Enemy does not have a NetworkObject component!");
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

                if (ShouldSpawnPotion())
                {
                    SpawnHealthPotions(enemy.transform.position, 1);
                }

                SpawnOrbsServerRpc(enemy.transform.position, 5);
            }
        }

        Collider2D[] bosses = Physics2D.OverlapBoxAll(
            attackPoints.position,
            boxSize,
            0f,
            bossLayer
        );

        foreach (Collider2D boss in bosses)
        {
            DamageInterface damageable = boss.GetComponent<DamageInterface>();

            if (damageable != null && damageable.CanBeDamaged() && !isAttackBoss)
            {
                damageable.TakeDamage(1);
                isAttackBoss = true;
                damageable.SetCanBeDamaged(false);

                SpawnCoins(
                    coinPrefab,
                    coinSpawnMin * 10,
                    coinSpawnMax * 10,
                    boss.transform.position
                );

                if (Random.value <= 0.25f)
                {
                    SpawnCoins(
                        secondaryCoinPrefab,
                        secondaryCoinSpawnMin * 5,
                        secondaryCoinSpawnMax * 5,
                        boss.transform.position
                    );
                }

                if (ShouldSpawnPotion())
                {
                    SpawnHealthPotions(boss.transform.position, 1);
                }

                SpawnOrbsServerRpc(boss.transform.position, 20);
            }
        }

        isAttackBoss = false;

        Collider2D[] Snake = Physics2D.OverlapBoxAll(attackPoints.position, boxSize, 0f, bossLayer);

        foreach (Collider2D sn in Snake)
        {
            MachineSnakeHealth partHealth = sn.GetComponent<MachineSnakeHealth>();
            SnakeHealth snakeHealth = sn.GetComponentInParent<SnakeHealth>();

            if (partHealth != null && !isAttackBoss && snakeHealth.IsStunned())
            {
                if (
                    (
                        MachineSnakeHealth.attackedPartID == -1
                        || MachineSnakeHealth.attackedPartID == partHealth.partID
                    ) && !partHealth.isAlreadyHit
                )
                {
                    partHealth.TakeDamage(1);
                    isAttackBoss = true;

                    snakeHealth.SetCanBeDamaged(false);

                    SpawnCoins(
                        coinPrefab,
                        coinSpawnMin * 13,
                        coinSpawnMax * 13,
                        partHealth.transform.position
                    );

                    if (Random.value <= 0.25f)
                    {
                        SpawnCoins(
                            secondaryCoinPrefab,
                            secondaryCoinSpawnMin * 5,
                            secondaryCoinSpawnMax * 5,
                            partHealth.transform.position
                        );
                    }

                    if (ShouldSpawnPotion())
                    {
                        SpawnHealthPotions(partHealth.transform.position, 1);
                    }

                    SpawnOrbsServerRpc(partHealth.transform.position, 25);
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
                    SpawnCoins(
                        coinPrefab,
                        coinSpawnMin * 13,
                        coinSpawnMax * 13,
                        headController.transform.position
                    );

                    if (Random.value <= 0.25f)
                    {
                        SpawnCoins(
                            secondaryCoinPrefab,
                            secondaryCoinSpawnMin * 5,
                            secondaryCoinSpawnMax * 5,
                            headController.transform.position
                        );
                    }

                    if (ShouldSpawnPotion())
                    {
                        SpawnHealthPotions(headController.transform.position, 1);
                    }

                    SpawnOrbsServerRpc(headController.transform.position, 25);
                }
            }
            else
            {
                Debug.LogWarning($"SnakeHealth is null for {sn.gameObject.name}");
            }
        }
        isAttackBoss = false;
    }

    private void ShowAttackVFX()
    {
        if (vfxPool != null)
        {
            GameObject vfx = vfxPool.Get();

            vfx.transform.position = attackPoints.position;
            vfx.transform.rotation = Quaternion.identity;

            VFXFollower follower = vfx.GetComponent<VFXFollower>();
            if (follower != null)
            {
                follower.SetTarget(attackPoints, player);
            }

            StartCoroutine(ReturnVFXToPool(vfx, 0.5f));
        }
    }

    private IEnumerator ReturnVFXToPool(GameObject vfx, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (vfxPool != null)
        {
            vfxPool.ReturnToPool(vfx);
        }
    }

    void SpawnCoins(GameObject coinType, float minAmount, float maxAmount, Vector3 position)
    {
        string coinPrefabName = coinType == coinPrefab ? "Coin" : "SecondaryCoin";
        // SpawnCoinsServerRpc(coinPrefabName, minAmount, maxAmount, position);
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

    void SpawnHealthPotions(Vector3 position, int potionCount)
    {
        for (int i = 0; i < potionCount; i++)
        {
            float randomAngle = Random.Range(0f, 360f);

            float potionX = position.x + Mathf.Cos(randomAngle * Mathf.Deg2Rad);
            float potionY = position.y + Mathf.Sin(randomAngle * Mathf.Deg2Rad);
            Vector3 spawnPosition = new Vector3(potionX, potionY, position.z);

            GameObject potion = Instantiate(healthPotionPrefab, spawnPosition, Quaternion.identity);
            if (potion.GetComponent<NetworkObject>() == null)
            {
                potion.AddComponent<NetworkObject>();
            }
            potion.GetComponent<NetworkObject>().Spawn(true);
            Rigidbody2D potionRb = potion.GetComponent<Rigidbody2D>();
            if (potionRb != null)
            {
                Vector2 randomForce =
                    new Vector2(Random.Range(-2f, 2f), Random.Range(1f, 1f)) * 2.5f;
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
                    potionRb.MovePosition(
                        potion.transform.position + direction * Time.deltaTime * potionMoveToPlayer
                    );

                    if (Vector3.Distance(potion.transform.position, player.position) < 0.5f)
                    {
                        Destroy(potion);
                        potion.GetComponent<NetworkObject>().Despawn();
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

    private bool ShouldSpawnPotion()
    {
        float baseRate = 0f; // Ban đầu tỉ lệ spawn là 0%

        if (weaponInfo != null && weaponInfo.weaponLevel > 3)
        {
            baseRate += 0.10f; // Tăng thêm 10% nếu weaponLevel > 3
        }

        if (lucky != null)
        {
            baseRate += 0.15f; // Tăng thêm 15% nếu có lucky
        }

        // trả về % từ 2 giá trị trên (nếu không có là 0%, weapon level > 3 thì 10%, có supply lucky thì 15%, cả 2 thì dồn 25%)
        return Random.value <= baseRate;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;

        Gizmos.DrawWireCube(attackPoints.position, boxSize);
    }
}
