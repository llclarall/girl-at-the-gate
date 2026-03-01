using UnityEngine;
using TMPro;

public class UISystem : MonoBehaviour
{
    [SerializeField] private GameObject InteractionPrompt; 

    public void ToggleInteractionPrompt(bool isActive)
    {
        if (InteractionPrompt != null)
        {
            InteractionPrompt.SetActive(isActive);
            Debug.Log("<color=green>UI Interaction : </color>" + isActive);
        }
        else
        {
            Debug.LogError("UISystem : L'objet 'InteractionPrompt' n'est pas assigné dans l'inspecteur !");
        }
    }
}