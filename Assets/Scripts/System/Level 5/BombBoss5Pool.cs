using System.Collections.Generic;
using UnityEngine;

public class BombBoss5Pool : MonoBehaviour
{
    public GameObject bombPrefab;
    public int poolSize;

    private Queue<GameObject> bombPool;

    private void Awake()
    {
        bombPool = new Queue<GameObject>();

        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(bombPrefab, transform);
            obj.SetActive(false);
            bombPool.Enqueue(obj);
        }
    }

    public GameObject GetBomb()
    {
        if (bombPool.Count > 0)
        {
            GameObject obj = bombPool.Dequeue();
            obj.SetActive(true);

            CircleCollider2D circleCollider2D = obj.GetComponent<CircleCollider2D>();
            if (circleCollider2D != null)
            {
                circleCollider2D.enabled = true;
            }

            Rigidbody2D rigidbody2D = obj.GetComponent<Rigidbody2D>();
            if (rigidbody2D != null)
            {
                rigidbody2D.simulated = true;
                rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
                rigidbody2D.linearVelocity = Vector2.zero;
            }

            return obj;
        }
        else
        {
            GameObject obj = Instantiate(bombPrefab, transform);
            return obj;
        }
    }

    public void ReturnBomb(GameObject bomb)
    {
        bomb.SetActive(false);
        bombPool.Enqueue(bomb);
    }
}
