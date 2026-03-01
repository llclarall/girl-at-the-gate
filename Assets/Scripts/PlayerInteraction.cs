using UnityEngine;
using UnityEngine.InputSystem; 

public class PlayerInteraction : MonoBehaviour
{
    public float InteractionDistance = 3f;
    public LayerMask InteractableLayer;
    public UISystem uiSystem;
    public Transform InteractionSource; 
    
    private IInteractable _currentInteractable;

    void Update()
    {
        CheckForInteractable();
    }

    private void CheckForInteractable()
    {
        // Crée une "bulle" invisible autour du joueur
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, InteractionDistance, InteractableLayer);
        
        if (hitColliders.Length > 0)
        {
            // On prend le premier objet interactif trouvé dans la bulle
            IInteractable interactable = hitColliders[0].GetComponent<IInteractable>();
            
            if (interactable != null && interactable != _currentInteractable)
            {
                _currentInteractable?.ShowAffordance(false);
                _currentInteractable = interactable;
                _currentInteractable.ShowAffordance(true);
                uiSystem.ToggleInteractionPrompt(true);
            }
        }
        else if (_currentInteractable != null)
        {
            _currentInteractable.ShowAffordance(false);
            _currentInteractable = null;
            uiSystem.ToggleInteractionPrompt(false);
        }
    }

    public void OnInteract(InputValue value)
    {
        Debug.Log("Appui sur E détecté !");
        
        if (value.isPressed)
        {
            if (uiSystem != null && uiSystem.IsBookOpen())
            {
                uiSystem.ShowBook(false);
                return;
            }

            if (_currentInteractable != null)
            {
                _currentInteractable.Interact();
            }
        }
    }
}