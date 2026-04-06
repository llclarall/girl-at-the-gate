using UnityEngine;
using StarterAssets;

public class ChestLock : MonoBehaviour, IInteractable
{
    [SerializeField] private GameObject highlightHalo;
    public GameObject codePanel;
    public string correctCode = "8267"; // code à trouver
    public GameObject itemInside;

    [Header("Contenu à afficher au déverrouillage")]
    public Sprite treasureImage;
    [TextArea] public string treasureText;

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

        // Affiche l'objet du coffre via UISystem pour pouvoir le fermer avec E
        UISystem ui = Object.FindAnyObjectByType<UISystem>();
        if (ui != null && treasureImage != null)
        {
            ui.OpenDisplay(treasureImage, treasureText ?? "");
        }
    }

    public void ShowAffordance(bool show)
    {
        // Affiche le prompt seulement si le coffre n'est pas encore ouvert
        if (highlightHalo != null)
        {
            highlightHalo.SetActive(show && !isOpened);
        }
    }

    public bool CanInteract() => !isOpened;

}