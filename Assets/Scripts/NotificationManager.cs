using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class NotificationManager : MonoBehaviour
{
    public static NotificationManager Instance { get; private set; }
    
    [Header("UI References")]
    [SerializeField] private GameObject notificationPanel;
    [SerializeField] private Text notificationText;
    
    [Header("Animation Settings")]
    [SerializeField] private float fadeInDuration = 0.3f;
    [SerializeField] private float fadeOutDuration = 0.3f;
    [SerializeField] private float slideDistance = 30f;
    
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Coroutine currentNotification;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(transform.root.gameObject); // Opcional: para mantener entre escenas
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    private void Start()
    {
        if (notificationPanel == null)
        {
            Debug.LogError("[NotificationManager] ¡Notification Panel no asignado!");
            return;
        }
        
        // Obtener componentes del panel
        canvasGroup = notificationPanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = notificationPanel.AddComponent<CanvasGroup>();
        }
        
        rectTransform = notificationPanel.GetComponent<RectTransform>();
        
        // Inicializar oculto (pero el GameObject permanece ACTIVO)
        canvasGroup.alpha = 0f;
        
        Debug.Log("[NotificationManager] Inicializado correctamente");
    }
    
    public void ShowNotification(string message, float duration = 2f)
    {
        if (notificationPanel == null)
        {
            Debug.LogError("[NotificationManager] ¡Panel no asignado!");
            return;
        }
        
        // Cancelar notificación anterior si existe
        if (currentNotification != null)
        {
            StopCoroutine(currentNotification);
        }
        
        currentNotification = StartCoroutine(NotificationCoroutine(message, duration));
    }
    
    private IEnumerator NotificationCoroutine(string message, float duration)
    {
        // Configurar texto
        if (notificationText != null)
        {
            notificationText.text = message;
        }
        else
        {
            Debug.LogWarning("[NotificationManager] ¡NotificationText no asignado!");
        }
        
        // Asegurar que el panel esté visible (pero transparente)
        // NO lo desactivamos, solo jugamos con el alpha
        
        // Posición inicial (abajo de la pantalla)
        Vector2 startPos = rectTransform.anchoredPosition;
        Vector2 hiddenPos = startPos - new Vector2(0, slideDistance);
        rectTransform.anchoredPosition = hiddenPos;
        
        // Fade In + Slide Up
        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / fadeInDuration;
            
            canvasGroup.alpha = progress;
            rectTransform.anchoredPosition = Vector2.Lerp(hiddenPos, startPos, progress);
            
            yield return null;
        }
        
        canvasGroup.alpha = 1f;
        rectTransform.anchoredPosition = startPos;
        
        // Esperar
        yield return new WaitForSeconds(duration);
        
        // Fade Out + Slide Down
        elapsed = 0f;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / fadeOutDuration;
            
            canvasGroup.alpha = 1f - progress;
            rectTransform.anchoredPosition = Vector2.Lerp(startPos, hiddenPos, progress);
            
            yield return null;
        }
        
        canvasGroup.alpha = 0f;
        // No lo desactivamos, solo lo hacemos invisible
        
        currentNotification = null;
    }
}
