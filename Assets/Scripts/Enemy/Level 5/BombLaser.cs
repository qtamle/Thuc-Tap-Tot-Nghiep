using System.Collections;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class BombLaser : NetworkBehaviour
{
    public static BombLaser Instance;
    public GameObject laserPrefab;
    public LayerMask groundLayer;
    private NetworkRigidbody2D rb;
    private Vector3 bombPosition;

    private void Start()
    {
        rb = GetComponent<NetworkRigidbody2D>();
        bombPosition = transform.position;
    }

    // private void OnCollisionEnter2D(Collision2D collision)
    // {
    //     if ((groundLayer.value & (1 << collision.gameObject.layer)) > 0)
    //     {
    //         rb.Rigidbody2D.gravityScale = 0;

    //         StartCoroutine(WaitForExplode());
    //     }
    // }


    public IEnumerator WaitForExplode()
    {
        yield return new WaitForSeconds(1.5f);
        StartCoroutine(CreateLaser(bombPosition));
    }

    public IEnumerator CreateLaser(Vector2 position)
    {
        GameObject laser = Instantiate(laserPrefab, position, Quaternion.identity);

        laser.transform.Rotate(Vector3.forward, 0f);
        laser.GetComponent<NetworkObject>().Spawn();
        LineRenderer lineRenderer = laser.GetComponentInChildren<LineRenderer>();

        yield return new WaitForSeconds(0.1f);
        StartCoroutine(DestroyLaserAfterDelay(laser, 0.7f));
    }

    private IEnumerator DestroyLaserAfterDelay(GameObject laser, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (laser != null && laser.TryGetComponent(out NetworkObject networkObject))
        {
            networkObject.Despawn(true);
        }

        // Destroy(laser);
        // Destroy(gameObject, 1f);
    }
}
