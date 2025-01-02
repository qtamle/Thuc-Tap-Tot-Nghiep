using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BossManager : MonoBehaviour
{
    public static BossManager Instance { get; private set; }

    [SerializeField] private List<HandleBoss> ListBoss;
    [SerializeField] private List<HandleBoss> ListBossDefeated = new List<HandleBoss>();

    public delegate void BossChangeHandler(HandleBoss boss);
    public event BossChangeHandler OnBossChanged;

    public HandleBoss CurrentBoss { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (ListBoss != null && ListBoss.Count > 0)
        {
            SetCurrentBoss(ListBoss[0]);
        }
    }

    public void SetCurrentBoss(HandleBoss boss)
    {
        if (boss != null && ListBoss.Contains(boss))
        {
            CurrentBoss = boss;
            OnBossChanged?.Invoke(CurrentBoss);
        }
    }

    // Xử lý khi boss bị đánh bại
    public void HandleBossDefeated(HandleBoss boss)
    {
        if (boss != null && !boss.isDeaftead)
        {
            // Đánh dấu boss đã bị đánh bại
            boss.isDeaftead = true;
            ListBossDefeated.Add(boss);

            // Load scene Supply
            LoadSupplyScene(boss);
        }
    }

    // Load scene Supply
    private void LoadSupplyScene(HandleBoss defeatedBoss)
    {
        if (!string.IsNullOrEmpty(defeatedBoss.supplyScene))
        {
            SceneManager.LoadScene(defeatedBoss.supplyScene);
        }
    }

    // Chuyển đến scene tiếp theo từ Supply Scene
    public void ProceedToNextBossScene()
    {
        if (CurrentBoss != null && !string.IsNullOrEmpty(CurrentBoss.nextScene))
        {
            // Tìm boss tiếp theo dựa trên nextScene
            HandleBoss nextBoss = ListBoss.Find(b =>
                SceneManager.GetActiveScene().name == CurrentBoss.supplyScene &&
                b != CurrentBoss &&
                !b.isDeaftead);

            if (nextBoss != null)
            {
                SetCurrentBoss(nextBoss);
                SceneManager.LoadScene(CurrentBoss.nextScene);
            }
        }
    }

    // Kiểm tra xem boss hiện tại đã bị đánh bại chưa
    public bool IsCurrentBossDefeated()
    {
        return CurrentBoss != null && CurrentBoss.isDeaftead;
    }

    // Lấy số lượng boss đã đánh bại
    public int GetDefeatedBossCount()
    {
        return ListBossDefeated.Count;
    }

    // Lấy tổng số boss
    public int GetTotalBossCount()
    {
        return ListBoss.Count;
    }

    // Reset tất cả boss
    public void ResetAllBosses()
    {
        foreach (var boss in ListBoss)
        {
            boss.isDeaftead = false;
        }
        ListBossDefeated.Clear();

        if (ListBoss.Count > 0)
        {
            SetCurrentBoss(ListBoss[0]);
        }
    }
}