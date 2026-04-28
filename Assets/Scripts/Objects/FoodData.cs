using Photon.Pun;
using UnityEngine;

public class FoodData : MonoBehaviourPun
{
    [SerializeField] private Food_SO foodData;
    private bool isUsed;

    private void OnEnable()
    {
        isUsed = false;
    }

    [PunRPC]
    public void RequestConsume(int playerPVID, PhotonMessageInfo info)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (isUsed) return;

        PhotonView playerPV = PhotonView.Find(playerPVID);
        if (playerPV == null) return;
        
        isUsed = true;
        //Debug.Log("Consume request received");

        playerPV.RPC(nameof(PlayerHunger.RestoreHunger), playerPV.Owner, foodData.HungerRestoreValue);
        PhotonNetwork.Destroy(gameObject);
    }
}
