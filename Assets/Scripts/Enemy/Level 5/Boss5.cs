using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using Unity.VisualScripting;
using UnityEngine;

public class Boss5 : MonoBehaviour
{
    [Header("Other")]
    private Rigidbody2D rb;
    [SerializeField] public float groundCheckRadius = 0.2f;
    [SerializeField] public LayerMask wallLayer;
    private Transform BossTrans;
    private Vector3 defaultPosition;

    [Header("Check")]
    [SerializeField] public FloorCheck floorCheck;
    [SerializeField] public SideManager sideManager;

    [Header("Skill 1 Settings")]
    [SerializeField] private GameObject Skill1Left;
    [SerializeField] private GameObject Skill1Right;
    private bool canMove = false;  
    [SerializeField] public float moveSpeed;
    private Transform targetTransform; 
    [SerializeField] private float targetOffset = 1f;

    private GameObject summonedObject;
    private Vector3 spawnPosition;
    private bool isMovingForward = true;
    private bool isMovingLeft;

    [Header("Skill 2 Settings")]
    [SerializeField] public GameObject[] SpamPointsLeft;
    [SerializeField] public GameObject[] SpamPointsRight;
    private List<GameObject> spawnedObjects = new List<GameObject>(); 
    public float spamDelay = 0.1f;
    
    [Header("Skull Projectile Settings")]
    [SerializeField] private GameObject skullPrefab;
    [SerializeField] private float orbitRadius = 2f; 
    [SerializeField] private float orbitSpeed = 180f; 
    [SerializeField] private float projectileSpeed = 15f; 
    [SerializeField] private int numberOfSkulls = 3;
    [SerializeField] private float orbitDuration = 2f; 
    private List<GameObject> orbitingSkulls = new List<GameObject>();
    private float orbitTimer = 0f;
    [SerializeField] private float skullSkillCooldown = 5f;
    private float skullSkillTimer = 0f;
    // Có thể thêm biến để kiểm tra trạng thái
    private bool isSkullSkillActive = false;
    [SerializeField] Transform playerTrans;

    [Header("Move Bomb")]
    public GameObject boomb;
    public Transform[] wayPoints;
    public float moveSpeedBomb = 10f;
    public float moveRightDuration = 3f;
    private int currentWaypointIndex = 0;

    [Header("Fire Bomb")]
    public GameObject bigbombPrefab;
    public GameObject bombPrefab;
    public Transform[] targetTransformBomb;
    public Transform[] newTarget;
    public GameObject bigBombLaserPrefab;
    public GameObject bombLaserPrefab;
    public float bombSpeed = 10f;

    private bool isSkillSequenceActive = false;
    private MoveDamagePlayer damage;
    private Boss5Health health;

    void Start()
    {
        health = GetComponent<Boss5Health>();
        damage = GetComponentInChildren<MoveDamagePlayer>();
        BossTrans = transform;
        rb = GetComponent<Rigidbody2D>();
        GameObject Player = GameObject.FindGameObjectWithTag("Player");
        playerTrans = Player.transform;
        defaultPosition = transform.position;
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

        CheckHealth();
    }

    public void Active()
    {
        StartCoroutine(SkillSequence());
    }

    private IEnumerator SkillSequence()
    {
        yield return new WaitForSeconds(2f);

        while (true)
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
        Debug.Log("Executing Teleport Skill...");
        yield return StartCoroutine(TeleportMultipleTimes(5, 2f));
        yield return new WaitForSeconds(0.5f);
        Debug.Log("Teleport Skill Completed.");
    }

    private IEnumerator MoveThroughSkill()
    {
        TeleportDefault();
        Debug.Log("Executing MoveThrough Skill...");
        if (wayPoints.Length > 0)
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
            if (health.currentHealth <= 0)
            {
                GameObject[] lasers = GameObject.FindGameObjectsWithTag("Laser");

                foreach (GameObject laser in lasers)
                {
                    Destroy(laser);
                }

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
            Debug.Log($"LeftFloor: {leftTransform.position}, RightFloor: {rightTransform.position}");
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
                Debug.Log($"Cannot teleport. Player position unknown or target floor is missing. Iteration: {i + 1}");
            }

            if (i < repeatCount - 1) 
            {
                yield return new WaitForSeconds(delayBetweenTeleports);
            }
        }
    }

    private IEnumerator SummonAfterDelay(Transform targetFloor, GameObject skillPrefab, bool movingLeft)
    {
        yield return new WaitForSeconds(0.5f);

        float spawnX = movingLeft ? -10f : 10f;  
        Vector3 spawnPosition = targetFloor.position + new Vector3(spawnX, 0.2f, 0);  

        summonedObject = Instantiate(skillPrefab, spawnPosition, Quaternion.identity);

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
        }
        else if (sideManager?.IsOnRight ?? false && leftTransform != null)
        {
            Debug.Log("Summoning object on Left Floor...");
            isMovingLeft = true;
            spawnPosition = leftTransform.position + yOffset;
            targetTransform = rightTransform;
            summonedObject = Instantiate(Skill1Left, spawnPosition, Quaternion.identity);
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
        if (summonedObject == null || targetTransform == null) return;

        if (isMovingForward)
        {
            float moveDirection = isMovingLeft ? 1 : -1;
            Vector3 targetPosition = targetTransform.position;
            float finalTargetX = targetPosition.x + (moveDirection * targetOffset);

            if (Mathf.Abs(summonedObject.transform.position.x - finalTargetX) > 2.5f)
            {
                summonedObject.transform.Translate(Vector3.right * moveDirection * moveSpeed * Time.deltaTime);
            }
            else
            {
                StartCoroutine(WaitAndMoveBack());
            }
        }
        else
        {
            float moveDirection = isMovingLeft ? -1 : 1;
            float finalSpawnX = spawnPosition.x + (moveDirection * targetOffset);

            if (Mathf.Abs(summonedObject.transform.position.x - finalSpawnX) > 0.01f)
            {
                summonedObject.transform.Translate(Vector3.right * moveDirection * moveSpeed * Time.deltaTime);
            }
            else
            {
                Destroy(summonedObject);
            }
        }
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
            if (skull != null)
                Destroy(skull);
        }
        orbitingSkulls.Clear();

        float angleStep = 360f / numberOfSkulls;
        numberOfSkulls = Random.Range(5, 10);
        for (int i = 0; i < numberOfSkulls; i++)
        {
            float angle = i * angleStep;
            Vector3 spawnPos = transform.position + Quaternion.Euler(0, 0, angle) * Vector3.right * orbitRadius;
            GameObject skull = Instantiate(skullPrefab, spawnPos, Quaternion.identity);
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
                if (orbitingSkulls[i] == null) continue;

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
                if (skull == null) continue;

                Vector2 direction = (playerTrans.position - skull.transform.position).normalized;

                Rigidbody2D rb = skull.GetComponent<Rigidbody2D>();
                if (rb == null)
                {
                    rb = skull.AddComponent<Rigidbody2D>();
                }

                rb.linearVelocity = direction * projectileSpeed;

                Destroy(skull, 3f);

                yield return new WaitForSeconds(1f);
            }
        }

        isSkullSkillActive = false;
    }

    private IEnumerator SpawnManyObject()
    {
        // Xóa tất cả object cũ nếu có
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
            spawnPoints = SpamPointsRight;
            prefabToSpawn = Skill1Right;
        }
        else
        {
            spawnPoints = SpamPointsLeft;
            prefabToSpawn = Skill1Left;
        }

        // Spawn tại mỗi điểm với delay
        foreach (var point in spawnPoints)
        {
            if (point != null)
            {
                GameObject spawnedObj = Instantiate(prefabToSpawn, point.transform.position, Quaternion.identity);
                spawnedObjects.Add(spawnedObj);
                yield return new WaitForSeconds(spamDelay);
            }
        }
    }

    private IEnumerator MoveThroughWayPoints()
    {
        damage.SetCanDamage(true);
        yield return new WaitForSeconds(1f);

        while (currentWaypointIndex < wayPoints.Length)
        {
            Transform target = wayPoints[currentWaypointIndex];
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
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeedBomb * Time.deltaTime);
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
            transform.position = Vector3.Lerp(startPosition, endPosition, elapsedTime / moveRightDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = endPosition;
        currentWaypointIndex = 0;

        Vector3 highPosition = defaultPosition + new Vector3(0f,15f,0f);    

        transform.position = highPosition;

        yield return StartCoroutine(MoveToTarget(defaultPosition));
    }

    private IEnumerator MoveToTarget(Vector3 targetPosition)
    {
        while (Vector3.Distance(transform.position, targetPosition) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, 5f * Time.deltaTime);
            yield return null;
        }
        transform.position = targetPosition;
    }

    private void SpawnBoomAtPosition(Vector3 position)
    {
        if (boomb != null)
        {
            Instantiate(boomb, position, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning("Boom prefab is not assigned in the Inspector!");
        }
    }

    private IEnumerator BombSKill()
    {
        for (int i = 0; i < targetTransformBomb.Length; i++)
        {
            Transform target = targetTransformBomb[i];
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
            int randomIndex = Random.Range(0, newTarget.Length);
            if (!selectedIndices.Contains(randomIndex))
            {
                selectedIndices.Add(randomIndex);
                Transform randomTarget = newTarget[randomIndex];
                if (randomTarget != null)
                {
                    StartCoroutine(MoveAndShootBomb(randomTarget));
                }
            }
        }

        health.SetCanBeDamaged(true);
        yield return new WaitForSeconds(4f);
        health.SetCanBeDamaged(false);
    }

    private IEnumerator MoveAndShootBigBomb(Transform target)
    {
        if (bigbombPrefab != null)
        {
            GameObject bigBomb = Instantiate(bigbombPrefab, transform.position, Quaternion.identity);
            while (Vector3.Distance(bigBomb.transform.position, target.position) > 1f)
            {
                bigBomb.transform.position = Vector3.MoveTowards(bigBomb.transform.position, target.position, bombSpeed * Time.deltaTime);
                yield return null;
            }
            yield return new WaitForSeconds(0.3f);
            Destroy(bigBomb);
            if (bigBombLaserPrefab != null)
            {
                GameObject bigBombLaser = Instantiate(bigBombLaserPrefab, bigBomb.transform.position, Quaternion.identity);
                LineRenderer laserLine = bigBombLaser.GetComponentInChildren<LineRenderer>();
                laserLine.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
                Vector3 newLaserPosition = laserLine.transform.position;
                newLaserPosition.y -= 0.7f;
                laserLine.transform.position = newLaserPosition;

                Destroy(bigBombLaser, 1.5f);
            }
        }
        else
        {
            Debug.LogWarning("Big Bomb prefab is not assigned!");
        }
    }

    private IEnumerator MoveAndShootBomb(Transform target)
    {
        if (bombPrefab != null)
        {
            GameObject bomb = Instantiate(bombPrefab, transform.position, Quaternion.identity);
            while (Vector3.Distance(bomb.transform.position, target.position) > 0.5f)
            {
                bomb.transform.position = Vector3.MoveTowards(bomb.transform.position, target.position, bombSpeed * Time.deltaTime);
                yield return null;
            }
            yield return new WaitForSeconds(0.3f);
            Destroy(bomb);
            if (bombLaserPrefab != null)
            {
                GameObject bombLaser = Instantiate(bombLaserPrefab, bomb.transform.position, Quaternion.identity);
                Vector3 newPosition = bombLaser.transform.position;
                newPosition.y -= 0.5f; 
                bombLaser.transform.position = newPosition;

                LineRenderer laserLine = bombLaser.GetComponentInChildren<LineRenderer>();
                Destroy(bombLaser, 1.5f);
            }
        }
        else
        {
            Debug.LogWarning("Bomb prefab is not assigned!");
        }
    }
}