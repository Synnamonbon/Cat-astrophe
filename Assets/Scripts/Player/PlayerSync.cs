using Photon.Pun;
using UnityEngine;

public class PlayerSync : MonoBehaviourPun, IPunObservable
{
    private Rigidbody playerRB;

    private Vector3 netPos;
    private Quaternion netRot;
    private Vector3 netVel;

    private void Awake()
    {
        playerRB = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (!photonView.IsMine)
        {
            transform.position = Vector3.Lerp(transform.position, netPos, 12f * Time.fixedDeltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, netRot, 12f * Time.fixedDeltaTime);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
            stream.SendNext(playerRB.linearVelocity);
        }
        else
        {
            netPos = (Vector3)stream.ReceiveNext();
            netRot = (Quaternion)stream.ReceiveNext();
            netVel = (Vector3)stream.ReceiveNext();

            // optional prediction
            netPos += netVel * 0.05f;
        }
    }
}
