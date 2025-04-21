using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ThrowDagger : NetworkBehaviour, ISupplyActive
{
    public SupplyData supplyData;
    [SerializeField] private bool isActive;
    [SerializeField] private float cooldownTime;
    public GameObject daggerPrefab;
    public float moveSpeed;
    //private string playerTag = "Player";
    private string enemyTag = "Enemy";

    private Transform playerTransform;

    public float CooldownTime => cooldownTime;
    private bool hasThrown = false;
    //private bool isPlayerFound = false;

    private void Awake()
    {
        //StartCoroutine(CheckForPlayer());
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //StartCoroutine(CheckForPlayer());
        StartCoroutine(Delay());
        //Active();
    }

    IEnumerator Delay()
    {
        yield return new WaitForSeconds(0.5f);
        isActive = true;
    }

    //IEnumerator CheckForPlayer()
    //{
    //    while (!isPlayerFound)
    //    {
    //        FindPlayer();
    //        yield return new WaitForSeconds(1f);  
    //    }
    //}

    //private void FindPlayer()
    //{
    //    //GameObject player = GameObject.FindGameObjectWithTag(playerTag);
    //    //if (player != null)
    //    //{
    //    //    playerTransform = player.transform;
    //    //    Debug.Log("Player found!");
    //    //}
    //    //else
    //    //{
    //    //    Debug.LogError("Player not found! Make sure the Player has the correct tag.");

    //    //    int playerLayer = LayerMask.NameToLayer("Player");
    //    //    if (playerLayer != -1)
    //    //    {
    //    //        GameObject[] objectsInLayer = FindObjectsOfType<GameObject>();
    //    //        foreach (GameObject obj in objectsInLayer)
    //    //        {
    //    //            if (obj.layer == playerLayer)
    //    //            {
    //    //                playerTransform = obj.transform;
    //    //                Debug.Log($"Player found using layer: {obj.name}");
    //    //                break;
    //    //            }
    //    //        }
    //    //    }
    //    //    if (playerTransform == null)
    //    //    {
    //    //        Debug.LogError("Player not found! Make sure the Player has the correct tag or layer.");
    //    //    }
    //    //}

    //    if (playerTransform == null)
    //    {
    //        if (transform.parent != null)
    //        {
    //            Vector3 parentPosition = transform.parent.position;
    //            Debug.Log("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
    //            isPlayerFound = true;
    //            isActive = true;
    //            return;
    //        }
    //        else
    //        {
    //            Debug.LogError("Player not found and no parent found!");
    //        }
    //    }
    //}

    private void Update()
    {
        if (!hasThrown)
        {
            StartCoroutine(ThrowDaggerAtEnemy());
            hasThrown = true;
            isActive = false;
            StartCoroutine(CooldownRoutine());
        }
    }

    public void Active()
    {
        if (!IsReady())
        {
            return;
        }

        isActive = false;
        StartCoroutine(ThrowDaggerAtEnemy());
        StartCoroutine(CooldownRoutine());
    }

    public void CanActive()
    {
        isActive = true;
    }

    public bool IsReady()
    {
        return isActive;
    }

    private IEnumerator CooldownRoutine()
    {
        yield return new WaitForSeconds(cooldownTime);
        isActive = true;
        hasThrown = false;
    }

    private IEnumerator ThrowDaggerAtEnemy()
    {
        //if (playerTransform == null)
        //{
        //    Debug.LogError("PlayerTransform not found!");
        //    yield break;
        //}

        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);

        if (enemies.Length == 0)
        {
            Debug.Log("No enemies to target.");
            yield break;
        }

        GameObject targetEnemy = enemies[Random.Range(0, enemies.Length)];
        GameObject dagger1 = Instantiate(daggerPrefab, transform.parent.position, Quaternion.identity);
        dagger1.GetComponent<NetworkObject>().Spawn(true);

        StartCoroutine(MoveDaggerToTarget(dagger1, targetEnemy.transform.position));

        yield return new WaitForSeconds(0.2f);

        enemies = GameObject.FindGameObjectsWithTag(enemyTag);

        if (enemies.Length == 0)
        {
            Debug.Log("No more enemies to target for the second dagger.");
            yield break;
        }

        GameObject targetEnemy2 = enemies[Random.Range(0, enemies.Length)];
        GameObject dagger2 = Instantiate(daggerPrefab, transform.parent.position, Quaternion.identity);

        dagger2.GetComponent<NetworkObject>().Spawn(true);

        StartCoroutine(MoveDaggerToTarget(dagger2, targetEnemy2.transform.position));
    }

    private IEnumerator MoveDaggerToTarget(GameObject dagger, Vector3 targetPosition)
    {
        Vector3 direction = targetPosition - dagger.transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        dagger.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle - 90f));

        while (Vector3.Distance(dagger.transform.position, targetPosition) > 0.1f)
        {
            dagger.transform.position = Vector3.MoveTowards(
                dagger.transform.position,
                targetPosition,
                moveSpeed * Time.deltaTime
            );

            yield return null;
        }

        dagger.transform.position = targetPosition;

        Rigidbody2D rb = dagger.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            DaggerCollision daggerCollision = dagger.GetComponent<DaggerCollision>();
            if (daggerCollision != null)
            {
                daggerCollision.DaggerDamage();
                SpriteRenderer spriteRenderer = daggerCollision.GetComponentInChildren<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    spriteRenderer.enabled = false;
                }
                yield return new WaitForSeconds(5f);
                
                //Destroy(daggerCollision.gameObject);
            }
        }
       NetworkObject no =  dagger.GetComponent<NetworkObject>();
        if(no == null )
        {
            Debug.Log("Dagger ko co no");
        }
        else
        {
            Debug.Log("dagger co no");
            no.Despawn();
        }

    }
}
