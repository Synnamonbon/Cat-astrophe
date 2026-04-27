using UnityEngine;

public class CageScript : MonoBehaviour
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
        // Move cat to the freed position
        cagedCat.transform.SetLocalPositionAndRotation(freePoint.transform.position, freePoint.transform.rotation);
        // Make cat visible
        if (cagedCat.TryGetComponent<PlayerController>(out PlayerController pc))
        {
            pc.isVisible = true;
        }
        else
        {
            Debug.Log("No Player Controller when freeing from cage.");
        }
        cagedCat = null;
        return true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<PlayerController>(out PlayerController pc))
        {
            if(pc.isVisible && cagedCat != null)
            {
                FreeCat();
            }
        }
    }
}
