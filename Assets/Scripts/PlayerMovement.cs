using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    public static PlayerMovement Instance { get; private set; }

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
    private float originalSpeed;

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

    [Header("Ice Movement Effect")]
    public float iceMoveSpeedMultiplier = 1.5f; 
    public float dragFactor = 0.1f;
    public bool isIceMovementActive = false;
    public bool isChangingDirection = false;
    private Vector2 previousVelocity;

    private Shoes boostMoveSpeed;
    private IceStaking iceStaking;

    private bool isUpSpeed = false;
    private bool isIceStaking = false;

    public bool isMovementLocked = true;
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
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();
        OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        originalSpeed = moveSpeed;
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

        boostMoveSpeed = FindFirstObjectByType<Shoes>();
        iceStaking = FindFirstObjectByType<IceStaking>();

        if (boostMoveSpeed != null && !isUpSpeed)
        {
            Debug.Log("Tim thay supply tang toc");
            moveSpeed += 0.2f;
            isUpSpeed = true;
        }

        if (iceStaking != null && !isIceStaking)
        {
            Debug.Log("Tim thay ice staking");
            isIceMovementActive = !isIceMovementActive;
            isIceStaking = true;
        }
    }
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
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
        if (Input.touchCount > 0) 
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                startInputPosition = touch.position;
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                endInputPosition = touch.position;

                Vector2 swipeDirection = endInputPosition - startInputPosition;
                float swipeDistance = swipeDirection.magnitude; 
                float swipeThreshold = 30f; 

                if (swipeDistance > swipeThreshold)
                {
                    float swipeAngle = Vector2.Angle(Vector2.right, swipeDirection); 

                    if (swipeAngle < 45f) // Vuốt phải
                    {
                        MoveRight();
                        FlipPlayer();
                    }
                    else if (swipeAngle > 135f) // Vuốt trái
                    {
                        MoveLeft();
                        FlipPlayer();
                    }
                    else if (swipeDirection.y > 0) // Vuốt lên
                    {
                        Jump();
                    }
                    else if (swipeDirection.y < 0) // Vuốt xuống
                    {
                        FallThrough();
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

            Vector2 swipeDirection = endInputPosition - startInputPosition;
            float swipeDistance = swipeDirection.magnitude; 
            float swipeThreshold = 30f; 

            if (swipeDistance > swipeThreshold)
            {
                float swipeAngle = Vector2.Angle(Vector2.right, swipeDirection); 

                if (swipeAngle < 45f) // Vuốt phải
                {
                    MoveRight();
                    FlipPlayer();
                }
                else if (swipeAngle > 135f) // Vuốt trái
                {
                    MoveLeft();
                    FlipPlayer();
                }
                else if (swipeDirection.y > 0) // Vuốt lên
                {
                    Jump();
                }
                else if (swipeDirection.y < 0) // Vuốt xuống
                {
                    FallThrough();
                }
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
        if (isMovementLocked || !canMove) return;

        if (isIceMovementActive)
        {
            float adjustedMoveSpeed = moveSpeed + iceMoveSpeedMultiplier;
            float lungeForce = 4f;

            if (Mathf.Sign(moveDirection) != Mathf.Sign(previousVelocity.x))
            {
                isChangingDirection = true;
                rb.AddForce(new Vector2(-Mathf.Sign(previousVelocity.x) * lungeForce, 0), ForceMode2D.Impulse);
            }

            if (isChangingDirection)
            {
                rb.linearVelocity = new Vector2(Mathf.Lerp(previousVelocity.x, moveDirection * adjustedMoveSpeed, dragFactor), rb.linearVelocity.y);
                if (Mathf.Abs(rb.linearVelocity.x - moveDirection * adjustedMoveSpeed) < 0.1f)
                {
                    isChangingDirection = false; 
                }
            }
            else
            {
                rb.linearVelocity = new Vector2(moveDirection * moveSpeed, rb.linearVelocity.y);
            }

            previousVelocity = rb.linearVelocity;
        }
        else
        {
            rb.linearVelocity = new Vector2(moveDirection * moveSpeed, rb.linearVelocity.y);
        }
    }

    private void MoveLeft()
    {
        if (isMovementLocked) isMovementLocked = false;
        moveDirection = -1f;
    }

    private void MoveRight()
    {
        if (isMovementLocked) isMovementLocked = false;
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
