using UnityEngine;

/// <summary>
/// This script manages the locked chest in the underground scene. It allows the player to interact with the chest, enter a code to unlock it, and then reveals the contents of the chest if the correct code is entered. It also handles showing a highlight when the player can interact with the chest and plays a sound effect upon interaction.
/// </summary>

public class ChestLock : MonoBehaviour, IInteractable
{
    [SerializeField] private GameObject highlightHalo;
    [SerializeField] private GameObject codePanel;
    public string correctCode = "8267";
    [SerializeField] private GameObject itemInside;

    [Header("Contenu à afficher au déverrouillage")]
    [SerializeField] private Sprite treasureImage;
    [SerializeField, TextArea] private string treasureText;
    [SerializeField] private GameObject obstacleToDisable;

    private bool isOpened = false;
    [SerializeField] private AudioSource chestAudioSource;
    [SerializeField] private AudioClip interactionSound;

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
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.Log("Son d'interaction joué !");
#endif
            }
        }
    }

    public void CheckCode(string inputCode)
    {
        if (inputCode == correctCode)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log("Code Bon !");
#endif
            OpenChest();
        }
        else
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log("Mauvais code...");
#endif
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