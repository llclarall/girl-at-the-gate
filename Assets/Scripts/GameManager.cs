using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// GameManager is responsible for tracking the player's progress in finding objects and unlocking the exit door when all objects have been found. It also manages the transition to the next scene when the player interacts with the exit door after finding all required objects.    
/// </summary>

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Configuration")]
    public int totalObjectsToFind;
    public GameObject exitDoor;
    public string nextSceneName;
    public GameObject doorLight;

    [Header("Feedback de déverrouillage")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip screamClip;
    [SerializeField, Range(0f, 1f)] private float screamVolume = 0.55f;
    [SerializeField] private Image unlockMessageImage;
    [SerializeField] private Sprite unlockMessageSprite;
    [SerializeField] private float unlockMessageDuration = 20f;

    private int _objectsFound = 0;
    private bool _exitUnlocked = false;
    private bool _unlockFeedbackPending = false;
    private Coroutine _unlockMessageRoutine;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void PlayGame()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }

    public void ObjectInteracted()
    {
        if (_exitUnlocked)
        {
            return;
        }

        _objectsFound++;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log("Objets trouvés : " + _objectsFound + "/" + totalObjectsToFind);
#endif

        if (_objectsFound >= totalObjectsToFind)
        {
            UnlockExit();
        }
    }

    void UnlockExit()
    {
        if (_exitUnlocked)
        {
            return;
        }

        _exitUnlocked = true;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log("Tous les objets vus ! La porte est ouverte.");
#endif
        if (exitDoor != null)
        {
            ExitDoor doorScript = exitDoor.GetComponent<ExitDoor>();

            if (doorScript != null)
            {
                doorScript.Unlock();
            }

        }

        _unlockFeedbackPending = true;
        TryPlayUnlockFeedback();
    }

    public void NotifyInteractionClosed()
    {
        TryPlayUnlockFeedback();
    }

    private void TryPlayUnlockFeedback()
    {
        if (!_unlockFeedbackPending)
        {
            return;
        }

        UISystem uiSystem = Object.FindAnyObjectByType<UISystem>();
        if (uiSystem != null && uiSystem.IsObjectOpen())
        {
            return;
        }

        _unlockFeedbackPending = false;
        PlayScreamSound();
        ShowUnlockSprite();
    }

    private void PlayScreamSound()
    {
        if (screamClip == null)
        {
            return;
        }

        if (audioSource != null)
        {
            audioSource.PlayOneShot(screamClip, screamVolume);
            return;
        }

        AudioSource.PlayClipAtPoint(screamClip, transform.position, screamVolume);
    }

    private void ShowUnlockSprite()
    {
        if (unlockMessageImage == null)
        {
            return;
        }

        if (_unlockMessageRoutine != null)
        {
            StopCoroutine(_unlockMessageRoutine);
        }

        _unlockMessageRoutine = StartCoroutine(ShowUnlockSpriteRoutine());
    }

    private IEnumerator ShowUnlockSpriteRoutine()
    {
        if (unlockMessageSprite != null)
        {
            unlockMessageImage.sprite = unlockMessageSprite;
        }

        unlockMessageImage.enabled = true;
        unlockMessageImage.gameObject.SetActive(true);

        yield return new WaitForSeconds(unlockMessageDuration);

        unlockMessageImage.gameObject.SetActive(false);
        _unlockMessageRoutine = null;
    }
}