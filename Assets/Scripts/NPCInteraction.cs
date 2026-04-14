using UnityEngine;
using TMPro;
using System;
using UnityEngine.SceneManagement;
using System.Collections;
using StarterAssets;


/// <summary>
/// This script manages the interaction with an NPC character in the game. It handles the display of dialogue lines when the player interacts with the NPC, and it also manages the end-of-game sequence by fading out the screen and displaying a series of ending images before returning to the main menu. The dialogue content is defined in an array of DialogueLine objects, allowing for easy customization of the conversation.
/// </summary>

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

    [Header("Fin du Jeu")]
    public CanvasGroup fadeAnimator;
    public string menuSceneName = "MainMenu";
    public UnityEngine.UI.Image endingImageDisplay;
    public Sprite[] endingSprites;

    [Header("Dialogue Content")]
    public DialogueLine[] conversation;
    private int _index = 0;
    private ThirdPersonController _thirdPersonController;
    private StarterAssetsInputs _starterAssetsInputs;
    private bool _storedThirdPersonControllerEnabled;
    private bool _hasStoredThirdPersonControllerState;

    private void Awake()
    {
        _thirdPersonController = FindFirstObjectByType<ThirdPersonController>();
        _starterAssetsInputs = FindFirstObjectByType<StarterAssetsInputs>();
    }

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
        SetMovementLocked(true);
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
        SetMovementLocked(false);
        StartCoroutine(FinishGameRoutine());

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Coroutine to handle the fade-out and display of ending images before returning to the main menu
    IEnumerator FinishGameRoutine()
    {
        float timer = 0;
        while (timer < 1.5f)
        {
            timer += Time.deltaTime;
            fadeAnimator.alpha = timer / 1.5f;
            yield return null;
        }
        fadeAnimator.alpha = 1;

        foreach (Sprite s in endingSprites)
        {
            endingImageDisplay.sprite = s;

            timer = 0;
            while (timer < 1f)
            {
                timer += Time.deltaTime;
                endingImageDisplay.color = new Color(1, 1, 1, timer / 1f);
                yield return null;
            }

            yield return new WaitForSeconds(3f);

            timer = 0;
            while (timer < 1f)
            {
                timer += Time.deltaTime;
                endingImageDisplay.color = new Color(1, 1, 1, 1 - (timer / 1f));
                yield return null;
            }
        }

        //yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(menuSceneName);
    }


    public void ShowAffordance(bool show) { }

    public bool CanInteract() => true;

    private void SetMovementLocked(bool isLocked)
    {
        if (_starterAssetsInputs != null && isLocked)
        {
            _starterAssetsInputs.OnOscMoveStop();
        }

        if (_thirdPersonController == null)
        {
            return;
        }

        if (isLocked)
        {
            if (!_hasStoredThirdPersonControllerState)
            {
                _storedThirdPersonControllerEnabled = _thirdPersonController.enabled;
                _hasStoredThirdPersonControllerState = true;
            }

            _thirdPersonController.enabled = false;
            return;
        }

        if (_hasStoredThirdPersonControllerState)
        {
            _thirdPersonController.enabled = _storedThirdPersonControllerEnabled;
            _hasStoredThirdPersonControllerState = false;
        }
    }

}