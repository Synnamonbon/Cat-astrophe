using UnityEngine;

public class PlayerGrounding : MonoBehaviour
{
    [SerializeField] private PlayerMovement playerMovement;
    private int groundContacts = 0;

    private void OnTriggerEnter(Collider other)
    {
        groundContacts++;
        playerMovement.isGrounded = true;
    }

    private void OnTriggerExit(Collider other)
    {
        groundContacts --;
        if(groundContacts <= 0)
        {
            groundContacts = 0;
            playerMovement.isGrounded = false;   
        }
    }
}
