using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    
    private Rigidbody2D rb;
    private Animator animator;
    private Vector2 movement;
    private Vector2 lastDirection = Vector2.down; // Dirección por defecto: abajo

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        
        // Verificaciones
        if (rb == null)
            Debug.LogError("❌ NO hay Rigidbody2D!");
        if (animator == null)
            Debug.LogError("❌ NO hay Animator!");
        
        rb.gravityScale = 0;
    }

    void Update()
    {
        // Capturar input
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
        
        // Normalizar para velocidad consistente
        movement = movement.normalized;
        
        // Actualizar animaciones
        UpdateAnimation();
    }

    void FixedUpdate()
    {
        // Aplicar movimiento
        rb.linearVelocity = movement * moveSpeed;
    }

    void UpdateAnimation()
    {
        bool isMoving = movement.magnitude > 0.01f;
        animator.SetBool("isMoving", isMoving);
        
        if (isMoving)
        {
            lastDirection = movement;
            
            // Manejar flip horizontal
            if (movement.x != 0)
            {
                // Usar animación r pero flipear según la dirección
                animator.SetFloat("moveX", 1); // Siempre usar r
                GetComponent<SpriteRenderer>().flipX = movement.x < 0; // Flip si va a la i
            }
            else
            {
                animator.SetFloat("moveX", 0);
            }
            
            animator.SetFloat("moveY", movement.y);
        }
        else
        {
            animator.SetFloat("moveX", lastDirection.x);
            animator.SetFloat("moveY", lastDirection.y);
        }
    }

}
