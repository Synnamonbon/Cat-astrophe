using Photon.Pun;
using UnityEngine;

public class CageScript : MonoBehaviourPun, IInteractable
{
    private PlayerController cagedCat;
    private GameObject freePoint;

    void Start()
    {
        freePoint = transform.GetChild(0).gameObject;
        cagedCat = null;
    }

    public void LockCat(PlayerController cat)
    {
        cagedCat = cat;
    }

    // Attempts to rescue a cat, returning true on success and false on failure.
    public bool FreeCat()
    {
        // Check if we have a cat caged, if not, return false
        if (cagedCat == null)
        {
            Debug.Log("No cat in cage.");
            return false;
        }
        photonView.RPC(nameof(FreeRPC), RpcTarget.All);
        // Make cat visible
        if (cagedCat.TryGetComponent<PlayerController>(out PlayerController pc))
        {
            Debug.Log("Freed " + cagedCat.name);
            pc.isVisible = true;
        }
        else
        {
            Debug.Log("No Player Controller when freeing from cage.");
        }
        cagedCat = null;
        return true;
    }

    [PunRPC]
    private void FreeRPC()
    {
        // Move cat to the freed position
        cagedCat.transform.SetLocalPositionAndRotation(freePoint.transform.position, freePoint.transform.rotation);
    }

    public void Interact(GameObject go)
    {
        FreeCat();
    }
}
