using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed;

    private Vector2 direction;

    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;
    }

    private void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime);    
    }
}
