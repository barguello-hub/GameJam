using UnityEngine;

public class Coin : MonoBehaviour
{
    [Header("Collect Settings")]
    [SerializeField] private int coinValue = 1;                    // Valor de la moneda
    
    [Header("Collect Effects")]
    [SerializeField] private float collectDuration = 0.5f;         // Duración del efecto
    [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 1, 1, 1.5f);
    [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
    [SerializeField] private float floatSpeed = 2f;                // Velocidad de elevación
    [SerializeField] private float floatHeight = 1f;               // Altura de elevación
    
    [Header("Effects Type")]
    [SerializeField] private bool useScale = true;                 // Escalar al recoger
    [SerializeField] private bool useFade = true;                  // Desvanecer
    [SerializeField] private bool useFloat = true;                 // Elevarse
    [SerializeField] private bool useSpin = true;                  // Girar
    [SerializeField] private float spinSpeed = 720f;               // Velocidad de rotación
    
    private bool isCollected = false;
    private float collectTimer = 0f;
    private Vector3 startPosition;
    private Vector3 startScale;
    private SpriteRenderer spriteRenderer;
    private Collider2D coinCollider;
    
    private void Start()
    {
        startPosition = transform.position;
        startScale = transform.localScale;
        spriteRenderer = GetComponent<SpriteRenderer>();
        coinCollider = GetComponent<Collider2D>();
        
        if (spriteRenderer == null)
        {
            Debug.LogError("[Coin] No se encontró SpriteRenderer");
        }
    }
    
    private void Update()
    {
        if (isCollected)
        {
            collectTimer += Time.deltaTime;
            float progress = collectTimer / collectDuration;
            
            if (progress >= 1f)
            {
                // Efecto completado, destruir la moneda
                Destroy(gameObject);
                return;
            }
            
            // Aplicar efectos
            ApplyCollectEffects(progress);
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isCollected)
        {
            CollectCoin();
        }
    }
    
    [Header("Particle Effects")]
    [SerializeField] private ParticleSystem collectParticles;
    private void CollectCoin()
   {
        isCollected = true;
        
        if (coinCollider != null)
        {
            coinCollider.enabled = false;
        }
        
        // Activar partículas ANTES de destruir
        if (collectParticles != null)
        {
            // Desacoplar las partículas del objeto padre
            collectParticles.transform.SetParent(null);
            collectParticles.Play();
            
            // Destruir el sistema de partículas después de que terminen
            Destroy(collectParticles.gameObject, collectParticles.main.duration + collectParticles.main.startLifetime.constantMax);
        }
        
        Debug.Log($"[Coin] Moneda recogida! Valor: {coinValue}");
    }

    
    private void ApplyCollectEffects(float progress)
    {
        // Efecto de escala (agrandarse)
        if (useScale)
        {
            float scaleMultiplier = scaleCurve.Evaluate(progress);
            transform.localScale = startScale * scaleMultiplier;
        }
        
        // Efecto de elevación (flotar hacia arriba)
        if (useFloat)
        {
            float yOffset = floatHeight * progress;
            transform.position = startPosition + Vector3.up * yOffset;
        }
        
        // Efecto de desvanecimiento (fade out)
        if (useFade && spriteRenderer != null)
        {
            float alpha = fadeCurve.Evaluate(progress);
            Color color = spriteRenderer.color;
            color.a = alpha;
            spriteRenderer.color = color;
        }
        
        // Efecto de rotación (spin)
        if (useSpin)
        {
            float rotation = spinSpeed * Time.deltaTime;
            transform.Rotate(0, 0, rotation);
        }
    }
}
