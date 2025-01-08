using System.Collections.Generic;
using UnityEngine;

public class RockPool : MonoBehaviour
{
    public GameObject rock;
    public int poolSize;

    private Queue<GameObject> queuePool;

    private void Awake()
    {
        queuePool = new Queue<GameObject>();

        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(rock, transform);
            obj.SetActive(false);
            queuePool.Enqueue(obj);
        }
    }
    public GameObject GetRock()
    {
        if (queuePool.Count > 0)
        {
            GameObject obj = queuePool.Dequeue();
            obj.SetActive(true);

            BulletDamage bullet = obj.GetComponent<BulletDamage>();
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
                rigidbody2D.bodyType = RigidbodyType2D.Dynamic;
                rigidbody2D.linearVelocity = Vector2.zero;
            }

            return obj;
        }
        else
        {
            GameObject obj = Instantiate(rock, transform);
            return obj;
        }
    }


    public void ReturnRock(GameObject rockDamage)
    {
        rockDamage.SetActive(false);
        queuePool.Enqueue(rockDamage);
    }
}
