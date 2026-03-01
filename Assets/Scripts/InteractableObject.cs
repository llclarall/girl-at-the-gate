using UnityEngine;

public class InteractableObject : MonoBehaviour, IInteractable
{
    [SerializeField] private GameObject highlightHalo; // Assigne ici ton halo (Point Light ou mesh)
    [SerializeField] private UnityEngine.Events.UnityEvent onInteract;

    public void ShowAffordance(bool show)
    {
        if (highlightHalo != null) highlightHalo.SetActive(show);
    }

    public void Interact()
    {
        onInteract?.Invoke();
        Debug.Log("Interaction avec : " + gameObject.name);
    }
}