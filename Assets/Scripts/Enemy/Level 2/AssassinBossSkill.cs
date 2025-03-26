using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class AssassinBossSkill : NetworkBehaviour
{
    public static AssassinBossSkill Instance;
    private NetworkRigidbody2D rb;
    private bool isGrounded = false;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask wallLayer;

    [Header("Trap")]
    public GameObject trapPrefab;
    public Transform[] targetPositions;
    public float trapSpeed;
    public LayerMask groundLayer;
    private List<int> usedIndices = new List<int>();

    [Header("Bomb")]
    public GameObject bombPrefab;
    public float bombSpeed;
    private Transform playerTransform;
    public GameObject bombFragmentPrefab;
    public float fragmentSpeed;
    public Transform[] randomBombTargets;

    [Header("Teleport")]
    public Transform playerTransforms;
    public float teleportOffsetX = 5f;
    public float additionalHeight = 0.5f;
    private bool isTeleporting = false;

    [Header("Shoot")]
    public GameObject bulletPrefab;
    public Transform shootingPoint;
    public float bulletSpeed;

    [Header("Clone Skill")]
    public GameObject clonePrefab;
    public float cloneSpeed;
    public Transform[] dashTargets;
    private Vector3 originalPosition;
    public string playerTag = "Player";
    public float blinkDuration = 0.2f;
    public float blinkInterval = 0.5f;

    private bool isSkillActive = false;
    private int[] skillOrder = new int[] { 1, 2, 3, 4 };
    private AssassinHealth assassinHealth;
    private TrapDamage trap;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        trap = GetComponent<TrapDamage>();
        assassinHealth = GetComponent<AssassinHealth>();
        rb = GetComponent<NetworkRigidbody2D>();
        dashTargets = DashTarget.Instance.dashTargets;
        randomBombTargets = BoomPosition.Instance.randomBombTargets;
        targetPositions = TrapPosition.Instance.targetPositions;
        GameObject playerObject = GameObject.FindWithTag("Player");

        if (playerObject != null)
        {
            playerTransform = playerObject.transform;
            playerTransforms = playerObject.transform;
        }
        else
        {
            Debug.LogWarning("Player not found using tag 'Player'. Searching by layer...");

            int playerLayer = LayerMask.NameToLayer("Player");
            if (playerLayer != -1)
            {
                GameObject[] objectsInLayer = FindObjectsOfType<GameObject>();
                foreach (GameObject obj in objectsInLayer)
                {
                    if (obj.layer == playerLayer)
                    {
                        playerTransform = obj.transform;
                        playerTransforms = obj.transform;
                        Debug.Log("Player found using layer 'Player'.");
                        break;
                    }
                }

                if (playerTransform == null)
                {
                    Debug.LogError(
                        "Player not found using layer 'Player'. Make sure to assign the correct layer."
                    );
                }
            }
            else
            {
                Debug.LogError(
                    "Layer 'Player' does not exist in the project. Add a layer named 'Player'."
                );
            }
        }

        originalPosition = transform.position;
    }

    private void Update()
    {
        // if (Input.GetKeyDown(KeyCode.Q))
        // {
        //     ThrowTraps(4);
        // }
        // if (Input.GetKeyDown(KeyCode.W))
        // {
        //     TeleportToPlayer();
        // }
        // if (Input.GetKeyDown(KeyCode.E))
        // {
        //     ThrowBombAtPlayer();
        // }
        // if (Input.GetKeyDown(KeyCode.R))
        // {
        //     CloneAndDash();
        // }
    }

    public void Active()
    {
        gameObject.SetActive(true);

        StartCoroutine(SkillActive());
    }

    IEnumerator SkillActive()
    {
        yield return new WaitForSeconds(2f);

        while (true)
        {
            CheckGround();

            if (!isGrounded)
            {
                Debug.Log("Assassin Boss phải ở ground mới active skill");
                yield return null;
                continue;
            }

            if (!isSkillActive)
            {
                isSkillActive = true;

                foreach (int skill in skillOrder)
                {
                    switch (skill)
                    {
                        case 1:
                            yield return new WaitForSeconds(1.5f);
                            Debug.Log("Throwing traps...");
                            ThrowTraps(4);
                            yield return new WaitForSeconds(0.5f);
                            break;

                        case 2:
                            Debug.Log("Teleporting to player...");
                            TeleportToPlayer();
                            yield return new WaitForSeconds(4f);
                            break;

                        case 3:
                            Debug.Log("Throwing bomb at player...");
                            ThrowBombAtPlayer();
                            break;

                        case 4:
                            Debug.Log("Cloning and dashing...");
                            CloneAndDash();
                            yield return new WaitForSeconds(8f);
                            break;

                        default:
                            Debug.LogWarning("Unknown skill number: " + skill);
                            break;
                    }

                    yield return new WaitForSeconds(2f);
                }

                isSkillActive = false;
            }
            else
            {
                yield return null;
            }
        }
    }

    void CheckGround()
    {
        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(
                groundCheck.position,
                groundCheckRadius,
                groundLayer
            );
        }
        else
        {
            Debug.LogWarning("Ground Check Transform is not assigned!");
        }

        if (assassinHealth != null && assassinHealth.currentHealth.Value <= 0)
        {
            StopAllCoroutines();
        }
    }

    private void FlipToPlayer()
    {
        if (playerTransforms == null)
            return;

        float directionX = playerTransforms.position.x - transform.position.x;

        if (directionX > 0 && transform.rotation != Quaternion.identity)
        {
            transform.rotation = Quaternion.identity; // Quay về hướng phải
        }
        else if (directionX < 0 && transform.rotation != Quaternion.Euler(0, 180, 0))
        {
            transform.rotation = Quaternion.Euler(0, 180, 0); // Quay về hướng trái
        }
    }

    void ThrowTraps(int trapCount)
    {
        isSkillActive = true;

        if (trapPrefab == null || targetPositions.Length == 0)
        {
            Debug.LogError("Trap Prefab or Target Positions is missing!");
            return;
        }

        usedIndices.Clear();

        for (int i = 0; i < trapCount; i++)
        {
            int randomIndex = GetUniqueRandomIndex();
            Transform target = targetPositions[randomIndex];

            GameObject trap = Instantiate(trapPrefab, transform.position, Quaternion.identity);
            trap.GetComponent<NetworkObject>().Spawn(true);

            StartCoroutine(MoveTrapToTarget(trap, target.position));
        }

        isSkillActive = false;
    }

    int GetUniqueRandomIndex()
    {
        int randomIndex;

        do
        {
            randomIndex = Random.Range(0, targetPositions.Length);
        } while (usedIndices.Contains(randomIndex));

        usedIndices.Add(randomIndex);
        return randomIndex;
    }

    IEnumerator<WaitForSeconds> MoveTrapToTarget(GameObject trap, Vector3 targetPosition)
    {
        float yOffset = 0.3f;

        targetPosition.y += yOffset;

        while (trap != null && Vector3.Distance(trap.transform.position, targetPosition) > 0.1f)
        {
            trap.transform.position = Vector3.MoveTowards(
                trap.transform.position,
                targetPosition,
                trapSpeed * Time.deltaTime
            );
            yield return null;
        }

        TrapDamage trapDamage = trap.GetComponent<TrapDamage>();
        if (trapDamage != null)
        {
            trapDamage.SetCanDamage(true);
        }
        else
        {
            Debug.LogError("TrapDamage component is missing on the trap!");
        }
        Debug.Log("Trap reached adjusted target: " + targetPosition);
    }

    void ThrowBombAtPlayer()
    {
        isSkillActive = true;

        if (playerTransform == null || bombPrefab == null || randomBombTargets.Length == 0)
        {
            Debug.LogError("Player Transform, Bomb Prefab, or Bomb Targets are missing!");
            return;
        }

        Vector3 playerTargetPosition = playerTransform.position;
        GameObject bomb1 = Instantiate(bombPrefab, transform.position, Quaternion.identity);
        bomb1.GetComponent<NetworkObject>().Spawn(true);
        StartCoroutine(MoveBombToTarget(bomb1, playerTargetPosition));

        Vector3 randomTargetPosition1 = randomBombTargets[
            Random.Range(0, randomBombTargets.Length)
        ].position;
        GameObject bomb2 = Instantiate(bombPrefab, transform.position, Quaternion.identity);
        bomb2.GetComponent<NetworkObject>().Spawn(true);
        StartCoroutine(MoveBombToTarget(bomb2, randomTargetPosition1));

        Vector3 randomTargetPosition2 = randomBombTargets[
            Random.Range(0, randomBombTargets.Length)
        ].position;
        GameObject bomb3 = Instantiate(bombPrefab, transform.position, Quaternion.identity);
        bomb3.GetComponent<NetworkObject>().Spawn(true);
        StartCoroutine(MoveBombToTarget(bomb3, randomTargetPosition2));

        isSkillActive = false;
    }

    IEnumerator<WaitForSeconds> MoveBombToTarget(GameObject bomb, Vector3 targetPosition)
    {
        float bombYOffset = -0.4f;
        targetPosition.y += bombYOffset;

        while (bomb != null && Vector3.Distance(bomb.transform.position, targetPosition) > 0.1f)
        {
            bomb.transform.position = Vector3.MoveTowards(
                bomb.transform.position,
                targetPosition,
                bombSpeed * Time.deltaTime
            );
            yield return null;
        }

        yield return new WaitForSeconds(2f);

        BombExplosion(bomb);
        bomb.GetComponent<NetworkObject>().Despawn(true);
    }

    void BombExplosion(GameObject bomb)
    {
        BombDamage Bomb = bomb.GetComponent<BombDamage>();

        if (Bomb != null)
        {
            Bomb.BombExplosion();
        }

        CreateBombFragments(bomb.transform.position);
    }

    void CreateBombFragments(Vector3 explosionPosition)
    {
        Vector3[] fragmentDirections = new Vector3[]
        {
            new Vector3(1f, 1, 0), // Đi chéo lên phải
            new Vector3(-1f, 1, 0), // Đi chéo lên trái
            new Vector3(1f, -1, 0), // Đi chéo xuống phải
            new Vector3(-1f, -1, 0), // Đi chéo xuống trái
        };

        foreach (Vector3 direction in fragmentDirections)
        {
            GameObject fragment = Instantiate(
                bombFragmentPrefab,
                explosionPosition,
                Quaternion.identity
            );
            fragment.GetComponent<NetworkObject>().Spawn(true);
            StartCoroutine(MoveFragment(fragment, direction.normalized));
        }
    }

    IEnumerator MoveFragment(GameObject fragment, Vector3 direction)
    {
        float fragmentLifetime = 4f;
        float timer = 0f;
        while (timer < fragmentLifetime && fragment != null)
        {
            fragment.transform.position += direction * fragmentSpeed * Time.deltaTime;
            timer += Time.deltaTime;
            yield return null;
        }

        if (fragment != null)
        {
            fragment.GetComponent<NetworkObject>().Despawn(true);
        }
    }

    public void TeleportToPlayer()
    {
        isSkillActive = true;

        if (playerTransforms == null)
        {
            Debug.LogError("Player Transform is missing!");
            return;
        }

        StartCoroutine(ExecuteTeleport());
    }

    private IEnumerator ExecuteTeleport()
    {
        if (isTeleporting)
            yield break;

        isTeleporting = true;

        yield return new WaitForSeconds(1f);

        FlipToPlayer();

        // Xác định vị trí gần Player
        float offsetX =
            playerTransforms.position.x > transform.position.x ? -teleportOffsetX : teleportOffsetX;
        Vector2 targetPosition = new Vector2(
            playerTransforms.position.x + offsetX,
            playerTransforms.position.y
        );
        targetPosition.y += additionalHeight;

        transform.position = targetPosition;

        rb.Rigidbody2D.gravityScale = 4f;

        Debug.Log("Assassin Boss teleported to: " + targetPosition);

        yield return new WaitForSeconds(0.5f);

        isTeleporting = false;

        FlipToPlayer();

        yield return new WaitForSeconds(0.5f);

        StartCoroutine(ShootBullets());

        isSkillActive = false;
    }

    IEnumerator ShootBullets()
    {
        if (bulletPrefab == null && shootingPoint == null)
        {
            yield break;
        }

        Vector3 direction =
            transform.rotation == Quaternion.identity ? Vector3.right : Vector3.left;

        for (int i = 0; i < 3; i++)
        {
            GameObject bullet = Instantiate(
                bulletPrefab,
                shootingPoint.position,
                Quaternion.identity
            );
            bullet.GetComponent<NetworkObject>().Spawn(true);
            Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();

            if (bulletRb != null)
            {
                bulletRb.linearVelocity = direction * bulletSpeed;
            }

            yield return new WaitForSeconds(1.2f);
        }
    }

    public void CloneAndDash()
    {
        if (!IsServer) // Chỉ server mới được phép tạo clone và thực hiện dash
        {
            return;
        }

        isSkillActive = true;

        if (clonePrefab == null || DashTarget.Instance.dashTargets.Length == 0)
        {
            Debug.LogError("Clone Prefab hoặc Dash Targets là thiếu!");
            return;
        }

        GameObject clone1 = Instantiate(clonePrefab, transform.position, Quaternion.identity);
        clone1.GetComponent<NetworkObject>().Spawn(true);
        GameObject clone2 = Instantiate(clonePrefab, transform.position, Quaternion.identity);
        clone2.GetComponent<NetworkObject>().Spawn(true);
        GameObject[] allCharacters = new GameObject[] { gameObject, clone1, clone2 };

        StartCoroutine(BlinkClones(clone1, clone2));

        StartCoroutine(MoveClonesToSides(clone1, clone2, allCharacters));
    }

    IEnumerator MoveClonesToSides(GameObject clone1, GameObject clone2, GameObject[] allCharacters)
    {
        Vector3 leftPosition = transform.position + Vector3.left * 2f;
        Vector3 rightPosition = transform.position + Vector3.right * 2f;

        float moveDuration = 1f;
        float elapsedTime = 0f;

        Vector3 clone1StartPosition = clone1.transform.position;
        Vector3 clone2StartPosition = clone2.transform.position;

        while (elapsedTime < moveDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / moveDuration;

            clone1.transform.position = Vector3.Lerp(clone1StartPosition, leftPosition, t);
            clone2.transform.position = Vector3.Lerp(clone2StartPosition, rightPosition, t);

            yield return null;
        }

        clone1.transform.position = leftPosition;
        clone2.transform.position = rightPosition;

        StartCoroutine(DelayBeforeDash(allCharacters));
    }

    IEnumerator DelayBeforeDash(GameObject[] allCharacters)
    {
        yield return new WaitForSeconds(1.5f);

        List<int> availableIndices = new List<int>();
        for (int i = 0; i < DashTarget.Instance.dashTargets.Length; i++)
        {
            availableIndices.Add(i);
        }

        int randomIndex1 = availableIndices[Random.Range(0, availableIndices.Count)];
        availableIndices.Remove(randomIndex1);

        int randomIndex2 = availableIndices[Random.Range(0, availableIndices.Count)];
        availableIndices.Remove(randomIndex2);

        Vector3 targetPosition1 = DashTarget.Instance.dashTargets[randomIndex1].position;
        Vector3 targetPosition2 = DashTarget.Instance.dashTargets[randomIndex2].position;

        float offsetY = 1.2f;
        targetPosition1.y += offsetY;
        targetPosition2.y += offsetY;

        Vector3 assassinTargetPosition = DashTarget
            .Instance
            .dashTargets[availableIndices[Random.Range(0, availableIndices.Count)]]
            .position;
        assassinTargetPosition.y += offsetY;

        // Tìm player bằng Tag và truyền vào CloneDash
        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
        if (player == null)
        {
            int playerLayer = LayerMask.NameToLayer("Player");
            player = FindObjectByLayer(playerLayer);
        }

        if (player != null)
        {
            StartCoroutine(
                MoveToRandomTargets(
                    allCharacters,
                    assassinTargetPosition,
                    targetPosition1,
                    targetPosition2,
                    player.transform
                )
            );
        }
        else
        {
            Debug.LogError("Player không tìm thấy theo cả tag và layer!");
        }
    }

    GameObject FindObjectByLayer(int layer)
    {
        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.layer == layer)
            {
                return obj;
            }
        }
        return null;
    }

    IEnumerator MoveToRandomTargets(
        GameObject[] characters,
        Vector3 assassinTargetPosition,
        Vector3 targetPosition1,
        Vector3 targetPosition2,
        Transform player
    )
    {
        characters[0].transform.position = assassinTargetPosition;
        characters[1].transform.position = targetPosition1;
        characters[2].transform.position = targetPosition2;

        yield return new WaitForSeconds(1f);

        CloneDash cloneDash1 = characters[1].GetComponent<CloneDash>();
        CloneDash cloneDash2 = characters[2].GetComponent<CloneDash>();

        if (cloneDash1 != null)
            cloneDash1.DashTowardsPlayer();

        if (cloneDash2 != null)
            cloneDash2.DashTowardsPlayer();

        CloneDash playerCloneDash = characters[0].GetComponent<CloneDash>();
        if (playerCloneDash != null)
            playerCloneDash.DashTowardsPlayer();

        yield return new WaitForSeconds(4f);

        // Sử dụng Despawn thay vì Destroy
        if (
            characters[1] != null
            && characters[1].TryGetComponent(out NetworkObject networkObject1)
            && networkObject1.IsSpawned
        )
        {
            networkObject1.Despawn(true);
        }

        if (
            characters[2] != null
            && characters[2].TryGetComponent(out NetworkObject networkObject2)
            && networkObject2.IsSpawned
        )
        {
            networkObject2.Despawn(true);
        }

        characters[0].transform.position = originalPosition;

        Rigidbody2D rb2d = characters[0].GetComponent<Rigidbody2D>();
        if (rb2d != null)
        {
            rb2d.gravityScale = 4f;
        }
        else
        {
            Rigidbody rb = characters[0].GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.useGravity = true;
            }
        }

        if (isGrounded)
        {
            StartCoroutine(EnableDamageTemporarily());
        }
        else
        {
            isSkillActive = false;
        }
    }

    private IEnumerator BlinkClones(GameObject clone1, GameObject clone2)
    {
        Renderer clone1Renderer = clone1.GetComponent<Renderer>();
        Renderer clone2Renderer = clone2.GetComponent<Renderer>();

        if (clone1Renderer == null || clone2Renderer == null)
        {
            Debug.LogError("Không tìm thấy Renderer trên clone!");
            yield break;
        }

        while (isSkillActive)
        {
            clone1Renderer.enabled = false;
            clone2Renderer.enabled = false;

            yield return new WaitForSeconds(blinkDuration);

            clone1Renderer.enabled = true;
            clone2Renderer.enabled = true;

            yield return new WaitForSeconds(blinkInterval);
        }
    }

    IEnumerator EnableDamageTemporarily()
    {
        assassinHealth.SetCanBeDamaged(true);
        yield return new WaitForSeconds(4f);
        assassinHealth.SetCanBeDamaged(false);
        isSkillActive = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if ((groundLayer.value & (1 << collision.gameObject.layer)) > 0)
        {
            rb.Rigidbody2D.gravityScale = 0;
        }
    }

    private void OnDrawGizmos()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
