using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class MusicManager : MonoBehaviour
{
    public static MusicManager instance;

    public AudioSource bossMusic; // Nhạc boss, thêm vào trong Inspector
    private bool isBossMusicPlaying = false;
    private EnemyManager enemyManager; // Tham chiếu đến EnemyManager

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // Tìm EnemyManager trong scene
        enemyManager = FindObjectOfType<EnemyManager>();

        if (enemyManager == null)
        {
            Debug.LogError("MusicManager: Không tìm thấy EnemyManager!");
            return;
        }

        // Đăng ký sự kiện khi số quái bị tiêu diệt thay đổi
        enemyManager.enemiesKilled.OnValueChanged += OnEnemyKilledUpdated;
    }

    private void OnDestroy()
    {
        if (enemyManager != null)
        {
            enemyManager.enemiesKilled.OnValueChanged -= OnEnemyKilledUpdated;
        }
    }

    private void OnEnemyKilledUpdated(int oldValue, int newValue)
    {
        Debug.Log($"MusicManager: Quái bị giết {newValue}/{enemyManager.killTarget.Value}");

        if (newValue >= enemyManager.killTarget.Value && !isBossMusicPlaying)
        {
            StartCoroutine(FadeInBossMusic(1.5f));
        }
    }

    private IEnumerator FadeInBossMusic(float fadeDuration)
    {
        isBossMusicPlaying = true;
        bossMusic.volume = 0;
        bossMusic.Play();

        float targetVolume = 0.6f; // Điều chỉnh âm lượng mong muốn

        while (bossMusic.volume < targetVolume)
        {
            bossMusic.volume += targetVolume * Time.deltaTime / fadeDuration;
            yield return null;
        }
    }
}
