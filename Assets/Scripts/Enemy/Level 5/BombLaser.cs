using System.Collections;
using UnityEngine;

public class BombLaser : MonoBehaviour
{
    public GameObject laserPrefab;
    public LayerMask groundLayer;
    private Rigidbody2D rb;
    private Vector3 bombPosition;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        bombPosition = transform.position;
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if ((groundLayer.value & (1 << collision.gameObject.layer)) > 0)
        {
            rb.gravityScale = 0;

            StartCoroutine(WaitForExplode());
        }
    }
    public IEnumerator WaitForExplode()
    {
        yield return new WaitForSeconds(1.5f);
        StartCoroutine(CreateLaser(bombPosition));
    }
    public IEnumerator CreateLaser(Vector2 position)
    {
        GameObject laser = Instantiate(laserPrefab, position, Quaternion.identity);

        laser.transform.Rotate(Vector3.forward, -90f);

        LineRenderer lineRenderer = laser.GetComponentInChildren<LineRenderer>();

        yield return new WaitForSeconds(0.1f);
        StartCoroutine(DestroyLaserAfterDelay(laser, 0.7f));
    }
    private IEnumerator DestroyLaserAfterDelay(GameObject laser, float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(laser);
        Destroy(gameObject,1f);
    }
}
