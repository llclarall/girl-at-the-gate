using UnityEngine;
using StarterAssets;

/// <summary>
/// This script manages the locked chest in the underground scene. It allows the player to interact with the chest, enter a code to unlock it, and then reveals the contents of the chest if the correct code is entered. It also handles showing a highlight when the player can interact with the chest and plays a sound effect upon interaction.
/// </summary>

public class ChestLock : MonoBehaviour, IInteractable
{
    [SerializeField] private GameObject highlightHalo;
    public GameObject codePanel;
    public string correctCode = "8267"; 
    public GameObject itemInside;

    [Header("Contenu à afficher au déverrouillage")]
    public Sprite treasureImage;
    [TextArea] public string treasureText;
    public GameObject obstacleToDisable;

    private bool isOpened = false;
    private StarterAssetsInputs _input;
    private bool _previousCursorInputForLook;
    private bool _previousOscMovementEnabled;
    public AudioSource chestAudioSource;
    public AudioClip interactionSound;

    private void Start()
    {
        _input = FindFirstObjectByType<StarterAssetsInputs>();
    }

    public void Interact()
    {
        if (!isOpened)
        {
            codePanel.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (chestAudioSource != null && interactionSound != null)
            {
                chestAudioSource.PlayOneShot(interactionSound);
                Debug.Log("Son d'interaction joué !");
            }
        }
    }

    public void CheckCode(string inputCode)
    {
        if (inputCode == correctCode)
        {
            Debug.Log("Code Bon !");
            OpenChest();
        }
        else
        {
            Debug.Log("Mauvais code...");
        }
    }

    void OpenChest()
    {
        isOpened = true;
        codePanel.SetActive(false);
        itemInside.SetActive(true);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        UISystem ui = Object.FindAnyObjectByType<UISystem>();
        if (ui != null && treasureImage != null)
        {
            ui.OpenDisplay(treasureImage, treasureText ?? "");
        }

        if (obstacleToDisable != null)
        {
            obstacleToDisable.SetActive(false);
        }
    }

    public void ShowAffordance(bool show)
    {
        if (highlightHalo != null)
        {
            highlightHalo.SetActive(show && !isOpened);
        }
    }

    public bool CanInteract() => !isOpened;

}