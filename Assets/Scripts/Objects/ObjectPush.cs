using Photon.Pun;
using UnityEngine;

public class ObjectPush : MonoBehaviourPun
{
    private Rigidbody objectRB;

    private void Awake()
    {
        objectRB = GetComponent<Rigidbody>();
    }

    public void ObjectGotHit(float force, Vector3 forceLocation)
    {
        Debug.Log("sending rpc call");
        photonView.RPC(nameof(RPC_ObjectGotHit), RpcTarget.All, force, forceLocation);
    }

    [PunRPC]
    public void RPC_ObjectGotHit(float force, Vector3 forceLocation)
    {
        Debug.Log("call recieved");
        objectRB.constraints = RigidbodyConstraints.None;
        Vector3 direction = (objectRB.position - forceLocation).normalized;
        direction.y += 0.3f;
        direction.Normalize();

        objectRB.AddForce(direction * force, ForceMode.Impulse);
    }
}
