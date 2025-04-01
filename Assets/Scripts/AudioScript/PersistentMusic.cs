using UnityEngine;

public class PersistentMusic : MonoBehaviour
{
    private static PersistentMusic instance;

    void Awake()
    {
        AudioSource existingAudio = FindObjectOfType<AudioSource>(); 

        if (existingAudio != null && existingAudio.gameObject != gameObject)
        {
            Destroy(gameObject); 
            return;
        }

        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            AudioSource audioSource = GetComponent<AudioSource>(); 
            if (audioSource != null)
            {
                audioSource.loop = true; 
                audioSource.playOnAwake = true;
                audioSource.volume = 0.6f;
                audioSource.Play();
            }
        }
        else
        {
            Destroy(gameObject); 
        }
    }
}
