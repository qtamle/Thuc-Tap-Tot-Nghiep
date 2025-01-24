using System.Collections;
using UnityEngine;

public class LightningChain : MonoBehaviour
{
    public float searchRadius = 10f; // Bán kính tìm kiếm kẻ thù
    public int maxEnemies = 4; // Tối đa số lượng kẻ thù để kết nối (chỉ dùng để giới hạn, nhưng không phải bắt buộc)
    public LineRenderer lineRenderer;
    public float jitterAmount = 0.2f; // Độ lệch ngẫu nhiên của tia sét
    public float duration = 0.5f; // Thời gian sống của tia sét
    public int pointsBetweenTargets = 5; // Số điểm trung gian giữa mỗi mục tiêu
    public float updateInterval = 0.05f; // Khoảng thời gian giữa mỗi lần cập nhật tia sét

    public LayerMask enemyLayer; // Layer của kẻ thù

    private bool isLightningActive = false;
    private Transform[] targets; // Khai báo biến targets ở đây

    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            TriggerLightning();
        }
    }

    public void TriggerLightning()
    {
        if (isLightningActive) return;

        // Sử dụng Physics2D.OverlapCircle để tìm các đối tượng kẻ thù trong phạm vi bán kính
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, searchRadius, enemyLayer);
        int enemyCount = 0;

        // Khởi tạo mảng targets với kích thước tối đa là maxEnemies
        targets = new Transform[maxEnemies];

        // Duyệt qua từng collider để thêm kẻ thù vào targets
        foreach (Collider2D collider in hitColliders)
        {
            if (collider.CompareTag("Enemy"))
            {
                if (enemyCount < maxEnemies)
                {
                    // Thêm kẻ thù vào danh sách targets
                    targets[enemyCount] = collider.transform;
                    enemyCount++;
                }
            }
        }

        // Nếu không có kẻ thù nào, không kích hoạt tia sét
        if (enemyCount == 0)
        {
            Debug.Log("No enemies found to trigger lightning.");
            return; // Không có kẻ thù để tạo tia sét
        }

        // Debug thông tin về các kẻ thù tìm được
        Debug.Log($"Found {enemyCount} enemies within radius {searchRadius}");

        // Debug vị trí của các kẻ thù
        foreach (var target in targets)
        {
            if (target != null) // Kiểm tra nếu target không phải null
            {
                Debug.Log($"Enemy found at position: {target.position}");
            }
        }

        isLightningActive = true;
        StartCoroutine(LightningEffect(enemyCount)); // Truyền số lượng kẻ thù đã tìm được vào Coroutine
    }

    private IEnumerator LightningEffect(int enemyCount)
    {
        if (lineRenderer == null)
        {
            Debug.LogError("LineRenderer is not assigned!");
            yield break;
        }

        if (targets == null || targets.Length == 0)
        {
            Debug.LogError("Targets array is empty or not initialized.");
            yield break;
        }

        float elapsedTime = 0f;
        int totalPoints = (enemyCount - 1) * (pointsBetweenTargets + 1) + 1;
        lineRenderer.positionCount = 0; // Bắt đầu không có điểm

        // Tăng tốc nối
        float timeFactor = 2f; // Hệ số để tăng tốc độ (càng lớn, càng nhanh)
        float adjustedUpdateInterval = updateInterval / timeFactor; // Giảm thời gian cập nhật

        while (elapsedTime < duration)
        {
            // Tính toán số điểm cần vẽ trong mỗi lần cập nhật
            int pointsToDraw = Mathf.FloorToInt(elapsedTime / duration * totalPoints);
            pointsToDraw = Mathf.Min(pointsToDraw, totalPoints); // Đảm bảo không vượt quá totalPoints

            lineRenderer.positionCount = pointsToDraw; // Cập nhật số điểm vẽ trong LineRenderer

            int pointIndex = 0;
            for (int i = 0; i < enemyCount - 1; i++)
            {
                Vector3 start = targets[i].position;
                Vector3 end = targets[i + 1].position;

                for (int j = 0; j <= pointsBetweenTargets; j++)
                {
                    float t = (float)j / pointsBetweenTargets;
                    Vector3 pointPos = Vector3.Lerp(start, end, t);
                    pointPos += new Vector3(Random.Range(-jitterAmount, jitterAmount), Random.Range(-jitterAmount, jitterAmount), 0);

                    // Kiểm tra và cập nhật điểm
                    if (pointIndex < lineRenderer.positionCount)
                    {
                        lineRenderer.SetPosition(pointIndex, pointPos);
                    }
                    pointIndex++;
                }
            }

            // Đặt vị trí của điểm cuối cùng (kẻ thù cuối cùng)
            if (pointIndex < lineRenderer.positionCount)
            {
                lineRenderer.SetPosition(pointIndex, targets[enemyCount - 1].position);
            }

            elapsedTime += adjustedUpdateInterval; // Tăng nhanh elapsedTime
            yield return new WaitForSeconds(adjustedUpdateInterval); // Giảm thời gian chờ giữa các lần cập nhật
        }

        float waitTimeAfterEffect = 0.1f;
        yield return new WaitForSeconds(waitTimeAfterEffect);

        lineRenderer.positionCount = 0;
        isLightningActive = false;
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow; 
        Gizmos.DrawWireSphere(transform.position, searchRadius); 
    }
}
