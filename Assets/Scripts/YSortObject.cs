using UnityEngine;

public class YSortObject : MonoBehaviour
{
    [Header("Sorting Settings")]
    [SerializeField] private string sortingLayerName = "Deco";
    [SerializeField] private int baseSortingOrder = 0;
    [SerializeField] private float sortingOrderMultiplier = -100f; // Negativo = más abajo = más atrás
    
    private SpriteRenderer spriteRenderer;
    
    private void Start()
    {
        // Buscar el SpriteRenderer en este objeto o en hijos
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        
        if (spriteRenderer == null)
        {
            Debug.LogError($"[YSort] No se encontró SpriteRenderer en {gameObject.name}");
            enabled = false;
            return;
        }
        
        // Configurar el Sorting Layer
        spriteRenderer.sortingLayerName = sortingLayerName;
    }
    
    private void LateUpdate()
    {
        // Calcular Order in Layer según posición Y
        int sortingOrder = baseSortingOrder + Mathf.RoundToInt(transform.position.y * sortingOrderMultiplier);
        spriteRenderer.sortingOrder = sortingOrder;
    }
}
