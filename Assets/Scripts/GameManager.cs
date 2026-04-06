using UnityEngine;
using UnityEngine.SceneManagement; 

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

    private int _objectsFound = 0;

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
        _objectsFound++;
        Debug.Log("Objets trouvés : " + _objectsFound + "/" + totalObjectsToFind);

        if (_objectsFound >= totalObjectsToFind)
        {
            UnlockExit();
        }
    }

    void UnlockExit()
    {
        Debug.Log("Tous les objets vus ! La porte est ouverte.");
        if (exitDoor != null)
        {            
            ExitDoor doorScript = exitDoor.GetComponent<ExitDoor>();
            
            if (doorScript != null) 
            {
                doorScript.Unlock(); 
            }

        }
    }
}