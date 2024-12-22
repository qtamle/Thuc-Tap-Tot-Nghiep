using UnityEngine;

public class CollisionHandler : MonoBehaviour
{
    private SnakeController snakeController;

    void Start()
    {
        snakeController = GetComponentInParent<SnakeController>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        snakeController.HandleCollision(collision.tag);
    }
}
