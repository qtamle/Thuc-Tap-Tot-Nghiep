using System.Collections;
using UnityEngine;

public class Boss5 : MonoBehaviour
{
    [Header("Other")]
    private Rigidbody2D rb;
    [SerializeField] public float groundCheckRadius = 0.2f;
    [SerializeField] public LayerMask wallLayer;
    private Transform BossTrans;

    [Header("Check")]
    [SerializeField] public FloorCheck floorCheck;
    [SerializeField] public SideManager sideManager;

    [Header("Skill Settings")]
    [SerializeField] private GameObject Skill1Left;
    [SerializeField] private GameObject Skill1Right;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float moveDistance = 5f;

    private GameObject summonedObject;
    private Vector3 spawnPosition;
    private bool isMovingForward = true;
    private bool isMovingLeft;

    void Start()
    {
        BossTrans = transform;
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        //if (Input.GetKey(KeyCode.Space))
        //{
        //    CheckPlayer();
        //}
        if (Input.GetKeyDown(KeyCode.T))
        {
            Teleport();
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            Teleport();
            StartCoroutine(SummonObject());
        }

    }
    //public void CheckPlayer()
    //{
    //    string currentFloor = floorCheck?.CurrentFloor ?? "Unknown";
    //    Transform leftTransform = floorCheck?.CurrentLeftFloor;
    //    Transform rightTransform = floorCheck?.CurrentRightFloor;
    //    Debug.Log($"Player is at {currentFloor}");

    //    if (leftTransform != null && rightTransform != null)
    //    {
    //        Debug.Log($"LeftFloor: {leftTransform.position}, RightFloor: {rightTransform.position}");
    //    }

    //    bool isOnLeft = sideManager?.IsOnLeft ?? false;
    //    bool isOnRight = sideManager?.IsOnRight ?? false;

    //    if (isOnLeft)
    //    {
    //        Debug.Log("Player is on the Left side.");
    //    }
    //    else if (isOnRight)
    //    {
    //        Debug.Log("Player is on the Right side.");
    //    }
    //    else
    //    {
    //        Debug.Log("Player position unknown.");
    //    }
    //}

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
    private IEnumerator SummonObject()
    {
        yield return new WaitForSeconds(1f);
        // Lấy reference đến các transform
        Transform leftTransform = floorCheck?.CurrentLeftFloor;
        Transform rightTransform = floorCheck?.CurrentRightFloor;

        // Hủy object cũ nếu có
        if (summonedObject != null)
        {
            Destroy(summonedObject);
        }

        // Spawn theo logic giống Teleport
        if (sideManager?.IsOnLeft ?? false && rightTransform != null)
        {
            Debug.Log("Summoning object on Right Floor...");
            isMovingLeft = false;
            spawnPosition = rightTransform.position;
            summonedObject = Instantiate(Skill1Right, spawnPosition, Quaternion.identity);
        }
        else if (sideManager?.IsOnRight ?? false && leftTransform != null)
        {
            Debug.Log("Summoning object on Left Floor...");
            isMovingLeft = true;
            spawnPosition = leftTransform.position;
            summonedObject = Instantiate(Skill1Left, spawnPosition, Quaternion.identity);
        }

        isMovingForward = true;
        StartCoroutine(MoveObject());
    }

    private IEnumerator MoveObject()
    {
        yield return new WaitForSeconds(1f);

        float moveDirection = isMovingLeft ? 1 : -1;

        if (isMovingForward)
        {
            // Di chuyển đi
            if (Mathf.Abs(summonedObject.transform.position.x - spawnPosition.x) < moveDistance)
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
            // Di chuyển về
            if (Mathf.Abs(summonedObject.transform.position.x - spawnPosition.x) > 0.1f)
            {
                summonedObject.transform.Translate(Vector3.right * -moveDirection * moveSpeed * Time.deltaTime);
            }
            else
            {
                // Khi object đã về gần vị trí ban đầu, destroy nó
                Destroy(summonedObject);
            }
        }
    }

}