using UnityEngine;
using UnityEngine.UI; 
using TMPro;

public class UISystem : MonoBehaviour
{
    [SerializeField] private GameObject interactionPrompt;
    [SerializeField] private GameObject objectPanel; 
    
    [SerializeField] private Image displayImage; 
    [SerializeField] private TextMeshProUGUI displayText;

    private bool _isObjectOpen = false;

    public void ToggleInteractionPrompt(bool isActive)
    {
        if (_isObjectOpen && isActive) return; 
        if (interactionPrompt != null) interactionPrompt.SetActive(isActive);
    }

// interaction system 
    public void OpenDisplay(Sprite photo, string message)
    {
        _isObjectOpen = true;
        objectPanel.SetActive(true);
        ToggleInteractionPrompt(false);

        if (displayImage != null) displayImage.sprite = photo;
        if (displayText != null) displayText.text = message;
    }

    public void CloseDisplay()
    {
        _isObjectOpen = false;
        objectPanel.SetActive(false);
    }

    public bool IsObjectOpen() => _isObjectOpen;
}