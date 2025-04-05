using System.Collections;
using System.Linq;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

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

    public ClientNetworkAnimator vfxAnim;
    private WeaponPlayerInfo weaponInfo;

    private bool isShaking;
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
        // coin and pool manager
        coinPoolManager = FindFirstObjectByType<CoinPoolManager>();
        orbPoolManager = FindFirstObjectByType<ExperienceOrbPoolManager>();
        coinsManager = UnityEngine.Object.FindFirstObjectByType<CoinsManager>();
        ClientNetworkAnimator networkAnimator = GetComponentInChildren<ClientNetworkAnimator>();
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
        if (!IsOwner)
            return;
        if (IsInputDetected() && CanAttack())
        {
            StartCoroutine(PerformAttack());
            lastAttackTime = Time.time;
        }
    }

    private IEnumerator ResetShakeState()
    {
        yield return new WaitForSeconds(0.2f);
        isShaking = false;
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

    [ServerRpc(RequireOwnership = false)]
    private void FindPlayerServerRpc()
    {
        NetworkObject myPlayerObject = NetworkManager.Singleton.LocalClient.PlayerObject;
        WeaponPlayerInfo weaponPlayerInfo = FindAnyObjectByType<WeaponPlayerInfo>(); // Hoặc dùng cách khác để tìm đúng object
        // Đưa WeaponPlayerInfo thành con của PlayerObject
        weaponPlayerInfo.transform.SetParent(myPlayerObject.transform);

        weaponInfo.weaponName = weaponPlayerInfo.weaponName;
        weaponInfo.weaponLevel = weaponPlayerInfo.weaponLevel;

        Debug.Log("Weapon info cua player = " + weaponInfo.weaponLevel);
    }

    private bool CanAttack()
    {
        return Time.time >= lastAttackTime + attackCooldown;
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
    private void NotifyEnemyKilledServerRpc()
    {
        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.OnEnemyKilled();
        }
    }

    private IEnumerator PerformAttack()
    {
        StartCoroutine(ShowAttackVFX());

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
                //GameObject enemyObject = enemy.transform.root.gameObject;

                if (!isShaking)
                {
                    isShaking = true;
                    CameraShake.Instance.StartShake(0.1f, 1f, 0.5f, 5f);
                    StartCoroutine(ResetShakeState());
                }

                yield return StartCoroutine(HandleEnemyDeath(enemy.gameObject, enemy.transform.position));

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
                NetworkObject networkObject =
                    enemy.gameObject.GetComponentInParent<NetworkObject>();
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

                    if (ShouldSpawnPotion())
                    {
                        SpawnHealthPotionsServerRpc(enemy.transform.position, 1);
                    }
                }
                else
                {
                    Debug.LogWarning("Enemy does not have a NetworkObject component!");
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
                NetworkObject bossNetworkObject = boss.GetComponent<NetworkObject>();
                if (bossNetworkObject != null && bossNetworkObject.IsSpawned)
                {
                    AttackBossServerRpc(bossNetworkObject);
                    isAttackBoss = true;

                    if (!isShaking)
                    {
                        isShaking = true;
                        CameraShake.Instance.StartShake(0.1f, 3f, 1.5f, 5f);
                        StartCoroutine(ResetShakeState());
                    }

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

                    if (ShouldSpawnPotion())
                    {
                        SpawnHealthPotionsServerRpc(boss.transform.position, 1);
                    }
                }
                else
                {
                    Debug.LogError("[Client] Boss NetworkObject is null or not spawned!");
                }

                // SpawnCoins(
                //     coinPrefab,
                //     coinSpawnMin * 10,
                //     coinSpawnMax * 10,
                //     boss.transform.position
                // );

                // if (Random.value <= 0.25f)
                // {
                //     SpawnCoins(
                //         secondaryCoinPrefab,
                //         secondaryCoinSpawnMin * 5,
                //         secondaryCoinSpawnMax * 5,
                //         boss.transform.position
                //     );
                // }

                // if (ShouldSpawnPotion())
                // {
                //     SpawnHealthPotions(boss.transform.position, 1);
                // }

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

        //             SpawnCoinsServerRpc(
        //                 false,
        //                 coinSpawnMin * 13,
        //                 coinSpawnMax * 13,
        //                 partHealth.transform.position
        //             );

        //             if (Random.value <= 0.30f)
        //             {
        //                 SpawnCoinsServerRpc(
        //                     true,
        //                     secondaryCoinSpawnMin * 5,
        //                     secondaryCoinSpawnMax * 5,
        //                     partHealth.transform.position
        //                 );
        //             }

        //             if (ShouldSpawnPotion())
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
        //             SpawnCoinsServerRpc(
        //                 false,
        //                 coinSpawnMin * 13,
        //                 coinSpawnMax * 13,
        //                 headController.transform.position
        //             );

        //             if (Random.value <= 0.30f)
        //             {
        //                 SpawnCoinsServerRpc(
        //                     true,
        //                     secondaryCoinSpawnMin * 5,
        //                     secondaryCoinSpawnMax * 5,
        //                     headController.transform.position
        //                 );
        //             }

        //             if (ShouldSpawnPotion())
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

            NetworkRigidbody2D coinRb = coin.GetComponent<NetworkRigidbody2D>();

            if (coinRb != null)
            {
                Vector2 forceDirection =
                    new Vector2(Random.Range(-1.5f, 1.5f), Random.Range(1f, 1f)) * 2.5f;
                coinRb.Rigidbody2D.AddForce(forceDirection, ForceMode2D.Impulse);
                StartCoroutine(CheckIfCoinIsStuck(coinRb.Rigidbody2D));
            }

            CoinsScript coinScript = coin.GetComponent<CoinsScript>();
            if (coinScript != null)
            {
                coinScript.SetCoinType(!isSecondary, isSecondary);
                // StartCoroutine(HookCoinsContinuously());
            }
        }
    }

    private IEnumerator ShowAttackVFX()
    {
        if (IsOwner)
        {
            ShowAttackVFXServerRpc();

            yield return new WaitForSeconds(0.1f);
        }
    }

    [ClientRpc(RequireOwnership = false)]
    private void ShowAttackVFXClientRpc()
    {
        vfxAnim.SetTrigger("Attack");
    }

    [ServerRpc(RequireOwnership = false)]
    private void ShowAttackVFXServerRpc()
    {
        vfxAnim.SetTrigger("Attack");
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

    [ServerRpc(RequireOwnership = false)]
    private void SpawnHealthPotionsServerRpc(Vector3 position, int potionCount)
    {
        for (int i = 0; i < potionCount; i++)
        {
            float randomAngle = Random.Range(0f, 360f);

            float potionX = position.x + Mathf.Cos(randomAngle * Mathf.Deg2Rad);
            float potionY = position.y + Mathf.Sin(randomAngle * Mathf.Deg2Rad);
            Vector3 spawnPosition = new Vector3(
                potionX,
                potionY,
                healthPotionPrefab.transform.position.z
            );

            GameObject potion = Instantiate(healthPotionPrefab, spawnPosition, Quaternion.identity);

            NetworkObject potionNetworkObject = potion.GetComponent<NetworkObject>();
            if (potionNetworkObject == null)
            {
                potionNetworkObject = potion.AddComponent<NetworkObject>();
            }

            potionNetworkObject.Spawn(true);

            NetworkRigidbody2D potionRb = potion.GetComponent<NetworkRigidbody2D>();
            if (potionRb != null)
            {
                Vector2 randomForce =
                    new Vector2(Random.Range(-2f, 2f), Random.Range(1f, 1f)) * 2.5f;
                potionRb.Rigidbody2D.AddForce(randomForce, ForceMode2D.Impulse);

                potionRb.Rigidbody2D.bodyType = RigidbodyType2D.Kinematic;

                StartCoroutine(MovePotionToPlayer(potion, potionMoveDelay, OwnerClientId));
            }
        }
    }

    IEnumerator MovePotionToPlayer(GameObject potion, float delay, ulong targetClientId)
    {
        // Find the NetworkObject of the player with the given clientId
        NetworkClient client = NetworkManager.Singleton.ConnectedClients[targetClientId];
        Transform playerTransform = client.PlayerObject.transform;
        yield return new WaitForSeconds(delay);

        if (potion != null && player != null)
        {
            NetworkRigidbody2D potionRb = potion.GetComponent<NetworkRigidbody2D>();
            if (potionRb != null)
            {
                while (potion != null && player != null)
                {
                    Vector3 direction = (
                        playerTransform.position - potion.transform.position
                    ).normalized;
                    potionRb.MovePosition(
                        potion.transform.position + direction * Time.deltaTime * potionMoveToPlayer
                    );

                    if (
                        Vector3.Distance(potion.transform.position, playerTransform.position) < 0.5f
                    )
                    {
                        health.HealHealth(3);
                        potion.GetComponent<NetworkObject>().Despawn(true);

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
        float baseRate = 1f; // Ban đầu tỉ lệ spawn là 0%

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

    private IEnumerator HandleEnemyDeath(GameObject enemyObject, Vector3 position)
    {
        SpriteRenderer firstSprite = enemyObject.GetComponentsInChildren<SpriteRenderer>(true).FirstOrDefault();

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

        Animator lastAnim = enemyObject
            .GetComponentsInChildren<Animator>(true) 
            .LastOrDefault();

        if (lastAnim != null)
        {
            lastAnim.SetTrigger("Explosion");
        }

        yield return new WaitForSeconds(1f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;

        Gizmos.DrawWireCube(attackPoints.position, boxSize);
    }
}
