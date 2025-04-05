using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SummonTurret : NetworkBehaviour, ISupplyActive
{
    public SupplyData supplyData;
    [SerializeField] private bool isActive = true;
    [SerializeField] private float cooldownTime = 5f;
    [SerializeField] private string spawnPointTag = "SpawnPoint";
    [SerializeField] private GameObject turretPrefab;

    private Transform[] spawnPoints;
    private bool hasSpawn = false;

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
        StartCoroutine(Delay());
    }

    private IEnumerator Delay()
    {
        yield return new WaitForSeconds(0.5f);
        isActive = true;
        FindSpawnPoints();
        IsLevelTagFound();
    }

    private void Update()
    {
        if (!hasSpawn)
        {
            StartCoroutine(SummonAndFire()); 
        }
    }

    private IEnumerator SummonAndFire()
    {
        if (Random.value > 0.35f)
        {
            yield break; 

        }
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

        // Spawn turret
        GameObject turret = Instantiate(turretPrefab, adjustedPosition, spawnPoint.rotation);
        turret.GetComponent<NetworkObject>().Spawn(true);
        TurretDamage turretScript = turret.GetComponent<TurretDamage>();
        if (turretScript != null)
        {
            turretScript.InitializeTurret();
        }

        hasSpawn = true;  
        isActive = false;

        yield return StartCoroutine(CooldownRoutine());
    }

    private IEnumerator CooldownRoutine()
    {
        yield return new WaitForSeconds(cooldownTime);

        isActive = true;
        hasSpawn = false; 
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

    public void Active()
    {
        if (!IsReady())
        {
            return;
        }

        if (IsLevelTagFound())
        {
            isActive = false;
            StartCoroutine(CooldownRoutine());
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
