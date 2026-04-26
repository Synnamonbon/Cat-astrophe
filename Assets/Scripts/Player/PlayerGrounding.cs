using UnityEngine;

public class PlayerGrounding : MonoBehaviour
{
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private LayerMask groundLayers;
    [SerializeField] private LayerMask furnitureLayers;

    private int groundContacts = 0;

    private void OnTriggerEnter(Collider other)
    {
        if (!IsGround(other)) return;
        groundContacts++;
        playerMovement.isGrounded = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsGround(other)) return;
        groundContacts--;
        if (groundContacts <= 0)
        {
            groundContacts = 0;
            playerMovement.isGrounded = false;
        }
    }

    private bool IsGround(Collider col)
    {
        return ((1 << col.gameObject.layer) & groundLayers) != 0 || ((1 << col.gameObject.layer) & furnitureLayers) != 0;
    }
}
