using UnityEngine;
using Unity.Cinemachine;
using StarterAssets;

/// <summary>
/// This script is attached to the camera trigger zones in the game. When the player enters a trigger zone, it automatically switches the camera to a predefined perspective (e.g., from 2D to 3D) and adjusts the player's movement restrictions accordingly. It also sets a checkpoint for respawning if the player dies within that zone. When the player exits the trigger zone, it reverts the camera and movement settings back to their original state.
/// </summary>

public class ZoneCameraAuto : MonoBehaviour
{
    [SerializeField] private CinemachineCamera platformCamera;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            platformCamera.Priority = 20;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log("Mode 3D : Activé");
#endif

            ThirdPersonController controller = other.GetComponentInParent<ThirdPersonController>();
            if (controller != null)
            {
                controller.RestrictToXAxis = false;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.Log("Mouvements débloqués sur tous les axes");
#endif
            }

            AutoRespawn respawnController = other.GetComponentInParent<AutoRespawn>();
            if (respawnController != null)
            {
                respawnController.SetCheckpoint();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            platformCamera.Priority = 5;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log("Mode 2D : Retour à la normale");
#endif

            ThirdPersonController controller = other.GetComponentInParent<ThirdPersonController>();
            if (controller != null)
            {
                controller.RestrictToXAxis = true;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.Log("Mouvements verrouillés sur X uniquement");
#endif
            }
        }
    }
}