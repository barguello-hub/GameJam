using UnityEngine;

public class EnemyPatrolRandom : MonoBehaviour
{
    [Header("Patrol Area")]
    [SerializeField] private Vector2 minBounds = new Vector2(12f, 0f);
    [SerializeField] private Vector2 maxBounds = new Vector2(14f, 1f);
    
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private float changeDirectionTime = 2f;
    
    [Header("Detection Settings")]
    [SerializeField] private float detectionRange = 2f;
    [SerializeField] private bool showDetectionRange = true;
    
    [Header("Battle Settings")]
    [SerializeField] private string battleSceneName = "FirstEnemyPhase";
    [SerializeField] private bool hasTriggeredBattle = false;
    [SerializeField] private bool stopMovementOnDetection = true;
    [SerializeField] private bool pelea = false;
    
    [Header("Cooldown Settings")]
    [SerializeField] private bool canReattackAfterDefeat = true; // ¿Puede volver a atacar si el player pierde?
    [SerializeField] private float reattackCooldown = 5f; // Tiempo de espera antes de poder atacar de nuevo
    [SerializeField] private bool showCooldownInGizmos = true;
    
    private Vector2 currentDirection;
    private float directionTimer = 0f;
    private Transform player;
    private bool isPlayerDetected = false;
    private bool isOnCooldown = false; // ¿Está en cooldown?
    private float cooldownTimer = 0f;
    
    [Header("Animation")]
    [SerializeField] private bool jumpTowardsPlayer = true;
    [SerializeField] private float jumpDuration = 0.3f;
    
    private void Start()
    {
        // Posición inicial dentro del área
        transform.position = new Vector3(
            Random.Range(minBounds.x, maxBounds.x),
            Random.Range(minBounds.y, maxBounds.y),
            transform.position.z
        );
        
        ChooseRandomDirection();
        
        // Buscar al jugador
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        
        if (player == null)
        {
            Debug.LogWarning("[Enemy] No se encontró el Player. Asegúrate de que tenga el tag 'Player'");
        }
    }
    
    private void Update()
    {
        // Actualizar cooldown
        if (isOnCooldown)
        {
            cooldownTimer -= Time.deltaTime;
            
            if (cooldownTimer <= 0f)
            {
                // ¡Cooldown terminado!
                isOnCooldown = false;
                hasTriggeredBattle = false;
                isPlayerDetected = false;
                
                Debug.Log($"[Enemy] Cooldown terminado. Puede atacar de nuevo.");
                
                // Volver a patrullar
                ChooseRandomDirection();
            }
        }
        
        // Detectar al jugador
        CheckPlayerProximity();
        
        // No moverse si está detenido, en combate o en cooldown
        if (isPlayerDetected || hasTriggeredBattle || isOnCooldown)
        {
            return;
        }
        
        // Timer para cambiar dirección
        directionTimer -= Time.deltaTime;
        if (directionTimer <= 0f)
        {
            ChooseRandomDirection();
            directionTimer = changeDirectionTime;
        }
        
        // Moverse
        Vector2 currentPos = transform.position;
        Vector2 newPos = currentPos + currentDirection * moveSpeed * Time.deltaTime;
        
        // Rebotar si toca los límites
        if (newPos.x < minBounds.x || newPos.x > maxBounds.x)
        {
            currentDirection.x = -currentDirection.x;
            newPos.x = Mathf.Clamp(newPos.x, minBounds.x, maxBounds.x);
        }
        
        if (newPos.y < minBounds.y || newPos.y > maxBounds.y)
        {
            currentDirection.y = -currentDirection.y;
            newPos.y = Mathf.Clamp(newPos.y, minBounds.y, maxBounds.y);
        }
        
        transform.position = new Vector3(newPos.x, newPos.y, transform.position.z);
    }
    
    private void CheckPlayerProximity()
    {
        if (player == null || hasTriggeredBattle || isOnCooldown) return;
        
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        // Si el jugador está dentro del rango
        if (distanceToPlayer <= detectionRange)
        {
            if (!isPlayerDetected)
            {
                OnPlayerDetected();
            }
        }
    }
    
    private void OnPlayerDetected()
    {
        if (pelea) {
            isPlayerDetected = true;
            hasTriggeredBattle = true;
        }
        
        Debug.Log($"[Enemy] ¡Jugador detectado! Iniciando batalla: {battleSceneName}");
        
        // Saltar hacia el jugador
        if (jumpTowardsPlayer)
        {
            StartCoroutine(JumpTowardsPlayer());
        }
        else
        {
            StartBattle();
        }      
    }
    
    private void StartBattle()
    {
        // Verificar que el GameManager existe
        if (GameManager.Instance != null)
        {
            if(!hasTriggeredBattle && !GameManager.Instance.isInBattle)
            {
                hasTriggeredBattle = true;
                GameManager.Instance.StartBattle(battleSceneName, gameObject);
            }
            Debug.Log($"[Enemy] Combate iniciado: {battleSceneName}");
        }
        else
        {
            Debug.LogError("[Enemy] ¡GameManager.Instance no encontrado! Asegúrate de que existe en la escena.");
        }
    }    

    private System.Collections.IEnumerator JumpTowardsPlayer()
    {
        Vector3 startPos = transform.position;
        Vector3 endPos = player.position;
        float elapsed = 0f;
        
        while (elapsed < jumpDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / jumpDuration;
            
            // Movimiento con arco (parábola)
            Vector3 currentPos = Vector3.Lerp(startPos, endPos, progress);
            currentPos.y += Mathf.Sin(progress * Mathf.PI) * 0.5f; // Altura del salto
            
            transform.position = currentPos;
            
            yield return null;
        }
        
        jumpTowardsPlayer = false;
        if(pelea) StartBattle();
    }
    
    /// <summary>
    /// Llamar desde GameManager cuando la batalla termina y el player PIERDE
    /// </summary>
    public void OnPlayerDefeated()
    {
        if (!canReattackAfterDefeat)
        {
            Debug.Log($"[Enemy] Player derrotado, pero este enemigo NO puede volver a atacar.");
            return; // No puede volver a atacar
        }
        
        Debug.Log($"[Enemy] Player derrotado. Iniciando cooldown de {reattackCooldown} segundos.");
        
        // Iniciar cooldown
        isOnCooldown = true;
        cooldownTimer = reattackCooldown;
    }
    
    /// <summary>
    /// Llamar desde GameManager cuando la batalla termina y el player GANA
    /// </summary>
    public void OnPlayerVictory()
    {
        Debug.Log($"[Enemy] Player ganó la batalla. Enemigo desactivado permanentemente.");
        
        // Desactivar completamente el enemigo
        hasTriggeredBattle = true;
        isOnCooldown = false;
        
        // Opcional: Destruir o desactivar el GameObject
        // gameObject.SetActive(false);
        // Destroy(gameObject);
    }
    
    /// <summary>
    /// Resetear manualmente el estado del enemigo (útil para testing)
    /// </summary>
    public void ResetEnemyState()
    {
        hasTriggeredBattle = false;
        isPlayerDetected = false;
        isOnCooldown = false;
        cooldownTimer = 0f;
        
        ChooseRandomDirection();
        
        Debug.Log($"[Enemy] Estado reseteado manualmente.");
    }
    
    private void ChooseRandomDirection()
    {
        currentDirection = new Vector2(
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f)
        ).normalized;
    }
    
    private void OnDrawGizmos()
    {
        // Área de patrullaje (amarillo)
        Gizmos.color = Color.yellow;
        
        Vector3 bottomLeft = new Vector3(minBounds.x, minBounds.y, 0);
        Vector3 bottomRight = new Vector3(maxBounds.x, minBounds.y, 0);
        Vector3 topRight = new Vector3(maxBounds.x, maxBounds.y, 0);
        Vector3 topLeft = new Vector3(minBounds.x, maxBounds.y, 0);
        
        Gizmos.DrawLine(bottomLeft, bottomRight);
        Gizmos.DrawLine(bottomRight, topRight);
        Gizmos.DrawLine(topRight, topLeft);
        Gizmos.DrawLine(topLeft, bottomLeft);
        
        // Rango de detección
        if (showDetectionRange)
        {
            // Rojo si está en cooldown, amarillo si puede atacar
            if (Application.isPlaying && isOnCooldown && showCooldownInGizmos)
            {
                Gizmos.color = Color.blue; // Azul = en cooldown
            }
            else if (Application.isPlaying && hasTriggeredBattle)
            {
                Gizmos.color = Color.gray; // Gris = ya atacó
            }
            else
            {
                Gizmos.color = Color.red; // Rojo = listo para atacar
            }
            
            Gizmos.DrawWireSphere(transform.position, detectionRange);
        }
    }
}
