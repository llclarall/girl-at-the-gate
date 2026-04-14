using UnityEngine;
using UnityEngine.SceneManagement;

/// This script makes the camera follow the player. It also limits the camera's horizontal movement in specific scenes (like the cabin) to prevent showing areas outside of the intended view. 

/// <remarks>
/// i know i could have made this more generic by using empty game objects as camera bounds, but i started developping this game before learning about that technique, and it was easier to just hardcode the limits for the cabin scene. I might refactor this later to make it more flexible, but for now it works fine for our needs.
/// </remarks>

public class CameraFollow : MonoBehaviour
{
    public Transform player;
    [SerializeField] private string[] boundedSceneNames = { "CabinScene", "CabinScene 1" };
    private Vector3 offset;
    private float maxCameraX = 5.7f + 5f; // limit to the right of the cabin so the camera doesn't show outside of the cabin
    private float minCameraX = -3f + 5f; // limit to the left of the cabin

    void Start()
    {
        if (player != null)
        {
            offset = transform.position - player.position;
            maxCameraX = 5.7f + offset.x;
            minCameraX = -3f + offset.x;
        }
    }

    void LateUpdate()
    {
        if (player == null) return;

        Vector3 targetPosition = new Vector3(player.position.x + offset.x, transform.position.y, player.position.z + offset.z);

        if (IsBoundedScene(SceneManager.GetActiveScene().name))
        {
            targetPosition.x = Mathf.Max(Mathf.Min(targetPosition.x, maxCameraX), minCameraX);
        }

        transform.position = targetPosition;
    }

    private bool IsBoundedScene(string activeSceneName)
    {
        if (string.IsNullOrWhiteSpace(activeSceneName) || boundedSceneNames == null || boundedSceneNames.Length == 0)
        {
            return false;
        }

        string normalizedActiveName = activeSceneName.Trim();

        for (int i = 0; i < boundedSceneNames.Length; i++)
        {
            string sceneName = boundedSceneNames[i];

            if (string.IsNullOrWhiteSpace(sceneName))
            {
                continue;
            }

            if (string.Equals(normalizedActiveName, sceneName.Trim(), System.StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}