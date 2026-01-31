using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

[Serializable]
public class InventoryPair
{
    public string name;
    public Sprite object_sprite;
    public int count = 0;
}

public class GameManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject Canvas;
    public GameObject TopLayer;
    public GameObject EnemyIndicators;
    public GameObject AmaraIndicators;
    public GameObject InventoryLayer;
    private DialogueManager dialogueManager;
    public int InitialAmaraDetermination;
    public int currentAmaraDetermination;
    public List<InventoryPair> AmaraInventory;
    private int currentEnemyAnger;
    private bool isInBattle;

    public static GameManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void OnApplicationQuit()
    {
        Instance = null;
    }

    void Start()
    {
        dialogueManager = Canvas.GetComponent<DialogueManager>();
        isInBattle = false;
        currentAmaraDetermination = InitialAmaraDetermination;
        UpdateUI();

        //DEBUG
        // StartBattle("FinalBossPhase");
    }

    public void WrongChoice()
    {
        currentEnemyAnger+=1;
        currentAmaraDetermination-=1;
        CheckEndBattle();
        UpdateUI();
    }

    public void StartBattle(string PhaseName)
    {
        dialogueManager.isInDialogue = true;
        dialogueManager.currentPhaseName = PhaseName;
        dialogueManager.currentPrompt = null;
        currentEnemyAnger = 0;
        isInBattle = true;
        UpdateUI();
    }

    public void ToggleInventory()
    {   
        InventoryLayer.transform.GetChild(1).gameObject.SetActive(!InventoryLayer.transform.GetChild(1).gameObject.activeSelf);
        // Transform[] children = inventoryLayer.transform.GetChild(0).GetComponentsInChildren<Transform>(true);
        // int i = 0;
        // foreach (Transform child in children)
        // {
        //     child.GetComponent<Image>().sprite = null;
        //     if(GameManager.Instance.AmaraInventory.Count >= i+1)
        //     {
        //         child.GetComponent<Image>().sprite = GameManager.Instance.AmaraInventory[i].object_sprite;
        //         child.GetChild(0).GetComponent<TextMeshProUGUI>().text = GameManager.Instance.AmaraInventory[i].name;
        //     }
        //     else
        //     {
        //         break;
        //     }
        //     i += 1;
        // }
    }

    void UpdateUI()
    {

        if(isInBattle)
        {
            AmaraIndicators.SetActive(true);
            EnemyIndicators.SetActive(true);
            AmaraIndicators.transform.GetChild(1).GetComponent<Slider>().value = currentAmaraDetermination;
            EnemyIndicators.transform.GetChild(1).GetComponent<Slider>().value = currentEnemyAnger;
        }
        else
        {
            AmaraIndicators.SetActive(false);
            EnemyIndicators.SetActive(false);
        }

        if(dialogueManager.isInDialogue)
        {
            TopLayer.SetActive(true);
            InventoryLayer.SetActive(false);
            InventoryLayer.transform.GetChild(1).gameObject.SetActive(false);
        }
        else
        {
            TopLayer.SetActive(false);
            InventoryLayer.SetActive(true);
            InventoryLayer.transform.GetChild(1).gameObject.SetActive(false);
        }
        
    }

    void CheckEndBattle()
    {
        if(isInBattle)
        {
            if(currentAmaraDetermination <= 0 || currentEnemyAnger >= 3)
            {
                EndBattle();
            }
        }
    }

    void EndBattle()
    {
        isInBattle = false;
        EndDialogue();
        UpdateUI();
    }

    void EndDialogue()
    {
        dialogueManager.isInDialogue = false;
        UpdateUI();
    }

    public void PickUpObject(string object_name)
    {
        foreach(InventoryPair pair in AmaraInventory)
        {
            if(string.Equals(object_name, pair.name, System.StringComparison.OrdinalIgnoreCase))
            {
                pair.count += 1;
                return;
            }
        }
    }

    void Update()
    {
        if(isInBattle)
        {
            
        }
    }
}
