using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] public float moveSpeed = 5f;
    
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    
    private Vector2 moveInput;
    private Rigidbody2D rb;
    private bool isFacingRight = true;  // Orientación actual
    
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // Auto-asignar referencias si no están
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
        
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        
        // Verificar componentes
        if (animator == null)
        {
            Debug.LogError("[Player] No se encontró Animator. Añade el componente.");
        }
        
        if (spriteRenderer == null)
        {
            Debug.LogError("[Player] No se encontró SpriteRenderer.");
        }
    }
    
    private void Update()
    {
        // Leer input
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");
        
        // Normalizar diagonal (para que no vaya más rápido)
        if (moveInput.magnitude > 1f)
        {
            moveInput.Normalize();
        }
        
        // Actualizar animaciones
        UpdateAnimation();
        
        // Actualizar orientación (flip)
        UpdateFacing();
    }
    
    private void FixedUpdate()
    {
        // Mover el personaje
        rb.linearVelocity = moveInput * moveSpeed;
    }
    
    private void UpdateAnimation()
    {
        if (animator == null) return;
        
        // Si se está moviendo (en cualquier dirección)
        bool isMoving = moveInput.magnitude > 0.1f;
        animator.SetBool("isMoving", isMoving);
    }
    
    private void UpdateFacing()
    {
        if (spriteRenderer == null) return;
        
        // Solo cambiar orientación si hay input HORIZONTAL
        if (moveInput.x > 0.1f && !isFacingRight)
        {
            // Mirar a la derecha
            Flip();
        }
        else if (moveInput.x < -0.1f && isFacingRight)
        {
            // Mirar a la izquierda
            Flip();
        }
        
        // Si solo se mueve verticalmente, mantiene la orientación actual
    }
    
    private void Flip()
    {
        isFacingRight = !isFacingRight;
        spriteRenderer.flipX = !isFacingRight;
        
        Debug.Log($"[Player] Flip! Mirando a: {(isFacingRight ? "Derecha" : "Izquierda")}");
    }
}
