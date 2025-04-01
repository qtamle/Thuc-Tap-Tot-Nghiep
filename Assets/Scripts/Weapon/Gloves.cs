using System.Collections;
using System.Linq;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Gloves : NetworkBehaviour
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

    private CoinsManager coinsManager;
    private Transform player;
    private IEnemySpawner[] enemySpawners;

    private Gold goldIncrease;
    private Brutal brutal;

    private CoinPoolManager coinPoolManager;
    private ExperienceOrbPoolManager orbPoolManager;
    private PlayerHealth health;
    private Lucky lucky;

    private WeaponPlayerInfo weaponInfo;

    public GameObject lightingLevel4;

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
        if (IsServer)
        {
            // NetworkObject myPlayerObject = NetworkManager
            //     .Singleton
            //     .ConnectedClients[OwnerClientId]
            //     .PlayerObject;

            // WeaponPlayerInfo weaponPlayerInfo = FindAnyObjectByType<WeaponPlayerInfo>(); // Hoặc dùng cách khác để tìm đúng object
            // // Đưa WeaponPlayerInfo thành con của PlayerObject
            // weaponPlayerInfo.transform.SetParent(myPlayerObject.transform);
            // weaponInfo = GetComponentInChildren<WeaponPlayerInfo>();
            ulong myClientId = NetworkManager.Singleton.LocalClientId;
            FindPlayerServerRpc(myClientId);
            weaponInfo = GetComponentInChildren<WeaponPlayerInfo>();
        }
        else
        {
            ulong myClientId = NetworkManager.Singleton.LocalClientId;

            FindPlayerServerRpc(myClientId);
        }
        OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    [ServerRpc(RequireOwnership = false)]
    private void FindPlayerServerRpc(ulong clientId) // Thêm clientId làm tham số
    {
        // Kiểm tra nếu client tồn tại
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client))
        {
            NetworkObject playerObject = client.PlayerObject;

            // Tìm tất cả WeaponPlayerInfo và chọn cái có OwnerClientId trùng khớp
            WeaponPlayerInfo[] allWeapons = FindObjectsByType<WeaponPlayerInfo>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );
            WeaponPlayerInfo targetWeapon = allWeapons.FirstOrDefault(w =>
                w.NetworkObject.OwnerClientId == clientId
            );

            if (targetWeapon != null)
            {
                targetWeapon.transform.SetParent(playerObject.transform);
                targetWeapon.transform.localPosition = Vector3.zero; // Đặt vị trí phù hợp
            }
            else
            {
                Debug.LogError($"Không tìm thấy WeaponPlayerInfo cho client {clientId}");
            }
            weaponInfo = GetComponentInChildren<WeaponPlayerInfo>();
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        coinPoolManager = FindFirstObjectByType<CoinPoolManager>();
        orbPoolManager = FindFirstObjectByType<ExperienceOrbPoolManager>();

        coinsManager = UnityEngine.Object.FindFirstObjectByType<CoinsManager>();

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

        enemySpawners = FindObjectsOfType<MonoBehaviour>().OfType<IEnemySpawner>().ToArray();

        goldIncrease = FindFirstObjectByType<Gold>();
        brutal = FindFirstObjectByType<Brutal>();
        lucky = FindFirstObjectByType<Lucky>();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Update()
    {
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

    private IEnumerator DespawnAfterDelay(GameObject laserObject, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (laserObject != null && laserObject.TryGetComponent(out NetworkObject networkObject))
        {
            networkObject.Despawn(true);
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

                // Kiểm tra và hủy enemy
                NetworkObject networkObject = enemy.GetComponentInParent<NetworkObject>();
                if (networkObject == null)
                {
                    networkObject = enemy.GetComponent<NetworkObject>();
                }
                if (networkObject != null)
                {
                    // Gọi ServerRpc để hủy đối tượng
                    AttackEnemyServerRpc(networkObject.NetworkObjectId);
                    SpawnCoinsServerRpc(
                        false,
                        coinSpawnMin,
                        coinSpawnMax,
                        enemy.transform.position
                    );

                    if (Random.value <= 0.30f)
                    {
                        SpawnCoinsServerRpc(
                            true,
                            secondaryCoinSpawnMin,
                            secondaryCoinSpawnMax,
                            enemy.transform.position
                        );
                    }

                    if (Random.value <= 0.15f && lucky != null)
                    {
                        SpawnHealthPotions(enemy.transform.position, 1);
                    }
                    if (Random.value <= 1f && weaponInfo.weaponLevel > 3)
                    {
                        GameObject lighting = Instantiate(
                            lightingLevel4,
                            transform.position,
                            Quaternion.identity
                        );
                        lighting.GetComponent<NetworkObject>().Spawn(true);

                        if (lighting != null)
                        {
                            lighting.GetComponent<LineRenderer>().enabled = true;
                            LightningChain light = lighting.GetComponent<LightningChain>();
                            if (light != null)
                            {
                                light.TriggerLightning();
                            }
                        }

                        StartCoroutine(DespawnAfterDelay(lighting, 5f));
                        // Destroy(lighting, 5f);
                    }
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
            NetworkObject bossNetworkObject = boss.GetComponent<NetworkObject>();
            if (damageable != null && damageable.CanBeDamaged() && !isAttackBoss)
            {
                AttackBossServerRpc(bossNetworkObject);
                isAttackBoss = true;
                damageable.SetCanBeDamaged(false);

                SpawnCoinsServerRpc(
                    false,
                    coinSpawnMin * 10,
                    coinSpawnMax * 10,
                    boss.transform.position
                );

                if (Random.value <= 0.30f)
                {
                    SpawnCoinsServerRpc(
                        true,
                        secondaryCoinSpawnMin * 5,
                        secondaryCoinSpawnMax * 5,
                        boss.transform.position
                    );
                }
                if (Random.value <= 0.15f && lucky != null)
                {
                    SpawnHealthPotions(boss.transform.position, 1);
                }

                SpawnOrbsServerRpc(boss.transform.position, 20);
            }
        }

        isAttackBoss = false;

        // Collider2D[] Snake = Physics2D.OverlapBoxAll(attackPoints.position, boxSize, 0f, bossLayer);

        // foreach (Collider2D sn in Snake)
        // {
        //     MachineSnakeHealth partHealth = sn.GetComponent<MachineSnakeHealth>();
        //     SnakeHealth snakeHealth = sn.GetComponentInParent<SnakeHealth>();

        //     if (partHealth != null && !isAttackBoss && snakeHealth.IsStunned())
        //     {
        //         if (
        //             (
        //                 MachineSnakeHealth.attackedPartID == -1
        //                 || MachineSnakeHealth.attackedPartID == partHealth.partID
        //             ) && !partHealth.isAlreadyHit
        //         )
        //         {
        //             partHealth.TakeDamage(1);
        //             isAttackBoss = true;

        //             snakeHealth.SetCanBeDamaged(false);

        //             SpawnCoins(
        //                 coinPrefab,
        //                 coinSpawnMin * 13,
        //                 coinSpawnMax * 13,
        //                 partHealth.transform.position
        //             );

        //             if (Random.value <= 0.25f)
        //             {
        //                 SpawnCoins(
        //                     secondaryCoinPrefab,
        //                     secondaryCoinSpawnMin * 5,
        //                     secondaryCoinSpawnMax * 5,
        //                     partHealth.transform.position
        //                 );
        //             }

        //             if (Random.value <= 0.15f && lucky != null)
        //             {
        //                 SpawnHealthPotions(partHealth.transform.position, 1);
        //             }

        //             SpawnOrbsServerRpc(partHealth.transform.position, 25);
        //         }
        //     }

        //     if (snakeHealth != null && snakeHealth.bodyPartsAttacked == snakeHealth.totalBodyParts)
        //     {
        //         HeadController headController = sn.GetComponentInChildren<HeadController>();
        //         if (headController != null && !headController.isHeadAttacked && !isAttackBoss)
        //         {
        //             headController.TakeDamage(1);
        //             isAttackBoss = true;
        //             headController.isHeadAttacked = true;

        //             snakeHealth.SetCanBeDamaged(false);
        //             SpawnCoins(
        //                 coinPrefab,
        //                 coinSpawnMin * 13,
        //                 coinSpawnMax * 13,
        //                 headController.transform.position
        //             );

        //             if (Random.value <= 0.25f)
        //             {
        //                 SpawnCoins(
        //                     secondaryCoinPrefab,
        //                     secondaryCoinSpawnMin * 5,
        //                     secondaryCoinSpawnMax * 5,
        //                     headController.transform.position
        //                 );
        //             }

        //             if (Random.value <= 0.15f && lucky != null)
        //             {
        //                 SpawnHealthPotions(headController.transform.position, 1);
        //             }

        //             SpawnOrbsServerRpc(headController.transform.position, 25);
        //         }
        //     }
        //     else
        //     {
        //         Debug.LogWarning($"SnakeHealth is null for {sn.gameObject.name}");
        //     }
        // }
        // isAttackBoss = false;
    }

    [ServerRpc(RequireOwnership = false)]
    private void AttackEnemyServerRpc(ulong enemyNetworkId)
    {
        if (
            !NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(
                enemyNetworkId,
                out NetworkObject enemyObject
            )
        )
        {
            Debug.LogError($"Enemy {enemyNetworkId} not found on server!");
            return;
        }
        if (enemyObject != null && enemyObject.IsSpawned)
        {
            Debug.Log("Despawning enemy: " + enemyNetworkId);
            enemyObject.Despawn(true);
        }
        else
        {
            Debug.Log("Enemy no networkobject");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void AttackBossServerRpc(NetworkObjectReference bossReference)
    {
        Debug.Log("[Server] AttackBossServerRpc called.");

        if (bossReference.TryGet(out NetworkObject bossObject))
        {
            DamageInterface damageable = bossObject.GetComponent<DamageInterface>();
            if (damageable != null)
            {
                Debug.Log(
                    $"[Server] DamageInterface found. CanBeDamaged: {damageable.CanBeDamaged()}"
                );

                if (damageable.CanBeDamaged())
                {
                    Debug.Log("[Server] Boss is taking damage...");
                    damageable.TakeDamage(1);
                    damageable.SetCanBeDamaged(false);
                }
                else
                {
                    Debug.Log("[Server] Boss is currently immune!");
                }
            }
            else
            {
                Debug.LogError("[Server] DamageInterface not found on this GameObject!");
            }
        }
        else
        {
            Debug.LogError("[Server] Failed to retrieve NetworkObject from reference!");
        }
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
        bool isGoldIncreaseActive = goldIncrease != null && goldIncrease.IsReady();

        int initialCoinCount = Random.Range((int)minAmount, (int)maxAmount + 1);

        int coinCount = initialCoinCount;

        if (isGoldIncreaseActive)
        {
            coinCount += goldIncrease.increaseGoldChange;
        }

        if (weaponInfo != null && weaponInfo.weaponLevel > 1)
        {
            coinCount += increaseCoin;
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

    [ServerRpc(RequireOwnership = false)]
    private void SpawnCoinsServerRpc(
        bool isSecondary,
        float minAmount,
        float maxAmount,
        Vector3 position
    )
    {
        Debug.Log($"ServerRpc called - isSecondary: {isSecondary}, position: {position}");
        bool isGoldIncreaseActive = goldIncrease != null && goldIncrease.IsReady();
        float initialCoinCount = Random.Range(minAmount, maxAmount + 1);
        float coinCount = initialCoinCount;

        if (isGoldIncreaseActive)
        {
            coinCount += goldIncrease.increaseGoldChange;
        }

        if (weaponInfo != null && weaponInfo.weaponLevel > 1)
        {
            coinCount += increaseCoin;
        }

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

        Gizmos.DrawWireCube(attackPoints.position, boxSize);
    }
}
