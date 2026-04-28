using Photon.Pun;
using UnityEngine;

public class TippableObject : MonoBehaviourPun, IInteractable
{
    [SerializeField] private float tippingIncrement = 50f;
    private float tippingForce;
    private Rigidbody RB;
    
    private void Start()
    {
        tippingForce = 0;
        RB = GetComponent<Rigidbody>();
    }

    public void Interact(GameObject go)
    {
        tippingForce += tippingIncrement;
        TransferOwnership(go);
        Push();
    }

    private void TransferOwnership(GameObject gameObject)
    {
        if (gameObject.TryGetComponent<PhotonView>(out PhotonView pv))
        {
            if (!pv.IsMine && pv != null)
            {
                pv.TransferOwnership(PhotonNetwork.LocalPlayer);
                Debug.Log("Transferred ownership");
            }
        }
    }

    private void Push()
    {
        //RB.AddForce(transform.forward * tippingForce, ForceMode.Impulse);
        RB.AddForceAtPosition(RB.rotation * Vector3.forward * tippingForce, transform.position + new Vector3(0, 1, 0));
    }
}
