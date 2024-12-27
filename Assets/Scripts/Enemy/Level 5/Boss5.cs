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
    private bool canMove = false;  // Thêm biến để kiểm soát việc di chuyển
    [SerializeField] public float moveSpeed;
    private Transform targetTransform; // Thêm biến để lưu transform đích
    [SerializeField] private float targetOffset = 1f;

    private GameObject summonedObject;
    private Vector3 spawnPosition;
    private bool isMovingForward = true;
    private bool isMovingLeft;

    [Header("Skill 2 Settings")]
    [SerializeField] public GameObject[] SpamPointsLeft;
    [SerializeField] public GameObject[] SpamPointsRight;
    private List<GameObject> spawnedObjects = new List<GameObject>(); // Để track các object đã spawn
    public float spamDelay = 0.1f; // Delay giữa mỗi lần spawn
    


    [Header("Skull Projectile Settings")]
    [SerializeField] private GameObject skullPrefab; // Prefab của skull
    [SerializeField] private float orbitRadius = 2f; // Bán kính xoay quanh boss
    [SerializeField] private float orbitSpeed = 180f; // Tốc độ xoay (độ/giây)
    [SerializeField] private float projectileSpeed = 15f; // Tốc độ bắn
    [SerializeField] private int numberOfSkulls = 3; // Số lượng skull
    [SerializeField] private float orbitDuration = 2f; // Thời gian xoay trước khi bắn
    private List<GameObject> orbitingSkulls = new List<GameObject>();
    private float orbitTimer = 0f;
    [SerializeField] private float skullSkillCooldown = 5f;
    private float skullSkillTimer = 0f;
    // Có thể thêm biến để kiểm tra trạng thái
    private bool isSkullSkillActive = false;


    [SerializeField] Transform playerTrans;
    void Start()
    {
        BossTrans = transform;
        rb = GetComponent<Rigidbody2D>();
        GameObject Player = GameObject.FindGameObjectWithTag("Player");
        playerTrans = Player.transform;
        defaultPosition = transform.position;
    }

    private void Update()
    {
        // Xử lý cooldown
        if (skullSkillTimer > 0)
        {
            skullSkillTimer -= Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.Space))
        {
            TeleportDefault();
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            Teleport();
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            SummonObject();
        }
        if (Input.GetKeyDown(KeyCode.F) && skullSkillTimer <= 0)
        {
            SummonSkullProjectile();
            skullSkillTimer = skullSkillCooldown; // Reset cooldown
            isSkullSkillActive = true;
        }
        if (Input.GetKeyDown(KeyCode.D))  // Sử dụng phím D cho skill spam
        {
            StartCoroutine(SpawnManyObject());
        }
        if (canMove) 
        {
            MoveObject();
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

    private void Teleport()
    {
        Transform leftTransform = floorCheck?.CurrentLeftFloor;
        Transform rightTransform = floorCheck?.CurrentRightFloor;

        if (sideManager?.IsOnLeft ?? false && rightTransform != null)
        {
            Debug.Log("Teleporting Boss to the Right Floor...");
            BossTrans.position = rightTransform.position;
        }
        else if (sideManager?.IsOnRight ?? false && leftTransform != null)
        {
            Debug.Log("Teleporting Boss to the Left Floor...");
            BossTrans.position = leftTransform.position;
        }
        else
        {
            Debug.Log("Cannot teleport. Player position unknown or target floor is missing.");
        }
    }

    private void TeleportDefault()
    {
        BossTrans.position = defaultPosition;
    }
    private void SummonObject()
    {
        Transform leftTransform = floorCheck?.CurrentLeftFloor;
        Transform rightTransform = floorCheck?.CurrentRightFloor;
        Vector3 yOffset = new Vector3(0, 0.5f, 0);

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
            targetTransform = leftTransform; // Set đích là leftTransform
            summonedObject = Instantiate(Skill1Right, spawnPosition, Quaternion.identity);
        }
        else if (sideManager?.IsOnRight ?? false && leftTransform != null)
        {
            Debug.Log("Summoning object on Left Floor...");
            isMovingLeft = true;
            spawnPosition = leftTransform.position + yOffset;
            targetTransform = rightTransform; // Set đích là rightTransform
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
    private IEnumerator SpawnManyObject()
    {
        // Xóa tất cả object cũ nếu có
        foreach (var obj in spawnedObjects)
        {
            if (obj != null)
                Destroy(obj);
        }
        spawnedObjects.Clear();

        // Xác định mảng points để spawn dựa vào vị trí người chơi
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
    private IEnumerator StartMovementAfterDelay()
    {
        yield return new WaitForSeconds(1f);
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
            float finalTargetX = targetPosition.x + (moveDirection * targetOffset); // Thêm offset theo hướng di chuyển

            if (Mathf.Abs(summonedObject.transform.position.x - finalTargetX) > 0.1f)
            {
                summonedObject.transform.Translate(Vector3.right * moveDirection * moveSpeed * Time.deltaTime);
            }
            else
            {
                isMovingForward = false;
            }
        }
        else
        {
            float moveDirection = isMovingLeft ? -1 : 1;
            float finalSpawnX = spawnPosition.x + (moveDirection * targetOffset); // Thêm offset cho vị trí quay về

            if (Mathf.Abs(summonedObject.transform.position.x - finalSpawnX) > 0.1f)
            {
                summonedObject.transform.Translate(Vector3.right * moveDirection * moveSpeed * Time.deltaTime);
            }
            else
            {
                Destroy(summonedObject);
            }
        }
    }

    private void SummonSkullProjectile()
    {
        // Xóa skulls cũ nếu có
        foreach (var skull in orbitingSkulls)
        {
            if (skull != null)
                Destroy(skull);
        }
        orbitingSkulls.Clear();

        // Tạo skulls mới
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
        // Xoay quanh boss
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

                // Tính hướng đến player
                Vector2 direction = (playerTrans.position - skull.transform.position).normalized;

                // Thêm component Rigidbody2D nếu chưa có
                Rigidbody2D rb = skull.GetComponent<Rigidbody2D>();
                if (rb == null)
                {
                    rb = skull.AddComponent<Rigidbody2D>();
                }

                // Bắn về phía player
                rb.linearVelocity = direction * projectileSpeed;

                // Tự hủy sau 5 giây
                Destroy(skull, 5f);
            }
        }
    }
}