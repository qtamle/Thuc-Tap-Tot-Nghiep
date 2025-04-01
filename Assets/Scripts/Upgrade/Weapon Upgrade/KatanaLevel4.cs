using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class KatanaLevel4 : NetworkBehaviour
{
    public GameObject bladePrefab;
    public float bladeSpeed;
    public float bladeLifetime = 5f;

    public void SpawnBlades()
    {
        Vector3 spawnPosition = new Vector3(transform.position.x, transform.position.y, 0f);

        GameObject leftBlade = Instantiate(bladePrefab, spawnPosition, Quaternion.identity);
        GameObject rightBlade = Instantiate(bladePrefab, spawnPosition, Quaternion.identity);

        leftBlade.GetComponent<NetworkObject>().Spawn(true);
        rightBlade.GetComponent<NetworkObject>().Spawn(true);

        MoveBlade(leftBlade, Vector2.left, true);
        MoveBlade(rightBlade, Vector2.right, false);

        StartCoroutine(DespawnAfterDelay(leftBlade, bladeLifetime));
        StartCoroutine(DespawnAfterDelay(rightBlade, bladeLifetime));
    }

    private void MoveBlade(GameObject blade, Vector2 direction, bool flipLeft)
    {
        Rigidbody2D rb = blade.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = direction * bladeSpeed;
        }

        FlipBlade(blade, flipLeft);
    }

    private void FlipBlade(GameObject blade, bool flipLeft)
    {
        Vector3 localScale = blade.transform.localScale;
        localScale.x = flipLeft ? -Mathf.Abs(localScale.x) : Mathf.Abs(localScale.x); 
        blade.transform.localScale = localScale;
    }

    private IEnumerator DespawnAfterDelay(GameObject laserObject, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (laserObject != null && laserObject.TryGetComponent(out NetworkObject networkObject))
        {
            networkObject.Despawn(true);
        }
    }
}
