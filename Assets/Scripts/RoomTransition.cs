using UnityEngine;

public class RoomTransition : MonoBehaviour
{
    [Header("Target Room")]
    [SerializeField] private Transform targetRoom;
    
    [Header("Player Spawn Settings")]
    [SerializeField] private Vector2 playerSpawnOffset;
    
    [Header("Camera Reference")]
    [SerializeField] private Camera mainCamera;
    
    [Header("Transition Settings")]
    [SerializeField] private float cooldownTime = 0.5f; // Tiempo de espera entre transiciones
    
    // Variable ESTÁTICA para controlar el cooldown global
    private static float lastTransitionTime = -999f;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Verificar cooldown global
            if (Time.time - lastTransitionTime < cooldownTime)
            {
                Debug.Log($"[{gameObject.name}] Cooldown activo, ignorando transición");
                return;
            }
            
            Debug.Log($"[{gameObject.name}] ✅ Transición iniciada");
            TransitionToRoom(other.transform);
        }
    }
    
    private void TransitionToRoom(Transform player)
    {
        if (targetRoom == null || mainCamera == null)
        {
            Debug.LogError($"[{gameObject.name}] Target Room o Camera no asignados");
            return;
        }
        
        // Actualizar el tiempo de la última transición
        lastTransitionTime = Time.time;
        
        // Mover la cámara
        Vector3 newCameraPosition = targetRoom.position;
        newCameraPosition.z = mainCamera.transform.position.z;
        mainCamera.transform.position = newCameraPosition;
        
        // Mover al jugador
        Vector3 newPlayerPosition = targetRoom.position + (Vector3)playerSpawnOffset;
        player.position = newPlayerPosition;
        
        Debug.Log($"[{gameObject.name}] Transición completada → {targetRoom.name}");
    }
}
