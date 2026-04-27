using UnityEngine;

public class Hide : MonoBehaviour, IInteractable
{
    public void Interact(GameObject go)
    {
        Debug.Log("Hiding");
    }
}
