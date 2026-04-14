public interface IInteractable
{
    void Interact(); 
    void ShowAffordance(bool show); // manages the display of interaction prompts or highlights when the player is near an interactable object
    bool CanInteract(); // determines if the player can currently interact with the object
}