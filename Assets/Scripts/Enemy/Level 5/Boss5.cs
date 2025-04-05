using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class Boss5 : NetworkBehaviour
{
    public static Boss5 Instance;

    [Header("Other")]
    private Rigidbody2D rb;

    [SerializeField]
    public float groundCheckRadius = 0.2f;

    [SerializeField]
    public LayerMask wallLayer;
    private Transform BossTrans;
    private Vector3 defaultPosition;

    [Header("Check")]
    [SerializeField]
    public FloorCheck floorCheck;

    [SerializeField]
    public SideManager sideManager;

    [Header("Skill 1 Settings")]
    [SerializeField]
    private GameObject Skill1Left;

    [SerializeField]
    private GameObject Skill1Right;
    private bool canMove = false;

    [SerializeField]
    public float moveSpeed;
    private Transform targetTransform;

    [SerializeField]
    private float targetOffset = 1f;

    private GameObject summonedObject;
    private Vector3 spawnPosition;
    private bool isMovingForward = true;
    private bool isMovingLeft;

    [Header("Skill 2 Settings")]
    [SerializeField]
    public GameObject[] SpamPointsLeft;

    [SerializeField]
    public GameObject[] SpamPointsRight;
    private List<GameObject> spawnedObjects = new List<GameObject>();
    public float spamDelay = 0.1f;

    [Header("Skull Projectile Settings")]
    [SerializeField]
    private GameObject skullPrefab;

    [SerializeField]
    private float orbitRadius = 2f;

    [SerializeField]
    private float orbitSpeed = 180f;

    [SerializeField]
    private float projectileSpeed = 15f;

    [SerializeField]
    private int numberOfSkulls = 3;

    [SerializeField]
    private float orbitDuration = 2f;
    private List<GameObject> orbitingSkulls = new List<GameObject>();
    private float orbitTimer = 0f;

    [SerializeField]
    private float skullSkillCooldown = 5f;
    private float skullSkillTimer = 0f;

    // Có thể thêm biến để kiểm tra trạng thái
    private bool isSkullSkillActive = false;

    [SerializeField]
    Transform playerTrans;

    [Header("Move Bomb")]
    public GameObject boomb;
    public Transform[] wayPoints;
    public float moveSpeedBomb = 10f;
    public float moveRightDuration = 3f;
    private int currentWaypointIndex = 0;

    [Header("Fire Bomb")]
    public GameObject bigbombPrefab;
    public Transform[] targetTransformBomb;
    public Transform[] newTarget;
    public GameObject bigBombLaserPrefab;
    public GameObject bombLaserPrefab;
    public float bombSpeed = 10f;

    private bool isSkillSequenceActive = false;
    private MoveDamagePlayer damage;
    private Boss5Health health;

    private BombBoss5Pool bombBossPool;

    private Vector3 originalPosition;
    private Vector3 originalPositionSkillRandom;
    private Vector3 lastBossPosition;
    private bool isSpawn;
    private bool isShaking;

    private Animator anim;
    
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

    void Start()
    {
        // Object Pooling
        bombBossPool = FindFirstObjectByType<BombBoss5Pool>();
        // Get Component
        health = GetComponent<Boss5Health>();
        damage = GetComponentInChildren<MoveDamagePlayer>();
        BossTrans = transform;
        rb = GetComponent<Rigidbody2D>();
        floorCheck = FindAnyObjectByType<FloorCheck>();
        sideManager = FindAnyObjectByType<SideManager>();
        SpamPointsLeft = SpawnPointCheck.Instance.SpamPointsLeft;
        SpamPointsRight = SpawnPointCheck.Instance.SpamPointsRight;
        wayPoints = MoveBoomSkill.Instance.wayPoints;
        targetTransformBomb = FireBoomTransfrom.Instance.targetTransformBomb;
        newTarget = FireBoomTransfrom.Instance.newTarget;
        // Transform Player
        GameObject Player = GameObject.FindGameObjectWithTag("Player");
        playerTrans = Player.transform;
        defaultPosition = transform.position;

        anim = GetComponentInChildren<Animator>();

        anim.SetBool("Idle", true);
    }

    private void Update()
    {
        if (skullSkillTimer > 0)
        {
            skullSkillTimer -= Time.deltaTime;
        }

        if (canMove)
        {
            MoveObject();
        }
        //if (Input.GetKeyDown(KeyCode.P))
        //{
        //    StartCoroutine(MoveThroughSkill());
        //}

        CheckHealth();
    }

    private IEnumerator MoveBossToTarget()
    {
        Vector3 startPosition = new Vector3(0.4f, 15.22f, transform.position.z);

        Vector3 targetPosition = new Vector3(0.4f, 3.5f, transform.position.z);

        float moveDuration = 3.5f;
        float elapsedTime = 0f;

        while (elapsedTime < moveDuration)
        {
            transform.position = Vector3.Lerp(
                startPosition,
                targetPosition,
                elapsedTime / moveDuration
            );
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition;
        originalPositionSkillRandom = targetPosition;
        lastBossPosition = targetPosition;

        isSpawn = true;

        StartCoroutine(SkillSequence());
    }

    public void Active()
    {
        StartCoroutine(MoveBossToTarget());
    }

    private IEnumerator SkillSequence()
    {
        yield return new WaitForSeconds(2f);

        while (isSpawn)
        {
            if (!isSkillSequenceActive)
            {
                isSkillSequenceActive = true;

                yield return StartCoroutine(SkullSkill());
                yield return new WaitForSeconds(1.5f);

                yield return StartCoroutine(TeleportSkill());
                yield return new WaitForSeconds(1.5f);

                yield return StartCoroutine(MoveThroughSkill());
                yield return new WaitForSeconds(1.5f);

                yield return StartCoroutine(BombSkill());
                yield return new WaitForSeconds(1.5f);

                isSkillSequenceActive = false;
            }
            yield return null;
        }
    }

    private IEnumerator SkullSkill()
    {
        anim.SetTrigger("Summon");
        yield return new WaitForSeconds(1.5f);

        Debug.Log("Executing Skull Skill...");
        if (skullSkillTimer <= 0)
        {
            SummonSkullProjectile();
            skullSkillTimer = skullSkillCooldown;
            isSkullSkillActive = true;
        }
        while (isSkullSkillActive)
        {
            yield return null;
        }
        Debug.Log("Skull Skill Completed.");
    }

    private IEnumerator TeleportSkill()
    {
        anim.SetTrigger("Teleport");
        yield return new WaitForSeconds(1.3f);

        Debug.Log("Executing Teleport Skill...");
        yield return StartCoroutine(TeleportMultipleTimes(5, 2f));
        yield return new WaitForSeconds(0.5f);
        Debug.Log("Teleport Skill Completed.");
    }

    private IEnumerator MoveThroughSkill()
    {
        TeleportDefault();
        Debug.Log("Executing MoveThrough Skill...");
        if (MoveBoomSkill.Instance.wayPoints.Length > 0)
        {
            yield return StartCoroutine(MoveThroughWayPoints());
        }
        else
        {
            Debug.LogError("Waypoints array is empty!");
        }
        Debug.Log("MoveThrough Skill Completed.");
    }

    private IEnumerator BombSkill()
    {
        Debug.Log("Executing Bomb Skill...");
        yield return StartCoroutine(BombSKill());
        Debug.Log("Bomb Skill Completed.");
    }

    private void CheckHealth()
    {
        if (health != null)
        {
            if (health.currentHealth.Value <= 0)
            {
                GameObject[] lasers = GameObject.FindGameObjectsWithTag("Laser");

                foreach (GameObject laser in lasers)
                {
                    laser.GetComponent<NetworkObject>().Despawn(true);
                    // Destroy(laser);
                }

                anim.SetTrigger("Death");
                StopAllCoroutines();
            }
        }
    }

    public void CheckPlayer()
    {
        string currentFloor = floorCheck?.CurrentFloor ?? "Unknown";
        Transform leftTransform = floorCheck?.CurrentLeftFloor;
        Transform rightTransform = floorCheck?.CurrentRightFloor;
        Debug.Log($"Player is at {currentFloor}");

        if (leftTransform != null && rightTransform != null)
        {
            Debug.Log(
                $"LeftFloor: {leftTransform.position}, RightFloor: {rightTransform.position}"
            );
        }

        bool isOnLeft = sideManager?.IsOnLeft ?? false;
        bool isOnRight = sideManager?.IsOnRight ?? false;

        if (isOnLeft)
        {
            Debug.Log("Player is on the Left side.");
        }
        else if (isOnRight)
        {
            Debug.Log("Player is on the Right side.");
        }
        else
        {
            Debug.Log("Player position unknown.");
        }
    }

    private void TeleportDefault()
    {
        BossTrans.position = defaultPosition;
    }

    private IEnumerator TeleportMultipleTimes(int repeatCount, float delayBetweenTeleports)
    {
        for (int i = 0; i < repeatCount; i++)
        {
            yield return new WaitForSeconds(0.5f);

            Transform leftTransform = floorCheck?.CurrentLeftFloor;
            Transform rightTransform = floorCheck?.CurrentRightFloor;

            if (sideManager?.IsOnLeft ?? false && rightTransform != null)
            {
                Debug.Log($"Teleporting Boss to the Right Floor... Iteration: {i + 1}");
                BossTrans.position = rightTransform.position;
                StartCoroutine(SummonAfterDelay(rightTransform, Skill1Right, false));
            }
            else if (sideManager?.IsOnRight ?? false && leftTransform != null)
            {
                Debug.Log($"Teleporting Boss to the Left Floor... Iteration: {i + 1}");
                BossTrans.position = leftTransform.position;
                StartCoroutine(SummonAfterDelay(leftTransform, Skill1Left, true));
            }
            else
            {
                Debug.Log(
                    $"Cannot teleport. Player position unknown or target floor is missing. Iteration: {i + 1}"
                );
            }

            if (i < repeatCount - 1)
            {
                yield return new WaitForSeconds(delayBetweenTeleports);
            }
        }
    }

    private IEnumerator SummonAfterDelay(
        Transform targetFloor,
        GameObject skillPrefab,
        bool movingLeft
    )
    {
        anim.SetTrigger("Hand");

        yield return new WaitForSeconds(0.5f);

        float spawnX = movingLeft ? -10f : 10f;
        Vector3 spawnPosition = targetFloor.position + new Vector3(spawnX, 0.2f, 0);

        summonedObject = Instantiate(skillPrefab, spawnPosition, Quaternion.identity);
        summonedObject.GetComponent<NetworkObject>().Spawn();
        targetTransform = (movingLeft) ? floorCheck.CurrentRightFloor : floorCheck.CurrentLeftFloor;
        isMovingLeft = movingLeft;
        canMove = false;

        StartCoroutine(StartMovementAfterDelay());
    }

    private void SummonObject()
    {
        Transform leftTransform = floorCheck?.CurrentLeftFloor;
        Transform rightTransform = floorCheck?.CurrentRightFloor;
        Vector3 yOffset = new Vector3(0f, 0.2f, 0);

        if (summonedObject != null)
        {
            Destroy(summonedObject);
        }

        if (Skill1Left == null || Skill1Right == null)
        {
            Debug.LogError("No prefab assigned for summoning!");
            return;
        }

        if (sideManager?.IsOnLeft ?? false && rightTransform != null)
        {
            Debug.Log("Summoning object on Right Floor...");
            isMovingLeft = false;
            spawnPosition = rightTransform.position + yOffset;
            targetTransform = leftTransform;
            summonedObject = Instantiate(Skill1Right, spawnPosition, Quaternion.identity);
            summonedObject.GetComponent<NetworkObject>().Spawn();
        }
        else if (sideManager?.IsOnRight ?? false && leftTransform != null)
        {
            Debug.Log("Summoning object on Left Floor...");
            isMovingLeft = true;
            spawnPosition = leftTransform.position + yOffset;
            targetTransform = rightTransform;
            summonedObject = Instantiate(Skill1Left, spawnPosition, Quaternion.identity);
            summonedObject.GetComponent<NetworkObject>().Spawn();
        }
        else
        {
            Debug.Log("Cannot summon. Player position unknown or target floor is missing.");
            return;
        }
        canMove = false;
        StartCoroutine(StartMovementAfterDelay());
    }

    private IEnumerator StartMovementAfterDelay()
    {
        yield return new WaitForSeconds(0.1f);
        isMovingForward = true;
        canMove = true;
    }

    private void MoveObject()
    {
        if (summonedObject == null || targetTransform == null)
            return;

        if (isMovingForward)
        {
            float moveDirection = isMovingLeft ? 1 : -1;
            Vector3 targetPosition = targetTransform.position;
            float finalTargetX = targetPosition.x + (moveDirection * targetOffset);

            if (Mathf.Abs(summonedObject.transform.position.x - finalTargetX) > 2.5f)
            {
                summonedObject.transform.Translate(
                    Vector3.right * moveDirection * moveSpeed * Time.deltaTime
                );
            }
            else
            {
                if (!isShaking)
                {
                    isShaking = true; 
                    CameraShake.Instance.StartShake(0.1f, 1.5f, 1f, 5f);
                    StartCoroutine(ResetShakeState());
                }
                StartCoroutine(WaitAndMoveBack());
            }
        }
        else
        {
            float moveDirection = isMovingLeft ? -1 : 1;
            float finalSpawnX = spawnPosition.x + (moveDirection * targetOffset);

            if (Mathf.Abs(summonedObject.transform.position.x - finalSpawnX) > 0.01f)
            {
                summonedObject.transform.Translate(
                    Vector3.right * moveDirection * moveSpeed * Time.deltaTime
                );
                StartCoroutine(DespawnAfterDelay(summonedObject, 1f));
                // Destroy(summonedObject, 1f);
            }
            else
            {
                summonedObject.GetComponent<NetworkObject>().Despawn(true);
                // Destroy(summonedObject);
            }
        }
    }

    private IEnumerator ResetShakeState()
    {
        yield return new WaitForSeconds(0.2f);
        isShaking = false;
    }

    private IEnumerator WaitAndMoveBack()
    {
        yield return new WaitForSeconds(0.5f);

        isMovingForward = false;
        canMove = true;
    }

    private void SummonSkullProjectile()
    {
        foreach (var skull in orbitingSkulls)
        {
            if (skull != null && skull.TryGetComponent(out NetworkObject no))
            {
                no.Despawn(true);
            }
            // Destroy(skull);
        }
        orbitingSkulls.Clear();

        float angleStep = 360f / numberOfSkulls;
        numberOfSkulls = Random.Range(5, 10);
        for (int i = 0; i < numberOfSkulls; i++)
        {
            float angle = i * angleStep;
            Vector3 spawnPos =
                transform.position + Quaternion.Euler(0, 0, angle) * Vector3.right * orbitRadius;
            GameObject skull = Instantiate(skullPrefab, spawnPos, Quaternion.identity);
            skull.GetComponent<NetworkObject>().Spawn();
            orbitingSkulls.Add(skull);
        }

        orbitTimer = 0f;
        StartCoroutine(SkullOrbitAndShoot());
    }

    private IEnumerator SkullOrbitAndShoot()
    {
        while (orbitTimer < orbitDuration)
        {
            orbitTimer += Time.deltaTime;
            float currentAngle = orbitSpeed * Time.time;

            for (int i = 0; i < orbitingSkulls.Count; i++)
            {
                if (orbitingSkulls[i] == null)
                    continue;

                float angle = currentAngle + (i * (360f / numberOfSkulls));
                Vector3 offset = Quaternion.Euler(0, 0, angle) * Vector3.right * orbitRadius;
                orbitingSkulls[i].transform.position = transform.position + offset;
            }
            yield return null;
        }

        if (playerTrans != null)
        {
            foreach (var skull in orbitingSkulls)
            {
                if (skull == null)
                    continue;

                Vector2 direction = (playerTrans.position - skull.transform.position).normalized;

                Rigidbody2D rb = skull.GetComponent<Rigidbody2D>();
                if (rb == null)
                {
                    rb = skull.AddComponent<Rigidbody2D>();
                }

                rb.linearVelocity = direction * projectileSpeed;
                StartCoroutine(DespawnAfterDelay(skull, 3f));
                // Destroy(skull, 3f);

                yield return new WaitForSeconds(1f);
            }
        }

        isSkullSkillActive = false;
    }

    private IEnumerator DespawnAfterDelay(GameObject go, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (go == null)
            yield break;

        if (!go.TryGetComponent(out NetworkObject netObj))
        {
            Destroy(go);
            yield break;
        }

        if (netObj.IsSpawned)
        {
            netObj.Despawn(true);
        }
        else
        {
            Destroy(go);
        }
    }

    private IEnumerator SpawnManyObject()
    {
        foreach (var obj in spawnedObjects)
        {
            if (obj != null)
                Destroy(obj);
        }
        spawnedObjects.Clear();

        GameObject[] spawnPoints;
        GameObject prefabToSpawn;

        if (sideManager?.IsOnLeft ?? false)
        {
            spawnPoints = SpawnPointCheck.Instance.SpamPointsLeft;
            prefabToSpawn = Skill1Right;
        }
        else
        {
            spawnPoints = SpawnPointCheck.Instance.SpamPointsLeft;
            prefabToSpawn = Skill1Left;
        }

        // Spawn tại mỗi điểm với delay
        foreach (var point in spawnPoints)
        {
            if (point != null)
            {
                GameObject spawnedObj = Instantiate(
                    prefabToSpawn,
                    point.transform.position,
                    Quaternion.identity
                );
                spawnedObjects.Add(spawnedObj);
                yield return new WaitForSeconds(spamDelay);
            }
        }
    }

    private IEnumerator MoveThroughWayPoints()
    {
        anim.SetBool("Move", true);
        anim.SetBool("Idle", false);

        damage.SetCanDamage(true);
        yield return new WaitForSeconds(1f);

        while (currentWaypointIndex < MoveBoomSkill.Instance.wayPoints.Length)
        {
            Transform target = MoveBoomSkill.Instance.wayPoints[currentWaypointIndex];
            yield return StartCoroutine(MoveToTarget(target));

            yield return new WaitForSeconds(1f);

            currentWaypointIndex++;
        }

        yield return StartCoroutine(MoveRight());
        damage.SetCanDamage(false);
    }

    private IEnumerator MoveToTarget(Transform target)
    {
        Vector3 targetPosition = target.position;
        targetPosition.y += 1f;

        while (Vector3.Distance(transform.position, targetPosition) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                moveSpeedBomb * Time.deltaTime
            );
            yield return null;
        }

        transform.position = targetPosition;
        SpawnBoomAtPosition(transform.position);
    }

    private IEnumerator MoveRight()
    {
        Vector3 startPosition = transform.position;
        Vector3 endPosition = startPosition + new Vector3(20f, 0, 0);

        float elapsedTime = 0f;

        while (elapsedTime < moveRightDuration)
        {
            transform.position = Vector3.Lerp(
                startPosition,
                endPosition,
                elapsedTime / moveRightDuration
            );
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = endPosition;
        currentWaypointIndex = 0;

        Vector3 highPosition = defaultPosition + new Vector3(0f, 15f, 0f);

        transform.position = highPosition;

        anim.SetBool("Move", false);
        anim.SetBool("Idle", true);

        yield return StartCoroutine(MoveToTarget(lastBossPosition));
    }

    private IEnumerator MoveToTarget(Vector3 targetPosition)
    {
        while (Vector3.Distance(transform.position, targetPosition) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                5f * Time.deltaTime
            );
            yield return null;
        }
        transform.position = targetPosition;
    }

    private void SpawnBoomAtPosition(Vector3 position)
    {
        if (boomb != null)
        { // Tạo instance
            GameObject boomInstance = Instantiate(boomb, position, Quaternion.identity);

            // Lấy component NetworkObject
            NetworkObject networkObject = boomInstance.GetComponent<NetworkObject>();
            if (networkObject != null)
            {
                // Spawn object trên mạng
                networkObject.Spawn();
                // Lấy component BombLaser và gọi hàm
                BombLaser bombLaser = boomInstance.GetComponent<BombLaser>();
                if (bombLaser != null)
                {
                    // Gọi hàm qua RPC để đảm bảo chạy trên tất cả clients
                    StartCoroutine(bombLaser.WaitForExplode());
                    // Gọi hàm qua MusicManager để gọi âm thanh khi Skill xuất hiện trên Scene
                    MusicManager.instance.PlaySoundEffect("Laser_Skill");
                    
                }
                else
                {
                    Debug.LogError("Boom prefab is missing BombLaser component!");
                }
            }
            else
            {
                Debug.LogError("Boom prefab is missing NetworkObject component!");
                Destroy(boomInstance);
            }
            StartCoroutine(DespawnAfterDelay(boomInstance, 3f));
        }
        else
        {
            Debug.LogWarning("Boom prefab is not assigned in the Inspector!");
        }
    }

    private IEnumerator BombSKill()
    {
        for (int i = 0; i < FireBoomTransfrom.Instance.targetTransformBomb.Length; i++)
        {
            Transform target = FireBoomTransfrom.Instance.targetTransformBomb[i];
            if (target != null)
            {
                yield return StartCoroutine(MoveAndShootBigBomb(target));
                yield return new WaitForSeconds(2f);
            }
        }

        yield return new WaitForSeconds(1f);

        List<int> selectedIndices = new List<int>();
        while (selectedIndices.Count < 10)
        {
            int randomIndex = Random.Range(0, FireBoomTransfrom.Instance.newTarget.Length);
            if (!selectedIndices.Contains(randomIndex))
            {
                selectedIndices.Add(randomIndex);
                Transform randomTarget = FireBoomTransfrom.Instance.newTarget[randomIndex];
                if (randomTarget != null)
                {
                    StartCoroutine(MoveAndShootBomb(randomTarget));
                }
            }
        }

        health.SetCanBeDamaged(true);
        yield return new WaitForSeconds(5.5f);
        health.SetCanBeDamaged(false);
    }

    private IEnumerator MoveAndShootBigBomb(Transform target)
    {
        if (bigbombPrefab != null)
        {
            GameObject bigBomb = Instantiate(
                bigbombPrefab,
                transform.position,
                Quaternion.identity
            );
            bigBomb.GetComponent<NetworkObject>().Spawn();
            while (Vector3.Distance(bigBomb.transform.position, target.position) > 1f)
            {
                bigBomb.transform.position = Vector3.MoveTowards(
                    bigBomb.transform.position,
                    target.position,
                    bombSpeed * Time.deltaTime
                );
                yield return null;
            }

            yield return new WaitForSeconds(0.3f);
            bigBomb.GetComponent<NetworkObject>().Despawn(true);
            // Destroy(bigBomb);
            if (bigBombLaserPrefab != null)
            {
                GameObject bigBombLaser = Instantiate(
                    bigBombLaserPrefab,
                    bigBomb.transform.position,
                    Quaternion.identity
                );

                LineRenderer laserLine = bigBombLaser.GetComponentInChildren<LineRenderer>();
                laserLine.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
                Vector3 newLaserPosition = laserLine.transform.position;
                bigBombLaser.GetComponent<NetworkObject>().Spawn();
                newLaserPosition.y += 0.5f;
                laserLine.transform.position = newLaserPosition;
                if (!isShaking)
                {
                    isShaking = true;
                    CameraShake.Instance.StartShake(0.1f, 1f, 0.5f, 3f);
                    StartCoroutine(ResetShakeState());
                }
                StartCoroutine(DespawnAfterDelay(bigBombLaser, 1.5f));
                // Destroy(bigBombLaser, 1.5f);
            }
        }
        else
        {
            Debug.LogWarning("Big Bomb prefab is not assigned!");
        }
    }

    private IEnumerator MoveAndShootBomb(Transform target)
    {
        if (bombBossPool != null)
        {
            GameObject bomb = bombBossPool.GetBomb(target.position);

            bomb.transform.position = transform.position;

            while (Vector3.Distance(bomb.transform.position, target.position) > 0.5f)
            {
                bomb.transform.position = Vector3.MoveTowards(
                    bomb.transform.position,
                    target.position,
                    bombSpeed * Time.deltaTime
                );
                yield return null;
            }
            yield return new WaitForSeconds(0.3f);

            bombBossPool.ReturnBomb(bomb);

            if (bombLaserPrefab != null)
            {
                GameObject bombLaser = Instantiate(
                    bombLaserPrefab,
                    bomb.transform.position,
                    Quaternion.identity
                );
                bombLaser.GetComponent<NetworkObject>().Spawn();
                Vector3 newPosition = bombLaser.transform.position;
                newPosition.y -= 0.5f;
                bombLaser.transform.position = newPosition;
                LineRenderer laserLine = bombLaser.GetComponentInChildren<LineRenderer>();
                if (!isShaking)
                {
                    isShaking = true;
                    CameraShake.Instance.StartShake(0.1f, 1f, 0.5f, 3f);
                    StartCoroutine(ResetShakeState());
                }
                StartCoroutine(DespawnAfterDelay(bombLaser, 1.5f));
                // Destroy(bombLaser, 1.5f);
            }
        }
        else
        {
            Debug.LogWarning("Bomb prefab is not assigned!");
        }
    }
}
