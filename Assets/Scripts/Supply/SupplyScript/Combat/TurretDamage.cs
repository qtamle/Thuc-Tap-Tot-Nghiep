using System.Collections;
using UnityEngine;

public class TurretDamage : MonoBehaviour
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

    private void Start()
    {
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

        yield return new WaitForSeconds(1.5f);

        Vector3 offsetPosition = transform.position + new Vector3(laserOffsetX, 0, 0);
        laser = CreateLaser(offsetPosition);

        yield return new WaitForSeconds(laserDuration);

        if (laser != null)
        {
            Destroy(laser);
        }

        isLaserActive = false;

        yield return new WaitForSeconds(0.5f);

        Destroy(gameObject);
    }

    private GameObject CreateLaser(Vector3 startPosition)
    {
        GameObject laserObject = Instantiate(linePrefab);
        laserObject.transform.position = startPosition;

        laserObject.transform.rotation = Quaternion.Euler(0, 0, 90);

        LineRenderer lineRenderer = laserObject.GetComponent<LineRenderer>();
        if (lineRenderer != null)
        {
            lineRenderer.SetPosition(0, startPosition);
            lineRenderer.SetPosition(1, startPosition + laserObject.transform.up * lineLength);
        }
        Destroy(laserObject, 2f);
        return laserObject;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if ((groundLayer.value & (1 << collision.gameObject.layer)) > 0)
        {
            rb.gravityScale = 0;
        }
    }
}
