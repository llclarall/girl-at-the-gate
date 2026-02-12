using UnityEngine;

/// This script makes the camera follow the player. 

public class CameraFollow : MonoBehaviour
{
    public Transform player;
    private Vector3 offset;
    private float maxCameraX = 8.4f + 5f; // limit to the right of the cabin so the camera doesn't show outside of the cabin
    private float minCameraX = -3f + 5f; // limit to the left of the cabin

    void Start()
    {
        if (player != null)
        {
            offset = transform.position - player.position;
            maxCameraX = 8.4f + offset.x;
            minCameraX = -3f + offset.x;
        }
    }

    void LateUpdate()
    {
        if (player == null) return;

        Vector3 targetPosition = new Vector3(player.position.x + offset.x, transform.position.y, player.position.z + offset.z);

        targetPosition.x = Mathf.Max(Mathf.Min(targetPosition.x, maxCameraX), minCameraX);

        transform.position = targetPosition;
    }
}