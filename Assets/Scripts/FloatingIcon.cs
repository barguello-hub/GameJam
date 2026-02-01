using UnityEngine;

public class FloatingIcon : MonoBehaviour
{
    [SerializeField] private float floatSpeed = 2f;
    [SerializeField] private float floatAmplitude = 0.2f;
    
    private Vector3 startPosition;
    
    private void Start()
    {
        startPosition = transform.localPosition;
    }
    
    private void Update()
    {
        float offset = Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
        transform.localPosition = startPosition + Vector3.up * offset;
    }
}
