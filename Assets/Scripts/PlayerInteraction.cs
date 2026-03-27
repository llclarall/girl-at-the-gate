using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    public float InteractionDistance = 3f;
    public LayerMask InteractableLayer;
    public UISystem uiSystem;
    public Transform InteractionSource;

    [Header("Inputs")]
    [SerializeField] private bool enableKeyboardFallback = true;
    [SerializeField] private KeyCode keyboardInteractKey = KeyCode.E;
    [SerializeField] private bool enableControllerFallback = true;
    [SerializeField] private float oscPressedThreshold = 0.5f;

    private IInteractable _currentInteractable;
    private bool _wasOscPressed;
    private int _lastInteractionFrame = -1;

    void Update()
    {
        CheckForInteractable();

        if (enableKeyboardFallback && Input.GetKeyDown(keyboardInteractKey))
        {
            TryInteract();
        }

        if (enableControllerFallback && IsControllerInteractPressedThisFrame())
        {
            TryInteract();
        }
    }

    private void CheckForInteractable()
    {
        if (InteractionSource == null || uiSystem == null)
        {
            return;
        }

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
        if (value.isPressed)
        {
            TryInteract();
        }
    }

    // OSC Jack can map this method from a float message (0/1).
    public void OnOscInteract(float value)
    {
        bool isPressed = value >= oscPressedThreshold;
        if (isPressed && !_wasOscPressed)
        {
            TryInteract();
        }

        _wasOscPressed = isPressed;
    }

    // OSC Jack can map this method from an int message (0/1).
    public void OnOscInteractInt(int value)
    {
        OnOscInteract(value);
    }

    // OSC Jack can map this method from a bang/trigger event.
    public void OnOscInteractBang()
    {
        TryInteract();
    }

    // French aliases for easier direct mapping from Chataigne labels.
    public void OnOscInteragir(float value)
    {
        OnOscInteract(value);
    }

    public void OnOscInteragirInt(int value)
    {
        OnOscInteractInt(value);
    }

    public void OnOscInteragirBang()
    {
        OnOscInteractBang();
    }

    private void TryInteract()
    {
        // Prevent double trigger when multiple input paths fire in the same frame.
        if (_lastInteractionFrame == Time.frameCount)
        {
            return;
        }

        _lastInteractionFrame = Time.frameCount;

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

    private bool IsControllerInteractPressedThisFrame()
    {
        Gamepad gamepad = Gamepad.current;

        if (gamepad != null)
        {
            // West/South cover common Xbox/PlayStation main face buttons.
            if (gamepad.buttonWest.wasPressedThisFrame || gamepad.buttonSouth.wasPressedThisFrame)
            {
                return true;
            }
        }

        // Legacy joystick fallback for some controllers/drivers.
        return Input.GetKeyDown(KeyCode.JoystickButton0) || Input.GetKeyDown(KeyCode.JoystickButton2);
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