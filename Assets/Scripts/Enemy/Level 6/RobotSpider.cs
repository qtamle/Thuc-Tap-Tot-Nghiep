using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class RobotSpider : NetworkBehaviour
{
    [Header("Raycast Check")]
    public LayerMask wallCheck;
    public float rayDistance;
    public Transform wallCheckTransform;
    public bool hasChecked = false;

    [Header("Move Zig Zag")]
    public float moveSpeed = 0.1f;
    public float zigZagDistance = 3f;
    public float zigZagHeight = 1f;
    public float pathUpdateInterval = 1f;
    public float stopDuration = 1.5f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundRadius = 0.5f;
    public LayerMask groundLayer;

    private Vector3 startingPosition;
    private bool goingRight = true;
    private bool moving = true;

    private List<Vector3> pathPoints = new List<Vector3>();
    private int currentTargetIndex = 0;

    private Rigidbody2D rb;
    private bool isFlipped = false;

    private bool isRaycastUsed = false;
    private bool isZigZagHeightPositive = false;
    private bool hasRaycastCollided = false;

    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        animator.SetBool("Idle", true);

        rb.bodyType = RigidbodyType2D.Dynamic;
        StartCoroutine(HandleRigidbodyAndMovement());
    }

    private IEnumerator HandleRigidbodyAndMovement()
    {
        yield return new WaitUntil(() => IsGrounded());

        if (IsGrounded())
        {
            startingPosition = transform.position;
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;

            CalculateZigZagPath();

            yield return new WaitForSeconds(1f);

            StartCoroutine(MoveInZigZag());
        }
    }

    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundLayer);
    }

    private void Flip()
    {
        isFlipped = !isFlipped;
        transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
        CalculateZigZagPath();
    }

    private void FlipCharacter()
    {
        Vector3 localScale = transform.localScale;
        localScale.x *= -1;
        transform.localScale = localScale;
    }

    private void CalculateZigZagPath()
    {
        Vector3 currentPosition = startingPosition;
        bool zigZagDirection = true;

        pathPoints.Clear();
        pathPoints.Add(currentPosition);

        for (int i = 0; i < 3; i++)
        {
            float directionMultiplier = isFlipped ? -1f : 1f;

            float heightDirection;
            if (isZigZagHeightPositive)
            {
                heightDirection = isFlipped ? -Mathf.Abs(zigZagHeight) : Mathf.Abs(zigZagHeight);
            }
            else
            {
                heightDirection = isFlipped ? -zigZagHeight : zigZagHeight;
            }

            Vector3 targetPosition = currentPosition +
                                     new Vector3(zigZagDirection ? zigZagDistance : -zigZagDistance, heightDirection, 0) * directionMultiplier;

            pathPoints.Add(targetPosition);
            currentPosition = targetPosition;
            zigZagDirection = !zigZagDirection; 
        }
    }


    private IEnumerator MoveInZigZag()
    {
        while (true)
        {
            if (currentTargetIndex < pathPoints.Count)
            {
                Vector3 targetPosition = pathPoints[currentTargetIndex];

                if ((targetPosition.x > transform.position.x && !isFlipped) ||
                (targetPosition.x < transform.position.x && isFlipped))
                {
                    FlipCharacter();
                }

                animator.SetBool("Move", true);
                animator.SetBool("Idle", false);

                while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
                {
                    transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
                    yield return null;
                }

                transform.position = targetPosition;

                animator.SetBool("Move", false);
                animator.SetBool("Idle", true);

                yield return new WaitForSeconds(stopDuration);

                currentTargetIndex++;
            }
            else
            {
                pathPoints.Reverse();
                currentTargetIndex = 0;
            }
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(groundCheck.position, groundRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(wallCheckTransform.position, wallCheckTransform.position + wallCheckTransform.right * rayDistance);

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + transform.up * 5f);

        if (pathPoints.Count == 0) return;

        Gizmos.color = Color.green;
        Vector3 currentPosition = transform.position;

        for (int i = 0; i < pathPoints.Count; i++)
        {
            Vector3 targetPosition = pathPoints[i];
            Gizmos.DrawLine(currentPosition, targetPosition);
            Gizmos.DrawSphere(targetPosition, 0.1f);

            currentPosition = targetPosition;
        }
    }

    void Update()
    {
        if (!hasChecked)
        {
            RaycastHit2D hit = Physics2D.Raycast(wallCheckTransform.position, wallCheckTransform.right, rayDistance, wallCheck);
            if (hit.collider != null)
            {
                Flip();
            }
            hasChecked = true;
        }

        RaycastHit2D hitCollider = Physics2D.Raycast(transform.position, Vector2.up, 5f, groundLayer);

        if (hitCollider.collider != null)
        {
            hasRaycastCollided = true;
            isRaycastUsed = true;  
            isZigZagHeightPositive = true;  
        }
        else
        {
            hasRaycastCollided = false;
            isRaycastUsed = true; 
            isZigZagHeightPositive = false; 
        }

        zigZagHeight = isZigZagHeightPositive ? Mathf.Abs(zigZagHeight) : -Mathf.Abs(zigZagHeight);
    }
}
