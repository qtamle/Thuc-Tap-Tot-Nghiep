using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class Turret : NetworkBehaviour
{
    [Header("Turret Settings")]
    public GameObject linePrefab;
    public float laserDuration = 2f;
    public float lineLength = 5f;
    public float laserOffsetX = 1f;
    public LayerMask groundLayer;

    private GameObject laser;
    private bool isLaserActive = false;
    private Rigidbody2D rb;
    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    public void InitializeTurret()
    {
        if (!isLaserActive)
        {
            StartCoroutine(FireLaser());
        }
    }

    private IEnumerator FireLaser()
    {
        isLaserActive = true;

        animator.SetTrigger("Shoot");

        yield return new WaitForSeconds(1.5f);

        Vector3 offsetPosition = transform.position + new Vector3(laserOffsetX, 0, 0);
        laser = CreateLaser(offsetPosition);

        yield return new WaitForSeconds(laserDuration);

        if (laser != null)
        {
            laser.GetComponent<NetworkObject>().Despawn(true);
            // Destroy(laser);
        }

        isLaserActive = false;
    }

    private GameObject CreateLaser(Vector3 startPosition)
    {
        GameObject laserObject = Instantiate(linePrefab);
        laserObject.GetComponent<NetworkObject>().Spawn();
        laserObject.transform.position = startPosition;

        laserObject.transform.rotation = Quaternion.Euler(0, 0, 90);

        LineRenderer lineRenderer = laserObject.GetComponent<LineRenderer>();
        if (lineRenderer != null)
        {
            lineRenderer.SetPosition(0, startPosition);
            lineRenderer.SetPosition(1, startPosition + laserObject.transform.up * lineLength);
        }

        StartCoroutine(DespawnAfterDelay(laserObject, 2f)); // Destroy(laserObject, 2f);
        return laserObject;
    }

    private IEnumerator DespawnAfterDelay(GameObject laserObject, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (laserObject != null && laserObject.TryGetComponent(out NetworkObject networkObject))
        {
            networkObject.Despawn(true);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if ((groundLayer.value & (1 << collision.gameObject.layer)) > 0)
        {
            rb.gravityScale = 0;
        }
    }
}
