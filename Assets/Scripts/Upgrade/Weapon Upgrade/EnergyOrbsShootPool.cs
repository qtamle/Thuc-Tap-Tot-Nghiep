using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class EnergyOrbsShootPool : MonoBehaviour
{
    public GameObject orbPrefab;
    public int poolSize = 10;

    private Queue<GameObject> orbPool = new Queue<GameObject>();

    private void Start()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject orb = Instantiate(orbPrefab, transform);

            orb.SetActive(false);
            orbPool.Enqueue(orb);
        }
    }

    public GameObject GetOrb()
    {
        GameObject orb;

        if (orbPool.Count > 0)
        {
            orb = orbPool.Dequeue();
        }
        else
        {
            orb = Instantiate(orbPrefab, transform);
        }

        orb.SetActive(true);

        Rigidbody2D orbRigidbody = orb.GetComponent<Rigidbody2D>();
        EnergyOrbDamage orbDamage = orb.GetComponent<EnergyOrbDamage>();

        return orb;
    }

    public void ReturnOrb(GameObject orb)
    {
        orb.SetActive(false);
        orbPool.Enqueue(orb);
    }
}
