using UnityEngine;
using UnityEngine.Events;

public class InteractableObject : MonoBehaviour, IInteractable
{
    [SerializeField] private GameObject highlightHalo;
    
    [Header("Contenu de l'interaction")]
    public Sprite imageToDisplay; 
    [TextArea] public string textToDisplay; 

    public void ShowAffordance(bool show)
    {
        if (highlightHalo != null) highlightHalo.SetActive(show);
    }

    public void Interact()
    {
        UISystem ui = Object.FindAnyObjectByType<UISystem>();
        if (ui != null)
        {
            ui.OpenDisplay(imageToDisplay, textToDisplay);
        }
    }
}