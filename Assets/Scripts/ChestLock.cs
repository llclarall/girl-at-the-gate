using UnityEngine;
using StarterAssets;

public class ChestLock : MonoBehaviour, IInteractable
{
    public GameObject codePanel;
    public string correctCode = "8261"; // code à trouver
    public GameObject itemInside;

    private bool isOpened = false;
    private StarterAssetsInputs _input;
    private bool _isMovementFrozen = false;

    private void Start()
    {
        _input = FindFirstObjectByType<StarterAssetsInputs>();
    }

    private void Update()
    {
        // Bloque les mouvements tant que le panel est ouvert
        if (_isMovementFrozen && _input != null)
        {
            _input.MoveInput(Vector2.zero);
        }
    }

    public void Interact()
    {
        if (!isOpened)
        {
            // On affiche l'interface et on débloque la souris
            codePanel.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            FreezeMovement(true);
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
            // Logique pour effacer les chiffres (voir script UI après)
        }
    }

    void OpenChest()
    {
        isOpened = true;
        codePanel.SetActive(false);
        itemInside.SetActive(true); // Fait apparaître le contenu

        Cursor.lockState = CursorLockMode.Locked;
        FreezeMovement(false);
    }

    public void FreezeMovement(bool freeze)
    {
        _isMovementFrozen = freeze;
    }

    public void ShowAffordance(bool show)
    {
    }
}