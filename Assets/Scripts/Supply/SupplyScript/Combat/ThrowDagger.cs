using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowDagger : MonoBehaviour, ISupplyActive
{
    public SupplyData supplyData;
    [SerializeField] private bool isActive;
    [SerializeField] private float cooldownTime;
    public GameObject daggerPrefab;
    public float moveSpeed;
    private string playerTag = "Player";
    private string enemyTag = "Enemy";

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

        isActive = false;
        ThrowDaggerAtEnemy();
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
    private IEnumerator ThrowDaggerAtEnemy()
    {
        if (playerTransform == null)
        {
            Debug.Log("Không tìm thấy player");
            yield break;
        }

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        if (enemies.Length == 0)
        {
            Debug.Log("Không có enemy");
            yield break;
        }

        GameObject targetEnemy = enemies[Random.Range(0, enemies.Length)];
        GameObject dagger1 = Instantiate(daggerPrefab, playerTransform.position, Quaternion.identity);
        DaggerCollision daggerCollision1 = dagger1.GetComponent<DaggerCollision>();
        if (daggerCollision1 == null)
        {
            Debug.LogError("Dagger prefab is missing the DaggerCollision script!");
            yield break;
        }
        StartCoroutine(MoveDaggerToTarget(dagger1, targetEnemy.transform.position));

        yield return new WaitForSeconds(0.2f);

        enemies = GameObject.FindGameObjectsWithTag("Enemy");

        if (enemies.Length == 0)
        {
            Debug.Log("Không còn enemy để ném dao thứ hai.");
            yield break;
        }

        GameObject targetEnemy2 = enemies[Random.Range(0, enemies.Length)];
        GameObject dagger2 = Instantiate(daggerPrefab, playerTransform.position, Quaternion.identity);
        DaggerCollision daggerCollision2 = dagger2.GetComponent<DaggerCollision>();
        if (daggerCollision2 == null)
        {
            Debug.LogError("Dagger prefab is missing the DaggerCollision script!");
            yield break;
        }
        StartCoroutine(MoveDaggerToTarget(dagger2, targetEnemy2.transform.position));
    }

    private IEnumerator MoveDaggerToTarget(GameObject Dagger, Vector3 targetPosition)
    {
        Vector3 direction = targetPosition - Dagger.transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        Dagger.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle - 90f));

        while (Vector3.Distance(Dagger.transform.position, targetPosition) > 0.1f)
        {
            Dagger.transform.position = Vector3.MoveTowards(
                Dagger.transform.position,
                targetPosition,
                moveSpeed * Time.deltaTime
            );

            yield return null;
        }

        Dagger.transform.position = targetPosition;

        Rigidbody2D rb = Dagger.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            DaggerCollision dag = Dagger.GetComponent<DaggerCollision>();
            if (dag != null)
            {
                dag.DaggerDamage();
                SpriteRenderer spriteRenderer = dag.GetComponentInChildren<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    spriteRenderer.enabled = false;
                }
                yield return new WaitForSeconds(5f);
                Destroy(dag.gameObject);
            }
        }
    }
}
