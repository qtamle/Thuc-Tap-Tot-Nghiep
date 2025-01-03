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

    private List<GameObject> thrownEnemies = new List<GameObject>();
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

    private void ThrowDaggerAtEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);
        if (enemies.Length == 0)
        {
            Debug.Log("No enemies found to throw daggers at.");
            return;
        }

        List<GameObject> availableEnemies = new List<GameObject>();
        foreach (GameObject enemy in enemies)
        {
            if (!thrownEnemies.Contains(enemy) && IsWithinRange(enemy))  
            {
                availableEnemies.Add(enemy);
            }
        }

        if (availableEnemies.Count == 0)
        {
            Debug.Log("No available enemies to throw daggers at.");
            return;
        }

        GameObject randomEnemy = availableEnemies[Random.Range(0, availableEnemies.Count)];
        thrownEnemies.Add(randomEnemy);
        StartCoroutine(ThrowDaggerCoroutine(randomEnemy));
    }

    private bool IsWithinRange(GameObject enemy)
    {
        float maxRange = 50f; 
        float distance = Vector3.Distance(playerTransform.position, enemy.transform.position);
        return distance <= maxRange; 
    }


    private IEnumerator ThrowDaggerCoroutine(GameObject enemy)
    {
        if (daggerPrefab == null || playerTransform == null)
        {
            Debug.LogError("Dagger prefab or player transform is not set.");
            yield break;
        }

        GameObject dagger = Instantiate(daggerPrefab, playerTransform.position, Quaternion.identity);
        Vector3 direction = (enemy.transform.position - playerTransform.position).normalized;

        Rigidbody2D rb = dagger.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = direction * moveSpeed;
        }
        else
        {
            Debug.LogError("Dagger prefab does not have a Rigidbody2D component.");
            yield break;
        }

        float travelTime = Vector3.Distance(playerTransform.position, enemy.transform.position) / moveSpeed;
        yield return new WaitForSeconds(travelTime);

        if (enemy != null)
        {
            DaggerCollision daggerDamage = dagger.GetComponent<DaggerCollision>();
            if (daggerDamage != null)
            {
                daggerDamage.DaggerDamage();
            }
        }

        SpriteRenderer spriteRenderer = dagger.GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }

        yield return new WaitForSeconds(5f);
        Destroy(dagger);
    }
}
