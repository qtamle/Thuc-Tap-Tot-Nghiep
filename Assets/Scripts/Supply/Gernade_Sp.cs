using UnityEngine;
using System.Collections;

public class Gernade_Sp : MonoBehaviour, ISupplyActive
{
    public SupplyData supplyData;
    [SerializeField] private bool isActive;
    [SerializeField] private float cooldownTime;
    public GameObject gernadePrefab;
    public float moveSpeed;
    private string playerTag = "Player";

    private Transform playerTransform;

    public float CooldownTime => cooldownTime;

    private void Awake()
    {
        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogError("Player not found! Make sure the Player has the correct tag.");

            int playerLayer = LayerMask.NameToLayer("Player");
            if (playerLayer != -1)
            {
                GameObject[] objectsInLayer = FindObjectsOfType<GameObject>();
                foreach (GameObject obj in objectsInLayer)
                {
                    if (obj.layer == playerLayer)
                    {
                        playerTransform = obj.transform;
                        Debug.Log($"Player found using layer: {obj.name}");
                        break;
                    }
                }
            }
            if (playerTransform == null)
            {
                Debug.LogError("Player not found! Make sure the Player has the correct tag or layer.");
            }
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
        CanActive();
    }

    private void ThrowGernade()
    {
        if (playerTransform == null)
        {
            Debug.Log("Không tìm thấy player");
            return;
        }

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        if (enemies.Length == 0)
        {
            Debug.Log("Không có enemy");
            return;
        }

        GameObject targetEnemy = enemies[Random.Range(0, enemies.Length)];

        GameObject gernade = Instantiate(gernadePrefab, playerTransform.position, Quaternion.identity);

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
        
        Rigidbody2D rb = gernade.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            GernadeCollision ger = gernade.GetComponent<GernadeCollision>();
            if (ger != null)
            {
                yield return StartCoroutine(ger.Explode()); 
                SpriteRenderer spriteRenderer = ger.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    spriteRenderer.enabled = false;
                }
                yield return new WaitForSeconds(5f);
                Destroy(ger.gameObject);
            }
        }
    }
}
