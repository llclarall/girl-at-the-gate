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
        // creates a sphere around the player to detect interactable objects
        Collider[] hitColliders = Physics.OverlapSphere(InteractionSource.position, InteractionDistance, InteractableLayer);

        if (hitColliders.Length > 0)
        {
            Debug.DrawRay(InteractionSource.position, InteractionSource.forward * InteractionDistance, Color.red);
            Debug.DrawLine(InteractionSource.position, InteractionSource.position + Vector3.right * InteractionDistance, Color.red);
            
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
            if (uiSystem != null && uiSystem.IsObjectOpen())
            {
                uiSystem.CloseDisplay();
                return;
            }

            if (_currentInteractable != null)
            {
                _currentInteractable.Interact();
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (InteractionSource != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(InteractionSource.position, InteractionDistance);
        }
    }

}