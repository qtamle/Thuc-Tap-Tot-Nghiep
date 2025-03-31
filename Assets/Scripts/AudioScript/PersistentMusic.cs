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
            audioSource.volume = 0.6f; // Điều chỉnh âm lượng
            audioSource.Play(); 
        }
        else
        {
            Destroy(gameObject); // Xóa GameObject nếu đã có nhạc chạy trước đó
        }
    }
}
