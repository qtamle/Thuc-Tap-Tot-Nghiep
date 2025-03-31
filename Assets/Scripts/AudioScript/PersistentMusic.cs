using UnityEngine;

public class PersistentMusic : MonoBehaviour
{
    private static PersistentMusic instance;
    private AudioSource audioSource;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            audioSource = GetComponent<AudioSource>(); 
            audioSource.loop = true; 
            audioSource.playOnAwake = true;
            audioSource.volume = 0.6f; // Điều chỉnh vol, trên máy t là nghe 0.6 là vừa, có gì chỉnh lại trên máy bây nha
            audioSource.Play(); 
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
