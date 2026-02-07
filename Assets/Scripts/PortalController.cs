using UnityEngine;

public class PortalController : MonoBehaviour
{
    [SerializeField] private Transform m_Destination; 

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && m_Destination != null)
        {
            CharacterController cc = other.GetComponent<CharacterController>();

            if (cc != null) cc.enabled = false;

            other.transform.position = m_Destination.position;
            other.transform.rotation = m_Destination.rotation;

            if (cc != null) cc.enabled = true;
        }
    }
}