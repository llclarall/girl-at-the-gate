using UnityEngine;

public class AutoRespawn : MonoBehaviour
{
    [SerializeField] private float m_KillThreshold = -10f;
    private Vector3 m_StartPosition;
    private CharacterController m_Controller;

    private void Awake()
    {
        m_StartPosition = transform.position;
        m_Controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        if (transform.position.y < m_KillThreshold)
        {
            Respawn();
        }
    }

    public void Respawn()
    {
        if (m_Controller != null) m_Controller.enabled = false;

        transform.position = m_StartPosition;

        if (m_Controller != null) m_Controller.enabled = true;
    }
}