using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnakeController : MonoBehaviour
{
    [Header("Body Parts")]
    public Transform head;
    public List<Transform> bodyParts = new List<Transform>();

    [Header("Speed Settings")]
    public float speed = 5f;
    public float boostedSpeed = 10f;
    private float originalSpeed;

    [Header("Distance")]
    public float baseDistance = 0.8f;
    public float headRotationSpeed = 15f;
    public float bodyRotationSpeed = 10f;

    [Header("Spawn")]
    public Transform spawnPointLeft;
    public Transform spawnPointRight;
    public Transform spawnPointUp;
    public Transform spawnPointDown;
    public Transform spawnPointNew;

    private Vector2 direction = Vector2.left;
    private List<(Vector3 position, Quaternion rotation)> previousPositions = new List<(Vector3, Quaternion)>();
    private bool isPaused = false;
    private bool isCollisionHandled = false;
    public bool spawnCollisionHandled = false;
    private bool isSkill = false;
    private Transform currentSpawnPoint;
    public bool isMoving = true;

    private SnakeSkill skill;
    private SnakeHealth health;
    private DamagePlayerSnake damage;
    void Start()
    {
        originalSpeed = speed; 
        previousPositions.Add((head.position, head.rotation));
        skill = GetComponent<SnakeSkill>();
        health = GetComponent<SnakeHealth>();
        damage = GetComponentInChildren<DamagePlayerSnake>();
    }

    void FixedUpdate()
    {
        if (isPaused) return;

        MoveHead();
        MoveBody();
    }

    private void Update()
    {
        CheckHealth();
    }

    void CheckHealth()
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

    public void Active()
    {
        StartCoroutine(SpawnAndHandleCollision());
    }

    private IEnumerator AutoMove()
    {
        yield return new WaitForSeconds(2f);

        while (spawnCollisionHandled)
        {
            yield return new WaitUntil(() => isMoving);

            yield return new WaitForSeconds(3f);

            // Chuyển hướng theo thứ tự: right -> down -> left -> up
            if (direction == Vector2.right && isMoving)
            {
                ResetCollisionState();
                SpawnAtPosition(spawnPointDown, Vector2.down);
            }
            else if (direction == Vector2.down && isMoving)
            {
                ResetCollisionState();
                SpawnAtPosition(spawnPointLeft, Vector2.left);
            }
            else if (direction == Vector2.left && isMoving)
            {
                ResetCollisionState();
                SpawnAtPosition(spawnPointUp, Vector2.up);
            }
            else if (direction == Vector2.up && isMoving)
            {
                ResetCollisionState();
                SpawnAtPosition(spawnPointRight, Vector2.right);
            }

            yield return new WaitForSeconds(4f);
        }
    }


    public IEnumerator SpawnAndHandleCollision()
    {
        yield return new WaitForSeconds(1f);
        StartCoroutine(SpawnAndCollisionCoroutine());
    }

    private IEnumerator SpawnAndCollisionCoroutine()
    {
        speed += 4f;
        SpawnAtPosition(spawnPointNew, Vector2.left);

        while (!spawnCollisionHandled)
        {
            if (Physics2D.OverlapCircle(head.position, 2f, LayerMask.GetMask("Left")))
            {
                direction = Vector2.down;
            }
            else if (Physics2D.OverlapCircle(head.position, 3f, LayerMask.GetMask("Down")))
            {
                direction = Vector2.right;
                spawnCollisionHandled = true;
            }
            yield return null;
        }
        yield return new WaitForSeconds(4f);
        speed = originalSpeed;
        direction = Vector2.up;
        damage.SetCanDamage(true);
        StartCoroutine(AutoMove());
    }

    void MoveHead()
    {
        Vector3 newPosition = head.position + (Vector3)(direction * speed * Time.fixedDeltaTime);
        Quaternion newRotation = Quaternion.Euler(0, 0, GetZRotationForDirection(direction));
        head.rotation = Quaternion.Lerp(head.rotation, newRotation, headRotationSpeed * Time.fixedDeltaTime);

        previousPositions.Insert(0, (newPosition, head.rotation));
        head.position = newPosition;
    }

    void MoveBody()
    {
        for (int i = 0; i < bodyParts.Count; i++)
        {
            Transform currentBodyPart = bodyParts[i];
            Vector3 targetPosition;
            Quaternion targetRotation;

            float requiredDistance = baseDistance * (i + 1);
            int index = Mathf.Min(previousPositions.Count - 1, Mathf.RoundToInt(requiredDistance / speed / Time.fixedDeltaTime));

            if (index < previousPositions.Count)
            {
                targetPosition = previousPositions[index].position;
                targetRotation = previousPositions[index].rotation;

                currentBodyPart.position = Vector3.Lerp(currentBodyPart.position, targetPosition, bodyRotationSpeed * Time.fixedDeltaTime);
                currentBodyPart.rotation = Quaternion.Lerp(currentBodyPart.rotation, targetRotation, bodyRotationSpeed * 10f);
            }
        }

        if (previousPositions.Count > bodyParts.Count * Mathf.CeilToInt(baseDistance / speed / Time.fixedDeltaTime))
        {
            previousPositions.RemoveAt(previousPositions.Count - 1);
        }
    }

    public void HandleCollision(string tag)
    {
        if (!spawnCollisionHandled) return;
        if (isCollisionHandled) return;

        if ((direction == Vector2.left && tag == "Left") ||
            (direction == Vector2.right && tag == "Right") ||
            (direction == Vector2.up && tag == "Up") ||
            (direction == Vector2.down && tag == "Down"))
        {
            StartCoroutine(PauseAndChangeDirection(tag));
            isCollisionHandled = true;
        }
    }

    private IEnumerator PauseAndChangeDirection(string tag)
    {
        isPaused = true;
        isMoving = false;
        yield return StartCoroutine(DetachBodyParts());

        skill.StartFiring(direction);
        yield return new WaitUntil(() => !skill.isFiring);

        yield return new WaitForSeconds(1f);

        yield return StartCoroutine(ReattachBodyParts());

        yield return new WaitForSeconds(0.1f);

        isPaused = false;
        speed = boostedSpeed;

        yield return new WaitForSeconds(2f);
        isMoving = true;
    }

    private IEnumerator DetachBodyParts()
    {
        List<Vector3> targetPositions = new List<Vector3>();

            for (int i = 0; i < bodyParts.Count; i++)
            {
                Vector3 offset = Vector3.zero;

                if (direction == Vector2.left)
                {
                    offset = new Vector3(0.5f * (i + 1), 0, 0);
                }
                else if (direction == Vector2.down)
                {
                    offset = new Vector3(0, 0.5f * (i + 1), 0);
                }
                else if (direction == Vector2.right)
                {
                    offset = new Vector3(-0.5f * (i + 1), 0, 0);
                }
                else if (direction == Vector2.up)
                {
                    offset = new Vector3(0, -0.5f * (i + 1), 0);
                }

            targetPositions.Add(bodyParts[i].position + offset);
            }

        bool allReached = false;
        while (!allReached)
        {
            allReached = true;
            for (int i = 0; i < bodyParts.Count; i++)
            {
                bodyParts[i].position = Vector3.Lerp(bodyParts[i].position, targetPositions[i], Time.deltaTime * 10f);
                if (Vector3.Distance(bodyParts[i].position, targetPositions[i]) > 0.01f)
                {
                    allReached = false;
                }
            }
            yield return null;
        }
    }

    private IEnumerator ReattachBodyParts()
    {
        bool allReached = false;
        while (!allReached)
        {
            allReached = true;
            for (int i = 0; i < bodyParts.Count; i++)
            {
                if (i < previousPositions.Count)
                {
                    Vector3 targetPosition = previousPositions[i].position;
                    Quaternion targetRotation = previousPositions[i].rotation;

                    bodyParts[i].position = Vector3.Lerp(bodyParts[i].position, targetPosition, Time.deltaTime * 10f);
                    bodyParts[i].rotation = Quaternion.Lerp(bodyParts[i].rotation, targetRotation, Time.deltaTime * 10f);

                    if (Vector3.Distance(bodyParts[i].position, targetPosition) > 0.01f ||
                        Quaternion.Angle(bodyParts[i].rotation, targetRotation) > 0.1f)
                    {
                        allReached = false;
                    }
                }
            }
            yield return null;
        }
    }

    void ResetCollisionState()
    {
        isCollisionHandled = false;
        speed = originalSpeed;
    }

    void SpawnAtPosition(Transform spawnPoint, Vector2 newDirection)
    {
        head.position = spawnPoint.position;
        direction = newDirection;
        previousPositions.Clear();
        previousPositions.Add((head.position, head.rotation));
    }

    float GetZRotationForDirection(Vector2 dir)
    {
        if (dir == Vector2.up) return 90f;
        if (dir == Vector2.down) return -90f;
        if (dir == Vector2.left) return 180f;
        if (dir == Vector2.right) return 0f;
        return 0f;
    }
}
