using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// This script is attached to the cemetery exit door. The door starts in a locked state, and the player must find all the necessary items to unlock it. The door can be unlocked by calling the Unlock() method, which changes the material of the door to indicate that it is now open.  
/// </summary>

public class ExitDoor : MonoBehaviour, IInteractable
{
    [SerializeField] private Material closedMaterial;
    [SerializeField] private Material openedMaterial;
    [SerializeField] private GameObject highlightHalo;
    private bool isUnlocked = false;
    private Renderer _renderer;


    void Start()
    {
        _renderer = GetComponent<Renderer>();

        if (_renderer == null)
        {
            _renderer = GetComponentInChildren<Renderer>();
        }

        if (_renderer != null && closedMaterial != null)
        {
            _renderer.material = closedMaterial;
        }
    }

    public void Unlock()
    {
        isUnlocked = true;

        if (_renderer != null && openedMaterial != null)
        {
            _renderer.material = openedMaterial;
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log("La porte est maintenant déverrouillée !");
#endif
    }

    public void Interact()
    {
        if (isUnlocked)
        {
            if (GameManager.Instance == null)
            {
                Debug.LogError("GameManager introuvable dans la scène. Impossible de charger la scène suivante.");
                return;
            }

            string nextScene = GameManager.Instance.nextSceneName;

            if (!string.IsNullOrEmpty(nextScene))
            {
                SceneManager.LoadScene(nextScene);
            }
            else
            {
                Debug.LogError("Le nom de la scène suivante n'est pas rempli dans le GameManager !");
            }
        }
        else
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log("La porte est verrouillée, trouve tous les objets !");
#endif
        }
    }

    public void ShowAffordance(bool show)
    {
        if (highlightHalo != null)
        {
            highlightHalo.SetActive(show && isUnlocked);
        }
    }

    public bool CanInteract() => isUnlocked;
}