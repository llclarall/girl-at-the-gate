using UnityEngine;

/// This script makes the camera follow the player. 

public class CameraFollow : MonoBehaviour
{
    public Transform player; 
    private Vector3 offset; 

    void Start()
    {
        if (player != null)
        {
            offset = transform.position - player.position;
        }
    }

    void LateUpdate()
    {
        if (player == null) return;

        // Calculate the new target position    
        Vector3 targetPosition = new Vector3(player.position.x + offset.x, transform.position.y, player.position.z + offset.z);

        transform.position = targetPosition;
    }
}