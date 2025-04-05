using UnityEngine;
using System.Collections;

public class SFXHandler : MonoBehaviour
{
    public static SFXHandler instance;

    [SerializeField] private string audioFolderPath = "Audio";
    private AudioSource effectSource;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            effectSource = gameObject.AddComponent<AudioSource>();
            effectSource.loop = false;
            effectSource.playOnAwake = false;
            effectSource.volume = 1f;
            Debug.Log("SFXHandler: Initialized");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlaySoundEffect(string audioPath)
    {
        if (effectSource.mute) return; // Không phát nếu đang mute
        StartCoroutine(LoadAndPlaySoundEffect(audioPath));
    }

    private IEnumerator LoadAndPlaySoundEffect(string audioPath)
    {
        string fullPath = $"{audioFolderPath}/{audioPath}";
        Debug.Log($"SFXHandler: Loading Sound Effect from {fullPath}");

        ResourceRequest request = Resources.LoadAsync<AudioClip>(fullPath);
        yield return request;

        AudioClip clip = request.asset as AudioClip;
        if (clip != null)
        {
            Debug.Log($"SFXHandler: Sound Effect loaded - {clip.name}");
            effectSource.PlayOneShot(clip);
        }
        else
        {
            Debug.LogWarning($"SFXHandler: Không tìm thấy âm thanh hiệu ứng tại {fullPath}");
        }
    }

    public void ToggleSFX(bool isOn)
    {
        effectSource.mute = !isOn;
        Debug.Log($"SFXHandler: SFX {(isOn ? "enabled" : "disabled")}");
    }
}