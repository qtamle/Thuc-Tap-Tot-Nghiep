using System.Collections;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class Drone : NetworkBehaviour
{
    [Header("Move Settings")]
    public float moveSpeed;
    private bool movingRight = true;

    [Header("Random Move Settings")]
    public Vector2 xMoveRange = new Vector2(-3.84f, 3.73f);
    public Vector2 yMoveRange = new Vector2(4.49f, -4.66f);
    private bool isRandomMoving = false;
    private Vector3 targetPosition;

    [Header("Shoot")]
    public GameObject bulletPrefab;
    public float bulletSpeed;
    private float nextShootTime;

    [Header("Raycast Settings")]
    public LayerMask wallLayer;
    public float rayDistance = 5f;

    private bool initialRaycastUsed = false;

    private Animator droneAnim;

    private void Start()
    {
        droneAnim = GetComponent<Animator>();

        PerformInitialRaycast();
        StartCoroutine(InitialMoveRoutine());
        SetNewRandomTarget();
        SetNextShootTime();
    }

    private void Update()
    {
        if (Time.time >= nextShootTime)
        {
            StartCoroutine(ShootSkillRoutine());
            SetNextShootTime();
        }
        else
        {
            if (isRandomMoving)
            {
                MoveToRandomTarget();
            }
            else
            {
                MoveStraight();
            }
        }
    }

    void MoveStraight()
    {
        transform.Translate(Vector2.right * moveSpeed * Time.deltaTime * (movingRight ? 1 : -1));
    }

    void PerformInitialRaycast()
    {
        if (!initialRaycastUsed)
        {
            RaycastHit2D hit = Physics2D.Raycast(
                transform.position,
                Vector2.right,
                rayDistance,
                wallLayer
            );

            if (hit.collider != null)
            {
                Debug.Log("Initial Raycast: Hit Wall - No Flip.");
            }
            else
            {
                Debug.Log("Initial Raycast: No Wall - Flip performed.");
                Flip();
                initialRaycastUsed = true;
            }
        }
    }

    void MoveToRandomTarget()
    {
        transform.position = Vector2.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        if (transform.position.x < targetPosition.x && !movingRight)
        {
            Flip();
        }
        else if (transform.position.x > targetPosition.x && movingRight)
        {
            Flip();
        }

        if (Vector2.Distance(transform.position, targetPosition) < 0.1f)
        {
            SetNewRandomTarget();
        }
    }

    void SetNewRandomTarget()
    {
        targetPosition = new Vector3(
            Random.Range(xMoveRange.x, xMoveRange.y),
            Random.Range(yMoveRange.y, yMoveRange.x),
            transform.position.z
        );
    }

    void Flip()
    {
        movingRight = !movingRight;
        Vector3 localScale = transform.localScale;
        localScale.x *= -1;
        transform.localScale = localScale;
    }

    IEnumerator InitialMoveRoutine()
    {
        yield return new WaitForSeconds(4f);
        isRandomMoving = true;
        Debug.Log("Switching to random movement.");
    }

    void SetNextShootTime()
    {
        nextShootTime = Time.time + Random.Range(4f, 6f);
    }

    IEnumerator ShootSkillRoutine()
    {
        float originalSpeed = moveSpeed;
        moveSpeed = 0;

        droneAnim.SetBool("Fly", false);
        droneAnim.SetTrigger("Fire");

        yield return new WaitForSeconds(1f);

        ShootCrossPattern();

        droneAnim.SetBool("Idle", true);

        yield return new WaitForSeconds(0.5f);

        moveSpeed = originalSpeed;

        droneAnim.SetBool("Idle", false);
        droneAnim.SetBool("Fly", true);
    }

    void ShootCrossPattern()
    {
        Vector3[] directions = { Vector3.up, Vector3.down, Vector3.left, Vector3.right };

        foreach (Vector3 dir in directions)
        {
            GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
            bullet.GetComponent<NetworkObject>().Spawn();
            NetworkRigidbody2D rb = bullet.GetComponent<NetworkRigidbody2D>();

            if (rb != null)
            {
                rb.Rigidbody2D.linearVelocity = dir * bulletSpeed;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(
            new Vector3((xMoveRange.x + xMoveRange.y) / 2, (yMoveRange.x + yMoveRange.y) / 2, 0),
            new Vector3(xMoveRange.y - xMoveRange.x, yMoveRange.x - yMoveRange.y, 0)
        );

        Gizmos.color = Color.blue;
        Vector2 direction = movingRight ? Vector2.right : Vector2.left;
        Gizmos.DrawLine(
            transform.position,
            transform.position + (Vector3)(direction * rayDistance)
        );
    }
}
