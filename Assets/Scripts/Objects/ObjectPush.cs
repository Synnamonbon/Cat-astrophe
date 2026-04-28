using Photon.Pun;
using UnityEngine;

public class ObjectPush : MonoBehaviour
{
    private Rigidbody objectRB;
    private PhotonView pv;
    private float lastTransferTime;
    private const float transferCooldown = 0.2f;

    private void Awake()
    {
        objectRB = GetComponent<Rigidbody>();
        pv = GetComponent<PhotonView>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (pv == null) return;
        if (!collision.gameObject.CompareTag("Player")) return;

        PhotonView playerPv = collision.collider.GetComponentInParent<PhotonView>();
        if (playerPv == null) return;
        if (!playerPv.IsMine) return;

        if (pv.Owner == PhotonNetwork.LocalPlayer) return;
        if (Time.time - lastTransferTime < transferCooldown) return;

        lastTransferTime = Time.time;
        pv.TransferOwnership(PhotonNetwork.LocalPlayer);
        Debug.Log($"Transferred ownership to {PhotonNetwork.LocalPlayer.NickName}");
    }

    public void ObjectGotHit(float force, Vector3 forceLocation)
    {
        //Debug.Log("Pushing object");
        objectRB.constraints = RigidbodyConstraints.None;
        Vector3 direction = (objectRB.position - forceLocation).normalized;
        direction.y += 0.3f;
        direction.Normalize();

        objectRB.AddForce(direction * force, ForceMode.Impulse);
    }
}
