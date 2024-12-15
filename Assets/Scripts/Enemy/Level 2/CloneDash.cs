using System.Collections;
using UnityEngine;

public class CloneDash : MonoBehaviour
{
    public float cloneSpeed = 5f;
    private Vector3 originalPosition;
    private string playerTag = "Player";

    void Start()
    {
        originalPosition = transform.position;
    }

    public void DashTowardsPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag(playerTag);

        if (player != null)
        {
            // Tính hướng lướt từ clone tới Player
            StartCoroutine(Dash(player.transform));
        }
        else
        {
            Debug.LogError("Player không tìm thấy!");
        }
    }

    IEnumerator Dash(Transform player)
    {
        float dashDuration = 3f;

        // Tính hướng di chuyển của clone dựa trên vị trí Player
        Vector3 directionToPlayer = player.position - transform.position;
        directionToPlayer.y = 0f; // Đảm bảo chỉ di chuyển trên trục X (tránh di chuyển lên/xuống)

        // Tính toán hướng riêng biệt cho clone
        Vector3 dashDirection = directionToPlayer.x > 0 ? Vector3.right : Vector3.left;

        // Tắt collider trong khi lướt
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = false;
        }

        // Thực hiện lướt
        float timer = 0f;
        while (timer < dashDuration)
        {
            // Di chuyển clone theo hướng tính toán
            transform.position += dashDirection * cloneSpeed * Time.deltaTime;
            timer += Time.deltaTime;
            yield return null;
        }

        // Đảm bảo clone đi hết quãng đường trong thời gian lướt
        transform.position += dashDirection * cloneSpeed * (dashDuration - timer);

        // Bật lại collider sau khi lướt
        if (collider != null)
        {
            collider.enabled = true;
        }

        // Đợi một thời gian trước khi quay lại vị trí gốc
        yield return new WaitForSeconds(0.5f);
        transform.position = originalPosition;
    }
}
