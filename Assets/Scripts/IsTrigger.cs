using UnityEngine;
using Unity.Cinemachine;
using StarterAssets;

public class ZoneCameraAuto : MonoBehaviour
{
    public CinemachineCamera platformCamera;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            platformCamera.Priority = 20;
            Debug.Log("Mode 3D : Activé");

            ThirdPersonController controller = other.GetComponentInParent<ThirdPersonController>();
            if (controller != null)
            {
                controller.RestrictToXAxis = false;
                Debug.Log("Mouvements débloqués sur tous les axes");
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
            Debug.Log("Mode 2D : Retour à la normale");

            ThirdPersonController controller = other.GetComponentInParent<ThirdPersonController>();
            if (controller != null)
            {
                controller.RestrictToXAxis = true;
                Debug.Log("Mouvements verrouillés sur X uniquement");
            }
        }
    }
} 