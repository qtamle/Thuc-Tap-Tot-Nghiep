using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnakeSkill : MonoBehaviour
{
    [Header("Bullet")]
    public float bulletSpeed = 8f;
    public float fireRate = 1f;

    [Header("Fire Points")]
    public Transform[] firePoints;

    [Header("Volley Settings")]
    public int volleys = 5;
    public float volleyInterval = 2f;

    [Header("Laser Settings")]
    public GameObject laserPrefab;

    public bool isFiring = false;
    private SnakeHealth snake;
    private BulletBoss4Pool bulletBoss4Pool;

    private void Start()
    {
        bulletBoss4Pool = FindFirstObjectByType<BulletBoss4Pool>();
        snake = GetComponent<SnakeHealth>();
    }

    private void Update()
    {
        CheckHealth();
    }

    void CheckHealth()
    {
        if (snake != null)
        {
            if (snake.currentHealth <= 0)
            {
                StopAllCoroutines();
            }
        }
    }

    public void StartFiring(Vector2 direction)
    {
        if (!isFiring)
        {
            if (snake != null)
            {
                snake.StunForDuration(12f);
            }
            StartCoroutine(FireMultipleVolleys(direction));
        }
    }

    private IEnumerator FireMultipleVolleys(Vector2 direction)
    {
        isFiring = true;

        if (direction == Vector2.left || direction == Vector2.right || direction == Vector2.up || direction == Vector2.down)
        {
            if (direction == Vector2.right)
            {
                FireSequentialVolley(direction, 0, 3);
                yield return new WaitForSeconds(1f);

                FireSequentialVolley(direction, 3, firePoints.Length);
                yield return new WaitForSeconds(1f); 

                yield return FireSingleVolley(direction, 5, 1f);

                yield return FireReverseSingleVolley(direction, 6, 3, 1f);

                yield return new WaitForSeconds(1f);

                FireSequentialVolley(direction, 0, 4);
            }
            else if (direction == Vector2.left)
            {
                FireSequentialVolley(direction, 0, 4);
                yield return new WaitForSeconds(1f);

                for (int volleyCount = 1; volleyCount < volleys; volleyCount++)
                {
                    FireRandomSingleBullet(direction);
                    yield return new WaitForSeconds(1.5f);
                }

                yield return new WaitForSeconds(1f);

                FireSequentialVolley(direction, 0, 2);
                yield return new WaitForSeconds(1f);
                FireSequentialVolley(direction, 2, 4);
                yield return new WaitForSeconds(1f);
                FireSequentialVolley(direction, 4, firePoints.Length);
            }
            else if (direction == Vector2.up)
            {
                FireLaserPairVolleyUp(direction, 0, 1);
                yield return new WaitForSeconds(2.5f);

                FireLaserPairVolleyUp(direction, 1, 2);
                yield return new WaitForSeconds(2.5f);

                FireLaserPairVolleyUp(direction, 2, 3);
                yield return new WaitForSeconds(3f);

                FireLaserSequentialVolleyUp(direction, 3, firePoints.Length);
                yield return new WaitForSeconds(1f);
            }
            else if (direction == Vector2.down)
            {
                FireLaserSequentialVolley(direction, 4, 6);
                yield return new WaitForSeconds(2.5f);

                FireLaserSequentialVolley(direction, 3, Mathf.Min(5, firePoints.Length));
                yield return new WaitForSeconds(2.5f);

                FireLaserPairVolley(direction, 0, 6);
                yield return new WaitForSeconds(2.5f);

                FireLaserPairVolley(direction, 1, 5);
                yield return new WaitForSeconds(2.5f);

                FireLaserPairVolley(direction, 2, 4);

                yield return new WaitForSeconds(1f);
            }
        }

        isFiring = false;
    }
    private void FireLaser(Transform firePoint)
    {
        Vector3 spawnPosition = firePoint.position + new Vector3(-2f, -0.15f, 0f);  

        GameObject laser = Instantiate(laserPrefab, spawnPosition, Quaternion.Euler(0, 0, -90));

        LineRenderer lineRenderer = laser.GetComponent<LineRenderer>();

        if (lineRenderer != null)
        {
            lineRenderer.startWidth = 0.5f;
            lineRenderer.endWidth = 0.5f;
            lineRenderer.SetPosition(0, spawnPosition);
            lineRenderer.SetPosition(1, spawnPosition + new Vector3(0, 5f, 0)); 
        }

        StartCoroutine(DestroyLaserAfterDuration(laser, 1.5f));
    }

    private void FireLaserPairVolleyUp(Vector2 direction, int index1, int index2)
    {
        if (index1 < firePoints.Length && index2 < firePoints.Length)
        {
            Transform firePoint1 = firePoints[index1];
            Transform firePoint2 = firePoints[index2];

            FireLaser(firePoint1);
            FireLaser(firePoint2);
        }
    }

    private void FireLaserSequentialVolleyUp(Vector2 direction, int startIndex, int endIndex)
    {
        int maxTransformsShoot = Mathf.Min(endIndex, firePoints.Length);

        for (int i = startIndex; i < maxTransformsShoot; i++)
        {
            Transform firePoint = firePoints[i];
            FireLaser(firePoint);
        }
    }

    private void FireLaserDown(Transform firePoint)
    {
        Vector3 spawnPosition = firePoint.position + new Vector3(2f, 0.1f, 0f);

        GameObject laser = Instantiate(laserPrefab, spawnPosition, Quaternion.Euler(0, 0, 90));  

        LineRenderer lineRenderer = laser.GetComponent<LineRenderer>();

        if (lineRenderer != null)
        {
            lineRenderer.startWidth = 0.5f;
            lineRenderer.endWidth = 0.5f;
            lineRenderer.SetPosition(0, spawnPosition);
            lineRenderer.SetPosition(1, spawnPosition + new Vector3(0, 5f, 0));  
        }

        StartCoroutine(DestroyLaserAfterDuration(laser, 2f));
    }

    private void FireLaserPairVolley(Vector2 direction, int index1, int index2)
    {
        if (index1 < firePoints.Length && index2 < firePoints.Length)
        {
            Transform firePoint1 = firePoints[index1];
            Transform firePoint2 = firePoints[index2];

            FireLaserDown(firePoint1);
            FireLaserDown(firePoint2);
        }
    }

    private void FireLaserSequentialVolley(Vector2 direction, int startIndex, int endIndex)
    {
        int maxTransformsShoot = Mathf.Min(endIndex, firePoints.Length);

        for (int i = startIndex; i < maxTransformsShoot; i++)
        {
            Transform firePoint = firePoints[i];
            FireLaserDown(firePoint);
        }
    }

    private IEnumerator DestroyLaserAfterDuration(GameObject laser, float duration)
    {
        yield return new WaitForSeconds(duration);
        Destroy(laser); 
    }

    private void FireSequentialVolley(Vector2 direction, int startIndex, int endIndex)
    {
        Vector2 fireDirection = GetFireDirection(direction);
        int maxTransformsShoot = Mathf.Min(endIndex, firePoints.Length);

        for (int i = startIndex; i < maxTransformsShoot; i++)
        {
            Transform firePoint = firePoints[i];
            FireBullet(firePoint, fireDirection);
        }
    }

    private IEnumerator FireSingleVolley(Vector2 direction, int endTransformIndex, float delay)
    {
        Vector2 fireDirection = GetFireDirection(direction);
        for (int i = 0; i < endTransformIndex; i++)
        {
            Transform firePoint = firePoints[i];
            FireBullet(firePoint, fireDirection);
            yield return new WaitForSeconds(delay);
        }
    }

    private IEnumerator FireReverseSingleVolley(Vector2 direction, int startTransformIndex, int endTransformIndex, float delay)
    {
        Vector2 fireDirection = GetFireDirection(direction);
        for (int i = startTransformIndex; i >= endTransformIndex; i--)
        {
            Transform firePoint = firePoints[i];
            FireBullet(firePoint, fireDirection);
            yield return new WaitForSeconds(delay);
        }
    }

    private void FireRandomVolley(Vector2 direction, int start, int end)
    {
        for (int i = start; i < end; i++)
        {
            Transform firePoint = firePoints[i];
            Vector2 fireDirection = GetFireDirection(direction);
            FireBullet(firePoint, fireDirection);
        }
    }

    private void FireRandomSingleBullet(Vector2 direction)
    {
        if (firePoints.Length == 0) return;

        int randomIndex = Random.Range(0, firePoints.Length);
        Transform firePoint = firePoints[randomIndex];

        Vector2 fireDirection = GetFireDirection(direction);
        FireBullet(firePoint, fireDirection);
    }

    private Vector2 GetFireDirection(Vector2 direction)
    {
        if (direction == Vector2.left)
            return Vector2.down;
        else if (direction == Vector2.right)
            return Vector2.up;
        else if (direction == Vector2.up)
            return Vector2.left;
        else if (direction == Vector2.down)
            return Vector2.right;

        return Vector2.zero;
    }

    private void FireBullet(Transform firePoint, Vector2 fireDirection)
    {
        GameObject bullet = bulletBoss4Pool.GetBullet();

        bullet.transform.position = firePoint.position;

        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = fireDirection.normalized * bulletSpeed;
        }
    }
}
