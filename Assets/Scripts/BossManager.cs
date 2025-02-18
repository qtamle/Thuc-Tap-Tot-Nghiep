using System.Collections;
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
    private int currentBossIndex = 0;

    private PlayerHealth health;
    private PlayerMovement playerMovement;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("BossManager instance created.");
        }
        else
        {
            Debug.LogWarning("Duplicate BossManager instance detected and destroyed.");
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        health = FindFirstObjectByType<PlayerHealth>();

        if (ListBoss != null && ListBoss.Count > 0)
        {
            currentBossIndex = 0;
            SetCurrentBoss(ListBoss[currentBossIndex]);
        }
        else
        {
            Debug.LogError("ListBoss is null or empty.");
        }
    }

    private void Update()
    {
        // 1
        //if (Input.GetKeyDown(KeyCode.W))
        //{
        //    Debug.Log("Key W pressed. Attempting to handle boss defeat.");
        //    HandleBossDefeated(CurrentBoss);
        //}

        //// 2
        //if (Input.GetKeyDown(KeyCode.R))
        //{
        //    Debug.Log("Key R pressed.");
        //    NextBossScene(CurrentBoss);
        //}

        //if (Input.GetKeyDown(KeyCode.Q))
        //{
        //    Debug.Log("Key Q pressed.");
        //    ProceedToNextBossScene();
        //}

        //if (Input.GetKeyDown(KeyCode.L))
        //{
        //    SetNextBoss();
        //}

        // 3
        if (Input.GetKeyDown(KeyCode.P))
        {
            SpawnWeaponOnSceneLoad();
        }
    }

    public void SetCurrentBoss(HandleBoss boss)
    {
        if (boss != null && ListBoss.Contains(boss))
        {
            CurrentBoss = boss;
            OnBossChanged?.Invoke(CurrentBoss);
            Debug.Log("Current boss set to: " + boss.name);
        }
        else
        {
            Debug.LogWarning("Attempted to set invalid boss or boss not in ListBoss.");
        }
    }

    public void HandleBossDefeated(HandleBoss boss)
    {
        if (boss != null && !boss.isDefeated)
        {
            boss.isDefeated = true;
            ListBossDefeated.Add(boss);
            Debug.Log("Boss defeated: " + boss.bossName);

            if (!string.IsNullOrEmpty(boss.supplyScene))
            {
                Debug.Log("Loading supply scene: " + boss.supplyScene);
                LoadSupplyScene(boss);
            }
        }
        else
        {
            Debug.LogWarning("Attempted to handle defeat for null or already defeated boss.");
        }
    }

    private void LoadSupplyScene(HandleBoss defeatedBoss)
    {
        if (!string.IsNullOrEmpty(defeatedBoss.supplyScene))
        {
            SceneManager.LoadScene(defeatedBoss.supplyScene);
        }
        else
        {
            Debug.LogError("Supply scene is null or empty for boss: " + defeatedBoss.name);
        }
    }

    public void NextBossScene(HandleBoss defeatedBoss)
    {
        if (defeatedBoss != null && defeatedBoss.isDefeated)
        {
            if (!string.IsNullOrEmpty(defeatedBoss.nextScene))
            {
                Debug.Log("Chuyển đến màn tiếp theo: " + defeatedBoss.nextScene);
                SceneManager.LoadScene(defeatedBoss.nextScene);
            }
            else
            {
                Debug.LogError("nextScene chưa được chỉ định cho boss: " + defeatedBoss.name);
            }
        }
        else
        {
            Debug.LogWarning("Boss chưa bị đánh bại hoặc không hợp lệ.");
        }
    }

    public void ProceedToNextBossScene()
    {
        if (CurrentBoss != null && !string.IsNullOrEmpty(CurrentBoss.nextScene))
        {
            HandleBoss nextBoss = ListBoss.Find(b =>
                SceneManager.GetActiveScene().name == CurrentBoss.supplyScene &&
                b != CurrentBoss &&
                !b.isDefeated);

            if (nextBoss != null)
            {
                SetCurrentBoss(nextBoss);
                SceneManager.LoadScene(CurrentBoss.nextScene);
            }
            else
            {
                Debug.LogWarning("No valid next boss found.");
            }
        }
        else
        {
            Debug.LogError("Current boss is null or nextScene is not set.");
        }
    }

    public void SetNextBossAfterSceneLoad()
    {
        StartCoroutine(SetNextBoss());
    }

    public IEnumerator SetNextBoss()
    {
        health = FindFirstObjectByType<PlayerHealth>();
        if (health != null)
        {
            health.isInvincible = false;
            playerMovement.enabled = false;
        }

        yield return new WaitForSeconds(3f);

        if (CurrentBoss != null)
        {
            int currentIndex = ListBoss.IndexOf(CurrentBoss);
            int nextIndex = currentIndex + 1;

            if (nextIndex < ListBoss.Count)
            {
                HandleBoss nextBoss = ListBoss[nextIndex];

                if (!nextBoss.isDefeated) 
                {
                    CurrentBoss = nextBoss;
                    OnBossChanged?.Invoke(CurrentBoss);
                    Debug.Log("Đã chuyển sang Boss tiếp theo: " + nextBoss.bossName);
                }
            }
            else
            {
                Debug.LogWarning("Không còn boss nào để chuyển tới.");
            }
        }
        else
        {
            Debug.LogError("CurrentBoss đang null, không thể chuyển boss.");
        }
    }


    public bool IsCurrentBossDefeated()
    {
        return CurrentBoss != null && CurrentBoss.isDefeated;
    }

    public void SpawnWeaponOnSceneLoad()
    {
        if (TestScene.weaponDataStore != null)
        {
            GameObject spawnPoint = GameObject.FindGameObjectWithTag("PlayerSpawn");
            if (spawnPoint != null)
            {
                GameObject weaponPrefab = TestScene.weaponDataStore.weapon;
                Instantiate(weaponPrefab, spawnPoint.transform.position, spawnPoint.transform.rotation);
            }
        }
    }

    public int GetDefeatedBossCount()
    {
        Debug.Log("Defeated boss count: " + ListBossDefeated.Count);
        return ListBossDefeated.Count;
    }

    public int GetTotalBossCount()
    {
        Debug.Log("Total boss count: " + ListBoss.Count);
        return ListBoss.Count;
    }

    public void ResetAllBosses()
    {
        Debug.Log("Resetting all bosses.");
        foreach (var boss in ListBoss)
        {
            boss.isDefeated = false;
        }
        ListBossDefeated.Clear();

        if (ListBoss.Count > 0)
        {
            SetCurrentBoss(ListBoss[0]);
        }
    }
}
