using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Cyborg : NetworkBehaviour
{
    public static Cyborg Instance;

    [Header("Disco Ball")]
    public GameObject orbPrefab;
    public GameObject linePrefab;
    public float orbDuration = 5f;
    public float lineDuration = 0.5f;
    public float lineLength = 5f;
    public Transform spawnPoint;
    private GameObject orb;

    [Header("Turret Skill")]
    public GameObject turretPrefab;
    public Transform[] gunPositions;

    [Header("Shoot Laser Skill")]
    public GameObject laserPrefab;
    public Transform laserSpawnPoint;
    public float moveDistance = 5f;
    public float moveDuration = 1f;
    public float laserDuration = 0.5f;

    [Header("Bomb Skill")]
    public GameObject bombPrefab;

    public Transform[] bombPositions;
    public float bombDuration = 3f;
    public float laserLength = 10f;
    public GameObject laserBomb;
    public float throwSpeed = 2f;
    public Transform spawnPosition;

    [Header("Other")]
    public LayerMask groundLayer;
    public Transform groundCheck;
    public NetworkRigidbody2D rb;

    private bool isSkillActive = false;
    private Vector3 originalPosition;
    private Vector3 originalPositionSkillRandom;
    private bool isStunned = false;
    private CyborgHealth Cyborghealth;

    private bool isSpawn;
    
    private Animator animator;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        animator = GetComponentInChildren<Animator>();
        Cyborghealth = GetComponent<CyborgHealth>();
        gunPositions = GunPositions.Instance.gunPositions;
        bombPositions = BoomPostion.Instance.bombPositions;
        // laserSpawnPoint = GameObject.FindWithTag("CyborgLaser").transform;
        spawnPoint = GameObject.FindWithTag("DiscoBall").transform;
        rb = GetComponent<NetworkRigidbody2D>();

        animator.SetBool("Idle", true);
    }

    public void Active()
    {
        StartCoroutine(MoveBossToTarget());
    }

    private void Update()
    {
        if (Cyborghealth != null && Cyborghealth.currentHealth.Value <= 0)
        {
            animator.SetTrigger("Death");
            StopAllCoroutines();
        }
        // if (Input.GetKeyDown(KeyCode.Q))
        // {
        //     StartCoroutine(DiscoBallSkill());
        // }
        // if (Input.GetKeyDown(KeyCode.W))
        // {
        //     StartCoroutine(BombSkill());
        // }
        // if (Input.GetKeyDown(KeyCode.E))
        // {
        //     StartCoroutine(TurretSkill());
        // }
        // if (Input.GetKeyDown(KeyCode.R))
        // {
        //     StartCoroutine(MoveAndFireLaser());
        // }
    }

    private IEnumerator MoveBossToTarget()
    {
        Vector3 startPosition = new Vector3(0.4f, 15.22f, transform.position.z);

        Vector3 targetPosition = new Vector3(0.4f, 3.5f, transform.position.z);

        float moveDuration = 3.5f;
        float elapsedTime = 0f;

        while (elapsedTime < moveDuration)
        {
            transform.position = Vector3.Lerp(
                startPosition,
                targetPosition,
                elapsedTime / moveDuration
            );
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition;
        originalPositionSkillRandom = targetPosition;

        isSpawn = true;

        StartCoroutine(AutomaticSkillCycle());
    }

    private IEnumerator AutomaticSkillCycle()
    {
        yield return new WaitForSeconds(3f);

        while (isSpawn)
        {
            yield return StartCoroutine(DiscoBallSkill());
            yield return new WaitForSeconds(1.5f);

            StartCoroutine(TurretSkill());
            yield return new WaitForSeconds(1.5f);
            StartCoroutine(BombSkill());

            yield return new WaitForSeconds(4.5f);

            yield return StartCoroutine(BombSkill());
            yield return new WaitForSeconds(1f);

            yield return StartCoroutine(MoveAndFireLaser());

            rb.Rigidbody2D.bodyType = RigidbodyType2D.Dynamic;
            yield return new WaitUntil(
                () => Physics2D.Raycast(groundCheck.position, Vector2.down, 0.1f, groundLayer)
            );
            isStunned = true;
            Debug.Log("Stunned: " + isStunned);
            rb.Rigidbody2D.bodyType = RigidbodyType2D.Static;

            if (isStunned)
            {
                Cyborghealth.SetCanBeDamaged(true);
                yield return new WaitForSeconds(4f);
                Cyborghealth.SetCanBeDamaged(false);
                // Cyborghealth.SetCanBeDamaged(true);
                // yield return new WaitForSeconds(4f);
                // Cyborghealth.SetCanBeDamaged(false);
            }
            isStunned = false;

            transform.position = originalPositionSkillRandom;

            yield return null;
        }
    }

    private IEnumerator DiscoBallSkill()
    {
        animator.SetTrigger("SummonDisco");

        yield return new WaitForSeconds(1.2f);

        isSkillActive = true;

        Vector3 startPosition = spawnPoint.position;
        Vector3 targetPosition = new Vector3(0, -1.5f, 0);
        orb = Instantiate(orbPrefab, startPosition, Quaternion.identity);
        orb.GetComponent<NetworkObject>().Spawn();
        float moveDuration = 2f;
        float elapsedTime = 0f;

        while (elapsedTime < moveDuration)
        {
            orb.transform.position = Vector3.Lerp(
                startPosition,
                targetPosition,
                elapsedTime / moveDuration
            );
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        orb.transform.position = targetPosition;

        yield return new WaitForSeconds(1f);
        GameObject[] plusLasers = CreatePlusLasers(targetPosition);

        yield return new WaitForSeconds(2f);
        foreach (GameObject laser in plusLasers)
        {
            laser.GetComponent<NetworkObject>().Despawn(true);
            // Destroy(laser);
        }

        yield return new WaitForSeconds(1f);
        GameObject[] xLasers = CreateXLasers(targetPosition);

        yield return new WaitForSeconds(2f);
        foreach (GameObject laser in xLasers)
        {
            laser.GetComponent<NetworkObject>().Despawn(true);
            // Destroy(laser);
        }

        yield return new WaitForSeconds(0.5f);
        orb.GetComponent<NetworkObject>().Despawn(true);

        isSkillActive = false;
    }

    private GameObject[] CreatePlusLasers(Vector3 centerPosition)
    {
        GameObject[] lasers = new GameObject[4];

        lasers[0] = CreateLaser(
            centerPosition,
            new Vector3(0, 0, 0).normalized,
            Quaternion.Euler(0, 0, 180)
        );
        lasers[1] = CreateLaser(
            centerPosition,
            new Vector3(0, 0, 0).normalized,
            Quaternion.Euler(0, 0, 0)
        );
        lasers[2] = CreateLaser(
            centerPosition,
            new Vector3(0, 0, 0).normalized,
            Quaternion.Euler(0, 0, -90)
        );
        lasers[3] = CreateLaser(
            centerPosition,
            new Vector3(0, 0, 0).normalized,
            Quaternion.Euler(0, 0, 90)
        );

        return lasers;
    }

    private GameObject[] CreateXLasers(Vector3 centerPosition)
    {
        GameObject[] lasers = new GameObject[2];

        Vector3 positionDownRight = new Vector3(7.13f, 8.94f, 0);
        Vector3 positionDownLeft = new Vector3(-7f, 8.41f, 0);
        lasers[0] = CreateLaser(
            positionDownRight,
            new Vector3(7.13f, 8.94f, 0).normalized,
            Quaternion.Euler(0, 0, -35)
        );
        lasers[1] = CreateLaser(
            positionDownLeft,
            new Vector3(-7f, 8.41f, 0).normalized,
            Quaternion.Euler(0, 0, 35)
        );

        return lasers;
    }

    private GameObject CreateLaser(Vector3 startPosition, Vector3 direction, Quaternion rotation)
    {
        GameObject lineObject = Instantiate(linePrefab);
        lineObject.GetComponent<NetworkObject>().Spawn();
        lineObject.transform.position = startPosition + direction * lineLength / 2;
        lineObject.transform.rotation = rotation;

        LineRenderer lineRenderer = lineObject.GetComponent<LineRenderer>();
        if (lineRenderer != null)
        {
            lineRenderer.SetPosition(0, startPosition);
            lineRenderer.SetPosition(1, startPosition + direction * lineLength);
        }

        return lineObject;
    }

    private IEnumerator TurretSkill()
    {
        isSkillActive = true;

        for (int i = 0; i < 2; i++)
        {
            List<Transform> availablePositions = new List<Transform>(gunPositions);
            Transform position1 = GetRandomGunPosition(availablePositions);
            Transform position2 = GetRandomGunPosition(availablePositions);

            animator.SetTrigger("Turret");
            yield return new WaitForSeconds(1.2f);

            GameObject turret1 = Instantiate(turretPrefab, position1.position, Quaternion.identity);
            turret1.GetComponent<NetworkObject>().Spawn();
            GameObject turret2 = Instantiate(turretPrefab, position2.position, Quaternion.identity);
            turret2.GetComponent<NetworkObject>().Spawn();
            Turret turretScript1 = turret1.GetComponent<Turret>();
            Turret turretScript2 = turret2.GetComponent<Turret>();

            turretScript1.InitializeTurret();
            turretScript2.InitializeTurret();

            yield return new WaitForSeconds(4.5f);

            turret1.GetComponent<NetworkObject>().Despawn(true);
            turret2.GetComponent<NetworkObject>().Despawn(true);
            // Destroy(turret1);
            // Destroy(turret2);
        }

        isSkillActive = false;
    }

    private IEnumerator MoveAndFireLaser()
    {
        isSkillActive = true;
        originalPosition = transform.position;

        Vector3 leftPosition = transform.position - new Vector3(moveDistance, 0, 0);
        yield return StartCoroutine(MoveToPositionAndFire(leftPosition));


        Vector3 rightPosition = transform.position + new Vector3(moveDistance * 2, 0, 0);
        yield return StartCoroutine(MoveToPositionAndFire(rightPosition));

        yield return StartCoroutine(MoveToPositionAndFire(originalPosition));

        isSkillActive = false;
    }

    private IEnumerator MoveToPositionAndFire(Vector3 targetPosition)
    {
        float elapsedTime = 0f;

        while (elapsedTime < moveDuration)
        {
            transform.position = Vector3.Lerp(
                transform.position,
                targetPosition,
                elapsedTime / moveDuration
            );
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition;

        animator.SetTrigger("Fire");
        yield return new WaitForSeconds(0.5f);

        FireLaser();

        yield return new WaitForSeconds(1.5f);
    }

    private void FireLaser()
    {
        GameObject laser = Instantiate(
            laserPrefab,
            laserSpawnPoint.position,
            Quaternion.Euler(0, 0, 0)
        );
        laser.GetComponent<NetworkObject>().Spawn();
        LineRenderer lineRenderer = laser.GetComponent<LineRenderer>();
        if (lineRenderer != null)
        {
            lineRenderer.SetPosition(0, laserSpawnPoint.position);
            lineRenderer.SetPosition(1, laserSpawnPoint.position + transform.right * 5f);
        }
        StartCoroutine(DespawnAfterSeconds(laser, laserDuration));
        // Destroy(laser, laserDuration);
    }

    private IEnumerator BombSkill()
    {
        isSkillActive = true;

        List<GameObject> bombs = new List<GameObject>();

        animator.SetTrigger("Bomb");
        yield return new WaitForSeconds(1.2f);

        for (int i = 0; i < 3; i++)
        {
            Transform bombPosition = GetRandomBombPosition();

            GameObject bomb = Instantiate(bombPrefab, spawnPosition.position, Quaternion.identity);
            bomb.GetComponent<NetworkObject>().Spawn();
            bombs.Add(bomb);

            StartCoroutine(MoveBombToPosition(bomb, bombPosition.position));
        }

        yield return new WaitForSeconds(throwSpeed);

        foreach (GameObject bomb in bombs)
        {
            GameObject laser = CreateLaserAtPosition(bomb.transform.position);
            StartCoroutine(DespawnAfterSeconds(laser, bombDuration));
            // Destroy(laser, laserDuration);

            bomb.GetComponent<NetworkObject>().Despawn(true);
            // Destroy(bomb);
        }

        yield return new WaitForSeconds(0.5f);

        isSkillActive = false;
    }

    IEnumerator DespawnAfterSeconds(GameObject laser, float seconds)
    {
        yield return new WaitForSeconds(seconds);

        laser.GetComponent<NetworkObject>().Despawn(true); // Đồng bộ hóa việc hủy đối tượng với các client
    }

    private IEnumerator MoveBombToPosition(GameObject bomb, Vector3 targetPosition)
    {
        float moveDuration = 1f;
        float elapsedTime = 0f;

        Vector3 startPosition = bomb.transform.position;

        while (elapsedTime < moveDuration)
        {
            bomb.transform.position = Vector3.Lerp(
                startPosition,
                targetPosition,
                elapsedTime / moveDuration
            );
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        bomb.transform.position = targetPosition;
    }

    private GameObject CreateLaserAtPosition(Vector3 position)
    {
        position.y = 10f;

        GameObject laser = Instantiate(laserBomb, position, Quaternion.Euler(0, 0, 0));
        laser.GetComponent<NetworkObject>().Spawn();
        LineRenderer lineRenderer = laser.GetComponent<LineRenderer>();
        if (lineRenderer != null)
        {
            lineRenderer.SetPosition(0, position);
            lineRenderer.SetPosition(1, position + transform.forward * laserLength);
        }

        return laser;
    }

    private Transform GetRandomBombPosition()
    {
        int randomIndex = Random.Range(0, BoomPostion.Instance.bombPositions.Length);
        return BoomPostion.Instance.bombPositions[randomIndex];
    }

    private Transform GetRandomGunPosition(List<Transform> positions)
    {
        int randomIndex = Random.Range(0, positions.Count);
        Transform selectedPosition = positions[randomIndex];
        positions.RemoveAt(randomIndex);
        return selectedPosition;
    }
}
