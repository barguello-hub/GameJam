using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using TMPro;

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

    public bool HasObjects()
    {
        foreach(InventoryPair pair in AmaraInventory)
        {
            if(pair.count > 0)
            {
                return true;
            }
        }
        return false;
    }

    public string GetRelevantObject()
    {
        if(!HasObjects()) return "Nada";
        List<string> available_objects = new List<string>();
        foreach(InventoryPair pair in AmaraInventory)
        {
            if(pair.count > 0)
            {
                available_objects.Add(pair.name);
            }
        }

        switch(dialogueManager.currentPhaseName)
        {
            case "FinalBossPhase":
                if(available_objects.Contains("Vestido Rojo"))
                {
                    return "Vestido Rojo";
                }
                break;
            case "StorePhase":
                if(available_objects.Contains("Oro"))
                {
                    return "Oro";
                }
                break;
            default:
                return available_objects[0];
        }
        return available_objects[0];
    }

    public void StartBattle(string PhaseName)
    {
        dialogueManager.isInDialogue = true;
        dialogueManager.currentPhaseName = PhaseName;
        dialogueManager.currentPrompt = null;
        dialogueManager.GetFirstPrompt();
        currentEnemyAnger = 0;
        isInBattle = true;
        UpdateUI();
    }

    public void ToggleInventory()
    {   
        InventoryLayer.transform.GetChild(0).gameObject.SetActive(!InventoryLayer.transform.GetChild(0).gameObject.activeSelf);
        InventoryLayer.transform.GetChild(2).gameObject.SetActive(!InventoryLayer.transform.GetChild(2).gameObject.activeSelf);
        Transform[] children = InventoryLayer.transform.GetChild(1).GetComponentsInChildren<Transform>(true);
        int i = 0;
        foreach (Transform child in InventoryLayer.transform.GetChild(2))
        {
            while(AmaraInventory.Count >= i+1 && AmaraInventory[i].count <= 0) i+=1;
            if(AmaraInventory.Count >= i+1)
            {
                child.gameObject.SetActive(true);
                child.GetComponent<Image>().sprite = AmaraInventory[i].object_sprite;
                child.GetChild(0).GetComponent<TextMeshProUGUI>().text = AmaraInventory[i].name;
                if(AmaraInventory[i].count > 1)
                {
                    child.GetChild(0).GetComponent<TextMeshProUGUI>().text += " x" + AmaraInventory[i].count.ToString();
                }
            }
            else
            {
                child.gameObject.SetActive(false);
            }
            i += 1;
        }
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
            InventoryLayer.transform.GetChild(2).gameObject.SetActive(false);
        }
        else
        {
            TopLayer.SetActive(false);
            InventoryLayer.SetActive(true);
            InventoryLayer.transform.GetChild(2).gameObject.SetActive(false);
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
            
        }else
        {
            // StartBattle("FinalBossPhase");
        }
    }
}
