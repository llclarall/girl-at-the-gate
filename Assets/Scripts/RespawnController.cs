using UnityEngine;
using Unity.Cinemachine;
using StarterAssets;

/// <summary>
/// This script is responsible for managing the player's respawn mechanics. It monitors the player's position and respawns them at a designated checkpoint if they fall below a certain threshold. 
/// </summary>

public class AutoRespawn : MonoBehaviour
{
    [SerializeField] private float m_KillThreshold = -10f;
    private Vector3 m_StartPosition;
    private Vector3 m_CheckpointPosition;
    private CharacterController m_Controller;

    private void Awake()
    {
        m_StartPosition = transform.position;
        m_CheckpointPosition = m_StartPosition;
        m_Controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        if (transform.position.y < m_KillThreshold)
        {
            Respawn();
        }
    }

    public void SetCheckpoint()
    {
        m_CheckpointPosition = transform.position;
    }

    public void Respawn()
    {
        if (m_Controller != null) m_Controller.enabled = false;

        transform.position = m_CheckpointPosition;

        if (m_Controller != null) m_Controller.enabled = true;
    }
}