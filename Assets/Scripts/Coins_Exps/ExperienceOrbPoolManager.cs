using System.Collections.Generic;
using UnityEngine;

public class ExperienceOrbPoolManager : MonoBehaviour
{
    public GameObject experienceOrbPrefab; 
    public int poolSize = 20;               
    private Queue<GameObject> orbPool;      

    private void Awake()
    {
        orbPool = new Queue<GameObject>();

        for (int i = 0; i < poolSize; i++)
        {
            GameObject orb = Instantiate(experienceOrbPrefab, transform);
            orb.SetActive(false); 
            orbPool.Enqueue(orb);
        }
    }

    public GameObject GetOrbFromPool(Vector3 position)
    {
        if (orbPool.Count > 0)
        {
            GameObject orb = orbPool.Dequeue();
            orb.SetActive(true);
            orb.transform.position = position;

            ExperienceScript experienceScript = orb.GetComponent<ExperienceScript>();
            if (experienceScript != null)
            {
                experienceScript.enabled = true; 
            }

            PolygonCollider2D collider = orb.GetComponent<PolygonCollider2D>();
            if (collider != null)
            {
                collider.enabled = true; 
            }

            Rigidbody2D rb = orb.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.simulated = true;    
                rb.bodyType = RigidbodyType2D.Dynamic;
                rb.linearVelocity = Vector2.zero; 
            }

            return orb;
        }
        else
        {
            GameObject orb = Instantiate(experienceOrbPrefab, position, Quaternion.identity);
            return orb;
        }
    }

    public void ReturnOrbToPool(GameObject orb)
    {
        Rigidbody2D rb = orb.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.gravityScale = 0f;
            rb.simulated = false; 
        }

        PolygonCollider2D collider = orb.GetComponent<PolygonCollider2D>();
        if (collider != null)
        {
            collider.enabled = false; 
        }

        ExperienceScript experienceScript = orb.GetComponent<ExperienceScript>();
        if (experienceScript != null)
        {
            experienceScript.enabled = false; 
        }

        orb.SetActive(false); 
        orbPool.Enqueue(orb); 
    }
}
