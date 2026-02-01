using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void Shake(float duration = 0.3f, float magnitude = 0.2f)
    {
        StartCoroutine(ShakeCoroutine(duration, magnitude));
    }
    
    private IEnumerator ShakeCoroutine(float duration, float magnitude)
    {
        Vector3 originalPos = transform.localPosition;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;
            
            transform.localPosition = new Vector3(originalPos.x + x, originalPos.y + y, originalPos.z);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        transform.localPosition = originalPos;
    }
}
