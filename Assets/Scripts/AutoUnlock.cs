using UnityEngine;

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