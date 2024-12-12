using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    [Header("Jump")]
    public float jumpForce = 5f;
    private Rigidbody2D rb;
    private Vector2 startInputPosition;
    private Vector2 endInputPosition;
    private bool isJumping = false;

    [Header("Movement")]
    public float moveSpeed = 5f;
    private bool canMove = false;
    private float moveDirection = 0f;

    [Header("Check Ground")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.1f;
    public LayerMask groundLayer;
    private bool isGrounded = false;

    [Header("Ground Tag")]
    public string finalFloorTag = "FinalFloor";
    private Collider2D currentGround;
    private Collider2D playerCollider;

    [Header("Wall Check")]
    public LayerMask wallLayer;
    public float wallCheckRadius = 0.2f;
    public Transform wallTransform;

    [Header("Block Layer")]
    public string blockLayerName;
    public string bossLayerName;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();

        int playerLayer = gameObject.layer;
        int blockLayer = LayerMask.NameToLayer(blockLayerName);
        int bossLayer = LayerMask.NameToLayer(bossLayerName);

        if (blockLayer != -1)
        {
            Physics2D.IgnoreLayerCollision(playerLayer, blockLayer, true);
            Physics2D.IgnoreLayerCollision(playerLayer, bossLayer, true);
            Debug.Log($"Đã bỏ qua va chạm giữa layer {LayerMask.LayerToName(playerLayer)} và {blockLayerName}.");
        }
        else
        {
            Debug.LogWarning($"Layer {blockLayerName} không tồn tại.");
        }
    }

    void Update()
    {
        CheckGrounded();

#if UNITY_EDITOR || UNITY_STANDALONE
        DetectMouseSwipe(); // Kiểm tra trên máy tính
#else
        DetectTouchSwipe(); // Kiểm tra trên thiết bị cảm ứng
#endif

        if (!IsTouchingWall())
        {
            MovePlayer();
        }

        FlipPlayer();
    }

    void CheckGrounded()
    {
        currentGround = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        isGrounded = currentGround != null;
        isJumping = !isGrounded;

        if (isGrounded)
        {
            canMove = true;
        }
        else
        {
            canMove = false;
        }
    }
    void DetectTouchSwipe()
    {
        if (Input.touchCount > 0) // Kiểm tra có thao tác cảm ứng
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                // Lưu vị trí bắt đầu chạm
                startInputPosition = touch.position;
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                // Lưu vị trí kết thúc chạm
                endInputPosition = touch.position;

                if (IsSwipeUp(startInputPosition, endInputPosition))
                {
                    Jump();
                }
                else if (IsSwipeDown(startInputPosition, endInputPosition))
                {
                    FallThrough();
                }
                else if (IsSwipeLeft(startInputPosition, endInputPosition))
                {
                    MoveLeft();  
                    FlipPlayer(); 
                }
                else if (IsSwipeRight(startInputPosition, endInputPosition))
                {
                    MoveRight(); 
                    FlipPlayer();
                }
            }
        }
    }

    void DetectMouseSwipe()
    {
        if (Input.GetMouseButtonDown(0))
        {
            startInputPosition = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            endInputPosition = Input.mousePosition;

            if (IsSwipeUp(startInputPosition, endInputPosition))
            {
                Jump();
            }
            else if (IsSwipeDown(startInputPosition, endInputPosition))
            {
                FallThrough();
            }
            else if (IsSwipeLeft(startInputPosition, endInputPosition))
            {
                MoveLeft(); 
                FlipPlayer(); 
            }
            else if (IsSwipeRight(startInputPosition, endInputPosition))
            {
                MoveRight(); 
                FlipPlayer(); 
            }
        }
    }

    bool IsSwipeUp(Vector2 start, Vector2 end)
    {
        float swipeDistance = end.y - start.y;
        float swipeThreshold = 50f;

        bool isSwipeUp = swipeDistance > swipeThreshold && Mathf.Abs(end.x - start.x) < swipeThreshold;
        return isSwipeUp;
    }

    bool IsSwipeDown(Vector2 start, Vector2 end)
    {
        float swipeDistance = start.y - end.y;
        float swipeThreshold = 50f;

        bool isSwipeDown = swipeDistance > swipeThreshold && Mathf.Abs(end.x - start.x) < swipeThreshold;
        return isSwipeDown;
    }

    bool IsSwipeLeft(Vector2 start, Vector2 end)
    {
        float swipeDistance = start.x - end.x;
        float swipeThreshold = 50f;

        bool isSwipeLeft = swipeDistance > swipeThreshold && Mathf.Abs(end.y - start.y) < swipeThreshold;
        return isSwipeLeft;
    }

    bool IsSwipeRight(Vector2 start, Vector2 end)
    {
        float swipeDistance = end.x - start.x;
        float swipeThreshold = 50f;

        bool isSwipeRight = swipeDistance > swipeThreshold && Mathf.Abs(end.y - start.y) < swipeThreshold;
        return isSwipeRight;
    }

    void Jump()
    {
        if (!isGrounded || isJumping) return; 

        isJumping = true;  
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

        PlatformEffector2D platformEffector = currentGround?.GetComponent<PlatformEffector2D>();

        if (platformEffector != null)
        {
            platformEffector.surfaceArc = 180;
        }
    }

    void FallThrough()
    {
        if (isGrounded && currentGround != null && !currentGround.CompareTag(finalFloorTag))
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -jumpForce * 0.3f);

            rb.gravityScale = 4f;

            PlatformEffector2D platformEffector = currentGround?.GetComponent<PlatformEffector2D>();

            if (platformEffector != null)
            {
                platformEffector.surfaceArc = 0; // Cho phép rơi qua

                StartCoroutine(ResetSurfaceArc(platformEffector));
            }
        }
        else if (isGrounded)
        {
            Debug.Log("Không thể rơi xuống, nền này có tag FinalFloor.");
        }
        else
        {
            Debug.Log("Không thể rơi xuống, không đứng trên mặt đất.");
        }
    }

    private IEnumerator ResetSurfaceArc(PlatformEffector2D platformEffector)
    {
        yield return new WaitForSeconds(0.5f);
        platformEffector.surfaceArc = 180;
    }

    void MovePlayer()
    {
        rb.linearVelocity = new Vector2(moveDirection * moveSpeed, rb.linearVelocity.y);
    }

    private void MoveLeft()
    {
        moveDirection = -1f;
    }

    private void MoveRight()
    {
        moveDirection = 1f;
    }

    private bool IsTouchingWall()
    {
        RaycastHit2D hit = Physics2D.Raycast(wallTransform.position, Vector2.right * Mathf.Sign(moveDirection), wallCheckRadius, wallLayer);
        return hit.collider != null;
    }

    private void FlipPlayer()
    {
        if (moveDirection > 0 && transform.rotation != Quaternion.identity)
        {
            transform.rotation = Quaternion.identity;  // Quay về hướng phải
        }
        else if (moveDirection < 0 && transform.rotation != Quaternion.Euler(0, 180, 0))
        {
            transform.rotation = Quaternion.Euler(0, 180, 0);  // Quay về hướng trái
        }
    }

    private void OnDrawGizmos()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(wallTransform.position, wallCheckRadius);
    }
}
