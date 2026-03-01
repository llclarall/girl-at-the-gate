using UnityEngine;
using TMPro;

public class UISystem : MonoBehaviour
{
    [SerializeField] private GameObject InteractionPrompt;
    [SerializeField] private GameObject BookUI; 

    private bool _isBookOpen = false;

    public void ToggleInteractionPrompt(bool isActive)
    {
        if (_isBookOpen && isActive) return; 
        
        if (InteractionPrompt != null)
            InteractionPrompt.SetActive(isActive);
    }

    public void ShowBook(bool show)
    {
        _isBookOpen = show;
        if (BookUI != null)
            BookUI.SetActive(show);
        
        if (show) ToggleInteractionPrompt(false);
    }

    public bool IsBookOpen() => _isBookOpen;
}