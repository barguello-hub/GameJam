using UnityEngine;
using System.Collections;

public class PlayerSpawnAnimation : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private Vector2 spawnStartPosition = new Vector2(0f, 8f); // Posición inicial (arriba)
    [SerializeField] private Vector2 spawnEndPosition = new Vector2(0f, 0f); // Posición final (centro)
    [SerializeField] private float spawnDuration = 1.5f; // Duración de la caída
    [SerializeField] private AnimationCurve spawnCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // Curva de movimiento
    
    [Header("Rotation Settings")]
    [SerializeField] private bool enableRotation = true;
    [SerializeField] private float rotationSpeed = 720f; // Grados por segundo (2 vueltas completas)
    [SerializeField] private bool randomRotationDirection = true;
    
    [Header("Sprites")]
    [SerializeField] private Sprite fallingSprite; // Sprite mientras cae (opcional)
    [SerializeField] private Sprite landingSprite; // Sprite al tocar el suelo
    [SerializeField] private float landingSpritesDuration = 0.3f; // Cuánto tiempo mostrar el sprite de aterrizaje
    
    [Header("Visual Effects")]
    [SerializeField] private bool enableFlashing = true;
    [SerializeField] private float flashDuration = 0.1f;
    [SerializeField] private int flashCount = 5;
    [SerializeField] private bool fadeIn = true;
    [SerializeField] private float fadeInDuration = 0.5f;
    
    [Header("Landing Effect")]
    [SerializeField] private bool enableLandingShake = true;
    [SerializeField] private float shakeIntensity = 0.1f;
    [SerializeField] private float shakeDuration = 0.2f;
    
    [Header("Control")]
    [SerializeField] private bool playOnStart = true;
    [SerializeField] private float delayBeforeStart = 0.5f;
    
    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator; // Si usas Animator

    [Header("Particles")]
    [SerializeField] private ParticleSystem landingDustParticles;
    
    private PlayerMovement playerMovement;
    private Rigidbody2D rb;
    private bool isAnimating = false;
    private Sprite originalSprite;
    private float rotationDirection = 1f;
    
    private void Start()
    {
        // Obtener referencias
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
        
        playerMovement = GetComponent<PlayerMovement>();
        rb = GetComponent<Rigidbody2D>();
        
        // Guardar sprite original
        if (spriteRenderer != null)
        {
            originalSprite = spriteRenderer.sprite;
        }
        
        // Reproducir animación al inicio
        if (playOnStart)
        {
            StartCoroutine(PlaySpawnAnimationDelayed(delayBeforeStart));
        }
    }
    
    private IEnumerator PlaySpawnAnimationDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);
        PlaySpawnAnimation();
    }
    
    /// <summary>
    /// Reproduce la animación de spawn completa
    /// </summary>
    public void PlaySpawnAnimation()
    {
        if (isAnimating) return;
        StartCoroutine(SpawnAnimationCoroutine());
    }
    
    private IEnumerator SpawnAnimationCoroutine()
    {
        isAnimating = true;
        
        Debug.Log("[PlayerSpawn] Iniciando animación de entrada");
        
        // Desactivar control del jugador
        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }
        
        // Desactivar Animator temporalmente
        if (animator != null)
        {
            animator.enabled = false;
        }
        
        // Desactivar física
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }
        
        // Establecer posición inicial
        transform.position = spawnStartPosition;
        transform.rotation = Quaternion.identity; // Reset rotación
        
        // Dirección de rotación aleatoria
        if (randomRotationDirection)
        {
            rotationDirection = Random.value > 0.5f ? 1f : -1f;
        }
        
        // Cambiar a sprite de caída si existe
        if (fallingSprite != null && spriteRenderer != null)
        {
            spriteRenderer.sprite = fallingSprite;
        }
        
        // Fade in si está habilitado
        if (fadeIn && spriteRenderer != null)
        {
            Color c = spriteRenderer.color;
            c.a = 0f;
            spriteRenderer.color = c;
        }
        
        // Pequeña pausa dramática
        yield return new WaitForSeconds(0.2f);
        
        // Iniciar animaciones simultáneas
        Coroutine moveCoroutine = StartCoroutine(MoveAndRotatePlayerCoroutine());
        Coroutine fadeCoroutine = null;
        
        if (fadeIn && spriteRenderer != null)
        {
            fadeCoroutine = StartCoroutine(FadeInCoroutine());
        }
        
        // Esperar a que termine el movimiento
        yield return moveCoroutine;
        
        // Esperar fade
        if (fadeCoroutine != null) yield return fadeCoroutine;
        
        // ¡ATERRIZAJE!
        Debug.Log("[PlayerSpawn] ¡Player aterrizó!");

        
        // Al aterrizar:
        if (landingDustParticles != null)
        {    
            landingDustParticles.transform.position = transform.position;    
            landingDustParticles.Play();
        }
        
        // Reset rotación
        transform.rotation = Quaternion.identity;
        
        // Cambiar a sprite de aterrizaje
        if (landingSprite != null && spriteRenderer != null)
        {
            spriteRenderer.sprite = landingSprite;
        }
        
        // Efecto de sacudida al aterrizar
        if (enableLandingShake)
        {
            yield return StartCoroutine(LandingShakeCoroutine());
        }
        
        // Mantener sprite de aterrizaje un momento
        yield return new WaitForSeconds(landingSpritesDuration);
        
        // Restaurar sprite idle (vía Animator o sprite original)
        if (animator != null)
        {
            animator.enabled = true; // Reactivar Animator (volverá a idle)
        }
        else if (originalSprite != null && spriteRenderer != null)
        {
            spriteRenderer.sprite = originalSprite;
        }
        
        // Parpadeo mientras vuelve a idle
        if (enableFlashing && spriteRenderer != null)
        {
            yield return StartCoroutine(FlashCoroutine());
        }
        
        // Restaurar física
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
        }
        
        // Reactivar control del jugador
        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }
        
        isAnimating = false;
        
        Debug.Log("[PlayerSpawn] Animación de entrada completada");
    }
    
    private IEnumerator MoveAndRotatePlayerCoroutine()
    {
        float elapsed = 0f;
        float currentRotation = 0f;
        
        while (elapsed < spawnDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / spawnDuration;
            float curveValue = spawnCurve.Evaluate(progress);
            
            // Movimiento vertical
            transform.position = Vector2.Lerp(spawnStartPosition, spawnEndPosition, curveValue);
            
            // Rotación mientras cae
            if (enableRotation)
            {
                currentRotation += rotationSpeed * rotationDirection * Time.deltaTime;
                transform.rotation = Quaternion.Euler(0f, 0f, currentRotation);
            }
            
            yield return null;
        }
        
        transform.position = spawnEndPosition;
    }
    
    private IEnumerator FadeInCoroutine()
    {
        float elapsed = 0f;
        Color c = spriteRenderer.color;
        
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / fadeInDuration;
            
            c.a = Mathf.Lerp(0f, 1f, progress);
            spriteRenderer.color = c;
            
            yield return null;
        }
        
        c.a = 1f;
        spriteRenderer.color = c;
    }
    
    private IEnumerator LandingShakeCoroutine()
    {
        Vector3 originalPosition = transform.position;
        float elapsed = 0f;
        
        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;
            
            // Sacudida horizontal
            float offsetX = Random.Range(-shakeIntensity, shakeIntensity);
            float offsetY = Random.Range(-shakeIntensity * 0.5f, shakeIntensity * 0.5f);
            
            transform.position = originalPosition + new Vector3(offsetX, offsetY, 0f);
            
            yield return null;
        }
        
        transform.position = originalPosition;
    }
    
    private IEnumerator FlashCoroutine()
    {
        Color originalColor = spriteRenderer.color;
        
        for (int i = 0; i < flashCount; i++)
        {
            // Transparente
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0.3f);
            yield return new WaitForSeconds(flashDuration);
            
            // Opaco
            spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(flashDuration);
        }
        
        spriteRenderer.color = originalColor;
    }
    
    /// <summary>
    /// Llamar desde el GameManager cuando el player muere
    /// </summary>
    public void PlayDeathRespawnAnimation()
    {
        Debug.Log("[PlayerSpawn] Player murió, reproduciendo animación de respawn");
        PlaySpawnAnimation();
    }
    
    public bool IsAnimating()
    {
        return isAnimating;
    }
}
