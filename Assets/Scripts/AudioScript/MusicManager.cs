using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    public static MusicManager instance;

    [SerializeField] private string audioFolderPath = "Audio";
    private AudioSource mainSource;
    private EnemyManager enemyManager;
    private string currentScene;
    private bool isBossMusicPlaying = false;
    private float fadeDuration = 1.5f;
    private float maxVolume = 0.6f;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            mainSource = gameObject.AddComponent<AudioSource>();
            mainSource.loop = true;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Đăng ký sự kiện chuyển scene
        SceneManager.sceneLoaded += OnSceneLoaded;

        // Tìm EnemyManager
        enemyManager = FindObjectOfType<EnemyManager>();
        if (enemyManager != null)
        {
            enemyManager.enemiesKilled.OnValueChanged += OnEnemyKilledUpdated;
        }
        else
        {
            Debug.LogWarning("MusicManager: Không tìm thấy EnemyManager!");
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (enemyManager != null)
        {
            enemyManager.enemiesKilled.OnValueChanged -= OnEnemyKilledUpdated;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string sceneName = FormatSceneName(scene.name);
        if (currentScene == sceneName) return;

        currentScene = sceneName;
        isBossMusicPlaying = false;

        // Phát nhạc nền của scene
        StartCoroutine(LoadAndPlayMusic($"{sceneName}_Background"));
    }

    private void OnEnemyKilledUpdated(int oldValue, int newValue)
    {
        if (enemyManager == null || isBossMusicPlaying) return;

        // Kiểm tra nếu scene hiện tại là level từ 1 đến 5 (bao gồm "R...")
        if (IsLevelScene(currentScene) && newValue >= enemyManager.killTarget.Value)
        {
            isBossMusicPlaying = true;
            StartCoroutine(LoadAndPlayMusic($"{currentScene}_Boss"));
        }
    }

    private IEnumerator LoadAndPlayMusic(string audioPath)
    {
        // Tải âm thanh từ thư mục được chỉ định
        string fullPath = $"{audioFolderPath}/{audioPath}";
        ResourceRequest request = Resources.LoadAsync<AudioClip>(fullPath);
        yield return request;

        AudioClip clip = request.asset as AudioClip;
        if (clip != null)
        {
            yield return StartCoroutine(SwitchMusic(clip));
        }
        else
        {
            Debug.LogWarning($"MusicManager: Không tìm thấy âm thanh tại {fullPath}");
            if (mainSource.isPlaying)
            {
                yield return StartCoroutine(FadeOut());
            }
        }
    }

    private IEnumerator SwitchMusic(AudioClip newClip)
    {
        if (mainSource.isPlaying)
        {
            yield return StartCoroutine(FadeOut());
        }
        mainSource.clip = newClip;
        mainSource.Play();
        yield return StartCoroutine(FadeIn());
    }

    private IEnumerator FadeOut()
    {
        while (mainSource.volume > 0)
        {
            mainSource.volume -= maxVolume * Time.deltaTime / fadeDuration;
            yield return null;
        }
        mainSource.Stop();
    }

    private IEnumerator FadeIn()
    {
        mainSource.volume = 0;
        while (mainSource.volume < maxVolume)
        {
            mainSource.volume += maxVolume * Time.deltaTime / fadeDuration;
            yield return null;
        }
        mainSource.volume = maxVolume;
    }

    private string FormatSceneName(string sceneName)
    {
        // Thay thế khoảng trắng và ký tự đặc biệt để khớp với tên file âm thanh
        return sceneName.Replace(" ", "").Replace("...", "");
    }

    private bool IsLevelScene(string sceneName)
    {
        // Kiểm tra nếu scene là level từ 1 đến 5 (bao gồm "R...")
        return sceneName.StartsWith("Level1-Remake") || sceneName.StartsWith("Level2-Remake") ||
               sceneName.StartsWith("Level3-Remake") || sceneName.StartsWith("Level4-Remake") ||
               sceneName.StartsWith("Level5-Remake");
    }
}