using UnityEngine;

public class VFXFollower : MonoBehaviour
{
    private Transform target;
    private Transform player; 

    public void SetTarget(Transform targetTransform, Transform playerTransform)
    {
        target = targetTransform;
        player = playerTransform;
    }

    private void Update()
    {
        if (target != null)
        {
            transform.position = target.position;

            if (player != null)
            {
                Vector3 scale = transform.localScale;
                scale.x = player.localScale.x > 0 ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
                transform.localScale = scale;
            }
        }
    }
}
