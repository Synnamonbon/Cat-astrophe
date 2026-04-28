using Photon.Pun;
using UnityEngine;

public class DistractOnTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<PlayerController>(out PlayerController playerController))
        {
            PhotonView targetView = playerController.photonView;

            targetView.RPC("ToyDistract", targetView.Owner, transform.position);
        }
    }
}
