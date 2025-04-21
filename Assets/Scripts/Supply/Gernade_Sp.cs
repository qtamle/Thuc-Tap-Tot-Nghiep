using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class Gernade_Sp : NetworkBehaviour, ISupplyActive
{
    public SupplyData supplyData;
    [SerializeField] private bool isActive;
    [SerializeField] private float cooldownTime;
    public GameObject gernadePrefab;
    public float moveSpeed;
    //private string playerTag = "Player";

    private Transform playerTransform;

    public float CooldownTime => cooldownTime;
    private bool hasThrown = false;

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
    //    while (playerTransform == null)
    //    {
    //        FindPlayer();
    //        yield return new WaitForSeconds(1f);
    //    }
    //}

    //private void FindPlayer()
    //{
    //    GameObject player = GameObject.FindGameObjectWithTag(playerTag);
    //    if (player != null)
    //    {
    //        playerTransform = player.transform;
    //        Debug.Log("Player found!");
    //    }
    //    else
    //    {
    //        Debug.LogError("Player not found! Make sure the Player has the correct tag.");

    //        int playerLayer = LayerMask.NameToLayer("Player");
    //        if (playerLayer != -1)
    //        {
    //            GameObject[] objectsInLayer = FindObjectsOfType<GameObject>();
    //            foreach (GameObject obj in objectsInLayer)
    //            {
    //                if (obj.layer == playerLayer)
    //                {
    //                    playerTransform = obj.transform;
    //                    Debug.Log($"Player found using layer: {obj.name}");
    //                    break;
    //                }
    //            }
    //        }
    //        if (playerTransform == null)
    //        {
    //            if (transform.parent != null)
    //            {
    //                Vector3 parentPosition = transform.parent.position;
    //            }
    //            else
    //            {
    //                Debug.LogError("Player not found and no parent found!");
    //            }
    //        }
    //    }
    //}

    private void Update()
    {
        if (!hasThrown)
        {
            ThrowGernade();
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

        ThrowGernade();
        isActive = false;
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

    private void ThrowGernade()
    {
        //if (playerTransform == null)
        //{
        //    Debug.Log("Không tìm thấy player");
        //    return;
        //}

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        if (enemies.Length == 0)
        {
            Debug.Log("Không có enemy");
            return;
        }

        GameObject targetEnemy = enemies[Random.Range(0, enemies.Length)];

        GameObject gernade = Instantiate(gernadePrefab, transform.parent.position, Quaternion.identity);

        gernadePrefab.GetComponent<NetworkObject>().Spawn(true);

        GernadeCollision gernadeCollision = gernade.GetComponent<GernadeCollision>();
        if (gernadeCollision == null)
        {
            Debug.LogError("Gernade prefab is missing the GernadeCollision script!");
            return;
        }

        StartCoroutine(MoveGernadeToTarget(gernade, targetEnemy.transform.position));
    }

    private IEnumerator MoveGernadeToTarget(GameObject gernade, Vector3 targetPosition)
    {
        Rigidbody2D rb = gernade.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero;
        }

        while (Vector3.Distance(gernade.transform.position, targetPosition) > 0.1f)
        {
            gernade.transform.position = Vector3.MoveTowards(
                gernade.transform.position,
                targetPosition,
                moveSpeed * Time.deltaTime
            );
            yield return null;
        }

        gernade.transform.position = targetPosition;
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Static;
        }

        yield return StartCoroutine(gernade.GetComponent<GernadeCollision>().Explode());

        SpriteRenderer spriteRenderer = gernade.GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }

        yield return new WaitForSeconds(5f);
        NetworkObject no = gernade.GetComponent<NetworkObject>();
        if (no != null && no.IsSpawned)
        {
            no.Despawn(true);
        }
        else
        {
            Destroy(gernade);
        }
    }
}

