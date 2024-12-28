using System.Collections;
using UnityEngine;

public class CloneScript : MonoBehaviour
{
    public float moveSpeed = 3f;
    private bool isMoving = false;
    private Vector3 targetPosition;

    public void StartMoving(Vector3 direction, float distance)
    {
        targetPosition = transform.position + direction * distance;
        isMoving = true;
        StartCoroutine(Move());
    }

    private IEnumerator Move()
    {
        while (isMoving && Vector3.Distance(transform.position, targetPosition) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = targetPosition;
        isMoving = false;
    }

    public void SetRotation(Quaternion rotation)
    {
        transform.rotation = rotation;
    }
}
