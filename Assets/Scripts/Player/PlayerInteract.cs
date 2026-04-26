using System;
using UnityEngine;

public interface IInteractable
{
    public void Interact();
}

public class PlayerInteract : MonoBehaviour
{
    [SerializeField] private float range = 10f;
    private PlayerMovement playerMovement;
    private Transform playerInteractor;

    private void Awake()
    {
        playerInteractor = transform;
        playerMovement = GetComponent<PlayerMovement>();
    }

    private void OnEnable()
    {
    InputManager.instance.OnInteract += InteractAction;
    }

    private void OnDisable()
    {
        InputManager.instance.OnInteract -= InteractAction;
    }

    private void InteractAction(object sender, EventArgs e)
    {
        Quaternion camForward = playerMovement.GetCameraForward();
        Vector3 direction = camForward * Vector3.forward;

        Ray r = new Ray(playerInteractor.position, direction);
        if (Physics.Raycast(r, out RaycastHit hitInfo, range))
        {
            if (hitInfo.collider.gameObject.TryGetComponent(out IInteractable obj))
            {
                obj.Interact();
            }
        }
    }
}
