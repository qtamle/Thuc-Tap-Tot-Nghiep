using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PersistentMusic : MonoBehaviour
{
    private static PersistentMusic instance;
    private AudioSource audioSource;
    private bool isFadingOut = false;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        // Nếu đã có một nhạc nền khác, kiểm tra xem có phải nhạc mới không
        if (instance != null && instance != this)
        {
            if (instance.audioSource.clip != audioSource.clip)
            {
                instance.StartCoroutine(instance.FadeOutAndDestroy(1.5f)); // Fade out nhạc cũ
                instance = this;
            }
            else
            {
                Destroy(gameObject); // Nếu cùng một bài nhạc, hủy nhạc mới để tiếp tục phát nhạc cũ
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
        // Kiểm tra xem Scene mới có nhạc nền không
        if (newScene.isLoaded && audioSource.clip != null)
        {
            // Kiểm tra xem có nhạc khác đang phát không, nếu có fade out nhạc cũ
            PersistentMusic newMusic = FindObjectOfType<PersistentMusic>();
            if (newMusic != null && newMusic != this && newMusic.audioSource.clip != audioSource.clip)
            {
                StartCoroutine(FadeOutAndDestroy(1.5f)); // Nếu có nhạc khác, fade out nhạc cũ
            }
        }
        else
        {
            // Nếu không có nhạc trong scene mới, giữ nhạc cũ
            if (!audioSource.isPlaying)
            {
                audioSource.Play();
            }
        }
    }

    private IEnumerator FadeOutAndDestroy(float fadeDuration)
    {
        if (isFadingOut) yield break; // Tránh chạy nhiều lần
        isFadingOut = true;

        float startVolume = audioSource.volume;
        float time = 0;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0, time / fadeDuration);
            yield return null;
        }

        Destroy(gameObject); // Hủy nhạc cũ sau khi fade out
    }
}
