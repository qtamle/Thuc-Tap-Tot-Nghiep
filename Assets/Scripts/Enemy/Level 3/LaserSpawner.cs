using System.Collections;
using UnityEngine;

public class LaserSpawner : MonoBehaviour
{
    public GameObject laserPrefab;  
    public Transform spawnPosition; 
    public float laserDuration = 5f; 

    private void Start()
    {
        StartCoroutine(SpawnLaser());
    }

    private IEnumerator SpawnLaser()
    {
        float initialDelay = Random.Range(25f, 30f);
        yield return new WaitForSeconds(initialDelay);

        SpawnLaserAtPosition();

        while (true)
        {
            float nextDelay = Random.Range(18f, 25f);
            yield return new WaitForSeconds(nextDelay);
            SpawnLaserAtPosition();
        }
    }

    private void SpawnLaserAtPosition()
    {
        GameObject laser = Instantiate(laserPrefab, spawnPosition.position, Quaternion.Euler(0f,0f,90f));

        Destroy(laser, laserDuration);
    }
}
