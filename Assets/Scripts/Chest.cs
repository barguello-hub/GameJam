using UnityEngine;

public class Chest : MonoBehaviour
{
    [Header("Chest Settings")]
    [SerializeField] private string itemName = "Vestido Rojo";
    [SerializeField] private bool isOpened = false;
    
    [Header("Interaction Settings")]
    [SerializeField] private float interactionRange = 1.5f;
    [SerializeField] private KeyCode interactionKey = KeyCode.E;
    [SerializeField] private bool showInteractionPrompt = true;
    
    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string openTrigger = "Open";
    
    [Header("Notification")]
    [SerializeField] private float notificationDuration = 2f;
    
    [Header("Visual Feedback")]
    [SerializeField] private GameObject interactionIndicator; // Sprite "E" encima del cofre
    
    private Transform player;
    private bool playerInRange = false;
    private SpriteRenderer spriteRenderer;
    
    private void Start()
    {
        // Buscar al jugador
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        
        if (player == null)
        {
            Debug.LogWarning("[Chest] No se encontró el Player. Asegúrate de que tenga el tag 'Player'");
        }
        
        // Auto-asignar Animator si no está
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
        
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Ocultar indicador al inicio
        if (interactionIndicator != null)
        {
            interactionIndicator.SetActive(false);
        }
        
        // Configurar Sorting Layer
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingLayerName = "Ground";
            spriteRenderer.sortingOrder = 0;
        }
    }
    
    private void Update()
    {
        if (isOpened || player == null) return;
        
        // Calcular distancia al jugador
        float distance = Vector2.Distance(transform.position, player.position);
        playerInRange = distance <= interactionRange;
        
        // Mostrar/ocultar indicador
        if (interactionIndicator != null && showInteractionPrompt)
        {
            interactionIndicator.SetActive(playerInRange);
        }
        
        // Detectar input de interacción
        if (playerInRange && Input.GetKeyDown(interactionKey))
        {
            OpenChest();
        }
    }
    
    private void OpenChest()
    {
        if (isOpened) return;
        
        isOpened = true;
        
        Debug.Log($"[Chest] Abriendo cofre! Obteniendo: {itemName}");
        
        // Reproducir animación
        if (animator != null)
        {
            animator.SetTrigger(openTrigger);
        }
        
        // Ocultar indicador
        if (interactionIndicator != null)
        {
            interactionIndicator.SetActive(false);
        }
        
        // Añadir objeto al inventario
        AddItemToInventory();
        
        // Mostrar notificación
        ShowNotification();
    }
    
    private void AddItemToInventory()
    {
        // Verificar que GameManager existe
        if (GameManager.Instance != null)
        {
            GameManager.Instance.PickUpObject(itemName);
            Debug.Log($"[Chest] Item '{itemName}' añadido al inventario");
        }
        else
        {
            Debug.LogError("[Chest] GameManager.Instance no encontrado!");
        }
    }
    
    private void ShowNotification()
    {
        // Usar el Singleton
        if (NotificationManager.Instance != null)
        {
            NotificationManager.Instance.ShowNotification($"Has obtenido: {itemName}", notificationDuration);
        }
        else
        {
            Debug.LogWarning("[Chest] No se encontró NotificationManager. Creando notificación básica...");
            Debug.Log($"★ HAS OBTENIDO: {itemName} ★");
        }
    }

    
    private void OnDrawGizmos()
    {
        // Dibujar rango de interacción
        Gizmos.color = isOpened ? Color.gray : Color.green;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}
