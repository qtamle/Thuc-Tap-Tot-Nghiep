using UnityEngine;

public class ExperienceScript : MonoBehaviour
{
    private ExperienceManager experienceManager;

    private void Start()
    {
        experienceManager = UnityEngine.Object.FindFirstObjectByType<ExperienceManager>();

        if (experienceManager == null )
        {
            Debug.Log("Khong co experience manager");
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
            Destroy(gameObject); 
        }
        else
        {
            Debug.LogError("ExperienceManager not assigned.");
        }
    }
}
