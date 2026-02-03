using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using TMPro;
using System.Numerics;
using UnityEngine.SceneManagement;

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
    public List<Sprite> AmaraMoodSprites;
    public GameObject InventoryLayer;
    public GameObject Player;
    public GameObject BattleTitleImage;
    public GameObject IntroBackground;
    public List<Sprite> IntroSprites;
    public int CurrentSpriteCounter = 0;
    private DialogueManager dialogueManager;
    public int InitialAmaraDetermination;
    public int currentAmaraDetermination;
    public List<InventoryPair> AmaraInventory;
    public int currentEnemyAnger;
    public bool isInBattle;

    public AudioClip idle_music;
    public AudioClip battle_music;
    public AudioClip intro_music;
    private AudioSource audioSource;
    public AudioSource sfxSource;

    public AudioClip MaskBreakingSfx;
    public AudioClip MaskBrokenSfx;
    public AudioClip ChestOpenSfx;
    public AudioClip ScreamSfx;

    public Sprite startTherapyTitle;
    public Sprite endTherapyTitle;
    public Sprite failTherapyTitle;

    private PlayerKnockback playerKnockback;

    [Header("Battle System")]    
    private GameObject currentEnemy;

    [Header("Health Settings")]    
    [SerializeField] private int maxHealth = 100;    
    private int currentHealth;        
    [Header("Respawn Settings")]    
    [SerializeField] private Transform initialSpawnPoint; // Punto de spawn en R00 
    [SerializeField] private GameObject initialRoom;

    [Header("Camera Settings")]    
    [SerializeField] private Camera mainCamera;
    [SerializeField] private UnityEngine.Vector3 initialCameraPosition; // Posición de cámara en R00    
    
    [SerializeField] private bool autoDetectCameraPosition = true; // Detectar automáticamente    

    [SerializeField] private PlayerSpawnAnimation playerSpawnAnimation;

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

        currentHealth = maxHealth;

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

        audioSource.volume = 1.0f;
        sfxSource.volume = 0.7f;

        if (Player != null)        
        {            
            playerKnockback = Player.GetComponent<PlayerKnockback>();        
        }
        if (playerSpawnAnimation == null && Player != null)        
        {            
            playerSpawnAnimation = Player.GetComponent<PlayerSpawnAnimation>();        
        }

        // Buscar R00 automáticamente si no está asignado        
        if (initialRoom == null)        
        {            
            initialRoom = GameObject.Find("R00");                        
            if (initialRoom == null)            
            {                
                Debug.LogWarning("[GameManager] No se encontró la room 'R00'. Asígnala manualmente.");            
            }       
        }                
        // Buscar spawn point en R00 si no está asignado        
        if (initialSpawnPoint == null && initialRoom != null)        
        {            
            // Buscar un objeto hijo llamado "SpawnPoint" o similar            
            Transform spawnChild = initialRoom.transform.Find("SpawnPoint");                        
            if (spawnChild != null)            
            {                
                initialSpawnPoint = spawnChild;            
            }            else            {                
                // Si no existe, usar el centro de R00                
                initialSpawnPoint = initialRoom.transform;                
                Debug.Log("[GameManager] Usando posición de R00 como spawn point");            
            }        
        }

        if (mainCamera == null)        
        {            
            mainCamera = Camera.main;        
        }
        //  Detectar posición inicial de cámara automáticamente        
        if (autoDetectCameraPosition && mainCamera != null)        
        {            
            // Guardar la posición de la cámara al inicio (debería estar en R00)            
            initialCameraPosition = mainCamera.transform.position;            
            Debug.Log($"[GameManager] Posición inicial de cámara guardada: {initialCameraPosition}");        
        }
    }

    public void WrongChoice()
    {
        currentEnemyAnger+=1;
        currentAmaraDetermination-=1;
        CheckEndBattle();
        UpdateUI();
    }

    /// <summary>    
    /// Llamar cuando el player muere    
    /// </summary>    
    private void OnPlayerDeath()    
    {       
        Debug.Log("[GameManager] ¡Player ha muerto!");                
        // Restaurar vida completa        
        currentAmaraDetermination = InitialAmaraDetermination;   
        IntroBackground.GetComponent<Image>().sprite = IntroSprites[3];
                                   
        // Cargar Room0_0 y reproducir animación        
        StartCoroutine(RespawnPlayerCoroutine()); 
        currentEnemy = null;
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

    private System.Collections.IEnumerator RespawnPlayerCoroutine()    
    {        
        // Fade out o efecto de transición aquí (opcional)        
        // yield return new WaitForSeconds(1f);                
        // Cargar escena Room0_0        
        //MOVER LA CÁMARA A R00        
        if (mainCamera != null)        
        {            
            mainCamera.transform.position = initialCameraPosition;            
            Debug.Log($"[GameManager] Cámara movida a: {initialCameraPosition}");        
        }
        //SceneManager.LoadScene(room0_0SceneName);                
        // Esperar a que cargue la escena        
        yield return new WaitForSeconds(0.1f);

        if (Player != null)        
        {            
            playerSpawnAnimation = Player.GetComponent<PlayerSpawnAnimation>();                        
            if (playerSpawnAnimation != null)            
            {                
                playerSpawnAnimation.PlaySpawnAnimation();            
            }        
        }                
        // Buscar referencias nuevamente (la escena cambió)       
        //player = GameObject.FindGameObjectWithTag("Player");                  
    }

    public int GetCurrentHealth()    
    {        
        return currentHealth;    
    }

    public void Heal(int amount)    
    {        
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);        
        Debug.Log($"[GameManager] Player curado. Vida: {currentHealth}/{maxHealth}");    
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

    /// <summary>    
    /// Llamar cuando la batalla termina y el PLAYER GANA    
    /// </summary>    
    public void OnBattleWon()    
    {        
        Debug.Log("[GameManager] ¡Player ganó la batalla!");                
        isInBattle = false;                
        // Notificar al enemigo que perdió        
        if (currentEnemy != null)        
        {            
            EnemyPatrolRandom enemyScript = currentEnemy.GetComponent<EnemyPatrolRandom>();            
            if (enemyScript != null)            
            {                
                enemyScript.OnPlayerVictory();            
            }        
        }                
        currentEnemy = null;    
    }

    public void StartBattle(string PhaseName, GameObject enemy) {

        currentEnemy = enemy; // ← GUARDAR REFERENCIA        
        Debug.Log($"[GameManager] Iniciando batalla con: {enemy.name}");

        if(isInBattle == true) return;
        dialogueManager.ResetSpriteCounters();
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

        if(PhaseName == "IntroPhase")
        {
            BattleTitleImage.SetActive(false);
            IntroBackground.SetActive(true);
            IntroBackground.GetComponent<Image>().sprite = IntroSprites[CurrentSpriteCounter];
            audioSource.clip = intro_music;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    public void ToggleInventory()
    {   
        InventoryLayer.transform.GetChild(0).gameObject.SetActive(!InventoryLayer.transform.GetChild(0).gameObject.activeSelf);
        InventoryLayer.transform.GetChild(2).gameObject.SetActive(!InventoryLayer.transform.GetChild(2).gameObject.activeSelf);
        UpdateInventory();
    }

    public void UpdateInventory()
    {
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

    public void UpdateUI()
    {

        if(isInBattle)
        {
            AmaraIndicators.SetActive(true);
            EnemyIndicators.SetActive(true);
            AmaraIndicators.transform.GetChild(1).GetComponent<Slider>().value = currentAmaraDetermination;
            EnemyIndicators.transform.GetChild(1).GetComponent<Slider>().value = currentEnemyAnger;
            AmaraIndicators.transform.GetChild(2).GetComponent<Image>().sprite = AmaraMoodSprites[Mathf.Max(currentAmaraDetermination-1, 0)];
            InventoryLayer.transform.GetChild(0).gameObject.SetActive(false);
            InventoryLayer.transform.GetChild(2).gameObject.SetActive(false);
        }
        else
        {
            AmaraIndicators.SetActive(false);
            EnemyIndicators.SetActive(false);
            IntroBackground.SetActive(false);
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
                isInBattle = false;
                Player.GetComponent<PlayerMovement>().moveSpeed = 3.0f;
                EndBattle();
                //  NOTIFICAR AL ENEMIGO QUE GANÓ        
                if (currentEnemy != null)        
                {            
                    EnemyPatrolRandom enemyScript = currentEnemy.GetComponent<EnemyPatrolRandom>();            
                    if (enemyScript != null)            
                    {                
                        enemyScript.OnPlayerDefeated(); // ¡Iniciar cooldown!            
                    }        
                } 

                if(currentAmaraDetermination <= 0)
                {
                    if (playerKnockback != null)        
                    {        
                        OnPlayerDeath();
                    } else {            
                        Debug.LogWarning("[GameManager] PlayerKnockback no encontrado!");        
                    }
                    Debug.Log("[GameManager] Player derrotado por enemigo en: " + 2);
                }
                else
                {
                    if (playerKnockback != null) playerKnockback.ApplyKnockback(currentEnemy.transform.position);    
                }
            }
        }
    }

    public void EndBattle()
    {
        isInBattle = false;
        Player.GetComponent<PlayerMovement>().moveSpeed = 3.0f;
        EndDialogue();
        UpdateUI();

        if(!(currentAmaraDetermination <= 0 || currentEnemyAnger >= 3) && dialogueManager.currentPhaseName == "SecondEnemyPhase")
        {
            PickUpObject("Blusa Roja");
            RemoveObject("Palito de Regaliz");
        }

        if(dialogueManager.currentPhaseName == "StorePhase")
        {
            PickUpObject("Palito de Regaliz");
            RemoveObject("Oro");
        }

        if(currentAmaraDetermination <= 0 || currentEnemyAnger >= 3)
        {  
            BattleTitleImage.GetComponent<Image>().sprite = failTherapyTitle;
        }
        else
        {
            BattleTitleImage.GetComponent<Image>().sprite = endTherapyTitle;
        }
        BattleTitleImage.SetActive(true);

        audioSource.clip = idle_music;
        audioSource.loop = true;
        audioSource.Play();
        anim_end_finished = false;
        acc_time = 0.0f;

        if(dialogueManager.currentPhaseName == "IntroPhase")
        {
            IntroBackground.GetComponent<Image>().sprite = IntroSprites[3];
            BattleTitleImage.SetActive(true);
            BattleTitleImage.GetComponent<Image>().sprite = IntroSprites[3];
            if (Player != null)        
            {            
                playerSpawnAnimation = Player.GetComponent<PlayerSpawnAnimation>();                        
                if (playerSpawnAnimation != null)            
                {                
                    playerSpawnAnimation.PlaySpawnAnimation();            
                }        
            }  
        }
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
                UpdateInventory();
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
            case "Scream":
            sfxSource.PlayOneShot(ScreamSfx);
            break;
        }
    }

    private float acc_time = 0.0f;
    private UnityEngine.Vector2 initial_pos;
    private bool anim_start_finished = false;
    private bool anim_end_finished = false;
    public float BattleStartLength;
    private bool entered_intro = false;
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
            if(entered_intro == false)
            {
                StartBattle("IntroPhase", gameObject);
                entered_intro = true;
                EnemyIndicators.SetActive(false);
                AmaraIndicators.SetActive(false);
            }
            if (!anim_end_finished)
            {
                if(acc_time > BattleStartLength)
                {
                    anim_end_finished = true;
                    acc_time = 0.0001f;
                    BattleTitleImage.SetActive(false);
                }

                if(dialogueManager.currentPhaseName == "IntroPhase")
                {
                    BattleTitleImage.GetComponent<Image>().sprite = IntroSprites[3];
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
