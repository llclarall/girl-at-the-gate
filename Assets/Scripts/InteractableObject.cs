using UnityEngine;
using UnityEngine.Events;

public class InteractableObject : MonoBehaviour, IInteractable
{
    [SerializeField] private GameObject highlightHalo;

    [Header("Contenu de l'interaction")]
    public Sprite imageToDisplay;
    [TextArea] public string textToDisplay;

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