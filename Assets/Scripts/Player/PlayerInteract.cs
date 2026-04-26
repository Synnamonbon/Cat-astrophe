using System;
using Photon.Pun;
using UnityEngine;

public interface IInteractable
{
    public void Interact(int playerID);
}

public class PlayerInteract : MonoBehaviour
{
    [SerializeField] private float range = 10f;
    private PlayerMovement playerMovement;
    [SerializeField] private Transform playerInteractor;

    public event Action<int, string> InteractWith;          // playerID, tag
    
    private void Awake()
    {
        // playerInteractor = transform;
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
            GameObject go = hitInfo.collider.gameObject;
            Debug.Log(go.tag);
            if (go.TryGetComponent(out IInteractable obj))
            {
                obj.Interact(PhotonNetwork.LocalPlayer.ActorNumber);
                InteractWith?.Invoke(PhotonNetwork.LocalPlayer.ActorNumber, go.tag);
            }
        }
    }

    public void SetLookDir(Transform tf)
    {
        
    }
}
