using System.Collections;
using System.Linq;
using EZCameraShake;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Claws : NetworkBehaviour
{
    [Header("Settings Attack")]
    public Vector2 boxSize;
    public Vector2 boxSizeUp;
    public Vector2 boxSizeDown;
    public LayerMask enemyLayer;
    public LayerMask bossLayer;
    public Transform attackPoints;
    public Transform attackPointUp;
    public Transform attackPointDown;
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

    [Header("Upgrade Level 2")]
    public int increaseExperience;

    [Header("Upgrade Level 4")]
    public GameObject plane;

    [Header("Attack Cooldown")]
    public float attackCooldown = 1f;
    private float lastAttackTime = -Mathf.Infinity;

    [Header("Swipe Android")]
    private Vector3 lastMousePosition;
    public float swipeThreshold = 50f;

    [Header("VFX Setting")]
    public VFXPooling vfxPool;

    private CoinsManager coinsManager;
    private Transform player;
    private IEnemySpawner[] enemySpawners;

    private enum SwipeDirection
    {
        None,
        Up,
        Down,
        Normal,
    }

    private SwipeDirection currentSwipeDirection = SwipeDirection.Normal;

    private Gold goldIncrease;
    private Brutal brutal;

    private CoinPoolManager coinPoolManager;
    private ExperienceOrbPoolManager orbPoolManager;
    private PlayerHealth health;
    private Lucky lucky;

    private WeaponPlayerInfo weaponInfo;
    private ClawsLevel4 clawsLevel4;

    public ClientNetworkAnimator LeftRightAnimator;

    public ClientNetworkAnimator UpAnimator;

    public ClientNetworkAnimator DownAnimator;
    //private bool isShaking;

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
            if (weaponInfo != null && weaponInfo.weaponLevel > 3)
            {
                GameObject planePrefab = Instantiate(plane);
                planePrefab.GetComponent<NetworkObject>().Spawn(true);
                if (planePrefab != null)
                {
                    ClawsLevel4 clawsLevel4 = planePrefab.GetComponent<ClawsLevel4>();
                    if (clawsLevel4 != null)
                    {
                        clawsLevel4.Activate();
                    }
                }
            }
            else
            {
                Debug.Log("Chua dat du level cua claws !!!!!!!");
            }
        }
        else
        {
            ulong myClientId = NetworkManager.Singleton.LocalClientId;

            FindPlayerServerRpc(myClientId);
            if (weaponInfo != null && weaponInfo.weaponLevel > 3)
            {
                GameObject planePrefab = Instantiate(plane);
                planePrefab.GetComponent<NetworkObject>().Spawn(true);

                if (planePrefab != null)
                {
                    ClawsLevel4 clawsLevel4 = planePrefab.GetComponent<ClawsLevel4>();
                    if (clawsLevel4 != null)
                    {
                        clawsLevel4.Activate();
                    }
                }
            }
            else
            {
                Debug.Log("Chua dat du level cua claws !!!!!!!");
            }
            weaponInfo = GetComponentInChildren<WeaponPlayerInfo>();
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

    private void FindPlayer()
    {
        NetworkObject myPlayerObject = NetworkManager.Singleton.LocalClient.PlayerObject;
        WeaponPlayerInfo weaponInfo = FindAnyObjectByType<WeaponPlayerInfo>(); // Hoặc dùng cách khác để tìm đúng object
        // Đưa WeaponPlayerInfo thành con của PlayerObject
        weaponInfo.transform.SetParent(myPlayerObject.transform);
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

        goldIncrease = GetComponentInChildren<Gold>();
        brutal = GetComponentInChildren<Brutal>();
        lucky = GetComponentInChildren<Lucky>();

        if (brutal != null)
        {
            Debug.Log("Tim thay Brutal");
        }

        if (goldIncrease != null)
        {
            Debug.Log("Tim thay Gold");
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Update()
    {
        if (IsInputDetected(out SwipeDirection direction) && CanAttack())
        {
            currentSwipeDirection = direction;
            StartCoroutine(PerformAttack(direction));
            lastAttackTime = Time.time;
            //Debug.Log($"Attack Direction: {direction}");
        }
    }

    //private IEnumerator ResetShakeState()
    //{
    //    yield return new WaitForSeconds(0.2f);
    //    isShaking = false;
    //}

    private bool IsInputDetected(out SwipeDirection direction)
    {
        direction = SwipeDirection.None;

        const float diagonalTolerance = 0.6f;

        if (Application.isEditor)
        {
            // Xử lý trong editor (dùng chuột)
            if (Input.GetMouseButtonDown(0))
            {
                lastMousePosition = Input.mousePosition;
                return false;
            }

            if (Input.GetMouseButtonUp(0)) // Nhấn chuột và nhả -> Normal Attack
            {
                direction = SwipeDirection.Normal;
                return true;
            }

            if (Input.GetMouseButton(0))
            {
                Vector3 mouseDelta = Input.mousePosition - lastMousePosition;

                // Kiểm tra nếu di chuyển đạt ngưỡng swipeThreshold
                if (mouseDelta.magnitude > swipeThreshold)
                {
                    float absX = Mathf.Abs(mouseDelta.x);
                    float absY = Mathf.Abs(mouseDelta.y);

                    if (absY > absX * diagonalTolerance) // Vuốt dọc
                    {
                        direction = mouseDelta.y > 0 ? SwipeDirection.Up : SwipeDirection.Down;
                        Debug.Log($"Swipe Direction: {direction}");
                        if (direction == SwipeDirection.Up)
                        {
                            // Add logic here if needed
                            UpAttackVFXServerRpc();
                        }
                        else
                        {
                            DownAttackVFXServerRpc();
                        }
                    }
                    else // Vuốt ngang hoặc không rõ
                    {
                        direction = SwipeDirection.Normal;
                        AttackVFXServerRpc();
                    }

                    lastMousePosition = Input.mousePosition;
                    return true;
                }
            }
        }
        else
        {
            // Xử lý trên thiết bị cảm ứng
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);

                if (touch.phase == TouchPhase.Began)
                {
                    lastMousePosition = touch.position;
                    return false;
                }

                if (touch.phase == TouchPhase.Ended) // Chạm và nhả -> Normal Attack
                {
                    direction = SwipeDirection.Normal;
                    return true;
                }

                if (touch.phase == TouchPhase.Moved)
                {
                    Vector3 touchDelta = (Vector3)touch.position - lastMousePosition;

                    // Kiểm tra nếu di chuyển đạt ngưỡng swipeThreshold
                    if (touchDelta.magnitude > swipeThreshold)
                    {
                        float absX = Mathf.Abs(touchDelta.x);
                        float absY = Mathf.Abs(touchDelta.y);

                        if (absY > absX * diagonalTolerance) // Vuốt dọc
                        {
                            direction = touchDelta.y > 0 ? SwipeDirection.Up : SwipeDirection.Down;
                            Debug.Log($"Swipe Direction: {direction}");
                            if (direction == SwipeDirection.Up)
                            {
                                // Add logic here if needed
                                UpAttackVFXServerRpc();
                            }
                            else
                            {
                                DownAttackVFXServerRpc();
                            }
                        }
                        else // Vuốt ngang hoặc không rõ
                        {
                            direction = SwipeDirection.Normal;
                            AttackVFXServerRpc();
                        }

                        lastMousePosition = touch.position;
                        return true;
                    }
                }
            }
        }

        return false;
    }

    [ServerRpc(RequireOwnership = false)]
    private void AttackVFXServerRpc()
    {
        LeftRightAnimator.SetTrigger("Attack");
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpAttackVFXServerRpc()
    {
        UpAnimator.SetTrigger("AttackUp");
    }

    [ServerRpc(RequireOwnership = false)]
    private void DownAttackVFXServerRpc()
    {
        DownAnimator.SetTrigger("AttackDown");
    }

    private bool CanAttack()
    {
        return Time.time >= lastAttackTime + attackCooldown;
    }

    private IEnumerator PerformAttack(SwipeDirection direction)
    {
        ShowAttackVFX(direction);

        //Vector2 attackBoxSize;
        Transform attackPoint;
        float attackRadius;

        switch (direction)
        {
            case SwipeDirection.Up:
                attackRadius = boxSizeUp.x / 2f;
                attackPoint = attackPointUp;
                break;
            case SwipeDirection.Down:
                attackRadius = boxSizeDown.x / 2f;
                attackPoint = attackPointDown;
                break;
            default:
                attackRadius = boxSize.x / 2f;
                attackPoint = attackPoints;
                break;
        }

        Collider2D[] enemies = Physics2D
            .OverlapCircleAll(attackPoint.position, attackRadius, enemyLayer)
            .OrderBy(c => Vector2.Distance(attackPoint.position, c.transform.position))
            .ToArray();

        foreach (Collider2D enemy in enemies)
        {
            if (enemy != null && enemy.gameObject != null && enemy.gameObject.activeInHierarchy)
            {
                CameraShaker.Instance.ShakeOnce(4f, 4f, 0.1f, 0.1f);

                yield return StartCoroutine(
                    HandleEnemyDeath(enemy.gameObject, enemy.transform.position)
                );

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
                NetworkObject networkObject =
                    enemy.gameObject.GetComponentInParent<NetworkObject>();
                if (networkObject == null)
                {
                    networkObject = enemy.GetComponent<NetworkObject>();
                }
                AttackEnemyServerRpc(networkObject.NetworkObjectId);
                SpawnCoinsServerRpc(false, coinSpawnMin, coinSpawnMax, enemy.transform.position);

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

                SpawnOrbsServerRpc(enemy.transform.position, 5);
            }
        }

        Collider2D[] bosses = Physics2D.OverlapCircleAll(
            attackPoint.position,
            attackRadius,
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

                CameraShaker.Instance.ShakeOnce(8f, 8f, 0.3f, 0.3f);

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

        // Collider2D[] Snake = Physics2D.OverlapCircleAll(
        //     attackPoint.position,
        //     attackRadius,
        //     bossLayer
        // );

        // // // foreach (Collider2D sn in Snake)
        // // // {
        // // //     MachineSnakeHealth partHealth = sn.GetComponent<MachineSnakeHealth>();
        // // //     SnakeHealth snakeHealth = sn.GetComponentInParent<SnakeHealth>();

        // // //     if (partHealth != null && !isAttackBoss && snakeHealth.IsStunned())
        // // //     {
        // // //         if (
        // // //             (
        // // //                 MachineSnakeHealth.attackedPartID == -1
        // // //                 || MachineSnakeHealth.attackedPartID == partHealth.partID
        // // //             ) && !partHealth.isAlreadyHit
        // // //         )
        // // //         {
        // // //             partHealth.TakeDamage(1);
        // // //             isAttackBoss = true;

        // // //             snakeHealth.SetCanBeDamaged(false);

        // // //             SpawnCoins(
        // // //                 coinPrefab,
        // // //                 coinSpawnMin * 13,
        // // //                 coinSpawnMax * 13,
        // // //                 partHealth.transform.position
        // // //             );

        // // //             if (Random.value <= 0.25f)
        // // //             {
        // // //                 SpawnCoins(
        // // //                     secondaryCoinPrefab,
        // // //                     secondaryCoinSpawnMin * 5,
        // // //                     secondaryCoinSpawnMax * 5,
        // // //                     partHealth.transform.position
        // // //                 );
        // // //             }

        // // //             if (Random.value <= 0.15f && lucky != null)
        // // //             {
        // // //                 SpawnHealthPotions(partHealth.transform.position, 1);
        // // //             }

        // // //             SpawnOrbsServerRpc(partHealth.transform.position, 25);
        // // //         }
        // // //     }

        // //     if (snakeHealth != null && snakeHealth.bodyPartsAttacked == snakeHealth.totalBodyParts)
        // //     {
        // //         HeadController headController = sn.GetComponentInChildren<HeadController>();
        // //         if (headController != null && !headController.isHeadAttacked && !isAttackBoss)
        // //         {
        // //             headController.TakeDamage(1);
        // //             isAttackBoss = true;
        // //             headController.isHeadAttacked = true;

        // //             snakeHealth.SetCanBeDamaged(false);
        // //             SpawnCoins(
        // //                 coinPrefab,
        // //                 coinSpawnMin * 13,
        // //                 coinSpawnMax * 13,
        // //                 headController.transform.position
        // //             );

        // //             if (Random.value <= 0.25f)
        // //             {
        // //                 SpawnCoins(
        // //                     secondaryCoinPrefab,
        // //                     secondaryCoinSpawnMin * 5,
        // //                     secondaryCoinSpawnMax * 5,
        // //                     headController.transform.position
        // //                 );
        // //             }

        // //             if (Random.value <= 0.15f && lucky != null)
        // //             {
        // //                 SpawnHealthPotions(headController.transform.position, 1);
        // //             }

        // //             SpawnOrbsServerRpc(headController.transform.position, 25);
        // //         }
        // //     }
        // //     else
        // //     {
        // //         Debug.LogWarning($"SnakeHealth is null for {sn.gameObject.name}");
        // //     }
        // // }
        // isAttackBoss = false;
    }

    private void ShowAttackVFX(SwipeDirection direction)
    {
        if (vfxPool != null)
        {
            GameObject vfx = vfxPool.Get();

            Transform attackPoint = direction switch
            {
                SwipeDirection.Up => attackPointUp,
                SwipeDirection.Down => attackPointDown,
                _ => attackPoints,
            };

            vfx.transform.position = attackPoint.position;
            vfx.transform.rotation = Quaternion.identity;

            VFXFollower follower = vfx.GetComponent<VFXFollower>();
            if (follower != null)
            {
                follower.SetTarget(attackPoint, player);
            }
        }
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

    private IEnumerator HandleEnemyDeath(GameObject enemyObject, Vector3 position)
    {
        SpriteRenderer firstSprite = enemyObject
            .GetComponentsInChildren<SpriteRenderer>(true)
            .FirstOrDefault();

        if (firstSprite == null)
        {
            firstSprite = enemyObject.GetComponent<SpriteRenderer>();
        }

        if (firstSprite == null && enemyObject.transform.parent != null)
        {
            firstSprite = enemyObject.transform.parent.GetComponent<SpriteRenderer>();
        }

        if (firstSprite != null)
        {
            firstSprite.enabled = false;
            //Debug.Log("SpriteRenderer found on: " + firstSprite.gameObject.name);
        }
        else
        {
            Debug.LogWarning("Không tìm thấy SpriteRenderer trong enemy hoặc cha.");
        }

        Animator lastAnim = enemyObject.GetComponentsInChildren<Animator>(true).LastOrDefault();

        if (lastAnim != null)
        {
            lastAnim.SetTrigger("Explosion");
        }

        yield return new WaitForSeconds(1f);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = currentSwipeDirection switch
        {
            SwipeDirection.Up => Color.green,
            SwipeDirection.Down => Color.red,
            _ => Color.blue,
        };

        Transform gizmoAttackPoint;
        float attackRadius;

        switch (currentSwipeDirection)
        {
            case SwipeDirection.Up:
                gizmoAttackPoint = attackPointUp;
                attackRadius = boxSizeUp.x / 2f;
                break;
            case SwipeDirection.Down:
                gizmoAttackPoint = attackPointDown;
                attackRadius = boxSizeDown.x / 2f;
                break;
            default:
                gizmoAttackPoint = attackPoints;
                attackRadius = boxSize.x / 2f;
                break;
        }

        if (gizmoAttackPoint != null)
        {
            Gizmos.DrawWireSphere(gizmoAttackPoint.position, attackRadius);
        }
        else
        {
            Debug.LogWarning("Attack point for the current swipe direction is not assigned!");
        }
    }
}
