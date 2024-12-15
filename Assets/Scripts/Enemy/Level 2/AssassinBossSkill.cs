using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AssassinBossSkill : MonoBehaviour
{
    private Rigidbody2D rb;
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

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        ThrowTraps(3);

        if (GameObject.FindWithTag("Player") != null)
        {
            playerTransform = GameObject.FindWithTag("Player").transform;
        }
        else
        {
            Debug.LogError("Player not found in the scene! Add a GameObject with the tag 'Player'.");
        }

        if (GameObject.FindWithTag("Player") != null)
        {
            playerTransforms = GameObject.FindWithTag("Player").transform;
        }
    }

    private void Update()
    {
        CheckGround();

        if (Input.GetKeyDown(KeyCode.P))
        {
            CloneAndDash();
        }
    }

    void CheckGround()
    {
        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        }
        else
        {
            Debug.LogWarning("Ground Check Transform is not assigned!");
        }
    }

    private void FlipToPlayer()
    {
        if (playerTransforms == null) return;

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

            StartCoroutine(MoveTrapToTarget(trap, target.position));
        }
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
            trap.transform.position = Vector3.MoveTowards(trap.transform.position, targetPosition, trapSpeed * Time.deltaTime);
            yield return null;
        }

        Debug.Log("Trap reached adjusted target: " + targetPosition);
    }

    void ThrowBombAtPlayer()
    {
        if (playerTransform == null || bombPrefab == null)
        {
            Debug.LogError("Player Transform or Bomb Prefab is missing!");
            return;
        }

        Vector3 targetPosition = playerTransform.position;

        GameObject bomb = Instantiate(bombPrefab, transform.position, Quaternion.identity);
        StartCoroutine(MoveBombToTarget(bomb, targetPosition));
    }

    IEnumerator<WaitForSeconds> MoveBombToTarget(GameObject bomb, Vector3 targetPosition)
    {
        float bombYOffset = -0.4f;
        targetPosition.y += bombYOffset;

        while (bomb != null && Vector3.Distance(bomb.transform.position, targetPosition) > 0.1f)
        {
            bomb.transform.position = Vector3.MoveTowards(bomb.transform.position, targetPosition, bombSpeed * Time.deltaTime);
            yield return null;
        }

        Debug.Log("Bomb reached Player's position: " + targetPosition);

        yield return new WaitForSeconds(2f);

        BombExplosion(bomb);
    }

    void BombExplosion(GameObject bomb)
    {
        Debug.Log("Bomb exploded!");
        CreateBombFragments(bomb.transform.position);
        Destroy(bomb);
    }

    void CreateBombFragments(Vector3 explosionPosition)
    {
        if (bombFragmentPrefab == null)
        {
            Debug.LogError("Bomb Fragment Prefab is missing!");
            return;
        }

        Vector3[] fragmentDirections = new Vector3[]
        {
            new Vector3(1f, 1, 0),   // Đi chéo lên phải
            new Vector3(-1f, 1, 0),  // Đi chéo lên trái
            new Vector3(1f, -1, 0),  // Đi chéo xuống phải
            new Vector3(-1f, -1, 0)  // Đi chéo xuống trái
        };

        foreach (Vector3 direction in fragmentDirections)
        {
            GameObject fragment = Instantiate(bombFragmentPrefab, explosionPosition, Quaternion.identity);

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

        Destroy(fragment);
    }

    public void TeleportToPlayer()
    {
        if (playerTransforms == null)
        {
            Debug.LogError("Player Transform is missing!");
            return;
        }

        StartCoroutine(ExecuteTeleport());
    }

    private IEnumerator ExecuteTeleport()
    {
        if (isTeleporting) yield break;

        isTeleporting = true;

        yield return new WaitForSeconds(1f);

        FlipToPlayer();

        // Xác định vị trí gần Player
        float offsetX = playerTransforms.position.x > transform.position.x ? -teleportOffsetX : teleportOffsetX;
        Vector2 targetPosition = new Vector2(playerTransforms.position.x + offsetX, playerTransforms.position.y);
        targetPosition.y += additionalHeight;

        transform.position = targetPosition;
        rb.gravityScale = 4f;

        Debug.Log("Assassin Boss teleported to: " + targetPosition);

        yield return new WaitForSeconds(0.5f);

        isTeleporting = false;

        FlipToPlayer();

        yield return new WaitForSeconds(1f);

        StartCoroutine(ShootBullets());
    }

    IEnumerator ShootBullets()
    {
        if (bulletPrefab == null && shootingPoint == null)
        {
            yield break;
        }

        Vector3 direction = transform.rotation == Quaternion.identity ? Vector3.right : Vector3.left;

        for (int i = 0; i < 3; i++)
        {
            GameObject bullet = Instantiate(bulletPrefab, shootingPoint.position, Quaternion.identity);
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
        if (clonePrefab == null || dashTargets.Length == 0)
        {
            Debug.LogError("Clone Prefab hoặc Dash Targets là thiếu!");
            return;
        }

        originalPosition = transform.position;

        // Tạo 2 clone
        GameObject clone1 = Instantiate(clonePrefab, transform.position, Quaternion.identity);
        GameObject clone2 = Instantiate(clonePrefab, transform.position, Quaternion.identity);

        GameObject[] allCharacters = new GameObject[] { gameObject, clone1, clone2 };

        int randomIndex1 = Random.Range(0, dashTargets.Length);
        int randomIndex2 = Random.Range(0, dashTargets.Length);

        while (randomIndex1 == randomIndex2)
        {
            randomIndex2 = Random.Range(0, dashTargets.Length);
        }

        Vector3 targetPosition1 = dashTargets[randomIndex1].position;
        Vector3 targetPosition2 = dashTargets[randomIndex2].position;

        float offsetY = 1.2f;
        targetPosition1.y += offsetY;
        targetPosition2.y += offsetY;

        Vector3 assassinTargetPosition = dashTargets[Random.Range(0, dashTargets.Length)].position;
        assassinTargetPosition.y += offsetY;

        // Tìm player bằng Tag và truyền vào CloneDash
        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
        if (player != null)
        {
            // Bắt đầu dịch chuyển các nhân vật
            StartCoroutine(MoveToRandomTargets(allCharacters, assassinTargetPosition, targetPosition1, targetPosition2, player.transform));
        }
        else
        {
            Debug.LogError("Player không tìm thấy!");
        }
    }

    IEnumerator MoveToRandomTargets(GameObject[] characters, Vector3 assassinTargetPosition, Vector3 targetPosition1, Vector3 targetPosition2, Transform player)
    {
        characters[0].transform.position = assassinTargetPosition;
        characters[1].transform.position = targetPosition1;
        characters[2].transform.position = targetPosition2;

        yield return new WaitForSeconds(2f);

        CloneDash cloneDash1 = characters[1].GetComponent<CloneDash>();
        CloneDash cloneDash2 = characters[2].GetComponent<CloneDash>();

        if (cloneDash1 != null)
            cloneDash1.DashTowardsPlayer();

        if (cloneDash2 != null)
            cloneDash2.DashTowardsPlayer();  

        CloneDash playerCloneDash = characters[0].GetComponent<CloneDash>();
        if (playerCloneDash != null)
            playerCloneDash.DashTowardsPlayer(); 

        yield return new WaitForSeconds(3f); 

        Destroy(characters[1]);
        Destroy(characters[2]);

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
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if ((groundLayer.value & (1 << collision.gameObject.layer)) > 0)
        {
            if(isGrounded)
            { rb.gravityScale = 0; }
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

    //private void OnDrawGizmos()
    //{
    //    // Kiểm tra và vẽ Gizmo tia Raycast
    //    if (wallLayer != 0)  // Kiểm tra xem layer có được chỉ định chưa
    //    {
    //        // Vị trí xuất phát của tia Raycast (ví dụ, từ vị trí của đối tượng)
    //        Vector2 rayOrigin = transform.position;

    //        // Chiều dài của tia (ví dụ, 5f)
    //        float rayLength = 5f;

    //        // Màu sắc của Gizmo (màu đỏ để dễ thấy)
    //        Gizmos.color = Color.red;

    //        // Vẽ tia Raycast từ transform.position ra hướng Vector2.right (phải)
    //        Gizmos.DrawLine(rayOrigin, rayOrigin + Vector2.right * rayLength);
    //    }
    //}

}
