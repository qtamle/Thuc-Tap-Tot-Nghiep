using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnergyOrbShooter : MonoBehaviour
{
    [Header("Orb Settings")]
    public float orbSpeed;

    private EnergyOrbsShootPool pool;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        pool = FindFirstObjectByType<EnergyOrbsShootPool>();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void ShootEnergyOrbs()
    {
        for (int i = 0; i < 2; i++)
        {
            GameObject orb = pool.GetOrb();

            float randomAngle = Random.Range(0f, 360f);

            Vector3 direction = new Vector3(Mathf.Cos(randomAngle * Mathf.Deg2Rad), Mathf.Sin(randomAngle * Mathf.Deg2Rad), 0f);

            orb.transform.position = new Vector3(transform.position.x, transform.position.y, 0f);

            Rigidbody2D rb = orb.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = direction * orbSpeed;
            }

            StartCoroutine(ReturnOrbToPool(orb));
        }
    }

    private IEnumerator ReturnOrbToPool(GameObject orb)
    {
        yield return new WaitForSeconds(5f); 

        pool.ReturnOrb(orb);
    }
}
