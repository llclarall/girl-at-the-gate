using UnityEngine;
using StarterAssets;

public class ChestLock : MonoBehaviour, IInteractable
{
    public GameObject codePanel;
    public string correctCode = "8261"; // code à trouver
    public GameObject itemInside;

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

    private void Update()
    {
        
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
    }

    public void ShowAffordance(bool show)
    {
    }
}