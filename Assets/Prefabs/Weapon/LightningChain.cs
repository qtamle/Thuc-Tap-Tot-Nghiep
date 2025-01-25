using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEngine.GraphicsBuffer;

public class LightningChain : MonoBehaviour
{
    [Header("Radius Check")]
    public float searchRadius = 10f; 

    [Header("Enemy Max")]
    public int maxEnemies = 4; 
    public LineRenderer lineRenderer;

    [Header("Setting Lightning Chain")]
    public float jitterAmount = 0.2f; // Độ lệch ngẫu nhiên của tia sét
    public float duration = 0.5f; // Thời gian sống của tia sét
    public int pointsBetweenTargets = 5; // Số điểm trung gian giữa mỗi mục tiêu
    public float updateInterval = 0.05f; // Khoảng thời gian giữa mỗi lần cập nhật tia sét

    [Header("Layer Enemy")]
    public LayerMask enemyLayer;

    [Header("Coins")]
    public GameObject coinPrefab;
    public GameObject secondaryCoinPrefab;
    public LayerMask groundLayer;

    [Header("Experience Orbs")]
    public GameObject experienceOrbPrefab;
    public float orbLaunchForce = 5f;
    public float orbMoveToPlayer = 15f;
    public float orbMoveDelay = 2f;

    [Header("Settings Amount")]
    public float coinSpawnMin = 3;
    public float coinSpawnMax = 6;
    public float secondaryCoinSpawnMin = 2;
    public float secondaryCoinSpawnMax = 4;

    private bool isLightningActive = false;
    private Transform[] targets;
    private Transform player;

    private IEnemySpawner[] enemySpawners;
    private CoinPoolManager coinPoolManager;
    private ExperienceOrbPoolManager orbPoolManager;
    private CoinsManager coinsManager;

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
        OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        lineRenderer = GetComponent<LineRenderer>();
        enemySpawners = FindObjectsOfType<MonoBehaviour>().OfType<IEnemySpawner>().ToArray();

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject == null)
        {
            int playerLayer = LayerMask.NameToLayer("Player");
            if (playerLayer >= 0)
            {
                playerObject = FindObjectsOfType<GameObject>().FirstOrDefault(obj => obj.layer == playerLayer);
            }
        }

        player = playerObject.transform;

        if (player != null)
        {
            Debug.Log("Tim thay Player!!!!!!!!!!!");
        }

        coinPoolManager = FindFirstObjectByType<CoinPoolManager>();
        orbPoolManager = FindFirstObjectByType<ExperienceOrbPoolManager>();
        coinsManager = UnityEngine.Object.FindFirstObjectByType<CoinsManager>();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void TriggerLightning()
    {
        if (isLightningActive) return;

        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, searchRadius, enemyLayer);
        int enemyCount = 0;

        targets = new Transform[maxEnemies];

        foreach (Collider2D collider in hitColliders)
        {
            if (collider.CompareTag("Enemy"))
            {
                if (enemyCount < maxEnemies)
                {
                    targets[enemyCount] = collider.transform;
                    enemyCount++;
                }
            }
        }

        if (enemyCount == 0)
        {
            Debug.Log("No enemies found to trigger lightning.");
            return;
        }

        isLightningActive = true;
        StartCoroutine(LightningEffect(enemyCount));
    }
    private IEnumerator LightningEffect(int enemyCount)
    {
        if (lineRenderer == null)
        {
            Debug.LogError("LineRenderer is not assigned!");
            yield break;
        }

        if (targets == null || targets.Length == 0)
        {
            Debug.LogError("Targets array is empty or not initialized.");
            yield break;
        }

        float elapsedTime = 0f;
        int totalPoints = 0;

        if (enemyCount == 1)
        {
            totalPoints = 2; 
            lineRenderer.positionCount = totalPoints;
        }
        else
        {
            totalPoints = (enemyCount - 1) * (pointsBetweenTargets + 1) + 1;
            lineRenderer.positionCount = totalPoints;
        }

        while (elapsedTime < duration)
        {
            if (enemyCount == 1)
            {
                lineRenderer.SetPosition(0, transform.position); 

                if (targets[0] != null)
                {
                    lineRenderer.SetPosition(1, targets[0].position);
                }
            }
            else
            {
                lineRenderer.SetPosition(0, transform.position); 

                int pointsToDraw = Mathf.FloorToInt(elapsedTime / duration * totalPoints);
                pointsToDraw = Mathf.Min(pointsToDraw, totalPoints);

                int pointIndex = 1; 
                for (int i = 0; i < enemyCount - 1; i++)
                {
                    Vector3 start = targets[i] != null ? targets[i].position : Vector3.zero;
                    Vector3 end = targets[i + 1] != null ? targets[i + 1].position : Vector3.zero;

                    for (int j = 0; j <= pointsBetweenTargets; j++)
                    {
                        float t = (float)j / pointsBetweenTargets;
                        Vector3 pointPos = Vector3.Lerp(start, end, t);
                        pointPos += new Vector3(Random.Range(-jitterAmount, jitterAmount), Random.Range(-jitterAmount, jitterAmount), 0);

                        if (pointIndex < lineRenderer.positionCount)
                        {
                            lineRenderer.SetPosition(pointIndex, pointPos);
                        }
                        pointIndex++;
                    }
                }

                if (lineRenderer.positionCount > 1 && targets[enemyCount - 1] != null)
                {
                    lineRenderer.SetPosition(lineRenderer.positionCount - 1, targets[enemyCount - 1].position);
                }
            }

            elapsedTime += updateInterval;
            yield return new WaitForSeconds(updateInterval);
        }

        lineRenderer.positionCount = 0;
        isLightningActive = false;

        foreach (Transform target in targets)
        {
            if (target != null)
            {
                HandleEnemyCollision(target.GetComponent<Collider2D>());
            }
        }
    }

    private void HandleEnemyCollision(Collider2D enemy)
    {
        if (enemy != null && enemy.gameObject.activeInHierarchy)
        {

            Vector3 position = enemy.transform.position;

            if (EnemyManager.Instance != null)
            {
                EnemyManager.Instance.OnEnemyKilled();
            }

            foreach (IEnemySpawner spawner in enemySpawners)
            {
                spawner.OnEnemyKilled();
            }

            GameObject rootObject = enemy.transform.root.gameObject;
            Destroy(rootObject);

            SpawnCoins(coinPrefab, coinSpawnMin, coinSpawnMax, position);

            if (Random.value <= 0.30f)
            {
                SpawnCoins(secondaryCoinPrefab, secondaryCoinSpawnMin, secondaryCoinSpawnMax, position);
            }

            SpawnExperienceOrbs(position, 5);
        }
    }

    void SpawnCoins(GameObject coinType, float minAmount, float maxAmount, Vector3 position)
    {
        int coinCount = Random.Range((int)minAmount, (int)maxAmount + 1);

        for (int i = 0; i < coinCount; i++)
        {
            Vector3 spawnPosition = position + Vector3.up * 0.2f;

            GameObject coin = coinPoolManager.GetCoinFromPool(coinType);
            coin.transform.position = spawnPosition;

            Rigidbody2D coinRb = coin.GetComponent<Rigidbody2D>();

            if (coinRb != null)
            {
                Vector2 forceDirection = new Vector2(Random.Range(-1.5f, 1.5f), Random.Range(1f, 1f)) * 2.5f;
                coinRb.AddForce(forceDirection, ForceMode2D.Impulse);

                StartCoroutine(CheckIfCoinIsStuck(coinRb));
            }

            CoinsScript coinScript = coin.GetComponent<CoinsScript>();
            if (coinScript != null)
            {
                if (coinType == coinPrefab)
                    coinScript.SetCoinType(true, false);
                else
                    coinScript.SetCoinType(false, true);

            }
        }
    }

    private IEnumerator CheckIfCoinIsStuck(Rigidbody2D coinRb)
    {
        yield return new WaitForSeconds(0.1f);

        if (coinRb != null)
        {
            RaycastHit2D hit = Physics2D.Raycast(coinRb.transform.position, Vector2.down, 0.5f, groundLayer);
            if (hit.collider != null)
            {
                coinRb.transform.position += Vector3.up * 0.3f;
                coinRb.AddForce(Vector2.up * 2f, ForceMode2D.Impulse);
            }
        }
        else
        {
            yield break;
        }
    }
    void SpawnExperienceOrbs(Vector3 position, int orbCount)
    {
        for (int i = 0; i < orbCount; i++)
        {
            float randomAngle = Random.Range(0f, 360f);

            float orbX = position.x + Mathf.Cos(randomAngle * Mathf.Deg2Rad);
            float orbY = position.y + Mathf.Sin(randomAngle * Mathf.Deg2Rad);
            Vector3 spawnPosition = new Vector3(orbX, orbY, position.z);

            GameObject orb = orbPoolManager.GetOrbFromPool(spawnPosition);
            Rigidbody2D orbRb = orb.GetComponent<Rigidbody2D>();

            if (orbRb != null)
            {
                Vector2 forceDirection = (spawnPosition - position).normalized * orbLaunchForce;
                orbRb.AddForce(forceDirection, ForceMode2D.Impulse);

                orbRb.bodyType = RigidbodyType2D.Kinematic;

                StartCoroutine(MoveOrbToPlayer(orb, orbMoveDelay));
            }
        }
    }

    IEnumerator MoveOrbToPlayer(GameObject orb, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (orb != null && player != null)
        {
            Rigidbody2D orbRb = orb.GetComponent<Rigidbody2D>();
            if (orbRb != null)
            {
                while (orb != null && player != null)
                {
                    Vector3 direction = (player.position - orb.transform.position).normalized;

                    orbRb.MovePosition(orb.transform.position + direction * Time.deltaTime * orbMoveToPlayer);

                    if (Vector3.Distance(orb.transform.position, player.position) < 0.5f)
                    {
                        orbPoolManager.ReturnOrbToPool(orb);
                        yield break;
                    }

                    yield return null;
                }
            }
        }
        else
        {
            // Nếu orb hoặc player bị xóa, dừng Coroutine
            yield break;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, searchRadius);
    }
}
