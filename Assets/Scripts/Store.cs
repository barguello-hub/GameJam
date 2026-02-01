using UnityEngine;

public class Store : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private bool has_entered_store = false;
    private void OnTriggerEnter2D(Collider2D other) {
        if(!has_entered_store)
        {
            has_entered_store = true;
            GameManager.Instance.StartBattle("StorePhase");
        }
    }
}
