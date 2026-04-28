using System;
using UnityEngine;

public class PlayerGrounding : MonoBehaviour
{
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private LayerMask groundLayers;
    [SerializeField] private LayerMask furnitureLayers;
    [SerializeField] private float landAlertRadius = 5f;

    private int groundContacts = 0;
    private bool landAlert;
    public event Action<Transform, float> LandAlert;
    private void Start()
    {
        landAlert = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsGround(other)) return;
        groundContacts++;
        if (landAlert)
        {
            LandAlert?.Invoke(transform, landAlertRadius);
            landAlert = false;
        }
        playerMovement.isGrounded = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsGround(other)) return;
        groundContacts--;
        if (groundContacts <= 0)
        {
            landAlert = true;
            groundContacts = 0;
            playerMovement.isGrounded = false;
        }
    }

    private bool IsGround(Collider col)
    {
        return ((1 << col.gameObject.layer) & groundLayers) != 0 || ((1 << col.gameObject.layer) & furnitureLayers) != 0;
    }
}
