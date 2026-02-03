// using System.Diagnostics;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;
using TMPro;
using System.Numerics;
using System.Diagnostics;
using UnityEngine.UI;

[Serializable]
public class Phases
{
    public List<DialoguePrompt> FinalBossPhase;
    public Dictionary<string, DialoguePrompt> FinalBossDict;
    public List<DialoguePrompt> FirstEnemyPhase;
    public Dictionary<string, DialoguePrompt> FirstEnemyDict;
    public List<DialoguePrompt> SecondEnemyPhase;
    public Dictionary<string, DialoguePrompt> SecondEnemyDict;
        public List<DialoguePrompt> StorePhase;
    public Dictionary<string, DialoguePrompt> StoreDict;

    public List<DialoguePrompt> IntroPhase;
    public Dictionary<string, DialoguePrompt> IntroDict;
}

[Serializable]
public class DialoguePrompt
{
    public string Name;
    public bool isChoice;

    public List<string> Texts;
    public List<string> choiceOutcomes;
    public List<string> NextPrompts;
    public List<string> Tags;

}

[Serializable]
public class SpritePair
{
    public string name;
    public List<Sprite> sprites;
    public int sprite_counter = 0;
}

public class DialogueManager : MonoBehaviour
{
    public GameObject dialogueLayer;
    public GameObject choiceLayer;
    public GameObject defaultChoice;
    public GameObject characterSprite;
    public GameObject EnemyIndicators;
    public List<SpritePair> SpriteList;
    // public GameObject inventoryLayer;    

    private List<GameObject> SpawnedChoices;

    public bool isInDialogue;
    public string currentPhaseName;
    private List<DialoguePrompt> currentPhase;
    private Dictionary<string,DialoguePrompt> currentPhaseDictionary;
    public string currentPromptName;
    private string prevChoicePromptName;
    public DialoguePrompt currentPrompt;
    private int currentPromptBreak;
    private bool waitForInput;

    public Phases Phases;

    public string currentCharName;
    public AudioSource sfxSource;
    public AudioClip DialogueBlipSfx;

    void LoadJson()
    {
        TextAsset json = Resources.Load<TextAsset>("Dialogue");

        if (json == null)
        {
            UnityEngine.Debug.LogError("Dialogue.json not found!");
            return;
        }

        Phases = JsonUtility.FromJson<Phases>(json.text);
    }
        void BuildDictionary()
    {
        Phases.FinalBossDict = new Dictionary<string, DialoguePrompt>();

        foreach (var prompt in Phases.FinalBossPhase)
        {
            if (Phases.FinalBossDict.ContainsKey(prompt.Name))
            {
                UnityEngine.Debug.LogWarning(
                    $"Duplicate PromptName found in Final Boss: {prompt.Name}"
                );
                continue;
            }

            Phases.FinalBossDict.Add(prompt.Name, prompt);
        }

        // UnityEngine.Debug.Log($"Boss Dictionary built with {Phases.FinalBossDict.Count} prompts");

        Phases.FirstEnemyDict = new Dictionary<string, DialoguePrompt>();
        foreach (var prompt in Phases.FirstEnemyPhase)
        {
            if (Phases.FirstEnemyDict.ContainsKey(prompt.Name))
            {
                UnityEngine.Debug.LogWarning(
                    $"Duplicate PromptName found in First Enemy: {prompt.Name}"
                );
                continue;
            }

            Phases.FirstEnemyDict.Add(prompt.Name, prompt);
        }

        // UnityEngine.Debug.Log($"First Enemy Dictionary built with {Phases.FirstEnemyDict.Count} prompts");

        Phases.SecondEnemyDict = new Dictionary<string, DialoguePrompt>();
        foreach (var prompt in Phases.SecondEnemyPhase)
        {
            if (Phases.SecondEnemyDict.ContainsKey(prompt.Name))
            {
                UnityEngine.Debug.LogWarning(
                    $"Duplicate PromptName found in Second Enemy: {prompt.Name}"
                );
                continue;
            }

            Phases.SecondEnemyDict.Add(prompt.Name, prompt);
        }

        // UnityEngine.Debug.Log($"Second Enemy Dictionary built with {Phases.SecondEnemyDict.Count} prompts");

        Phases.StoreDict = new Dictionary<string, DialoguePrompt>();
        foreach (var prompt in Phases.StorePhase)
        {
            if (Phases.StoreDict.ContainsKey(prompt.Name))
            {
                UnityEngine.Debug.LogWarning(
                    $"Duplicate PromptName found in Second Enemy: {prompt.Name}"
                );
                continue;
            }

            Phases.StoreDict.Add(prompt.Name, prompt);
        }

        // UnityEngine.Debug.Log($"Second Enemy Dictionary built with {Phases.StoreDict.Count} prompts");

        Phases.IntroDict = new Dictionary<string, DialoguePrompt>();
        foreach (var prompt in Phases.IntroPhase)
        {
            if (Phases.IntroDict.ContainsKey(prompt.Name))
            {
                UnityEngine.Debug.LogWarning(
                    $"Duplicate PromptName found in Second Enemy: {prompt.Name}"
                );
                continue;
            }

            Phases.IntroDict.Add(prompt.Name, prompt);
        }

        // UnityEngine.Debug.Log($"Second Enemy Dictionary built with {Phases.IntroDict.Count} prompts");
    }
    
    public void UpdateDialogueVariables(string PhaseName)
    {
        switch (PhaseName)
        {
            case "FinalBossPhase": 
            default:
                currentPhase = Phases.FinalBossPhase;
                currentPhaseDictionary = Phases.FinalBossDict;
                break;
            case "FirstEnemyPhase": 
                currentPhase = Phases.FirstEnemyPhase;
                currentPhaseDictionary = Phases.FirstEnemyDict;
                break;
            case "SecondEnemyPhase": 
                currentPhase = Phases.SecondEnemyPhase;
                currentPhaseDictionary = Phases.SecondEnemyDict;
                break;
            case "StorePhase": 
                currentPhase = Phases.StorePhase;
                currentPhaseDictionary = Phases.StoreDict;
                break;
            case "IntroPhase": 
                currentPhase = Phases.IntroPhase;
                currentPhaseDictionary = Phases.IntroDict;
                break;
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentPromptBreak = 0;
        currentPrompt = null;
        SpawnedChoices = new List<GameObject>(); 
        LoadJson();
        BuildDictionary();

        isInDialogue = false;
        for(int i=0; i < 4; ++i)
        {
            GameObject new_choice = Instantiate(defaultChoice, choiceLayer.transform);
            RectTransform transform = new_choice.GetComponent<RectTransform>();
            transform.anchoredPosition = new UnityEngine.Vector2(0.0f, -90.0f - i * 100.0f);

            transform.anchorMin = new UnityEngine.Vector2(0.5f, 0.0f);
            transform.anchorMax = new UnityEngine.Vector2(0.5f, 0.0f);
            transform.pivot = new UnityEngine.Vector2(0.5f, 0.0f);

            new_choice.SetActive(false);
            SpawnedChoices.Add(new_choice);
        }

        //DEBUG
        // currentPhaseName = "FinalBossPhase";
    }

    void ResetUI()
    {
        foreach(GameObject choice in SpawnedChoices)
        {
            choice.SetActive(false);
            choice.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = "";
        }
        dialogueLayer.SetActive(false);
        dialogueLayer.transform.parent.GetComponent<Image>().raycastTarget = !currentPrompt.isChoice;


        // inventoryLayer.SetActive(false);
        // inventoryLayer.transform.GetChild(1).gameObject.SetActive(false);
        // Transform[] children = inventoryLayer.transform.GetChild(0).GetComponentsInChildren<Transform>(true);
        // foreach (Transform child in children)
        // {
        //     child.GetComponent<Image>().sprite = null;
        //     child.GetChild(0).GetComponent<TextMeshProUGUI>().text = "";
        // }
    }

    Sprite GetSpriteByName(string name)
    {
        foreach(SpritePair pair in SpriteList)
        {
            if(string.Equals(pair.name, name, System.StringComparison.OrdinalIgnoreCase))
            {
                int i = Math.Min(pair.sprites.Count-1, pair.sprite_counter);
                return pair.sprites[i];
            }
        }
        return null;
    }
    void AdvanceSpriteCounter(string name)
    {
        foreach(SpritePair pair in SpriteList)
        {
            if(string.Equals(pair.name, name, System.StringComparison.OrdinalIgnoreCase))
            {
                pair.sprite_counter += 1;
            }
        }
    }
    public void ResetSpriteCounters()
    {
        foreach(SpritePair pair in SpriteList)
        {
            pair.sprite_counter = 0;
        }
    }

    public Coroutine typingCoroutine;
    void UpdateText()
    {
        ResetUI();
        if(currentPrompt.isChoice)
        {
            characterSprite.SetActive(true);
            characterSprite.transform.GetChild(0).GetComponent<Image>().sprite = GetSpriteByName("Amara");

            dialogueLayer.SetActive(false);
            prevChoicePromptName = currentPromptName;
            int i=0;
            foreach(string text in currentPrompt.Texts)
            {

                TextMeshProUGUI choice_text = SpawnedChoices[i].transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();
                choice_text.text = text;

                if(text.Contains("objeto"))
                {
                    if(GameManager.Instance.HasObjects())
                    {
                        choice_text.text = choice_text.text.Replace("objeto", GameManager.Instance.GetRelevantObject());
                    }
                    else
                    {
                        choice_text.text = choice_text.text.Replace("objeto", "nada");
                    }
                }

                SpawnedChoices[i].SetActive(true);
                
                string choice_next_prompt_name = currentPrompt.NextPrompts[i];
                // UnityEngine.Debug.Log($"Setting button for prompt: {choice_next_prompt_name}");
                
                SpawnedChoices[i].GetComponent<Button>().onClick.RemoveAllListeners();
                SpawnedChoices[i].GetComponent<Button>().onClick.AddListener(
                    () => GetNextPrompt(choice_next_prompt_name)
                );
                i+=1;
            }
        }
        else
        {
            dialogueLayer.SetActive(true);
            TextMeshProUGUI dialogue_text = dialogueLayer.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            if(currentPrompt.Texts.Count >= currentPromptBreak+1)
            {
                typingCoroutine = StartCoroutine(MakeTextAppear(currentPrompt.Texts[currentPromptBreak]));
                if (currentPrompt.Tags.Contains("Start"))
                {
                    SkipAppearingText();
                }
                currentPromptBreak+=1; 
                
                //set name for character
                currentCharName = currentPromptName.Split('_')[0];
                dialogueLayer.transform.GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>().text = currentCharName;

                Sprite spr = GetSpriteByName(currentCharName);
                if (spr != null)
                {
                    characterSprite.SetActive(true);
                    characterSprite.transform.GetChild(0).GetComponent<Image>().sprite = spr;
                }
                else
                {
                    characterSprite.SetActive(false);
                    // UnityEngine.Debug.Log($"The char sprite for {currentCharName} wasn't found!");
                }

                if(currentPhaseName == "IntroPhase")
                {
                    characterSprite.SetActive(false);
                }

                Sprite spr_portrait = GetSpriteByName(currentCharName+"Portrait");

                if (spr_portrait != null)
                {
                    EnemyIndicators.transform.GetChild(0).GetComponent<Image>().sprite = spr_portrait;
                }
                else
                {
                    // UnityEngine.Debug.Log($"The char sprite for {currentCharName} wasn't found!");
                }
                
            }
        }
    }

    bool CheckTextIsExhausted()
    {
        if(currentPrompt.isChoice) return true;
        if(currentPrompt.Texts.Count < currentPromptBreak+1) return true;
        return false;
    }
    public void GetNextPrompt(string NextPromptNameOverride = "")
    {
        if(skip_text == false)
        {
            SkipAppearingText();
            return;
        }
        if(CheckTextIsExhausted())
        {   
            if(currentPrompt.NextPrompts.Count > 0)
            {
                //choice logic. going to first for the time being
                // if(currentPhaseDictionary.ContainsKey(currentPrompt.NextPrompts[0]))
                if(currentPrompt.isChoice && currentPhaseDictionary.ContainsKey(NextPromptNameOverride))
                {
                    currentPromptBreak = 0;
                    currentPrompt = currentPhaseDictionary[NextPromptNameOverride];
                    currentPromptName = currentPrompt.Name;
                    // UnityEngine.Debug.Log($"Moving to key {NextPromptNameOverride} prompt");

                    if (currentPrompt.Tags.Contains("CrackMask"))
                    {
                        GameManager.Instance.ReproduceSound("MaskBreaking");
                        AdvanceSpriteCounter(currentPromptName.Split('_')[0]);    
                    }
                    else if (currentPrompt.Tags.Contains("BreakMask"))
                    {
                        GameManager.Instance.ReproduceSound("MaskBroken");
                        GameManager.Instance.currentEnemyAnger = 0;
                        GameManager.Instance.currentAmaraDetermination = GameManager.Instance.InitialAmaraDetermination;
                        GameManager.Instance.UpdateUI();
                        AdvanceSpriteCounter(currentPromptName.Split('_')[0]);    
                    }
                }
                else if(currentPhaseDictionary.ContainsKey(currentPrompt.NextPrompts[0]))
                {
                    currentPromptBreak = 0;
                    currentPrompt = currentPhaseDictionary[currentPrompt.NextPrompts[0]];
                    currentPromptName = currentPrompt.Name;
                    // UnityEngine.Debug.Log($"Moving to key {currentPrompt.NextPrompts[0]} prompt");
                    if(currentPrompt.Tags.Contains("CrackMask")) AdvanceSpriteCounter(currentPromptName.Split('_')[0]);

                    if (currentPrompt.Tags.Contains("Scream")) GameManager.Instance.ReproduceSound("Scream");

                    if (currentPrompt.Tags.Contains("ChangeScreen"))
                    {
                        GameManager.Instance.CurrentSpriteCounter+=1;
                        GameManager.Instance.IntroBackground.GetComponent<Image>().sprite = 
                        GameManager.Instance.IntroSprites[GameManager.Instance.CurrentSpriteCounter];
                    }
                }
                else
                {
                    // UnityEngine.Debug.Log($"The key {currentPrompt.NextPrompts[0]} wasn't found! in dictionary");

                    foreach(string tag in currentPrompt.Tags)
                    {
                        if(string.Equals(tag, "end", System.StringComparison.OrdinalIgnoreCase))
                        {
                            GameManager.Instance.EndBattle();
                        }
                    }
                }
            }
            else //return to previous choice
            {
                currentPrompt = currentPhaseDictionary[prevChoicePromptName];
                currentPromptName = currentPrompt.Name;

                GameManager.Instance.WrongChoice();
            }
        }

        UpdateText();
        // if(CheckTextIsExhausted() && currentPrompt.Tags.Contains("CrackMask"))
        // {
        //     AdvanceSpriteCounter(currentCharName);
        // } 

        //DEBUG
        // Invoke(nameof(GetNextPrompt),2.0f);
    }

    public void GetFirstPrompt()
    {
        UpdateDialogueVariables(currentPhaseName);
        foreach(DialoguePrompt prompt in  currentPhase)
        {
            foreach(string tag in prompt.Tags)
            {
                if(string.Equals(tag, "start", System.StringComparison.OrdinalIgnoreCase))
                {
                    currentPrompt = prompt;
                    currentPromptName = currentPrompt.Name;
                    currentPromptBreak = 0;
                    UpdateText();
                    
                    //DEBUG
                    // Invoke(nameof(GetNextPrompt),2.0f);
                    break;
                }
            }
        }
    }

    public float charDelay = 0.75f;
    private bool skip_text;
    System.Collections.IEnumerator MakeTextAppear(string fullText)
    {
        TMP_Text tmp_text = dialogueLayer.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        
        tmp_text.text = fullText;
        tmp_text.maxVisibleCharacters = 0;
        skip_text = false;

        foreach (char c in fullText)
        {
            if (skip_text)
            {
                tmp_text.maxVisibleCharacters = 
                tmp_text.textInfo.characterCount;
                yield break;
            }
            sfxSource.PlayOneShot(DialogueBlipSfx);
            tmp_text.maxVisibleCharacters+=1;
            yield return new WaitForSeconds(charDelay);
        }
        skip_text = true;
    }

    public void SkipAppearingText()
    {
        skip_text = true;
    }

    private UnityEngine.Vector2 initial_pos;
    private bool initialised_pos = false;
    private float acc_time = 0.0f;
    // Update is called once per frame
    void Update()
    {
        if(initialised_pos == false)
        {
            initial_pos = dialogueLayer.transform.GetChild(2).GetComponent<RectTransform>().anchoredPosition;
            initialised_pos = true;
        }
        if(currentPrompt != null && isInDialogue && currentPrompt.isChoice == false)
        {
            dialogueLayer.transform.GetChild(2).gameObject.SetActive(skip_text);
            dialogueLayer.transform.GetChild(2).GetComponent<RectTransform>().anchoredPosition = 
                initial_pos + new UnityEngine.Vector2(-5.0f + ((acc_time % 0.75f) / 0.75f) * 10.0f, 0.0f);
            acc_time+=Time.deltaTime;
        }
    }
}
