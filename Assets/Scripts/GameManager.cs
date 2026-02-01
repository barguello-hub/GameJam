using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using TMPro;
using System.Numerics;

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
    public GameObject dialogueLayer;
    public GameObject EnemyIndicators;
    public GameObject AmaraIndicators;
    public GameObject InventoryLayer;
    public GameObject Player;
    public GameObject BattleTitleImage;
    private DialogueManager dialogueManager;
    public int InitialAmaraDetermination;
    public int currentAmaraDetermination;
    public List<InventoryPair> AmaraInventory;
    private int currentEnemyAnger;
    public bool isInBattle;

    public AudioClip idle_music;
    public AudioClip battle_music;
    private AudioSource audioSource;
    public AudioSource sfxSource;

    public AudioClip MaskBreakingSfx;
    public AudioClip MaskBrokenSfx;
    public AudioClip ChestOpenSfx;

    public Sprite startTherapyTitle;
    public Sprite endTherapyTitle;
    public Sprite failTherapyTitle;

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

        audioSource = GetComponent<AudioSource>();
        audioSource.clip = idle_music;
        audioSource.loop = true;
        audioSource.Play();
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

        Screen.SetResolution(1280, 768, false);
        Screen.fullScreenMode = FullScreenMode.Windowed;
        initial_pos = dialogueLayer.GetComponent<RectTransform>().anchoredPosition;

        BattleTitleImage.SetActive(false);
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
                if(available_objects.Contains("Blusa Roja"))
                {
                    return "Blusa Roja";
                }
                break;
            case "StorePhase":
                if(available_objects.Contains("Oro"))
                {
                    return "Oro";
                }
                break;
            case "FirstEnemyPhase":
                if(available_objects.Contains("Palito de Regaliz"))
                {
                    return "Palito de Regaliz";
                }
                break;
            default:
                return available_objects[0];
        }
        return available_objects[0];
    }

    public void StartBattle(string PhaseName)
    {
        if(isInBattle == true) return;
        dialogueManager.isInDialogue = true;
        dialogueManager.currentPhaseName = PhaseName;
        dialogueManager.currentPrompt = null;
        dialogueManager.GetFirstPrompt();
        currentEnemyAnger = 0;
        isInBattle = true;
        anim_start_finished = false;
        acc_time = 0.0f;
        Player.GetComponent<PlayerMovement>().moveSpeed = 0.0f;
        UpdateUI();

        audioSource.clip = battle_music;
        audioSource.loop = true;
        audioSource.Play();

        BattleTitleImage.SetActive(true);
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

    public void EndBattle()
    {
        isInBattle = false;
        Player.GetComponent<PlayerMovement>().moveSpeed = 3.0f;
        EndDialogue();
        UpdateUI();

        if(dialogueManager.currentPhaseName == "SecondEnemyPhase")
        {
            PickUpObject("Blusa Roja");
            RemoveObject("Palito de Regaliz");
        }

        if(dialogueManager.currentPhaseName == "StorePhase")
        {
            PickUpObject("Palito de Regaliz");
            RemoveObject("Oro");
        }

        audioSource.clip = idle_music;
        audioSource.loop = true;
        audioSource.Play();

        BattleTitleImage.SetActive(true);
        anim_end_finished = false;
        acc_time = 0.0f;
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

    public void RemoveObject(string object_name)
    {
        foreach(InventoryPair pair in AmaraInventory)
        {
            if(string.Equals(object_name, pair.name, System.StringComparison.OrdinalIgnoreCase))
            {
                pair.count = 0;
                return;
            }
        }
    }

    public void ReproduceSound(string soundName)
    {
        switch (soundName)
        {
            case "MaskBreaking":
            sfxSource.PlayOneShot(MaskBreakingSfx);
            break;
            case "OpenChest":
            sfxSource.PlayOneShot(ChestOpenSfx);
            break;
            case "MaskBroken":
            sfxSource.PlayOneShot(MaskBrokenSfx);
            break;
        }
    }

    private float acc_time = 0.0f;
    private UnityEngine.Vector2 initial_pos;
    private bool anim_start_finished = false;
    private bool anim_end_finished = false;
    public float BattleStartLength;
    void Update()
    {
        if(isInBattle)
        {
            if(!anim_start_finished)
            {
                acc_time+=Time.deltaTime;

                float t = acc_time / BattleStartLength;
                t = Mathf.SmoothStep(1f, 0f, t);

                RectTransform transform = TopLayer.GetComponent<RectTransform>();
                transform.sizeDelta = new UnityEngine.Vector2(0.0f, t * 2000.0f);

                transform = BattleTitleImage.GetComponent<RectTransform>();
                transform.anchoredPosition = new UnityEngine.Vector2(0.0f, 0.0f);

                BattleTitleImage.GetComponent<Image>().sprite = startTherapyTitle;

                float alpha = acc_time / BattleStartLength;
                alpha =  Mathf.SmoothStep(0f, 1.0f, t);
                
                Color c = BattleTitleImage.GetComponent<Image>().color;
                c.a = alpha;   // 0 = transparent, 1 = opaque
                BattleTitleImage.GetComponent<Image>().color = c;


                if(acc_time > BattleStartLength)
                {
                    anim_start_finished = true;
                    acc_time = 0.0001f;
                    BattleTitleImage.SetActive(false);
                }
                // transform = dialogueLayer.GetComponent<RectTransform>();
                // transform.anchoredPosition = initial_pos + new UnityEngine.Vector2(0.0f, MathF.Sin(acc_time * 1.5f) * 12.5f);
            }
            else
            {
                acc_time+=Time.deltaTime;
                RectTransform transform = TopLayer.GetComponent<RectTransform>();
                transform.sizeDelta = new UnityEngine.Vector2(0.0f, MathF.Sin(acc_time * 1.5f) * 25.0f);
                transform = dialogueLayer.GetComponent<RectTransform>();
                transform.anchoredPosition = initial_pos + new UnityEngine.Vector2(0.0f, MathF.Sin(acc_time * 1.5f) * 12.5f);
            }


        }
        else
        {
            if (!anim_end_finished)
            {
                if(acc_time > BattleStartLength)
                {
                    anim_end_finished = true;
                    acc_time = 0.0001f;
                    BattleTitleImage.SetActive(false);
                }

                if(currentAmaraDetermination <= 0 || currentEnemyAnger >= 3)
                {  
                    BattleTitleImage.GetComponent<Image>().sprite = failTherapyTitle;
                }
                else
                {
                    BattleTitleImage.GetComponent<Image>().sprite = endTherapyTitle;
                }

                acc_time+=Time.deltaTime;
                float t = acc_time / (BattleStartLength*0.66f);
                t = Mathf.SmoothStep(1f, 0f, t);

                RectTransform transform = BattleTitleImage.GetComponent<RectTransform>();
                transform.anchoredPosition = new UnityEngine.Vector2(0.0f, 0.0f);

                Color c = BattleTitleImage.GetComponent<Image>().color;
                c.a = t;   // 0 = transparent, 1 = opaque
                BattleTitleImage.GetComponent<Image>().color = c;

            }
        }
    }
}
