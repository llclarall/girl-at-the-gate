using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// This script is attached to all interactable objects in the game. It defines the behavior when the player interacts with these objects, such as displaying a panel with information or triggering specific events in the GameManager. It also manages the display of interaction prompts or highlights when the player is near an interactable object.
/// </summary>

public class InteractableObject : MonoBehaviour, IInteractable
{
    [SerializeField] private GameObject highlightHalo;

    [Header("Contenu de l'interaction")]
    [SerializeField] private Sprite imageToDisplay;
    [SerializeField, TextArea] private string textToDisplay;

    private bool _hasBeenInteracted = false;

    public void ShowAffordance(bool show)
    {
        if (highlightHalo != null) highlightHalo.SetActive(show);
    }

    public bool CanInteract() => true;

    public void Interact()
    {
        // opens the display panel with the specified image and text
        UISystem ui = Object.FindAnyObjectByType<UISystem>();
        if (ui != null)
        {
            ui.OpenDisplay(imageToDisplay, textToDisplay);
        }

        // checks if the object has already been interacted with, if not it will trigger the event in the GameManager
        if (!_hasBeenInteracted)
        {
            _hasBeenInteracted = true;
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ObjectInteracted();
            }
        }
    }


}