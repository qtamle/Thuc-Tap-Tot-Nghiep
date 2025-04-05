using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class MusicHandler : MonoBehaviour
{
    public static MusicHandler instance;

    [SerializeField] private string audioFolderPath = "Audio";
    private AudioSource mainSource;
    private EnemyManager enemyManager;
    private string currentScene;
    private string currentAudioClipName;
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
            mainSource.volume = maxVolume;
            Debug.Log("MusicHandler: Initialized");
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
            Debug.Log("MusicHandler: EnemyManager found and event registered");
        }
        else
        {
            Debug.LogWarning("MusicHandler: Không tìm thấy EnemyManager!");
        }

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
        Debug.Log($"MusicHandler: Scene loaded - {scene.name}");
        string sceneName = FormatSceneName(scene.name);
        if (currentScene == sceneName) return;

        currentScene = sceneName;
        isBossMusicPlaying = false;

        enemyManager = FindObjectOfType<EnemyManager>();
        if (enemyManager != null)
        {
            enemyManager.enemiesKilled.OnValueChanged -= OnEnemyKilledUpdated;
            enemyManager.enemiesKilled.OnValueChanged += OnEnemyKilledUpdated;
            Debug.Log("MusicHandler: EnemyManager found in new scene and event re-registered");
        }
        else
        {
            Debug.LogWarning("MusicHandler: Không tìm thấy EnemyManager in new scene!");
        }

        PlayMusicForScene(sceneName);
    }

    private void PlayMusicForScene(string sceneName)
    {
        if (IsShopOnlineGroup(sceneName))
        {
            if (currentAudioClipName == "Shop_Online-Background")
            {
                Debug.Log("MusicHandler: Already playing Shop_Online-Background, skipping...");
                return;
            }
            StartCoroutine(LoadAndPlayMusic("Shop_Online-Background"));
        }
        else
        {
            StartCoroutine(LoadAndPlayMusic($"{sceneName}-Background"));
        }
    }

    private void OnEnemyKilledUpdated(int oldValue, int newValue)
    {
        Debug.Log($"MusicHandler: OnEnemyKilledUpdated called - oldValue: {oldValue}, newValue: {newValue}, killTarget: {enemyManager.killTarget.Value}");
        if (enemyManager == null || isBossMusicPlaying) return;

        if (IsLevelScene(currentScene) && newValue >= enemyManager.killTarget.Value)
        {
            Debug.Log($"MusicHandler: Kill target reached, switching to boss music for {currentScene}");
            isBossMusicPlaying = true;
            StartCoroutine(LoadAndPlayMusic($"{currentScene}-Boss"));
        }
    }

    private IEnumerator LoadAndPlayMusic(string audioPath)
    {
        string fullPath = $"{audioFolderPath}/{audioPath}";
        Debug.Log($"MusicHandler: Loading audio from {fullPath}");

        ResourceRequest request = Resources.LoadAsync<AudioClip>(fullPath);
        yield return request;

        AudioClip clip = request.asset as AudioClip;
        if (clip != null)
        {
            Debug.Log($"MusicHandler: Audio loaded - {clip.name}");
            currentAudioClipName = audioPath;
            yield return StartCoroutine(SwitchMusic(clip));
        }
        else
        {
            Debug.LogWarning($"MusicHandler: Không tìm thấy âm thanh tại {fullPath}");
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
        if (!mainSource.mute) // Chỉ phát nếu không bị mute
        {
            mainSource.Play();
        }
        Debug.Log($"MusicHandler: Playing audio - {newClip.name}");
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
        if (mainSource.mute) yield break; // Không fade in nếu đang mute

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
        return sceneName.Replace(" ", "").Replace("...", "").Replace("_", "-");
    }

    private bool IsLevelScene(string sceneName)
    {
        bool isLevel = sceneName.StartsWith("Level1-Remake") || sceneName.StartsWith("Level2-Remake") ||
                       sceneName.StartsWith("Level3-Remake") || sceneName.StartsWith("Level4-Remake") ||
                       sceneName.StartsWith("Level5-Remake");
        Debug.Log($"MusicHandler: IsLevelScene({sceneName}) = {isLevel}");
        return isLevel;
    }

    private bool IsShopOnlineGroup(string sceneName)
    {
        return sceneName == "Shop-Online" || sceneName == "MainMenu" || sceneName == "Lobby" /*|| sceneName =="Supply"*/;
    }

    // Hàm để bật/tắt music (dùng trong settings)
    public void ToggleMusic(bool isOn)
    {
        mainSource.mute = !isOn;
        if (isOn && !mainSource.isPlaying && mainSource.clip != null)
        {
            mainSource.Play();
        }
        Debug.Log($"MusicHandler: Music {(isOn ? "enabled" : "disabled")}");
    }
}