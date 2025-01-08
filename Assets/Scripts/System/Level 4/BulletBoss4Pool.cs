using System.Collections.Generic;
using UnityEngine;

public class BulletBoss4Pool : MonoBehaviour
{
    public GameObject bullet;
    public int poolSize;

    private Queue<GameObject> queuePool;

    private void Awake()
    {
        queuePool = new Queue<GameObject>();

        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(bullet, transform);
            obj.SetActive(false);
            queuePool.Enqueue(obj);
        }
    }
    public GameObject GetBullet()
    {
        if (queuePool.Count > 0)
        {
            GameObject obj = queuePool.Dequeue();
            obj.SetActive(true);

            BulletSnakeDamage bullet = obj.GetComponent<BulletSnakeDamage>();
            if (bullet != null)
            {
                bullet.enabled = true;
            }

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
            GameObject obj = Instantiate(bullet, transform);
            return obj;
        }
    }


    public void ReturnBullet(GameObject bulletSnake)
    {
        bulletSnake.SetActive(false);
        queuePool.Enqueue(bulletSnake);
    }

}
