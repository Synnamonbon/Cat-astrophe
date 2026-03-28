using Photon.Pun;
using UnityEngine;

public class FoodData : MonoBehaviour
{
    [SerializeField] private Food_SO foodData;
    private PhotonView pv;
    public bool isUsed {get; private set;}

    private void OnEnable()
    {
        pv = GetComponent<PhotonView>();
        isUsed = false;
    }

    public float Consume()
    {
        if (pv == null)
        {
            Debug.Log("Food missing photonview");
            return 0f;
        }
        isUsed = true;
        pv.TransferOwnership(PhotonNetwork.LocalPlayer);
        return foodData.HungerRestoreValue;
    }
    
    public void DestroyFood()
    {
        if (pv == null) return;
        if (!pv.IsMine) return;
        PhotonNetwork.Destroy(gameObject);
    }
}
