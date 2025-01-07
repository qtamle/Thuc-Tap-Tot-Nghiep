using UnityEngine;

public class ExperienceScript : MonoBehaviour
{
    private ExperienceManager experienceManager;
    private ExperienceOrbPoolManager orbPoolManager;

    private void Start()
    {
        experienceManager = UnityEngine.Object.FindFirstObjectByType<ExperienceManager>();

        if (experienceManager == null )
        {
            Debug.Log("Khong co experience manager");
        }

        orbPoolManager = UnityEngine.Object.FindFirstObjectByType<ExperienceOrbPoolManager>();

        if (orbPoolManager == null)
        {
            Debug.LogError("ExperienceOrbPoolManager not found in the scene!");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            CollectExperience();
        }
    }
    private void CollectExperience()
    {
        int randomExperience = Random.Range(5, 10);

        if (experienceManager != null)
        {
            experienceManager.AddExperience(randomExperience); 
        }
        else
        {
            Debug.LogError("ExperienceManager not assigned.");
        }

        if (orbPoolManager != null)
        {
            orbPoolManager.ReturnOrbToPool(gameObject);
        }
        else
        {
            Debug.LogError("OrbPoolManager not assigned. Destroying the object.");
            Destroy(gameObject); 
        }
    }
}
