using UnityEngine;

public class LaternFly : MonoBehaviour
{
    private Vector3 startPos;
    private Vector3 targetPos;
    private bool movingToTarget = true;

    [SerializeField] private float waveAmplitude; 
    [SerializeField] private float waveFrequency;
    [SerializeField] private float moveSpeed; 
    [SerializeField] private float travelDistance; 
    [SerializeField] private bool moveLeft = false; 

    private void Start()
    {
        startPos = transform.position;
        targetPos = moveLeft ? startPos + Vector3.left * travelDistance : startPos + Vector3.right * travelDistance;
    }

    private void Update()
    {
        MoveInWavePattern();
    }

    private void MoveInWavePattern()
    {
        float step = moveSpeed * Time.deltaTime;
        float totalDistance = travelDistance;
        float currentDistance = movingToTarget
            ? Vector3.Distance(startPos, transform.position)
            : Vector3.Distance(targetPos, transform.position);

        float t = currentDistance / totalDistance;

        float waveOffset = Mathf.Sin(t * Mathf.PI * 2 * waveFrequency) * waveAmplitude;

        Vector3 nextPos = movingToTarget
            ? Vector3.MoveTowards(transform.position, targetPos, step)
            : Vector3.MoveTowards(transform.position, startPos, step);

        nextPos.y = startPos.y + waveOffset;
        transform.position = nextPos;

        if (movingToTarget && Vector3.Distance(transform.position, targetPos) < 0.1f)
            movingToTarget = false;
        else if (!movingToTarget && Vector3.Distance(transform.position, startPos) < 0.1f)
            movingToTarget = true;
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        Gizmos.color = Color.green;
        int resolution = 30;
        Vector3 prevPoint = startPos;

        for (int i = 1; i <= resolution; i++)
        {
            float t = i / (float)resolution;
            Vector3 nextPoint = Vector3.Lerp(startPos, targetPos, t);
            float waveOffset = Mathf.Sin(t * Mathf.PI * 2 * waveFrequency) * waveAmplitude;
            nextPoint.y = startPos.y + waveOffset;

            Gizmos.DrawLine(prevPoint, nextPoint);
            prevPoint = nextPoint;
        }
    }
}
