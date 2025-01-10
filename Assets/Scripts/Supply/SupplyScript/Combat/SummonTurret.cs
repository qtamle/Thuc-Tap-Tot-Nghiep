using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SummonTurret : MonoBehaviour, ISupplyActive
{
    public SupplyData supplyData;
    [SerializeField] private bool isActive = true;
    [SerializeField] private float cooldownTime = 5f;
    [SerializeField] private string spawnPointTag = "SpawnPoint";
    [SerializeField] private GameObject turretPrefab;

    private Transform[] spawnPoints;
    private Coroutine cooldownCoroutine;

    public float CooldownTime => cooldownTime;

    private string[] levelTags = { "Level1", "Level2", "Level3", "Level4", "Level5", "Level6" };

    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;

        FindSpawnPoints();
        IsLevelTagFound();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        isActive = true;
        FindSpawnPoints();
        IsLevelTagFound();
    }

    private void FindSpawnPoints()
    {
        GameObject[] spawnPointObjects = GameObject.FindGameObjectsWithTag(spawnPointTag);

        if (spawnPointObjects.Length == 0)
        {
            spawnPoints = new Transform[0];
        }
        else
        {
            spawnPoints = new Transform[spawnPointObjects.Length];
            for (int i = 0; i < spawnPointObjects.Length; i++)
            {
                spawnPoints[i] = spawnPointObjects[i].transform;
            }
        }
    }

    private IEnumerator SummonAndFire()
    {
        if (!IsLevelTagFound())
        {
            yield break;
        }

        if (spawnPoints.Length == 0)
        {
            yield break;
        }

        int randomIndex = Random.Range(0, spawnPoints.Length);
        Transform spawnPoint = spawnPoints[randomIndex];

        Vector3 adjustedPosition = spawnPoint.position + Vector3.up * 1.5f;

        GameObject turret = Instantiate(turretPrefab, adjustedPosition, spawnPoint.rotation);

        TurretDamage turretScript = turret.GetComponent<TurretDamage>();
        if (turretScript != null)
        {
            turretScript.InitializeTurret();
        }
    }

    private IEnumerator CooldownRoutineWithChance()
    {
        while (true)
        {
            yield return new WaitForSeconds(cooldownTime);

            if (Random.value <= 0.35f)
            {
                if (IsReady() && IsLevelTagFound())
                {
                    yield return SummonAndFire();
                }
            }

            CanActive();
        }
    }

    public void Active()
    {
        if (!IsReady())
        {
            return;
        }

        if (cooldownCoroutine != null)
        {
            StopCoroutine(cooldownCoroutine);
        }

        if (IsLevelTagFound())
        {
            isActive = false;
            cooldownCoroutine = StartCoroutine(CooldownRoutineWithChance());
        }
    }

    public void CanActive()
    {
        isActive = true;
    }

    public bool IsReady()
    {
        return isActive;
    }

    private bool IsLevelTagFound()
    {
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            foreach (string tag in levelTags)
            {
                if (obj.CompareTag(tag))
                {
                    Debug.Log("Tìm thấy đúng level");
                    return true;
                }
            }
        }
        return false;
    }
}
