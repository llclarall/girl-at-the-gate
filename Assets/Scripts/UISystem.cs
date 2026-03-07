using UnityEngine;
using TMPro;

public class UISystem : MonoBehaviour
{
    [SerializeField] private GameObject InteractionPrompt;
    [SerializeField] private GameObject BookUI; 
    [SerializeField] private GameObject PictureUI;

    private bool _isBookOpen = false;
    private bool _isPictureOpen = false;

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

    public void ShowPicture(bool show)
    {
       _isPictureOpen = show;
        if (PictureUI != null)
            PictureUI.SetActive(show);
        
        if (show) ToggleInteractionPrompt(false);
    }

    public bool IsPictureOpen() => _isPictureOpen;
}