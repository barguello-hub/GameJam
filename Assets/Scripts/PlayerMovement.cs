using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    
    private Rigidbody2D rb;
    private Vector2 moveInput;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    
    void Update()
    {
        // Leer inputs (WASD o flechas)
        moveInput.x = Input.GetAxisRaw("Horizontal"); // A/D o ←/→
        moveInput.y = Input.GetAxisRaw("Vertical");   // W/S o ↑/↓
    }
    
    void FixedUpdate()
    {
        // Aplicar movimiento
        rb.linearVelocity = moveInput.normalized * moveSpeed;
    }
}
