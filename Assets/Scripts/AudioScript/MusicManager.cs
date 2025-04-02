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
    private string currentAudioClipName; // Lưu tên clip âm thanh hiện tại
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
            mainSource.playOnAwake = false;
            mainSource.volume = 1f; // Đảm bảo volume không bị mute
            Debug.Log("MusicManager: Initialized");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;

        enemyManager = FindObjectOfType<EnemyManager>();
        if (enemyManager != null)
        {
            enemyManager.enemiesKilled.OnValueChanged += OnEnemyKilledUpdated;
        }
        else
        {
            Debug.LogWarning("MusicManager: Không tìm thấy EnemyManager!");
        }

        // Phát âm thanh cho scene hiện tại khi khởi động
        string sceneName = FormatSceneName(SceneManager.GetActiveScene().name);
        currentScene = sceneName;
        PlayMusicForScene(sceneName);
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
        Debug.Log($"MusicManager: Scene loaded - {scene.name}");
        string sceneName = FormatSceneName(scene.name);
        if (currentScene == sceneName) return;

        currentScene = sceneName;
        isBossMusicPlaying = false;

        PlayMusicForScene(sceneName);
    }

    private void PlayMusicForScene(string sceneName)
    {
        // Kiểm tra nếu scene thuộc nhóm Shop_Online, MainMenu, Lobby
        if (IsShopOnlineGroup(sceneName))
        {
            // Nếu âm thanh hiện tại đã là Shop_Online-Background, không làm gì
            if (currentAudioClipName == "Shop_Online-Background")
            {
                Debug.Log("MusicManager: Already playing Shop_Online-Background, skipping...");
                return;
            }
            StartCoroutine(LoadAndPlayMusic("Shop_Online-Background"));
        }
        else
        {
            // Phát âm thanh riêng cho scene
            StartCoroutine(LoadAndPlayMusic($"{sceneName}-Background"));
        }
    }

    private void OnEnemyKilledUpdated(int oldValue, int newValue)
    {
        if (enemyManager == null || isBossMusicPlaying) return;

        if (IsLevelScene(currentScene) && newValue >= enemyManager.killTarget.Value)
        {
            isBossMusicPlaying = true;
            StartCoroutine(LoadAndPlayMusic($"{currentScene}-Boss"));
        }
    }

    private IEnumerator LoadAndPlayMusic(string audioPath)
    {
        string fullPath = $"{audioFolderPath}/{audioPath}";
        Debug.Log($"MusicManager: Loading audio from {fullPath}");

        ResourceRequest request = Resources.LoadAsync<AudioClip>(fullPath);
        yield return request;

        AudioClip clip = request.asset as AudioClip;
        if (clip != null)
        {
            Debug.Log($"MusicManager: Audio loaded - {clip.name}");
            currentAudioClipName = audioPath; // Lưu tên clip hiện tại
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
        Debug.Log($"MusicManager: Playing audio - {newClip.name}");
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
        // Thay thế khoảng trắng và ký tự đặc biệt, đảm bảo khớp với tên file
        return sceneName.Replace(" ", "").Replace("...", "").Replace("_", "-");
    }

    private bool IsLevelScene(string sceneName)
    {
        return sceneName.StartsWith("Level1-Remake") || sceneName.StartsWith("Level2-Remake") ||
               sceneName.StartsWith("Level3-Remake") || sceneName.StartsWith("Level4-Remake") ||
               sceneName.StartsWith("Level5-Remake");
    }

    private bool IsShopOnlineGroup(string sceneName)
    {
        return sceneName == "Shop-Online" || sceneName == "MainMenu" || sceneName == "Lobby";
    }
}