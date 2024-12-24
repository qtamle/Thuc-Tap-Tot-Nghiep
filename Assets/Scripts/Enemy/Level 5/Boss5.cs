using UnityEngine;

public class Boss5 : MonoBehaviour
{

    [Header("Other")]
    private Rigidbody2D rb;
    private Transform GroundCheck;
    public float groundCheckRadius = 0.2f;
    private bool isGrounded = false;
    public LayerMask wallLayer;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

   
}
