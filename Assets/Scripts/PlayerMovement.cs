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

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();

        int playerLayer = gameObject.layer;
        int blockLayer = LayerMask.NameToLayer(blockLayerName);

        if (blockLayer != -1)
        {
            Physics2D.IgnoreLayerCollision(playerLayer, blockLayer, true);
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

        // Always move the player if grounded and not jumping
        if (canMove && isGrounded && !IsTouchingWall())
        {
            MovePlayer();
        }
    }

    void CheckGrounded()
    {
        currentGround = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        isGrounded = currentGround != null;
        isJumping = !isGrounded;

        Debug.Log("currentGround: " + currentGround);

        if (isGrounded)
        {
            canMove = true; // Enable movement when grounded
            Debug.Log("Đã chạm đất.");
        }
        else
        {
            canMove = false; // Disable movement when in the air
            Debug.Log("Không chạm đất.");
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

                // Kiểm tra thao tác vuốt
                if (!isJumping)
                {
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
                    }
                    else if (IsSwipeRight(startInputPosition, endInputPosition))
                    {
                        MoveRight();
                    }
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

            // Kiểm tra thao tác kéo chuột
            if (!isJumping)
            {
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
                }
                else if (IsSwipeRight(startInputPosition, endInputPosition))
                {
                    MoveRight();
                }
            }
        }
    }

    bool IsSwipeUp(Vector2 start, Vector2 end)
    {
        // Tính toán khoảng cách vuốt
        float swipeDistance = end.y - start.y;
        float swipeThreshold = 50f; // Ngưỡng vuốt tối thiểu để kích hoạt

        bool isSwipeUp = swipeDistance > swipeThreshold && Mathf.Abs(end.x - start.x) < swipeThreshold;

        return isSwipeUp;
    }

    bool IsSwipeDown(Vector2 start, Vector2 end)
    {
        // Tính toán khoảng cách vuốt
        float swipeDistance = start.y - end.y;
        float swipeThreshold = 50f; // Ngưỡng vuốt tối thiểu để kích hoạt

        bool isSwipeDown = swipeDistance > swipeThreshold && Mathf.Abs(end.x - start.x) < swipeThreshold;

        return isSwipeDown;
    }

    bool IsSwipeLeft(Vector2 start, Vector2 end)
    {
        // Tính toán khoảng cách vuốt sang trái
        float swipeDistance = start.x - end.x;
        float swipeThreshold = 50f;

        bool isSwipeLeft = swipeDistance > swipeThreshold && Mathf.Abs(end.y - start.y) < swipeThreshold;

        return isSwipeLeft;
    }

    bool IsSwipeRight(Vector2 start, Vector2 end)
    {
        // Tính toán khoảng cách vuốt sang phải
        float swipeDistance = end.x - start.x;
        float swipeThreshold = 50f;

        bool isSwipeRight = swipeDistance > swipeThreshold && Mathf.Abs(end.y - start.y) < swipeThreshold;

        return isSwipeRight;
    }

    void Jump()
    {
        isJumping = true;
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        Debug.Log("Nhảy!");

        // Lấy PlatformEffector2D từ currentGround (nếu có)
        PlatformEffector2D platformEffector = currentGround?.GetComponent<PlatformEffector2D>();

        if (platformEffector != null)
        {
            platformEffector.surfaceArc = 180;
            Debug.Log("Khôi phục giá trị PlatformEffector2D surfaceArc = 180");
        }
    }

    void FallThrough()
    {
        if (isGrounded && currentGround != null && !currentGround.CompareTag(finalFloorTag))
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -jumpForce * 0.3f); 

            rb.gravityScale = 4f;
            Debug.Log("Đang rơi xuống!");

            // Lấy PlatformEffector2D từ currentGround (nếu có)
            PlatformEffector2D platformEffector = currentGround?.GetComponent<PlatformEffector2D>();

            if (platformEffector != null)
            {
                platformEffector.surfaceArc = 0; // Đặt giá trị để cho phép rơi qua
                Debug.Log("Đặt giá trị PlatformEffector2D surfaceArc = 0 để cho phép rơi qua");

                // Đợi 0,25 giây rồi khôi phục lại giá trị surfaceArc thành 180
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
        // Đợi 0,25 giây trước khi quay lại giá trị 180
        yield return new WaitForSeconds(0.5f);
        platformEffector.surfaceArc = 180;
        Debug.Log("Khôi phục giá trị PlatformEffector2D surfaceArc = 180");
    }

    private void MovePlayer()
    {
        // Di chuyển dựa vào giá trị moveDirection được cập nhật khi vuốt
        rb.linearVelocity = new Vector2(moveDirection * moveSpeed, rb.linearVelocity.y);

        if (moveDirection > 0 && transform.rotation != Quaternion.identity)
            transform.rotation = Quaternion.identity; 
        else if (moveDirection < 0 && transform.rotation != Quaternion.Euler(0, 180, 0))
            transform.rotation = Quaternion.Euler(0, 180, 0); 
    }


    private void MoveLeft()
    {
        if (isGrounded && !isJumping)
        {
            moveDirection = -1f; // Di chuyển sang trái
        }
    }

    private void MoveRight()
    {
        if (isGrounded && !isJumping)
        {
            moveDirection = 1f; // Di chuyển sang phải
        }
    }

    private bool IsTouchingWall()
    {
        RaycastHit2D hit = Physics2D.Raycast(wallTransform.position, Vector2.right * Mathf.Sign(moveDirection), wallCheckRadius, wallLayer);
        return hit.collider != null;
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
