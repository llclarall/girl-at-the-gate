using UnityEngine;

/// <summary>
/// This script is attached to the cemetery exit door. It automatically unlocks the door when the game starts, allowing players to exit the cemetery without needing to find a key. 
/// </summary>

public class AutoUnlock : MonoBehaviour
{
    void Start()
    {
        ExitDoor door = GetComponent<ExitDoor>();

        if (door != null)
        {
            door.Unlock(); 
            Debug.Log("La porte du cimetière a été déverrouillée automatiquement au lancement.");
        }
    }
}