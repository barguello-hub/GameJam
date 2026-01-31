using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    [Header("Patrol Area")]
    [SerializeField] private Vector2 minBounds = new Vector2(12f, 0f);    // Esquina inferior izquierda
    [SerializeField] private Vector2 maxBounds = new Vector2(14f, 1f);    // Esquina superior derecha
    
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 1f;                        // Velocidad de patrullaje
    [SerializeField] private float waitTimeAtPoint = 1f;                   // Tiempo de espera en cada esquina
    
    [Header("Patrol Type")]
    [SerializeField] private bool randomPatrol = false;                    // ¿Movimiento aleatorio o en línea?
    
    private Vector2 currentTarget;
    private bool isWaiting = false;
    private float waitTimer = 0f;
    
    private void Start()
    {
        // Empezar en la posición actual o en el centro del área
        transform.position = new Vector3(
            Mathf.Clamp(transform.position.x, minBounds.x, maxBounds.x),
            Mathf.Clamp(transform.position.y, minBounds.y, maxBounds.y),
            transform.position.z
        );
        
        // Elegir primer destino
        ChooseNewTarget();
        
        Debug.Log($"[Enemy] Patrullaje iniciado entre {minBounds} y {maxBounds}");
    }
    
    private void Update()
    {
        if (isWaiting)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f)
            {
                isWaiting = false;
                ChooseNewTarget();
            }
            return;
        }
        
        // Moverse hacia el objetivo
        Vector2 currentPos = transform.position;
        Vector2 newPos = Vector2.MoveTowards(currentPos, currentTarget, moveSpeed * Time.deltaTime);
        transform.position = new Vector3(newPos.x, newPos.y, transform.position.z);
        
        // ¿Llegó al objetivo?
        if (Vector2.Distance(currentPos, currentTarget) < 0.1f)
        {
            isWaiting = true;
            waitTimer = waitTimeAtPoint;
        }
    }
    
    private void ChooseNewTarget()
    {
        if (randomPatrol)
        {
            // Movimiento aleatorio dentro del área
            currentTarget = new Vector2(
                Random.Range(minBounds.x, maxBounds.x),
                Random.Range(minBounds.y, maxBounds.y)
            );
        }
        else
        {
            // Movimiento en línea (esquinas del rectángulo)
            // Alterna entre las 4 esquinas
            int corner = Random.Range(0, 4);
            switch (corner)
            {
                case 0: currentTarget = new Vector2(minBounds.x, minBounds.y); break; // Abajo-Izquierda
                case 1: currentTarget = new Vector2(maxBounds.x, minBounds.y); break; // Abajo-Derecha
                case 2: currentTarget = new Vector2(maxBounds.x, maxBounds.y); break; // Arriba-Derecha
                case 3: currentTarget = new Vector2(minBounds.x, maxBounds.y); break; // Arriba-Izquierda
            }
        }
        
        Debug.Log($"[Enemy] Nuevo objetivo: {currentTarget}");
    }
    
    // DEBUG: Dibujar el área de patrullaje en la Scene view
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        
        Vector3 bottomLeft = new Vector3(minBounds.x, minBounds.y, 0);
        Vector3 bottomRight = new Vector3(maxBounds.x, minBounds.y, 0);
        Vector3 topRight = new Vector3(maxBounds.x, maxBounds.y, 0);
        Vector3 topLeft = new Vector3(minBounds.x, maxBounds.y, 0);
        
        Gizmos.DrawLine(bottomLeft, bottomRight);
        Gizmos.DrawLine(bottomRight, topRight);
        Gizmos.DrawLine(topRight, topLeft);
        Gizmos.DrawLine(topLeft, bottomLeft);
        
        // Dibujar el objetivo actual
        if (Application.isPlaying)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(currentTarget, 0.2f);
        }
    }
}
