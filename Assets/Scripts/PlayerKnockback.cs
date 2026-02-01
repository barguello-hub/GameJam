using UnityEngine;
using System.Collections;

public class PlayerKnockback : MonoBehaviour
{
    [Header("Knockback Settings")]
    [SerializeField] private float knockbackForce = 3f;
    [SerializeField] private float knockbackDuration = 0.5f;
    [SerializeField] private float bounceDamping = 0.6f; // Cuánto rebota (0.6 = 60% del empujón)
    [SerializeField] private int bounceCount = 2; // Número de rebotes
    
    [Header("Visual Feedback")]
    [SerializeField] private bool enableFlashing = true;
    [SerializeField] private float flashDuration = 0.1f;
    [SerializeField] private int flashCount = 3;
    
    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Effects")]
    [SerializeField] private ParticleSystem hitParticles;
    
    private PlayerMovement playerMovement; // Para desactivar el control
    private Rigidbody2D rb;
    private bool isKnockedBack = false;
    
    private void Start()
    {
        // Auto-asignar componentes
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        
        rb = GetComponent<Rigidbody2D>();
        playerMovement = GetComponent<PlayerMovement>();
    }
    
    /// <summary>
    /// Aplica un empujón al jugador en dirección contraria al enemigo
    /// </summary>
    /// <param name="enemyPosition">Posición del enemigo que causó el knockback</param>
    public void ApplyKnockback(Vector2 enemyPosition)
    {
        if (isKnockedBack) return;
        
        // Calcular dirección opuesta al enemigo
        Vector2 direction = ((Vector2)transform.position - enemyPosition).normalized;
        
        StartCoroutine(KnockbackCoroutine(direction));
    }
    
    /// <summary>
    /// Aplica un empujón en una dirección específica
    /// </summary>
    public void ApplyKnockback2(Vector2 direction)
    {
        if (isKnockedBack) return;
        
        StartCoroutine(KnockbackCoroutine(direction.normalized));
    }
    
    /// <summary>
    /// Aplica knockback hacia atrás (dirección actual del player)
    /// </summary>
    public void ApplyKnockbackBackward()
    {
        // Usar la última dirección de movimiento o izquierda por defecto
        Vector2 direction = Vector2.left;
        
        if (playerMovement != null)
        {
            // Si el player mira a la derecha, empujamos a la izquierda
            float scale = transform.localScale.x;
            direction = scale > 0 ? Vector2.left : Vector2.right;
        }
        
        StartCoroutine(KnockbackCoroutine(direction));
    }    
    
    private IEnumerator KnockbackCoroutine2(Vector2 direction)
    {
        isKnockedBack = true;
        
        // Desactivar control del jugador
        
        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        if (hitParticles != null)    
        {        
            hitParticles.Play();    
        }

        if (CameraShake.Instance != null){    
            CameraShake.Instance.Shake(0.3f, 0.2f);
        }
        
        Debug.Log("[PlayerKnockback] Aplicando empujón en dirección: " + direction);
        
        // Iniciar parpadeo si está habilitado
        if (enableFlashing && spriteRenderer != null)
        {
            StartCoroutine(FlashCoroutine());
        }
        
        // Aplicar empujón con rebotes
        Vector2 startPosition = transform.position;
        float currentForce = knockbackForce;
        
        for (int i = 0; i < bounceCount; i++)
        {
            // Movimiento de empujón
            float elapsed = 0f;
            Vector2 targetPosition = (Vector2)transform.position + (direction * currentForce);
            Vector2 bounceStartPos = transform.position;
            
            while (elapsed < knockbackDuration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / knockbackDuration;
                
                // Curva de desaceleración (ease out)
                float easedProgress = 1f - Mathf.Pow(1f - progress, 3f);
                
                // Interpolar posición
                transform.position = Vector2.Lerp(bounceStartPos, targetPosition, easedProgress);
                
                yield return null;
            }
            
            // Reducir fuerza para el siguiente rebote
            currentForce *= bounceDamping;
            
            // Invertir dirección para el rebote
            direction = -direction;
            
            // Pequeña pausa entre rebotes
            yield return new WaitForSeconds(0.05f);
        }
        
        Debug.Log("[PlayerKnockback] Knockback completado");
        
        // Pequeña pausa final
        yield return new WaitForSeconds(0.2f);
        
        // Reactivar control del jugador
        
        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }
        
        //playerMovement.enabled = true;
        isKnockedBack = false;
    }

    private IEnumerator KnockbackCoroutine(Vector2 direction)
{
    isKnockedBack = true;
    
    // Desactivar control del jugador
    if (playerMovement != null)
    {
        playerMovement.enabled = false;
    }
    
    Debug.Log("[PlayerKnockback] Aplicando empujón en dirección: " + direction);
    
    // Iniciar parpadeo si está habilitado
    if (enableFlashing && spriteRenderer != null)
    {
        StartCoroutine(FlashCoroutine());
    }
    
    // Aplicar empujón con rebotes
    float currentForce = knockbackForce;
    
    for (int i = 0; i < bounceCount; i++)
    {
        float elapsed = 0f;
        Vector2 targetPosition = (Vector2)transform.position + (direction * currentForce);
        Vector2 bounceStartPos = transform.position;
        
        while (elapsed < knockbackDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / knockbackDuration;
            
            float easedProgress = 1f - Mathf.Pow(1f - progress, 3f);
            
            // Calcular nueva posición
            Vector2 newPosition = Vector2.Lerp(bounceStartPos, targetPosition, easedProgress);
            
            // Verificar si hay obstáculos con un Raycast
            RaycastHit2D hit = Physics2D.Linecast(transform.position, newPosition, LayerMask.GetMask("Default"));
            
            if (hit.collider != null && hit.collider.CompareTag("Wall"))
            {
                // ¡CHOCAMOS CON UN MURO! Invertir dirección
                direction = -direction;
                Debug.Log("[PlayerKnockback] ¡Rebote contra muro!");
                break; // Salir del while para empezar el siguiente rebote
            }
            
            transform.position = newPosition;
            
            yield return null;
        }
        
        currentForce *= bounceDamping;
        direction = -direction;
        
        yield return new WaitForSeconds(0.05f);
    }
    
    Debug.Log("[PlayerKnockback] Knockback completado");
    
    yield return new WaitForSeconds(0.2f);
    
    if (playerMovement != null)
    {
        playerMovement.enabled = true;
    }
    
    isKnockedBack = false;
}

    
    private IEnumerator FlashCoroutine()
    {
        Color originalColor = spriteRenderer.color;
        
        for (int i = 0; i < flashCount; i++)
        {
            // Transparente (parpadeo)
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0.3f);
            yield return new WaitForSeconds(flashDuration);
            
            // Opaco
            spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(flashDuration);
        }
        
        // Asegurar que vuelve al color original
        spriteRenderer.color = originalColor;
    }
    
    public bool IsKnockedBack()
    {
        return isKnockedBack;
    }
}
