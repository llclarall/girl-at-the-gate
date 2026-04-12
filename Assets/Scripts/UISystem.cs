using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UISystem : MonoBehaviour
{
    [SerializeField] private GameObject interactionPrompt;
    [SerializeField] private GameObject objectPanel;

    [SerializeField] private Image displayImage;
    [SerializeField] private TextMeshProUGUI displayText;
    [SerializeField] private bool useNativeImageSize = false;

    private bool _isObjectOpen = false;

    private void Awake()
    {
        ResolveDisplayReferences();
    }

    private void ResolveDisplayReferences()
    {
        if (objectPanel != null)
        {
            if (displayImage == null)
            {
                displayImage = objectPanel.GetComponentInChildren<Image>(true);
            }

            if (displayText == null)
            {
                displayText = objectPanel.GetComponentInChildren<TextMeshProUGUI>(true);
            }
        }
    }

    public void ToggleInteractionPrompt(bool isActive)
    {
        if (_isObjectOpen && isActive) return;
        if (interactionPrompt != null) interactionPrompt.SetActive(isActive);
    }

    // interaction system 
    public void OpenDisplay(Sprite photo, string message)
    {
        ResolveDisplayReferences();
        _isObjectOpen = true;
        if (objectPanel != null)
        {
            objectPanel.SetActive(true);
        }
        ToggleInteractionPrompt(false);

        if (displayImage != null)
        {
            displayImage.sprite = photo;
            displayImage.enabled = photo != null;
            displayImage.color = new Color(1f, 1f, 1f, 1f);

            if (photo != null)
            {
                if (useNativeImageSize)
                {
                    displayImage.SetNativeSize();
                }
                displayImage.gameObject.SetActive(true);
            }
        }
        if (displayText != null) displayText.text = message;
    }

    public void CloseDisplay()
    {
        _isObjectOpen = false;
        if (objectPanel != null)
        {
            objectPanel.SetActive(false);
        }
    }

    public bool IsObjectOpen()
    {
        return _isObjectOpen || (objectPanel != null && objectPanel.activeSelf);
    }
}