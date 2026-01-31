// // using System.Diagnostics;
// using UnityEngine;
// using System;
// using System.Collections.Generic;
// using TMPro;
// using System.Numerics;
// using System.Diagnostics;
// using UnityEngine.UI;

// [Serializable]
// public class Phases
// {
//     public List<DialoguePrompt> FinalBossPhase;
//     public Dictionary<string, DialoguePrompt> FinalBossDict;
//     public List<DialoguePrompt> FirstEnemyPhase;
//     public Dictionary<string, DialoguePrompt> FirstEnemyDict;
//     public List<DialoguePrompt> SecondEnemyPhase;
//     public Dictionary<string, DialoguePrompt> SecondEnemyDict;
// }

// [Serializable]
// public class DialoguePrompt
// {
//     public string Name;
//     public bool isChoice;

//     public List<string> Texts;
//     public List<string> choiceOutcomes;
//     public List<string> NextPrompts;
//     public List<string> Tags;

// }

// [Serializable]
// public class SpritePair
// {
//     public string name;
//     public Sprite sprite;
// }

// public class DialogueManager : MonoBehaviour
// {
//     public GameObject dialogueLayer;
//     public GameObject choiceLayer;
//     public GameObject defaultChoice;
//     public GameObject characterSprite;
//     public List<SpritePair> SpriteList;
//     private List<GameObject> SpawnedChoices;


//     private bool isInDialogue;
//     private string currentPhaseName;
//     private List<DialoguePrompt> currentPhase;
//     private Dictionary<string,DialoguePrompt> currentPhaseDictionary;
//     private string currentPromptName;
//     private string prevChoicePromptName;
//     private DialoguePrompt currentPrompt;
//     private int currentPromptBreak;
//     private bool waitForInput;

//     public Phases Phases;

//     void LoadJson()
//     {
//         TextAsset json = Resources.Load<TextAsset>("Dialogue");

//         if (json == null)
//         {
//             UnityEngine.Debug.LogError("Dialogue.json not found!");
//             return;
//         }

//         Phases = JsonUtility.FromJson<Phases>(json.text);
//     }
//         void BuildDictionary()
//     {
//         Phases.FinalBossDict = new Dictionary<string, DialoguePrompt>();

//         foreach (var prompt in Phases.FinalBossPhase)
//         {
//             if (Phases.FinalBossDict.ContainsKey(prompt.Name))
//             {
//                 UnityEngine.Debug.LogWarning(
//                     $"Duplicate PromptName found: {prompt.Name}"
//                 );
//                 continue;
//             }

//             Phases.FinalBossDict.Add(prompt.Name, prompt);
//         }

//         UnityEngine.Debug.Log($"Dictionary built with {Phases.FinalBossDict.Count} prompts");
//     }
    
//     public void UpdateDialogueVariables(string PhaseName)
//     {
//         switch (PhaseName)
//         {
//             case "FinalBossPhase": 
//             default:
//                 currentPhase = Phases.FinalBossPhase;
//                 currentPhaseDictionary = Phases.FinalBossDict;
//                 break;
//             case "FirstEnemyPhase": 
//                 currentPhase = Phases.FirstEnemyPhase;
//                 currentPhaseDictionary = Phases.FirstEnemyDict;
//                 break;
//             case "SecondEnemyPhase": 
//                 currentPhase = Phases.SecondEnemyPhase;
//                 currentPhaseDictionary = Phases.SecondEnemyDict;
//                 break;
//         }
//     }

//     // Start is called once before the first execution of Update after the MonoBehaviour is created
//     void Start()
//     {
//         currentPromptBreak = 0;
//         SpawnedChoices = new List<GameObject>(); 
//         LoadJson();
//         BuildDictionary();

//         isInDialogue = true;
//         for(int i=0; i < 3; ++i)
//         {
//             GameObject new_choice = Instantiate(defaultChoice, choiceLayer.transform);
//             RectTransform transform = new_choice.GetComponent<RectTransform>();
//             transform.anchoredPosition = new UnityEngine.Vector2(0.0f, - 114.0f - i * 120.0f);

//             transform.anchorMin = new UnityEngine.Vector2(0.5f, 0.0f);
//             transform.anchorMax = new UnityEngine.Vector2(0.5f, 0.0f);
//             transform.pivot = new UnityEngine.Vector2(0.5f, 0.0f);

//             new_choice.SetActive(false);
//             SpawnedChoices.Add(new_choice);
//         }

//         //DEBUG
//         currentPhaseName = "FinalBossPhase";
//     }

//     void ResetChoices()
//     {
//         foreach(GameObject choice in SpawnedChoices)
//         {
//             choice.SetActive(false);
//             choice.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = "";
//         }
//     }

//     Sprite GetSpriteByName(string name)
//     {
//         foreach(SpritePair pair in SpriteList)
//         {
//             if(string.Equals(pair.name, name, System.StringComparison.OrdinalIgnoreCase))
//             {
//                 return pair.sprite;
//             }
//         }
//         return null;
//     }

//     void UpdateText()
//     {
//         ResetChoices();
//         if(currentPrompt.isChoice)
//         {
//             characterSprite.SetActive(true);
//             characterSprite.transform.GetChild(0).GetComponent<Image>().sprite = GetSpriteByName("Amara");

//             dialogueLayer.SetActive(false);
//             prevChoicePromptName = currentPromptName;
//             int i=0;
//             foreach(string text in currentPrompt.Texts)
//             {
//                 SpawnedChoices[i].SetActive(true);
//                 TextMeshProUGUI choice_text = SpawnedChoices[i].transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();
//                 choice_text.text = text;
                
//                 string choice_next_prompt_name = currentPrompt.NextPrompts[i];
//                 UnityEngine.Debug.Log($"Setting button for prompt: {choice_next_prompt_name}");
                
//                 SpawnedChoices[i].GetComponent<Button>().onClick.RemoveAllListeners();
//                 SpawnedChoices[i].GetComponent<Button>().onClick.AddListener(
//                     () => GetNextPrompt(choice_next_prompt_name)
//                 );
//                 i+=1;
//             }
//         }
//         else
//         {
//             dialogueLayer.SetActive(true);
//             TextMeshProUGUI dialogue_text = dialogueLayer.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
//             if(currentPrompt.Texts.Count >= currentPromptBreak+1)
//             {
//                 dialogue_text.text = currentPrompt.Texts[currentPromptBreak];
//                 currentPromptBreak+=1; 
                
//                 //set name for character
//                 string char_name = currentPromptName.Split('_')[0];
//                 dialogueLayer.transform.GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>().text = char_name;
                
//                 Sprite spr = GetSpriteByName(char_name);
//                 if (spr != null)
//                 {
//                     characterSprite.SetActive(true);
//                     characterSprite.transform.GetChild(0).GetComponent<Image>().sprite = spr;
//                 }else
//                 {
//                     characterSprite.SetActive(false);
//                     UnityEngine.Debug.Log($"The char sprite for {char_name} wasn't found!");
//                 }
//             }
//         }
//     }

//     public void GetNextPrompt(string NextPromptNameOverride = "")
//     {
//         if(currentPrompt.NextPrompts.Count > 0)
//         {
//             //choice logic. going to first for the time being
//             // if(currentPhaseDictionary.ContainsKey(currentPrompt.NextPrompts[0]))
//             if(currentPrompt.isChoice && currentPhaseDictionary.ContainsKey(NextPromptNameOverride))
//             {
//                 currentPromptBreak = 0;
//                 currentPrompt = currentPhaseDictionary[NextPromptNameOverride];
//                 currentPromptName = currentPrompt.Name;
//                 UnityEngine.Debug.Log($"Moving to key {NextPromptNameOverride} prompt");
//             }
//             else if(currentPhaseDictionary.ContainsKey(currentPrompt.NextPrompts[0]))
//             {
//                 currentPromptBreak = 0;
//                 currentPrompt = currentPhaseDictionary[currentPrompt.NextPrompts[0]];
//                 currentPromptName = currentPrompt.Name;
//                 UnityEngine.Debug.Log($"Moving to key {currentPrompt.NextPrompts[0]} prompt");
//             }
//             else
//             {
//                 UnityEngine.Debug.Log($"The key {currentPrompt.NextPrompts[0]} wasn't found! in dictionary");
//             }
//         }
//         else //return to previous choice
//         {
//             currentPrompt = currentPhaseDictionary[prevChoicePromptName];
//             currentPromptName = currentPrompt.Name;
//         }

//         UpdateText(); 
//         //DEBUG
//         // Invoke(nameof(GetNextPrompt),2.0f);
//     }

//     // Update is called once per frame
//     void Update()
//     {
//         if (isInDialogue)
//         {
//             if(currentPrompt == null)
//             {                
//                 UpdateDialogueVariables(currentPhaseName);
//                 foreach(DialoguePrompt prompt in  currentPhase)
//                 {
//                     foreach(string tag in prompt.Tags)
//                     {
//                         if(string.Equals(tag, "start", System.StringComparison.OrdinalIgnoreCase))
//                         {
//                             currentPrompt = prompt;
//                             currentPromptName = currentPrompt.Name;
//                             UpdateText();
//                             waitForInput = true;
                            
//                             //DEBUG
//                             // Invoke(nameof(GetNextPrompt),2.0f);
//                             break;
//                         }
//                     }
//                 }
//             }
//         }   
//     }
// }
