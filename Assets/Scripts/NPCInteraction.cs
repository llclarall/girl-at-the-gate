using UnityEngine;
using TMPro;
using System; 

[Serializable] 
public class DialogueLine
{
    public string name;     
    [TextArea(2, 5)]
    public string text;     
}

public class NPCInteraction : MonoBehaviour, IInteractable
{
    [Header("UI References")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI nameText;

    [Header("Dialogue Content")]
    public DialogueLine[] conversation; 

    private int _index = 0;

    public void Interact()
    {
        if (!dialoguePanel.activeSelf)
        {
            StartDialogue();
        }
        else
        {
            NextLine();
        }
    }

    void StartDialogue()
    {
        _index = 0;
        dialoguePanel.SetActive(true);
        

        DisplayCurrentLine();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void DisplayCurrentLine()
    {
        if (_index < conversation.Length)
        {
            DialogueLine currentLine = conversation[_index];
            nameText.text = currentLine.name;
            dialogueText.text = currentLine.text;
        }
        else
        {
            EndDialogue();
        }
    }

    void NextLine()
    {
        _index++;
        DisplayCurrentLine();
    }

    void EndDialogue()
    {
        dialoguePanel.SetActive(false);
        

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void ShowAffordance(bool show) { }
}