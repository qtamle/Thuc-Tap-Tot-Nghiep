using System.Linq;
using UnityEngine;

[RequireComponent(typeof(LineRenderer), typeof(EdgeCollider2D))]
public class LaserCollider : MonoBehaviour
{
    private LineRenderer lineRenderer;
    private EdgeCollider2D edgeCollider;

    [Header("Enemy Settings")]
    public LayerMask enemyLayer;

    private IEnemySpawner[] enemySpawners;
    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        edgeCollider = GetComponent<EdgeCollider2D>();
    }

    void Start()
    {
        UpdateCollider();

        enemySpawners = FindObjectsOfType<MonoBehaviour>().OfType<IEnemySpawner>().ToArray();
    }

    /// <summary>
    /// Cập nhật các điểm của EdgeCollider2D dựa trên LineRenderer
    /// </summary>
    public void UpdateCollider()
    {
        if (lineRenderer == null || edgeCollider == null) return;

        int pointCount = lineRenderer.positionCount;

        Vector2[] edgePoints = new Vector2[pointCount];

        for (int i = 0; i < pointCount; i++)
        {
            Vector3 linePoint = lineRenderer.GetPosition(i);
            edgePoints[i] = new Vector2(linePoint.x, linePoint.y);
        }

        edgeCollider.points = edgePoints;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            DamagePlayerInterface damage = collision.GetComponent<DamagePlayerInterface>();

            if (damage != null)
            {
                damage.DamagePlayer(2);
            }
        }

        if ((enemyLayer.value & (1 << collision.gameObject.layer)) > 0) 
        {
            HandleEnemyCollision(collision);
        }
    }

    private void HandleEnemyCollision(Collider2D enemy)
    {
        if (enemy != null && enemy.gameObject.activeInHierarchy)
        {
            if (EnemyManager.Instance != null)
            {
                EnemyManager.Instance.OnEnemyKilled();
            }

            foreach (IEnemySpawner spawner in enemySpawners)
            {
                spawner.OnEnemyKilled();
            }

            Destroy(enemy.gameObject);
        }
    }
}
