﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaptainSkill : MonoBehaviour
{
    [Header("Knife Blade")]
    public GameObject bladePrefab;
    public float speed = 2f;
    public float radius = 3f;
    public float height = 5f;
    public float rotationSpeed = 360f;

    [Header("Laser")]
    public GameObject laserPrefab;
    public float laserDuration = 1f;
    public float laserDistance = 10f;
    public Transform laserShootTransform;
    public Transform laserShootTransform2;

    [Header("Bomb")]
    public GameObject bombPrefab;
    public float throwSpeed = 5f;
    public Transform[] throwTargets;
    private Vector3 playerLastPosition;
    public float laserLength = 10f;
    public GameObject laserBomb;
    public GameObject smallBombPrefab;

    [Header("Big Bomb")]
    public GameObject ballPrefab;
    public float moveDistance = 4f;
    public float scaleDuration = 2f;
    public float throwSpeedBigBomb = 5f;
    private Vector3 playerLastPositionBig;

    [Header("Flash")]
    public float jumpHeight = 15f;
    public float moveDuration = 0.5f;
    public float dashSpeed = 5f;
    public float offScreenOffset = 2f;
    private Vector3 originalPosition;

    [Header("Shoot")]
    public GameObject linePrefab;
    public float laserDistances = 2f;
    public Transform[] shootingPoints;
    public GameObject bulletPrefab;
    public float fireDelay = 2f;
    public int totalRounds = 5;
    public float bulletSpeedLaser;
    public Transform laserPositionTransform;
    private GameObject laserObject;

    [Header("Ground Check")]
    public Transform groundCheckTransform;
    public float groundCheckRadius = 0.1f;
    public LayerMask groundLayer;

    private GameObject bladeInstance;
    private Vector3 origin;
    private float t;
    private bool isSkillActive = false;
    private bool isSpawn = false;

    private GameObject player;
    private LineRenderer laserRenderer;
    private Vector3 playerPositionOriginal;
    private Quaternion lastRotation;
    private GameObject ball;
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private LineRenderer lineRenderer;
    private Collider2D collider2d;

    private FlashDamagePlayer damagePlayer;
    private CaptainHealth health;
    private Coroutine skillSequenceCoroutine;

    private void Awake()
    {
        originalPosition = transform.position;
        playerPositionOriginal = transform.position;
        startPosition = transform.position;
        targetPosition = new Vector3(transform.position.x, transform.position.y + moveDistance, transform.position.z);
    }

    private void Start()
    {
        health = GetComponent<CaptainHealth>();

        collider2d = GetComponent<Collider2D>();
        if (collider2d != null )
        {
            collider2d.enabled = false;
        }
        damagePlayer = GetComponent<FlashDamagePlayer>();
    }

    private void Update()
    {
        CheckHealth();

        if (!isSkillActive)
        { playerLastPositionBig = FindPlayerPosition(); }

        if (isSkillActive && bladeInstance != null)
        {
            MoveBlade();
        }
    }

    public void Active()
    {
        Spawn();
    }

    private IEnumerator SkillSequenceLoop()
    {
        yield return new WaitForSeconds(3f);

        while (true)
        {
            // BladeSkill
            BladeSkill();
            yield return new WaitUntil(() => !isSkillActive);
            yield return new WaitForSeconds(1f);

            // FireLaserRounds
            StartCoroutine(FireLaserRounds());
            yield return new WaitUntil(() => !isSkillActive);
            yield return new WaitForSeconds(1f);

            // LaserSkill
            LaserSkill();
            yield return new WaitUntil(() => !isSkillActive);
            yield return new WaitForSeconds(1f);

            // ActivateBombSkill
            yield return StartCoroutine(ActivateBombSkill());
            yield return new WaitForSeconds(1f);

            // FlashSkill
            yield return StartCoroutine(FlashSkill());
            yield return new WaitForSeconds(1.5f);
        }
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

    void Spawn()
    {
        Vector3 spawnPosition = originalPosition + new Vector3(0f, 15f, 0f);

        transform.position = spawnPosition;

        StartCoroutine(MovePosition(originalPosition, 2f));

        isSpawn = true;

        if (isSpawn && !isSkillActive && skillSequenceCoroutine == null)
        {
            skillSequenceCoroutine = StartCoroutine(SkillSequenceLoop());
        }
    }

    void BladeSkill()
    {
        isSkillActive = true;
        origin = transform.position;
        t = 0;

        bladeInstance = Instantiate(bladePrefab, origin, Quaternion.identity);
    }

    void MoveBlade()
    {
        t += speed * Time.deltaTime;

        float progress = t % 2f;
        float x, y;

        if (progress <= 0.5f)
        {
            x = Mathf.Lerp(origin.x, origin.x - radius, progress * 2);
            y = Mathf.Lerp(origin.y, origin.y - (height * 0.5f), progress * 2);
        }
        else if (progress <= 1f)
        {
            float angle = Mathf.Lerp(0f, Mathf.PI, (progress - 0.5f) * 2);
            x = origin.x - radius * Mathf.Cos(angle);
            y = origin.y - (height * 0.5f) - radius * Mathf.Sin(angle);
        }
        else if (progress <= 1.5f)
        {
            x = Mathf.Lerp(origin.x + radius, origin.x, (progress - 1f) * 2);
            y = Mathf.Lerp(origin.y - (height * 0.5f), origin.y, (progress - 1f) * 2);
        }
        else
        {
            x = origin.x;
            y = origin.y;
        }

        bladeInstance.transform.position = new Vector3(x, y, bladeInstance.transform.position.z);

        bladeInstance.transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);

        if (t >= 1.5f)
        {
            Destroy(bladeInstance);
            isSkillActive = false;
        }
    }

    void LaserSkill()
    {
        isSkillActive = true;

        player = GameObject.FindGameObjectWithTag("Player");

        if (player == null)
        {
            int playerLayer = LayerMask.NameToLayer("Player");
            player = FindPlayerByLayer(playerLayer);
        }

        if (player != null)
        {
            Vector3 behindPlayer = player.transform.position - player.transform.forward * 5f;
            transform.position = behindPlayer;

            StartCoroutine(FlipAndFireLaser(player));
        }
        else
        {
            Debug.LogError("Player not found!");
        }
    }

    IEnumerator FlipAndFireLaser(GameObject player)
    {
        yield return new WaitForSeconds(0.5f);

        FlipCharacter(player);

        yield return new WaitForSeconds(0.2f);

        FireLaser();
    }

    void FlipCharacter(GameObject player)
    {
        Vector3 directionToPlayer = player.transform.position - transform.position;
        directionToPlayer.y = 0;

        if (directionToPlayer.x > 0)
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        else
        {
            transform.rotation = Quaternion.Euler(0, 180, 0);
        }
    }

    GameObject FindPlayerByLayer(int layer)
    {
        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();

        foreach (GameObject obj in allObjects)
        {
            if (obj.layer == layer)
            {
                return obj;
            }
        }

        return null;
    }
    IEnumerator FireLaserAndReturn()
    {
        if (laserShootTransform == null || laserShootTransform2 == null)
        {
            Debug.LogError("Laser Shoot Transform is not assigned.");
            yield break;
        }

        Debug.Log("Laser Shoot Transform Position: " + laserShootTransform.position);

        float rotationAngle1 = 0f;
        float rotationAngle2 = 0f;

        if (transform.rotation.eulerAngles.y == 0f)
        {
            rotationAngle1 = 90f;  
            rotationAngle2 = -90f; 
        }
        else if (transform.rotation.eulerAngles.y == 180f)
        {
            rotationAngle1 = -90f; 
            rotationAngle2 = 90f; 
        }

        FireLaserFromTransform(laserShootTransform, rotationAngle1);
        FireLaserFromTransform(laserShootTransform2, rotationAngle2);

        yield return new WaitForSeconds(laserDuration);

        yield return new WaitForSeconds(0.5f);

        Vector3 originalPosition = playerPositionOriginal;
        lastRotation = transform.rotation;

        yield return new WaitForSeconds(1f);

        Vector3 leftPosition = originalPosition + Vector3.left * 3f;
        yield return StartCoroutine(MoveToPosition(leftPosition, 1f));
        FireLaserAtAngle(34f); 
        yield return new WaitForSeconds(laserDuration);

        Vector3 rightPosition = originalPosition + Vector3.right * 3f;
        yield return StartCoroutine(MoveToPosition(rightPosition, 0.5f)); 
        FireLaserAtAngle(-34f); 
        yield return new WaitForSeconds(laserDuration);

        yield return StartCoroutine(MoveToPosition(originalPosition, 1f));

        transform.rotation = lastRotation;

        isSkillActive = false;
    }

    void FireLaserFromTransform(Transform laserShootTransform, float rotationAngle)
    {
        GameObject laser = Instantiate(laserPrefab, laserShootTransform.position, Quaternion.Euler(0, 0, rotationAngle));

        LineRenderer laserRenderer = laser.GetComponent<LineRenderer>();
        if (laserRenderer != null)
        {
            laserRenderer.positionCount = 2;

            Vector3 laserStart = laserShootTransform.position;
            Vector3 laserEnd = laserStart + transform.right * laserDistance;

            laserRenderer.SetPosition(0, laserStart);
            laserRenderer.SetPosition(1, laserEnd);

            laserRenderer.startWidth = 0.1f;
            laserRenderer.endWidth = 0.1f;
            laserRenderer.numCapVertices = 5;
        }

        Destroy(laser, laserDuration);
    }

    IEnumerator MoveToPosition(Vector3 targetPosition, float duration)
    {
        Vector3 startPosition = transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(startPosition, targetPosition, elapsed / duration);
            yield return null;
        }

        transform.position = targetPosition;
    }

    void FireLaserAtAngle(float angle)
    {
        if (laserShootTransform == null)
        {
            Debug.LogError("Laser Shoot Transform is not assigned.");
            return;
        }

        FlipCharacterBasedOnAngle(angle);

        GameObject laser = Instantiate(laserPrefab, laserShootTransform.position, Quaternion.Euler(0, 0, angle));

        LineRenderer laserRenderer = laser.GetComponent<LineRenderer>();
        if (laserRenderer != null)
        {
            laserRenderer.positionCount = 2;

            Vector3 laserStart = laserShootTransform.position;
            Vector3 laserEnd = laserStart + new Vector3(Mathf.Cos(Mathf.Deg2Rad * angle) * laserDistance, Mathf.Sin(Mathf.Deg2Rad * angle) * laserDistance, 0);

            laserRenderer.SetPosition(0, laserStart);
            laserRenderer.SetPosition(1, laserEnd);

            laserRenderer.startWidth = 0.1f;
            laserRenderer.endWidth = 0.1f;
            laserRenderer.numCapVertices = 5;
        }

        Destroy(laser, laserDuration);
    }

    void FlipCharacterBasedOnAngle(float angle)
    {
        if (angle > 0)
        {
            transform.rotation = Quaternion.Euler(0, 0, -45f);
        }
        else if (angle < 0)
        {
            transform.rotation = Quaternion.Euler(0, 180, -45f); 
        }
    }

    void FireLaser()
    {
        StartCoroutine(FireLaserAndReturn());
    }

    private IEnumerator BombSkill()
    {
        isSkillActive = true;

        List<GameObject> bombs = new List<GameObject>();

        for (int i = 0; i < 2; i++)
        {
            Vector3 targetPosition;

            if (i == 0)
            {
                GameObject player = null;

                // Tìm player bằng tag
                player = GameObject.FindGameObjectWithTag("Player");

                // Nếu không tìm thấy, tìm bằng layer
                if (player == null)
                {
                    int playerLayer = LayerMask.NameToLayer("Player");
                    GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
                    foreach (GameObject obj in allObjects)
                    {
                        if (obj.layer == playerLayer)
                        {
                            player = obj;
                            break;
                        }
                    }
                }

                if (player != null)
                {
                    targetPosition = player.transform.position;
                }
                else
                {
                    Debug.LogError("Player not found!");
                    break;
                }
            }
            else
            {
                Transform randomTarget = GetRandomBombPosition();
                if (randomTarget != null)
                {
                    targetPosition = randomTarget.position;
                }
                else
                {
                    Debug.LogError("No random bomb positions available!");
                    break;
                }
            }

            GameObject bomb = Instantiate(bombPrefab, transform.position, Quaternion.identity);
            bombs.Add(bomb);

            StartCoroutine(MoveBombToPosition(bomb, targetPosition));
        }

        yield return new WaitForSeconds(1.5f);

        foreach (GameObject bomb in bombs)
        {
            if (bomb != null)
            {
                GameObject laser = CreateLaserAtPosition(bomb.transform.position);
                Destroy(laser, 2f);
                SpawnSmallBombs(bomb.transform.position);
                Destroy(bomb);
            }
        }

        yield return new WaitForSeconds(0.5f);

        isSkillActive = false;
    }


    private IEnumerator MoveBombToPosition(GameObject bomb, Vector3 targetPosition)
    {
        float moveDuration = 1f; 
        float elapsedTime = 0f;

        Vector3 startPosition = bomb.transform.position;

        while (elapsedTime < moveDuration)
        {
            bomb.transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / moveDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        bomb.transform.position = targetPosition; 
    }

    private Transform GetRandomBombPosition()
    {
        if (throwTargets != null && throwTargets.Length > 0)
        {
            int randomIndex = Random.Range(0, throwTargets.Length);
            return throwTargets[randomIndex];
        }
        return null;
    }

    private GameObject CreateLaserAtPosition(Vector3 position)
    {
        position.y = 15f;

        GameObject laser = Instantiate(laserBomb, position, Quaternion.Euler(0, 0, 0));

        LineRenderer lineRenderer = laser.GetComponent<LineRenderer>();
        if (lineRenderer != null)
        {
            float adjustedLaserLength = laserLength * 5f;
            lineRenderer.SetPosition(0, position);
            lineRenderer.SetPosition(1, position + transform.forward * adjustedLaserLength);
        }

        return laser;
    }
    private void SpawnSmallBombs(Vector3 position)
    {
        float launchForce = 5f;
        float upwardForce = 3f; 

        GameObject leftBomb = Instantiate(smallBombPrefab, position, Quaternion.identity);
        Rigidbody2D leftRb = leftBomb.GetComponent<Rigidbody2D>();
        if (leftRb != null)
        {
            Vector2 leftDirection = new Vector2(-1, 1).normalized;
            leftRb.AddForce(leftDirection * launchForce + Vector2.up * upwardForce, ForceMode2D.Impulse);  
        }

        GameObject rightBomb = Instantiate(smallBombPrefab, position, Quaternion.identity);
        Rigidbody2D rightRb = rightBomb.GetComponent<Rigidbody2D>();
        if (rightRb != null)
        {
            Vector2 rightDirection = new Vector2(1, 1).normalized;
            rightRb.AddForce(rightDirection * launchForce + Vector2.up * upwardForce, ForceMode2D.Impulse);  
        }
    }
    private IEnumerator ActivateBombSkill()
    {
        isSkillActive = true;

        for (int i = 0; i < 3; i++)
        {
            playerLastPositionBig = FindPlayerPosition();

            GameObject bomb = Instantiate(bombPrefab, transform.position, Quaternion.identity);
            BigBomb bigBomb = bomb.GetComponent<BigBomb>();

            Vector3 targetPosition = bomb.transform.position + Vector3.up * moveDistance;
            float moveSpeed = 4f;
            while (Vector3.Distance(bomb.transform.position, targetPosition) > 0.1f)
            {
                bomb.transform.position = Vector3.MoveTowards(bomb.transform.position, targetPosition, moveSpeed * Time.deltaTime);
                yield return null;
            }

            Vector3 originalScale = bomb.transform.localScale;
            Vector3 targetScale = originalScale * 2;
            float scaleDuration = 1f;
            float elapsedTime = 0f;

            while (elapsedTime < scaleDuration)
            {
                bomb.transform.localScale = Vector3.Lerp(originalScale, targetScale, (elapsedTime / scaleDuration));
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            Vector3 directionToPlayer = (playerLastPositionBig - bomb.transform.position).normalized;
            float distanceToPlayer = Vector3.Distance(bomb.transform.position, playerLastPositionBig);
            float fixedTravelTime = 5f;
            float travelTime = Mathf.Max(fixedTravelTime, distanceToPlayer / throwSpeedBigBomb);

            elapsedTime = 0f;
            while (elapsedTime < travelTime)
            {
                bomb.transform.position = Vector3.Lerp(bomb.transform.position, playerLastPositionBig, (elapsedTime / travelTime));
                elapsedTime += Time.deltaTime;

                if (Vector3.Distance(bomb.transform.position, playerLastPositionBig) < 0.1f)
                {
                    bigBomb.Explode();
                    yield return new WaitForSeconds(0.5f);
                    break;
                }

                yield return null;
            }

            if (bomb != null)
            {
                bigBomb.Explode();
            }

            yield return new WaitForSeconds(0.5f); 
        }

        isSkillActive = false;
    }

    Vector3 FindPlayerPosition()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            return player.transform.position;
        }

        int playerLayer = LayerMask.NameToLayer("Player"); 
        player = FindObjectInLayer(playerLayer);
        if (player != null)
        {
            return player.transform.position;
        }

        return Vector3.zero;
    }

    GameObject FindObjectInLayer(int layer)
    {
        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.layer == layer)
            {
                return obj;
            }
        }
        return null;
    }

    IEnumerator FlashSkill()
    {
        isSkillActive = true;
        damagePlayer.SetCanDamage(true);

        // 1. Dịch chuyển đến góc trên trái và đâm xuống dưới phải
        Vector3 jumpPosition = originalPosition + new Vector3(0, jumpHeight, 0);
        yield return MovePosition(jumpPosition, moveDuration);

        Vector3 targetPosition = transform.position;

        Vector3 topLeftOffScreen = Camera.main.ViewportToWorldPoint(new Vector3(0, 1, Camera.main.nearClipPlane));
        topLeftOffScreen.z = transform.position.z;
        topLeftOffScreen += new Vector3(-offScreenOffset, offScreenOffset, 0);

        transform.position = topLeftOffScreen;

        Vector3 bottomRight = Camera.main.ViewportToWorldPoint(new Vector3(1, 0, Camera.main.nearClipPlane));
        bottomRight.z = transform.position.z;

        Vector3 dashDirection = (bottomRight - transform.position).normalized;

        yield return new WaitForSeconds(0.5f);
        RotateTowards(dashDirection, -135f);
        yield return DashInDirection(dashDirection, 3f);

        // 2. Dịch chuyển đến góc dưới trái và đâm lên góc trên phải
        Vector3 bottomLeft = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, Camera.main.nearClipPlane));
        bottomLeft.z = transform.position.z;
        bottomLeft += new Vector3(-offScreenOffset, -offScreenOffset, 0);

        transform.position = bottomLeft;

        Vector3 topRight = Camera.main.ViewportToWorldPoint(new Vector3(1, 1, Camera.main.nearClipPlane));
        topRight.z = transform.position.z;

        dashDirection = (topRight - transform.position).normalized;

        yield return new WaitForSeconds(0.2f);
        RotateTowards(dashDirection, -45f);
        yield return DashInDirection(dashDirection, 3f);

        // 3. Dịch chuyển đến góc dưới phải và đâm lên góc trên trái
        transform.position = bottomRight;

        Vector3 topLeft = Camera.main.ViewportToWorldPoint(new Vector3(0, 1, Camera.main.nearClipPlane));
        topLeft.z = transform.position.z;

        bottomRight += new Vector3(offScreenOffset, -offScreenOffset, 0); 
        transform.position = bottomRight;

        dashDirection = (topLeft - transform.position).normalized;

        yield return new WaitForSeconds(0.2f);
        RotateTowards(dashDirection, 45f);
        yield return DashInDirection(dashDirection, 3f);

        // 4. Dịch chuyển đến góc trên phải và đâm xuống góc dưới trái
        transform.position = topRight;
        topRight += new Vector3(offScreenOffset, offScreenOffset, 0);
        transform.position = topRight;

        dashDirection = (bottomLeft - transform.position).normalized;

        yield return new WaitForSeconds(0.2f);
        RotateTowards(dashDirection, 135f);
        yield return DashInDirection(dashDirection, 3f);

        // 5. Dịch chuyển góc trên và đâm thẳng xuống
        transform.position = targetPosition;

        yield return new WaitForSeconds(0.2f);
        RotateTowards(Vector3.down, 180f);

        Vector3 leftClonePosition = transform.position + new Vector3(-5f, 0, 0);
        Vector3 rightClonePosition = transform.position + new Vector3(5f, 0, 0);

        GameObject leftClone = Instantiate(gameObject, leftClonePosition, transform.rotation);
        GameObject rightClone = Instantiate(gameObject, rightClonePosition, transform.rotation);

        leftClone.GetComponent<CaptainHealth>().enabled = false; 
        rightClone.GetComponent<CaptainHealth>().enabled = false;
        leftClone.GetComponent<BoxCollider2D>().enabled = false;
        rightClone.GetComponent<BoxCollider2D>().enabled = false;

        leftClone.transform.Rotate(0, 0, 180f);  
        rightClone.transform.Rotate(0, 0, 180f); 

        dashDirection = Vector3.down;

        StartCoroutine(BlinkEffect(leftClone, 3f, 0.1f));
        StartCoroutine(BlinkEffect(rightClone, 3f, 0.1f));

        Coroutine mainCharacterDash = StartCoroutine(DashInDirection(dashDirection, 3f));
        Coroutine leftCloneDash = StartCoroutine(leftClone.GetComponent<CaptainSkill>().DashInDirection(dashDirection, 3f)); 
        Coroutine rightCloneDash = StartCoroutine(rightClone.GetComponent<CaptainSkill>().DashInDirection(dashDirection, 3f)); 

        yield return mainCharacterDash;
        yield return leftCloneDash;
        yield return rightCloneDash;

        Destroy(leftClone);
        Destroy(rightClone);

        // 6. Dịch chuyển xuống dưới và đâm thẳng lên
        Vector3 bottomPosition = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0, Camera.main.nearClipPlane)); 
        bottomPosition.z = transform.position.z;
        bottomPosition += new Vector3(0, -offScreenOffset, 0);

        transform.position = bottomPosition;
        yield return new WaitForSeconds(0.2f);

        RotateTowards(Vector3.up, 0f);

        Vector3 leftClonePositionNew = transform.position + new Vector3(-5f, 0, 0);
        Vector3 rightClonePositionNew = transform.position + new Vector3(5f, 0, 0);

        GameObject leftCloneNew = Instantiate(gameObject, leftClonePositionNew, transform.rotation);
        GameObject rightCloneNew = Instantiate(gameObject, rightClonePositionNew, transform.rotation);

        leftCloneNew.GetComponent<CaptainHealth>().enabled = false;
        rightCloneNew.GetComponent<CaptainHealth>().enabled = false;
        leftCloneNew.GetComponent<BoxCollider2D>().enabled = false;
        rightCloneNew.GetComponent<BoxCollider2D>().enabled = false;

        leftCloneNew.transform.Rotate(0, 0, 0f);  
        rightCloneNew.transform.Rotate(0, 0, 0f); 

        Vector3 dashDirectionUp = Vector3.up;

        StartCoroutine(BlinkEffect(leftCloneNew, 3f, 0.1f));
        StartCoroutine(BlinkEffect(rightCloneNew, 3f, 0.1f));

        Coroutine mainCharacterDashUp = StartCoroutine(DashInDirection(dashDirectionUp, 3f)); 
        Coroutine leftCloneDashUp = StartCoroutine(leftCloneNew.GetComponent<CaptainSkill>().DashInDirection(dashDirectionUp, 3f)); 
        Coroutine rightCloneDashUp = StartCoroutine(rightCloneNew.GetComponent<CaptainSkill>().DashInDirection(dashDirectionUp, 3f)); 

        yield return mainCharacterDashUp;
        yield return leftCloneDashUp;
        yield return rightCloneDashUp;

        Destroy(leftCloneNew);
        Destroy(rightCloneNew);

        // 7. Dịch chuyển về target position và hạ cánh
        transform.position = targetPosition;
        yield return StartCoroutine(BombSkill());
        damagePlayer.SetCanDamage(false);

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        collider2d.enabled = true;

        bool hasLanded = false;
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            while (!hasLanded)
            {
                if (collider.IsTouchingLayers(LayerMask.GetMask("Ground")))
                {
                    hasLanded = true;
                }
                yield return null;
            }
        }

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        health.SetCanBeDamaged(true);

        yield return new WaitForSeconds(4f);

        collider2d.enabled = false;
        health.SetCanBeDamaged(false);
        yield return MovePosition(originalPosition, 1.5f);

        Debug.Log("FlashSkill complete!");
        isSkillActive = false;
    }

    void RotateTowards(Vector3 direction, float fixedAngle)
    {
        transform.rotation = Quaternion.Euler(0, 0, fixedAngle);
    }

    IEnumerator MovePosition(Vector3 targetPosition, float duration)
    {
        Vector3 startPosition = transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition;
    }

    IEnumerator DashInDirection(Vector3 direction, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            transform.position += direction * dashSpeed * Time.deltaTime;
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator FireLaserRounds()
    {
        isSkillActive = true;

        CreateLaser();

        yield return new WaitForSeconds(1.5f);

        for (int round = 0; round < totalRounds; round++)
        {
            List<int> availableIndexes = new List<int>(shootingPoints.Length);
            for (int i = 0; i < shootingPoints.Length; i++)
            {
                availableIndexes.Add(i);
            }

            int index1 = GetRandomIndex(availableIndexes);
            int index2 = GetRandomIndex(availableIndexes);

            FireBullet(shootingPoints[index1]);
            FireBullet(shootingPoints[index2]);

            yield return new WaitForSeconds(fireDelay);
        }

        yield return new WaitForSeconds(1f);

        DestroyLaser();

        yield return new WaitForSeconds(0.5f);

        isSkillActive = false;
    }

    private int GetRandomIndex(List<int> availableIndexes)
    {
        int randomIndex = Random.Range(0, availableIndexes.Count);
        int selectedIndex = availableIndexes[randomIndex];
        availableIndexes.RemoveAt(randomIndex);
        return selectedIndex;
    }

    private void CreateLaser()
    {
        if (laserPositionTransform != null)
        {
            Vector3 laserPosition = laserPositionTransform.position - new Vector3(15f, 1.6f, 0);
            laserObject = Instantiate(linePrefab, laserPosition, Quaternion.identity);
        }
    }

    private void DestroyLaser()
    {
        if (laserObject != null)
        {
            Destroy(laserObject);
        }
    }

    private void FireBullet(Transform firePoint)
    {
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

        Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
        if (bulletRb != null)
        {
            bulletRb.linearVelocity = Vector2.down * bulletSpeedLaser;
        }
    }

    IEnumerator BlinkEffect(GameObject clone, float blinkDuration, float blinkInterval)
    {
        SpriteRenderer spriteRenderer = clone.GetComponentInChildren<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            float elapsedTime = 0f;
            bool isVisible = true;

            while (elapsedTime < blinkDuration)
            {
                spriteRenderer.enabled = isVisible;

                isVisible = !isVisible; 

                elapsedTime += blinkInterval; 
                yield return new WaitForSeconds(blinkInterval);
            }

            spriteRenderer.enabled = true; 
        }
    }

}