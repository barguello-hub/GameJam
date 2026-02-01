using UnityEngine;

public class PlayerKnockbackTest : MonoBehaviour
{
    private PlayerKnockback knockback;
    
    private void Start()
    {
        knockback = GetComponent<PlayerKnockback>();
    }
    
    private void Update()
    {
        // Presiona K para probar el knockback
        if (Input.GetKeyDown(KeyCode.K))
        {
            Debug.Log("[Test] Aplicando knockback de prueba");
            knockback.ApplyKnockbackBackward();
        }
    }
}
