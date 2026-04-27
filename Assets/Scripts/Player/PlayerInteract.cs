using System;
using Photon.Pun;
using UnityEngine;

public interface IInteractable
{
    public void Interact(GameObject go);
}

public class PlayerInteract : MonoBehaviour
{
    [SerializeField] private float range = 10f;
    [SerializeField] private Transform playerInteractor;
    private Transform lookDir;
    public event Action<int, string> InteractWith;          // playerID, tag

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
        Ray r = new Ray(playerInteractor.position, (playerInteractor.position - lookDir.position).normalized);
        Debug.DrawRay(playerInteractor.position, (playerInteractor.position - lookDir.position).normalized, Color.yellow, 1f);
        if (Physics.Raycast(r, out RaycastHit hitInfo, range))
        {
            GameObject go = hitInfo.collider.gameObject;
            Debug.Log(go.tag);
            if (go.TryGetComponent(out IInteractable obj))
            {
                obj.Interact(gameObject);
                InteractWith?.Invoke(PhotonNetwork.LocalPlayer.ActorNumber, go.tag);
            }
        }
    }

    public void SetLookDir(Transform tf)
    {
        lookDir = tf;
    }
}
