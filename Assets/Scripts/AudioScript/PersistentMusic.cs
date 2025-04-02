using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PersistentMusic : MonoBehaviour
{
    private static PersistentMusic instance;
    private AudioSource audioSource;
    private bool isFading = false;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        if (instance != null && instance != this)
        {
            if (instance.audioSource.clip != audioSource.clip)
            {
                // Bắt đầu quá trình chuyển đổi mượt mà
                StartCoroutine(SmoothTransition(instance, this, 1.5f));
                return;
            }
            else
            {
                Destroy(gameObject); // Nếu cùng nhạc, hủy nhạc mới
                return;
            }
        }
        else
        {
            instance = this;
        }

        DontDestroyOnLoad(gameObject);

        if (audioSource != null && !audioSource.isPlaying)
        {
            audioSource.loop = true;
            audioSource.playOnAwake = true;
            audioSource.volume = 0.6f;
            audioSource.Play();
        }

        SceneManager.activeSceneChanged += OnSceneChanged;
    }

    void OnDestroy()
    {
        SceneManager.activeSceneChanged -= OnSceneChanged;
    }

    private void OnSceneChanged(Scene oldScene, Scene newScene)
    {
        PersistentMusic newMusic = FindObjectOfType<PersistentMusic>();
        if (newMusic != null && newMusic != this)
        {
            if (newMusic.audioSource.clip != audioSource.clip)
            {
                StartCoroutine(SmoothTransition(this, newMusic, 1.5f));
            }
            else
            {
                Destroy(newMusic.gameObject);
            }
        }
    }

    private IEnumerator SmoothTransition(PersistentMusic oldMusic, PersistentMusic newMusic, float transitionDuration)
    {
        if (oldMusic.isFading) yield break;
        oldMusic.isFading = true;

        // Chuẩn bị nhạc mới
        newMusic.audioSource.loop = true;
        newMusic.audioSource.volume = 0f; // Bắt đầu với volume 0
        newMusic.audioSource.Play(); // Phát nhạc mới ngay lập tức

        float time = 0;
        float oldStartVolume = oldMusic.audioSource.volume; // Volume ban đầu của nhạc cũ
        float newTargetVolume = 0.6f; // Volume mục tiêu của nhạc mới

        // Fade out nhạc cũ và fade in nhạc mới đồng thời
        while (time < transitionDuration)
        {
            time += Time.deltaTime;
            float t = time / transitionDuration;
            oldMusic.audioSource.volume = Mathf.Lerp(oldStartVolume, 0, t); // Fade out
            newMusic.audioSource.volume = Mathf.Lerp(0, newTargetVolume, t); // Fade in
            yield return null;
        }

        // Đảm bảo volume cuối cùng chính xác
        oldMusic.audioSource.volume = 0;
        newMusic.audioSource.volume = newTargetVolume;

        // Gán instance mới và hủy nhạc cũ
        instance = newMusic;
        DontDestroyOnLoad(newMusic.gameObject);
        Destroy(oldMusic.gameObject);
        newMusic.isFading = false;
    }
    public IEnumerator FadeOutMusic(float duration)
{
    float startVolume = audioSource.volume;
    float time = 0;

    while (time < duration)
    {
        time += Time.deltaTime;
        audioSource.volume = Mathf.Lerp(startVolume, 0, time / duration);
        yield return null;
    }

    audioSource.volume = 0;
    audioSource.Stop();
}

}