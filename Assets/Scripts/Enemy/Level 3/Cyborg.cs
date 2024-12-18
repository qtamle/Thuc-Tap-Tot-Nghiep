using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Cyborg : MonoBehaviour
{
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

    private bool isSkillActive = false;
    private Vector3 originalPosition;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P) && !isSkillActive)
        {
            StartCoroutine(DiscoBallSkill());
        }

        if (Input.GetKeyDown(KeyCode.O) && !isSkillActive)
        {
            StartCoroutine(TurretSkill());
        }

        if (Input.GetKeyDown(KeyCode.L) && !isSkillActive)  
        {
            StartCoroutine(MoveAndFireLaser());
        }

        if (Input.GetKeyDown(KeyCode.B) && !isSkillActive)
        {
            StartCoroutine(BombSkill());
        }
    }

    private IEnumerator DiscoBallSkill()
    {
        isSkillActive = true;

        Vector3 startPosition = spawnPoint.position;
        Vector3 targetPosition = Vector3.zero;
        orb = Instantiate(orbPrefab, startPosition, Quaternion.identity);

        float moveDuration = 2f;
        float elapsedTime = 0f;

        while (elapsedTime < moveDuration)
        {
            orb.transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / moveDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        orb.transform.position = targetPosition;

        yield return new WaitForSeconds(1f);
        GameObject[] plusLasers = CreatePlusLasers(targetPosition);

        yield return new WaitForSeconds(2f);
        foreach (GameObject laser in plusLasers)
        {
            Destroy(laser);
        }
        GameObject[] xLasers = CreateXLasers(targetPosition);

        yield return new WaitForSeconds(2f);
        foreach (GameObject laser in xLasers)
        {
            Destroy(laser);
        }

        yield return new WaitForSeconds(0.5f);
        Destroy(orb);

        isSkillActive = false;
    }

    private GameObject[] CreatePlusLasers(Vector3 centerPosition)
    {
        GameObject[] lasers = new GameObject[4];

        lasers[0] = CreateLaser(centerPosition, new Vector3(0, 0, 0).normalized, Quaternion.Euler(0, 0, 180));
        lasers[1] = CreateLaser(centerPosition, new Vector3(0, 0, 0).normalized, Quaternion.Euler(0, 0, 0));
        lasers[2] = CreateLaser(centerPosition, new Vector3(0, 0, 0).normalized, Quaternion.Euler(0, 0, -90));
        lasers[3] = CreateLaser(centerPosition, new Vector3(0, 0, 0).normalized, Quaternion.Euler(0, 0, 90));

        return lasers;
    }

    private GameObject[] CreateXLasers(Vector3 centerPosition)
    {
        GameObject[] lasers = new GameObject[2];

        Vector3 positionDownRight = new Vector3(8, 8, 0);
        Vector3 positionDownLeft = new Vector3(-8, 8, 0);
        lasers[0] = CreateLaser(positionDownRight, new Vector3(8, 8, 0).normalized, Quaternion.Euler(0, 0, -45));
        lasers[1] = CreateLaser(positionDownLeft, new Vector3(-8, 8, 0).normalized, Quaternion.Euler(0, 0, 45));

        return lasers;
    }

    private GameObject CreateLaser(Vector3 startPosition, Vector3 direction, Quaternion rotation)
    {
        GameObject lineObject = Instantiate(linePrefab);
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

        List<Transform> availablePositions = new List<Transform>(gunPositions);
        Transform position1 = GetRandomGunPosition(availablePositions);
        Transform position2 = GetRandomGunPosition(availablePositions);

        GameObject turret1 = Instantiate(turretPrefab, position1.position, Quaternion.identity);
        GameObject turret2 = Instantiate(turretPrefab, position2.position, Quaternion.identity);

        Turret turretScript1 = turret1.GetComponent<Turret>();
        Turret turretScript2 = turret2.GetComponent<Turret>();

        turretScript1.InitializeTurret();
        turretScript2.InitializeTurret();

        yield return new WaitForSeconds(4.5f);

        Destroy(turret1);
        Destroy(turret2);

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
            transform.position = Vector3.Lerp(transform.position, targetPosition, elapsedTime / moveDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition; 

        FireLaser();

        yield return new WaitForSeconds(1.5f);
    }

    private void FireLaser()
    {
        GameObject laser = Instantiate(laserPrefab, laserSpawnPoint.position, Quaternion.Euler(0, 0, 0));
        LineRenderer lineRenderer = laser.GetComponent<LineRenderer>();
        if (lineRenderer != null)
        {
            lineRenderer.SetPosition(0, laserSpawnPoint.position);
            lineRenderer.SetPosition(1, laserSpawnPoint.position + transform.right * 5f);  
        }

        Destroy(laser, laserDuration); 
    }

    private IEnumerator BombSkill()
    {
        isSkillActive = true;

        List<GameObject> bombs = new List<GameObject>();

        for (int i = 0; i < 3; i++)
        {
            Transform bombPosition = GetRandomBombPosition();
            GameObject bomb = Instantiate(bombPrefab, spawnPosition.position, Quaternion.identity); 

            bombs.Add(bomb);

            StartCoroutine(MoveBombToPosition(bomb, bombPosition.position));
        }

        yield return new WaitForSeconds(throwSpeed);

        foreach (GameObject bomb in bombs)
        {
            GameObject laser = CreateLaserAtPosition(bomb.transform.position);
            Destroy(laser, laserDuration);
            Destroy(bomb);
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

    private GameObject CreateLaserAtPosition(Vector3 position)
    {
        position.y = 10f; 

        GameObject laser = Instantiate(laserBomb, position, Quaternion.Euler(0, 0, 0));

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
        int randomIndex = Random.Range(0, bombPositions.Length);
        return bombPositions[randomIndex];
    }

    private Transform GetRandomGunPosition(List<Transform> positions)
    {
        int randomIndex = Random.Range(0, positions.Count);
        Transform selectedPosition = positions[randomIndex];
        positions.RemoveAt(randomIndex);
        return selectedPosition;
    }
}
