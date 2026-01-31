using UnityEngine;
using TMPro;
using System.Collections.Generic;

[System.Serializable]
public class DialogueLine
{
    public string speakerName;
    [TextArea(2, 5)]
    public string text;
}

public class DialogueManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private TextMeshProUGUI speakerNameText;
    
    [Header("Dialogue Settings")]
    [SerializeField] private float typingSpeed = 0.05f;
    
    private Queue<DialogueLine> dialogueQueue;
    private bool isTyping = false;
    private bool isDialogueActive = false;
    private string currentFullText = "";
    private Coroutine typingCoroutine;
    
    private void Start()
    {
        dialogueQueue = new Queue<DialogueLine>();
        dialoguePanel.SetActive(false);
    }
    
    private void Update()
    {
        if (isDialogueActive && Input.GetKeyDown(KeyCode.E))
        {
            if (isTyping)
            {
                // Si está escribiendo, mostrar todo el texto inmediatamente
                StopTyping();
                dialogueText.text = currentFullText;
                isTyping = false;
            }
            else
            {
                // Avanzar al siguiente diálogo
                DisplayNextLine();
            }
        }
    }
    
    public void StartDialogue(DialogueLine[] lines)
    {
        if (isDialogueActive) return;
        
        isDialogueActive = true;
        dialogueQueue.Clear();
        
        foreach (DialogueLine line in lines)
        {
            dialogueQueue.Enqueue(line);
        }
        
        dialoguePanel.SetActive(true);
        DisplayNextLine();
        
        Debug.Log("[DialogueManager] Diálogo iniciado");
    }
    
    private void DisplayNextLine()
    {
        if (dialogueQueue.Count == 0)
        {
            EndDialogue();
            return;
        }
        
        DialogueLine line = dialogueQueue.Dequeue();
        
        if (speakerNameText != null)
        {
            speakerNameText.text = line.speakerName;
        }
        
        currentFullText = line.text;
        StopTyping();
        typingCoroutine = StartCoroutine(TypeText(line.text));
    }
    
    private System.Collections.IEnumerator TypeText(string text)
    {
        isTyping = true;
        dialogueText.text = "";
        
        foreach (char letter in text.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
        
        isTyping = false;
    }
    
    private void StopTyping()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
    }
    
    private void EndDialogue()
    {
        dialoguePanel.SetActive(false);
        isDialogueActive = false;
        
        Debug.Log("[DialogueManager] Diálogo terminado");
    }
    
    public bool IsDialogueActive()
    {
        return isDialogueActive;
    }
}
