using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitDoor : MonoBehaviour, IInteractable
{
    public Material closedMaterial;
    public Material openedMaterial;
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

        Debug.Log("La porte est maintenant déverrouillée !");
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
            Debug.Log("La porte est verrouillée, trouve tous les objets !");
        }
    }

    public void ShowAffordance(bool show)
    {
        // Affiche le prompt seulement si la porte est déverrouillée
        if (highlightHalo != null)
        {
            highlightHalo.SetActive(show && isUnlocked);
        }
    }

    public bool CanInteract() => isUnlocked;
}